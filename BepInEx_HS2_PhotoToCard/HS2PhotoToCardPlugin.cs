using System;
using System.Collections;
using System.IO;
using System.Reflection;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using CharaCustom;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HS2.PhotoToCard.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class HS2PhotoToCardPlugin : BaseUnityPlugin
    {
        private ConfigEntry<string> _requestFilePath;
        private ConfigEntry<bool> _autoEnterCharaCustom;
        private ConfigEntry<int> _startupDelaySeconds;
        private ConfigEntry<string> _readyFileName;
        private float _lastCheckTime;
        private const float CheckInterval = 0.5f;
        private bool _isProcessing;
        private bool _readyFileWritten;
        private bool _hasZoomedThisSession;
        private float _screenshotFov = -1f;

        private void Awake()
        {
            _requestFilePath = Config.Bind(
                "Paths",
                "RequestFile",
                @"D:\HS4\output\load_card_request.txt",
                "Full path to the request file. Content: one line = full path to character card to load (face only). Screenshot will be written to same directory as game_screenshot.png");
            _autoEnterCharaCustom = Config.Bind(
                "Auto",
                "AutoEnterCharaCustom",
                true,
                "If true, after startup delay load CharaCustom scene and write game_ready.txt so Python can run fully unattended.");
            _startupDelaySeconds = Config.Bind(
                "Auto",
                "StartupDelaySeconds",
                25,
                "Seconds to wait after game start before loading CharaCustom (give game time to init).");
            _readyFileName = Config.Bind(
                "Paths",
                "ReadyFileName",
                "game_ready.txt",
                "File name written in request file directory when CharaCustom is ready (Python waits for this).");
            _lastCheckTime = 0f;
            _isProcessing = false;
            _readyFileWritten = false;
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded. Request file: {_requestFilePath.Value}");
            if (_autoEnterCharaCustom.Value)
            {
                Logger.LogInfo($"[PhotoToCard] AutoEnterCharaCustom enabled. Will load CharaCustom after {_startupDelaySeconds.Value}s.");
                StartCoroutine(AutoEnterCharaCustomCoroutine());
            }
        }

        private IEnumerator AutoEnterCharaCustomCoroutine()
        {
            yield return new WaitForSeconds(Mathf.Clamp(_startupDelaySeconds.Value, 5, 120));
            Logger.LogInfo("[PhotoToCard] Loading CharaCustom scene...");
            var op = SceneManager.LoadSceneAsync("CharaCustom");
            if (op != null)
            {
                while (!op.isDone)
                    yield return null;
                Logger.LogInfo("[PhotoToCard] CharaCustom scene loaded.");
            }
            else
                Logger.LogWarning("[PhotoToCard] LoadSceneAsync(CharaCustom) returned null.");
        }

        private void Update()
        {
            if (_isProcessing) return;

            var sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "CharaCustom" && !_readyFileWritten)
            {
                var baseObj = Singleton<CustomBase>.Instance;
                if (baseObj != null && baseObj.chaCtrl != null)
                {
                    var reqPath = _requestFilePath.Value;
                    if (!string.IsNullOrEmpty(reqPath))
                    {
                        try
                        {
                            reqPath = Path.GetFullPath(reqPath);
                            var dir = Path.GetDirectoryName(reqPath);
                            if (!string.IsNullOrEmpty(dir))
                            {
                                var readyPath = Path.Combine(dir, _readyFileName.Value);
                                File.WriteAllText(readyPath, DateTime.UtcNow.ToString("o"));
                                _readyFileWritten = true;
                                Logger.LogInfo($"[PhotoToCard] Ready file written: {readyPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning($"[PhotoToCard] Could not write ready file: {ex.Message}");
                        }
                    }
                }
            }

            if (Time.time - _lastCheckTime < CheckInterval) return;
            _lastCheckTime = Time.time;

            if (sceneName != "CharaCustom")
                return;

            var requestPath = _requestFilePath.Value;
            if (string.IsNullOrEmpty(requestPath))
                return;
            try { requestPath = Path.GetFullPath(requestPath); } catch { }
            if (!File.Exists(requestPath))
                return;

            string cardPath = null;
            try
            {
                var lines = File.ReadAllLines(requestPath);
                foreach (var line in lines)
                {
                    var s = line.Trim();
                    if (string.IsNullOrEmpty(s) || s.StartsWith("#"))
                        continue;
                    cardPath = s;
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PhotoToCard] Failed to read request file: {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(cardPath))
            {
                try { File.WriteAllText(requestPath, ""); } catch { }
                return;
            }
            try { cardPath = Path.GetFullPath(cardPath); } catch { }
            if (!File.Exists(cardPath))
            {
                try { File.WriteAllText(requestPath, ""); } catch { }
                Logger.LogWarning($"[PhotoToCard] Card file not found: {cardPath}. Cleared request.");
                return;
            }

            Logger.LogInfo($"[PhotoToCard] Request file found. Card to load: {cardPath}");
            try
            {
                File.WriteAllText(requestPath, "");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PhotoToCard] Failed to clear request file: {ex.Message}");
                return;
            }

            var customBase = Singleton<CustomBase>.Instance;
            if (customBase == null)
            {
                Logger.LogWarning("[PhotoToCard] CustomBase is null. Not in character maker?");
                return;
            }
            if (customBase.chaCtrl == null)
            {
                Logger.LogWarning("[PhotoToCard] chaCtrl is null. Open character load UI?");
                return;
            }

            Logger.LogInfo("[PhotoToCard] Starting LoadCardAndScreenshot.");
            _isProcessing = true;
            StartCoroutine(LoadCardAndScreenshot(cardPath, requestPath));
        }

        private static readonly string DebugLogPath = Path.Combine(@"D:\HS4", "debug-526b9a.log");

        private void AppendDebugLog(string requestFilePath, string hypothesisId, string message, string dataJson)
        {
            // #region agent log
            try
            {
                var logPath = DebugLogPath;
                var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var line = "{\"sessionId\":\"526b9a\",\"hypothesisId\":\"" + hypothesisId + "\",\"location\":\"HS2PhotoToCardPlugin.LoadCardAndScreenshot\",\"message\":\"" + (message ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\",\"data\":" + (dataJson ?? "{}") + ",\"timestamp\":" + ts + "}\n";
                File.AppendAllText(logPath, line);
            }
            catch { }
            // #endregion
        }

        private IEnumerator LoadCardAndScreenshot(string cardPath, string requestFilePath)
        {
            AppendDebugLog(requestFilePath, "H0", "LoadCardAndScreenshot started", "{}");
            var chaCtrl = Singleton<CustomBase>.Instance?.chaCtrl;
            if (chaCtrl == null)
            {
                Logger.LogError("[PhotoToCard] chaCtrl null in coroutine.");
                _isProcessing = false;
                yield break;
            }

            bool loadOk = false;
            int headId = -1;
            IEnumerator reloadEn = null;
            try
            {
                Logger.LogInfo($"[PhotoToCard] LoadFileLimited (face only): {cardPath}");
                loadOk = chaCtrl.chaFile.LoadFileLimited(cardPath, chaCtrl.sex, true, false, false, false, false);
                try { headId = chaCtrl.chaFile.custom.face.headId; } catch { }
                AppendDebugLog(requestFilePath, "H2", "LoadFileLimited done", "{\"loadOk\":" + (loadOk ? "true" : "false") + ",\"headId\":" + headId + "}");
                chaCtrl.ChangeNowCoordinate();
                Singleton<Character>.Instance.customLoadGCClear = false;
                var customBase = Singleton<CustomBase>.Instance;
                if (customBase != null) customBase.updateCustomUI = true;
                reloadEn = InvokeReloadAsync(chaCtrl, noChangeClothes: true, noChangeHead: false, noChangeHair: true, noChangeBody: true);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PhotoToCard] Load/Reload exception: {ex.Message}\n{ex.StackTrace}");
                AppendDebugLog(requestFilePath, "H2", "Load/Reload exception", "{\"ex\":\"" + (ex.Message ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"}");
                _isProcessing = false;
                yield break;
            }

            if (reloadEn != null)
            {
                AppendDebugLog(requestFilePath, "H1", "Yielding ReloadAsync", "{}");
                yield return reloadEn;
            }
            else
            {
                AppendDebugLog(requestFilePath, "H1", "ReloadAsync fallback wait", "{}");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(3f);
            }
            Singleton<Character>.Instance.customLoadGCClear = true;

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1f);

            var dir = Path.GetDirectoryName(requestFilePath);
            if (string.IsNullOrEmpty(dir)) dir = @"D:\HS4\output";
            var screenshotPath = Path.Combine(dir, "game_screenshot.png");
            try { screenshotPath = Path.GetFullPath(screenshotPath); } catch { }

            if (!_hasZoomedThisSession)
            {
                AppendDebugLog(requestFilePath, "H4", "first capture branch", "{\"hasZoomed\":false}");
                var cam = Camera.main;
                var camCtrl = cam != null ? cam.GetComponent<BaseCameraControl_Ver2>() : null;
                if (cam == null || camCtrl == null)
                {
                    AppendDebugLog(requestFilePath, "H4", "cam or camCtrl null", "{\"camNull\":" + (cam == null) + ",\"camCtrlNull\":" + (camCtrl == null) + "}");
                }
                else
                {
                    // H2 fix: point camera at head (same bone as game Q key: cf_J_Head) so zoom centers on face
                    Transform headTr = chaCtrl.objBodyBone != null
                        ? chaCtrl.objBodyBone.transform.FindLoop("cf_J_Head")
                        : (chaCtrl.objHeadBone != null ? chaCtrl.objHeadBone.transform : (chaCtrl.cmpBoneBody?.targetEtc.trfHeadParent));
                    if (headTr != null)
                    {
                        Vector3 headWorld = headTr.position;
                        camCtrl.TargetPos = camCtrl.transBase != null
                            ? camCtrl.transBase.InverseTransformPoint(headWorld)
                            : headWorld;
                        AppendDebugLog(requestFilePath, "H2", "target set to head cf_J_Head", "{\"headWorld\":{\"x\":" + headWorld.x.ToString("F3") + ",\"y\":" + headWorld.y.ToString("F3") + ",\"z\":" + headWorld.z.ToString("F3") + "},\"transBaseNotNull\":" + (camCtrl.transBase != null) + "}");
                        yield return null;
                        yield return null;
                        var posAfterYields = camCtrl.TargetPos;
                        AppendDebugLog(requestFilePath, "H7", "TargetPos after 2 yields", "{\"x\":" + posAfterYields.x.ToString("F3") + ",\"y\":" + posAfterYields.y.ToString("F3") + ",\"z\":" + posAfterYields.z.ToString("F3") + "}");
                    }
                    float currentHeadPixels = GetHeadHeightInScreenPixels(chaCtrl, cam);
                    var targetPos = camCtrl.TargetPos;
                    AppendDebugLog(requestFilePath, "H1", "head pixels and target", "{\"currentHeadPixels\":" + currentHeadPixels.ToString("F2") + ",\"targetPos\":{\"x\":" + targetPos.x.ToString("F3") + ",\"y\":" + targetPos.y.ToString("F3") + ",\"z\":" + targetPos.z.ToString("F3") + "}}");
                    const float targetFaceFractionOfScreen = 0.35f;
                    float desiredHeadPixels = UnityEngine.Screen.height * targetFaceFractionOfScreen;
                    if (currentHeadPixels <= 1f || desiredHeadPixels <= 1f)
                    {
                        AppendDebugLog(requestFilePath, "H1", "skip zoom: head pixels too small or desired invalid", "{\"currentHeadPixels\":" + currentHeadPixels.ToString("F2") + ",\"desiredHeadPixels\":" + desiredHeadPixels.ToString("F2") + "}");
                    }
                    else
                    {
                        float curFov = camCtrl.CameraFov;
                        float curFovRadHalf = curFov * Mathf.Deg2Rad * 0.5f;
                        float newFovRadHalf = Mathf.Atan(Mathf.Tan(curFovRadHalf) * currentHeadPixels / desiredHeadPixels);
                        float newFov = Mathf.Clamp(newFovRadHalf * 2f * Mathf.Rad2Deg, 10f, 40f);
                        AppendDebugLog(requestFilePath, "H3", "FOV computed", "{\"curFov\":" + curFov.ToString("F2") + ",\"newFov\":" + newFov.ToString("F2") + ",\"currentHeadPixels\":" + currentHeadPixels.ToString("F2") + ",\"desiredHeadPixels\":" + desiredHeadPixels.ToString("F2") + "}");
                        _screenshotFov = newFov;
                        camCtrl.CameraFov = newFov;
                        _hasZoomedThisSession = true;
                        Logger.LogInfo($"[PhotoToCard] First capture: zoom FOV {curFov:F1} -> {newFov:F1} (head {currentHeadPixels:F0}px -> target {desiredHeadPixels:F0}px). FOV recorded, will not restore.");
                        try
                        {
                            var fovPath = Path.Combine(dir, "game_screenshot_fov.txt");
                            File.WriteAllText(fovPath, newFov.ToString("F2"));
                        }
                        catch (Exception ex) { Logger.LogWarning($"[PhotoToCard] Could not write FOV file: {ex.Message}"); }
                    }
                }
                yield return null;
            }

            AppendDebugLog(requestFilePath, "H4", "Before CaptureScreenshot", _screenshotFov >= 0 ? "{\"screenshotFov\":" + _screenshotFov.ToString("F2") + "}" : "{}");

            try
            {
                Logger.LogInfo($"[PhotoToCard] CaptureScreenshot: {screenshotPath}");
                UnityEngine.ScreenCapture.CaptureScreenshot(screenshotPath, 1);
                Logger.LogInfo($"[PhotoToCard] Screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[PhotoToCard] Screenshot failed: {ex.Message}\n{ex.StackTrace}");
            }

            _isProcessing = false;
        }

        private static float GetHeadHeightInScreenPixels(ChaControl chaCtrl, Camera cam)
        {
            if (chaCtrl == null || cam == null) return 0f;
            Transform headTr = null;
            if (chaCtrl.objHeadBone != null)
                headTr = chaCtrl.objHeadBone.transform;
            else if (chaCtrl.cmpBoneBody?.targetEtc.trfHeadParent != null)
                headTr = chaCtrl.cmpBoneBody.targetEtc.trfHeadParent;
            if (headTr == null) return 0f;
            Vector3 headCenter = headTr.position;
            const float headHalfHeight = 0.08f;
            Vector3 headTop = headCenter + Vector3.up * headHalfHeight;
            Vector3 screenCenter = cam.WorldToScreenPoint(headCenter);
            Vector3 screenTop = cam.WorldToScreenPoint(headTop);
            float dy = screenTop.y - screenCenter.y;
            return Mathf.Abs(dy) * 2f;
        }

        private static IEnumerator InvokeReloadAsync(ChaControl chaCtrl, bool noChangeClothes, bool noChangeHead, bool noChangeHair, bool noChangeBody)
        {
            try
            {
                var t = chaCtrl.GetType();
                var method = t.GetMethod("ReloadAsync", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }, null);
                if (method == null) return null;
                var en = method.Invoke(chaCtrl, new object[] { noChangeClothes, noChangeHead, noChangeHair, noChangeBody, true, false }) as IEnumerator;
                return en;
            }
            catch { return null; }
        }
    }
}
