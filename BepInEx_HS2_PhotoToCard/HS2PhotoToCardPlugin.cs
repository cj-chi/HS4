using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Globalization;
#if PHOTOTOCARD_AUTOMATION
using System.Reflection;
using IllusionUtility.GetUtility;
#endif
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using CharaCustom;
using Manager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HS2.PhotoToCard.Plugin
{
    /// <summary>樹狀節點：臉部骨骼之一，用於除錯選單與 2D 點顯示。</summary>
    public class BoneNode
    {
        public string Name;
        public Transform Transform;
        public List<BoneNode> Children = new List<BoneNode>();
        public bool Show;
        public bool Foldout = true;
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class HS2PhotoToCardPlugin : BaseUnityPlugin
    {
#if PHOTOTOCARD_AUTOMATION
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
#endif

        // 方案 A：臉部骨骼樹狀選單與 2D 點顯示（僅頭部骨架）
        private ConfigEntry<KeyCode> _boneDebugToggleKey;
        private bool _boneDebugVisible;
        private BoneNode _boneRoot;
        private ChaControl _boneTreeChaCtrl;
        /// <summary>objHeadBone.transform.GetInstanceID() when tree was built; changes after ReloadAsync → must rebuild.</summary>
        private int _boneHeadRootTransformId = int.MinValue;
        private Vector2 _boneMenuScroll;
        private const float BoneMenuWidth = 320f;
        private const float BoneMenuMaxHeightRatio = 0.85f;
        private const int BoneWindowId = 0x7b0e;
        private const float DotSize = 11f;
        private const float LabelMaxWidth = 220f;
        private const float LabelHeight = 22f;
        private const int LabelFontSize = 14;
        private static GUIStyle _dotStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _labelBgStyle;
        private Rect _boneMenuRect;
        private bool _hideTextures;

        private bool _structureMode;
        private BoneNode _structureAnchorNode;
        private Material _structureLineMat;
        private Vector2 _bonePickMouseDownGui;
        private bool _bonePickWaitingUp;
        private const float BonePickMaxMovePx = 6f;
        private static readonly Color StructureLineColor = new Color(0.2f, 1f, 0.35f, 0.92f);

        // #region agent log
        private const string AgentDbgPath = @"d:\HS4\debug-a30eab.log";
        private float _agentDbgNextTime;
        private bool _agentBoneTreeJustRebuilt;

        private static void AgentDbgLog(string hypothesisId, string location, string message, string dataJsonObject)
        {
            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                File.AppendAllText(AgentDbgPath,
                    "{\"sessionId\":\"a30eab\",\"hypothesisId\":\"" + hypothesisId + "\",\"location\":\"" + location + "\",\"message\":\"" + message + "\",\"data\":" + dataJsonObject + ",\"timestamp\":" + ts.ToString(CultureInfo.InvariantCulture) + "}\n");
            }
            catch { }
        }

        private static int CountShowTrueTotal(BoneNode node)
        {
            if (node == null) return 0;
            int n = node.Show ? 1 : 0;
            for (int i = 0; i < node.Children.Count; i++)
                n += CountShowTrueTotal(node.Children[i]);
            return n;
        }
        // #endregion

        private void Awake()
        {
#if PHOTOTOCARD_AUTOMATION
            _requestFilePath = Config.Bind(
                "Paths",
                "RequestFile",
                @"D:\HS4\output\load_card_request.txt",
                "Full path to the request file. Content: one line = full path to character card to load (face only). Screenshot will be written to same directory as game_screenshot.png");
            _autoEnterCharaCustom = Config.Bind(
                "Auto",
                "AutoEnterCharaCustom",
                false,
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
#endif
            _boneDebugToggleKey = Config.Bind("Debug", "BoneDebugToggleKey", KeyCode.ScrollLock, "Key to toggle face bone debug menu (tree + 2D dots, face only). Only in CharaCustom. Default: ScrollLock.");
            _boneDebugVisible = false;
#if PHOTOTOCARD_AUTOMATION
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded. Request file: {_requestFilePath.Value}");
            // #region agent log
            AgentDbgLog("H0", "Awake", "plugin_init",
                "{\"autoEnterCharaCustom\":" + (_autoEnterCharaCustom.Value ? "true" : "false")
                + ",\"startupDelaySeconds\":" + _startupDelaySeconds.Value.ToString(CultureInfo.InvariantCulture) + "}");
            // #endregion
            if (_autoEnterCharaCustom.Value)
            {
                Logger.LogInfo($"[PhotoToCard] AutoEnterCharaCustom enabled. Will load CharaCustom after {_startupDelaySeconds.Value}s.");
                StartCoroutine(AutoEnterCharaCustomCoroutine());
            }
#else
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded (臉部骨骼除錯；含請求檔/自動進場景之自動化請以 -p:PhotoToCardAutomation=true 建置)。");
#endif
        }

        private void OnEnable()
        {
            Camera.onPostRender += OnCameraPostRenderDrawStructure;
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= OnCameraPostRenderDrawStructure;
            if (_structureLineMat != null)
            {
                Destroy(_structureLineMat);
                _structureLineMat = null;
            }
        }

#if PHOTOTOCARD_AUTOMATION
        private IEnumerator AutoEnterCharaCustomCoroutine()
        {
            yield return new WaitForSeconds(Mathf.Clamp(_startupDelaySeconds.Value, 5, 120));
            var active = SceneManager.GetActiveScene().name;
            // #region agent log
            AgentDbgLog("H-A", "AutoEnterCharaCustomCoroutine", "delay_elapsed",
                "{\"activeScene\":\"" + active + "\",\"willLoadScene\":true}");
            // #endregion
            if (active == "CharaCustom")
            {
                Logger.LogInfo("[PhotoToCard] Already in CharaCustom; skipping LoadSceneAsync so editor state (e.g. bone debug) is not reset.");
                // #region agent log
                AgentDbgLog("H-A", "AutoEnterCharaCustomCoroutine", "skipped_already_in_chara_custom", "{}");
                // #endregion
                yield break;
            }
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
#endif

        private void Update()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "CharaCustom" && Input.GetKeyDown(_boneDebugToggleKey.Value))
            {
                if (_boneDebugVisible)
                {
                    var chaCtrl = Singleton<CustomBase>.Instance?.chaCtrl;
                    CloseBoneDebugAndRestore(chaCtrl);
                }
                else
                    _boneDebugVisible = true;
            }

#if PHOTOTOCARD_AUTOMATION
            if (_isProcessing) return;

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
                                Directory.CreateDirectory(dir);
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
            // #region agent log
            long reqMtimeMs = 0;
            try { reqMtimeMs = new DateTimeOffset(File.GetLastWriteTimeUtc(requestPath)).ToUnixTimeMilliseconds(); } catch { }
            AgentDbgLog("H-B", "Update", "load_card_request_triggered",
                "{\"requestBase\":\"" + (Path.GetFileName(requestPath) ?? "").Replace("\\", "\\\\").Replace("\"", "'")
                + "\",\"cardBase\":\"" + (Path.GetFileName(cardPath) ?? "").Replace("\\", "\\\\").Replace("\"", "'")
                + "\",\"requestMtimeUtcMs\":" + reqMtimeMs.ToString(CultureInfo.InvariantCulture) + "}");
            // #endregion
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
#endif
        }

#if PHOTOTOCARD_AUTOMATION
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
            // #region agent log
            var chr = Singleton<Character>.Instance;
            int lstCount = chr?.lstLoadAssetBundleInfo?.Count ?? -1;
            AppendDebugLog(requestFilePath, "Hlst", "lstLoadAssetBundleInfo.Count before Begin", "{\"lstCount\":" + lstCount + "}");
            chr?.BeginLoadAssetBundle();
            // #endregion
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
                // noChangeHead: true = 不拆頭、不重載頭，避免 lstLoadAssetBundleInfo 累積；配合 Begin/End 每輪清空，Hlst 應恆為 0。
                reloadEn = InvokeReloadAsync(chaCtrl, noChangeClothes: true, noChangeHead: true, noChangeHair: true, noChangeBody: true);
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
                AppendDebugLog(requestFilePath, "H1", "ReloadAsync done", "{}");
            }
            else
            {
                AppendDebugLog(requestFilePath, "H1", "ReloadAsync fallback wait", "{}");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(3f);
                AppendDebugLog(requestFilePath, "H1", "ReloadAsync done (fallback)", "{}");
            }
            Singleton<Character>.Instance.customLoadGCClear = true;

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1f);
            AppendDebugLog(requestFilePath, "H1", "AfterFixedWait", "{}");

            var dir = Path.GetDirectoryName(requestFilePath);
            if (string.IsNullOrEmpty(dir)) dir = @"D:\HS4\output";
            try { dir = Path.GetFullPath(dir); } catch { }
            try { Directory.CreateDirectory(dir); } catch (Exception ex) { Logger.LogWarning($"[PhotoToCard] Could not create output dir: {ex.Message}"); }
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

            Singleton<Character>.Instance?.EndLoadAssetBundle();
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

#endif

        // ---------- 方案 A：臉部骨骼樹狀選單 + 2D 點顯示 ----------

        private static BoneNode BuildBoneTree(Transform tr)
        {
            if (tr == null) return null;
            var node = new BoneNode { Name = tr.name, Transform = tr, Show = false };
            for (int i = 0; i < tr.childCount; i++)
            {
                var child = BuildBoneTree(tr.GetChild(i));
                if (child != null) node.Children.Add(child);
            }
            return node;
        }

        private void EnsureBoneTree(ChaControl chaCtrl)
        {
            if (chaCtrl == null) return;
            var rootTr = chaCtrl.objHeadBone != null ? chaCtrl.objHeadBone.transform : null;
            int headId = rootTr != null ? rootTr.GetInstanceID() : int.MinValue;
            if (_boneTreeChaCtrl != chaCtrl || _boneRoot == null || _boneHeadRootTransformId != headId)
            {
                // #region agent log
                _agentBoneTreeJustRebuilt = true;
                // #endregion
                _boneTreeChaCtrl = chaCtrl;
                _boneHeadRootTransformId = headId;
                // 只顯示臉部相關骨骼（頭部骨架），點數較少、方便對照 MediaPipe / 滑桿
                _boneRoot = rootTr != null ? BuildBoneTree(rootTr) : null;
            }
        }

        private void DrawBoneTreeRecursive(BoneNode node, int depth)
        {
            if (node == null) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(depth * 12f);
            if (node.Children.Count > 0)
            {
                node.Foldout = GUILayout.Toggle(node.Foldout, node.Foldout ? "▼" : "▶", GUILayout.Width(20f));
            }
            else
            {
                GUILayout.Space(20f);
            }
            bool newShow = GUILayout.Toggle(node.Show, "", GUILayout.Width(18f));
            if (newShow != node.Show) node.Show = newShow;
            var nameContent = new GUIContent(node.Name);
            GUILayout.Label(nameContent, GUILayout.MaxWidth(LabelMaxWidth));
            GUILayout.EndHorizontal();
            if (node.Foldout && node.Children.Count > 0)
            {
                for (int i = 0; i < node.Children.Count; i++)
                    DrawBoneTreeRecursive(node.Children[i], depth + 1);
            }
        }

        private void CollectVisibleBones(BoneNode node, List<BoneNode> outList)
        {
            if (node == null) return;
            if (node.Show && node.Transform != null) outList.Add(node);
            foreach (var c in node.Children) CollectVisibleBones(c, outList);
        }

        private static void SetAllBoneShow(BoneNode node, bool show)
        {
            if (node == null) return;
            node.Show = show;
            for (int i = 0; i < node.Children.Count; i++)
                SetAllBoneShow(node.Children[i], show);
        }

        private static BoneNode PickBoneNodeAtGui(Vector2 guiMouse, List<BoneNode> visibleList, Camera cam, float half)
        {
            BoneNode best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < visibleList.Count; i++)
            {
                var n = visibleList[i];
                if (n.Transform == null) continue;
                Vector3 world = n.Transform.position;
                Vector3 screen = cam.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                float guiY = Screen.height - screen.y;
                var center = new Vector2(screen.x, guiY);
                var dotRect = new Rect(screen.x - half, guiY - half, half * 2f, half * 2f);
                if (!dotRect.Contains(guiMouse)) continue;
                float d = Vector2.Distance(guiMouse, center);
                if (d < bestD)
                {
                    bestD = d;
                    best = n;
                }
            }
            return best;
        }

        private static BoneNode FindBoneNodeByTransform(BoneNode root, Transform t)
        {
            if (root == null || t == null) return null;
            if (root.Transform == t) return root;
            for (int i = 0; i < root.Children.Count; i++)
            {
                var f = FindBoneNodeByTransform(root.Children[i], t);
                if (f != null) return f;
            }
            return null;
        }

        private static bool TryBuildPathToTransform(BoneNode node, Transform t, List<BoneNode> path)
        {
            if (node == null || t == null) return false;
            path.Add(node);
            if (node.Transform == t) return true;
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (TryBuildPathToTransform(node.Children[i], t, path))
                    return true;
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }

        private static void SetFoldoutOpenToNode(BoneNode root, BoneNode target)
        {
            if (root == null || target == null || target.Transform == null) return;
            var path = new List<BoneNode>();
            if (!TryBuildPathToTransform(root, target.Transform, path)) return;
            for (int i = 0; i < path.Count - 1; i++)
                path[i].Foldout = true;
        }

        private static void SetShowTrueSubtree(BoneNode node)
        {
            if (node == null) return;
            node.Show = true;
            for (int i = 0; i < node.Children.Count; i++)
                SetShowTrueSubtree(node.Children[i]);
        }

        private static void CollectSubtreeBoneNodesPreOrder(BoneNode node, List<BoneNode> outList)
        {
            if (node == null) return;
            outList.Add(node);
            for (int i = 0; i < node.Children.Count; i++)
                CollectSubtreeBoneNodesPreOrder(node.Children[i], outList);
        }

        private void ApplyStructurePick(BoneNode hit)
        {
            if (hit == null)
            {
                _structureAnchorNode = null;
                return;
            }
            BoneNode pick = hit;
            if (pick.Children.Count == 0 && pick.Transform != null && pick.Transform.parent != null)
            {
                BoneNode parentNode = FindBoneNodeByTransform(_boneRoot, pick.Transform.parent);
                if (parentNode != null)
                    pick = parentNode;
            }
            _structureAnchorNode = pick;
            SetFoldoutOpenToNode(_boneRoot, pick);
        }

        private void IsolateAnchorSubtreeAndCopyNamesToClipboard()
        {
            if (_structureAnchorNode == null)
            {
                Logger.LogMessage("[PhotoToCard] 結構錨點為空：請先在「結構模式」下點選一個骨骼點，再按此鈕。");
                return;
            }
            SetAllBoneShow(_boneRoot, false);
            SetShowTrueSubtree(_structureAnchorNode);
            var nodes = new List<BoneNode>();
            CollectSubtreeBoneNodesPreOrder(_structureAnchorNode, nodes);
            var lines = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                lines[i] = nodes[i].Name ?? "";
            GUIUtility.systemCopyBuffer = string.Join("\r\n", lines);
            Logger.LogInfo($"[PhotoToCard] 已僅顯示錨點子樹（{nodes.Count} 個節點），骨骼名稱已複製到剪貼簿（每行一個）。");
        }

        private void EnsureStructureLineMaterial()
        {
            if (_structureLineMat != null) return;
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) return;
            _structureLineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _structureLineMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _structureLineMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _structureLineMat.SetInt("_Cull", (int)CullMode.Off);
            _structureLineMat.SetInt("_ZWrite", 0);
            _structureLineMat.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        private static void DrawSubtreeEdgesWorld(Transform t)
        {
            if (t == null) return;
            Vector3 p = t.position;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                Vector3 q = c.position;
                GL.Vertex3(p.x, p.y, p.z);
                GL.Vertex3(q.x, q.y, q.z);
                DrawSubtreeEdgesWorld(c);
            }
        }

        private void OnCameraPostRenderDrawStructure(Camera cam)
        {
            if (!_boneDebugVisible || !_structureMode || _structureAnchorNode == null || _structureAnchorNode.Transform == null)
                return;
            if (SceneManager.GetActiveScene().name != "CharaCustom") return;
            if (cam == null || cam != Camera.main) return;
            EnsureStructureLineMaterial();
            if (_structureLineMat == null) return;

            _structureLineMat.SetPass(0);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(GL.GetGPUProjectionMatrix(cam.projectionMatrix, false));
            GL.modelview = cam.worldToCameraMatrix;
            GL.Begin(GL.LINES);
            GL.Color(StructureLineColor);
            DrawSubtreeEdgesWorld(_structureAnchorNode.Transform);
            GL.End();
            GL.PopMatrix();
        }

        private static void SetCharacterRenderersEnabled(ChaControl chaCtrl, bool enabled)
        {
            if (chaCtrl == null) return;
            var renderers = chaCtrl.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = enabled;
        }

        private void CloseBoneDebugAndRestore(ChaControl chaCtrl)
        {
            _boneDebugVisible = false;
            _structureAnchorNode = null;
            _bonePickWaitingUp = false;
            if (_hideTextures && chaCtrl != null)
            {
                SetCharacterRenderersEnabled(chaCtrl, true);
                _hideTextures = false;
            }
        }

        private void DrawBoneDebugWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("拖曳此處移動選單", GUILayout.Height(18f));
            GUI.DragWindow(new Rect(0, 0, BoneMenuWidth, 22f));

            if (GUILayout.Button("關閉臉部骨骼除錯 (熱鍵)"))
            {
                var chaCtrl = Singleton<CustomBase>.Instance?.chaCtrl;
                CloseBoneDebugAndRestore(chaCtrl);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("全選顯示點", GUILayout.Width(110f)))
                SetAllBoneShow(_boneRoot, true);
            if (GUILayout.Button("全部不顯示", GUILayout.Width(100f)))
                SetAllBoneShow(_boneRoot, false);
            GUILayout.EndHorizontal();

            bool newStruct = GUILayout.Toggle(_structureMode, "結構模式（點骨骼：綠線畫子樹從屬）");
            if (newStruct != _structureMode && !newStruct)
                _structureAnchorNode = null;
            _structureMode = newStruct;
            if (GUILayout.Button("清除綠線錨點"))
                _structureAnchorNode = null;

            if (GUILayout.Button("僅留錨點子樹＋複製名稱到剪貼簿"))
                IsolateAnchorSubtreeAndCopyNamesToClipboard();

            bool newHide = GUILayout.Toggle(_hideTextures, "只顯示骨骼（隱藏所有貼圖）");
            if (newHide != _hideTextures)
            {
                var chaCtrl = Singleton<CustomBase>.Instance?.chaCtrl;
                _hideTextures = newHide;
                if (chaCtrl != null)
                    SetCharacterRenderersEnabled(chaCtrl, !_hideTextures);
            }

            _boneMenuScroll = GUILayout.BeginScrollView(_boneMenuScroll, GUILayout.Height(Mathf.Min(Screen.height * BoneMenuMaxHeightRatio, 600f) - 80f));
            DrawBoneTreeRecursive(_boneRoot, 0);
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void OnGUI()
        {
            if (!_boneDebugVisible) return;
            if (SceneManager.GetActiveScene().name != "CharaCustom") return;
            bool rep = Event.current.type == EventType.Repaint;
            bool logNow = rep && Time.unscaledTime >= _agentDbgNextTime;
            if (logNow) _agentDbgNextTime = Time.unscaledTime + 0.5f;

            var customBase = Singleton<CustomBase>.Instance;
            var chaCtrl = customBase?.chaCtrl;
            if (chaCtrl == null)
            {
                if (logNow) AgentDbgLog("H4", "OnGUI", "early_exit", "{\"reason\":\"chaCtrl_null\"}");
                return;
            }

            EnsureBoneTree(chaCtrl);
            if (_boneRoot == null)
            {
                if (logNow) AgentDbgLog("H4", "OnGUI", "early_exit", "{\"reason\":\"boneRoot_null\",\"headBoneMissing\":true}");
                return;
            }

            float menuHeight = Mathf.Min(Screen.height * BoneMenuMaxHeightRatio, 600f);
            float winW = BoneMenuWidth;
            float winH = menuHeight + 60f;
            if (_boneMenuRect.width < 1f)
                _boneMenuRect = new Rect(8f, 8f, winW, winH);
            _boneMenuRect = GUI.Window(BoneWindowId, _boneMenuRect, DrawBoneDebugWindow, "臉部骨骼除錯");

            var cam = Camera.main;
            if (cam == null)
            {
                if (logNow) AgentDbgLog("H4", "OnGUI", "early_exit", "{\"reason\":\"cam_main_null\"}");
                return;
            }

            if (_dotStyle == null)
            {
                _dotStyle = new GUIStyle(GUI.skin.box);
                _dotStyle.normal.background = MakeTex(2, 2, new Color(1f, 0.9f, 0.2f, 0.98f));
            }
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = LabelFontSize;
                _labelStyle.normal.textColor = Color.yellow;
            }
            if (_labelBgStyle == null)
            {
                _labelBgStyle = new GUIStyle(GUI.skin.box);
                _labelBgStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.92f));
            }

            var visibleList = new List<BoneNode>();
            CollectVisibleBones(_boneRoot, visibleList);
            // #region agent log
            if (logNow)
            {
                int showTrue = CountShowTrueTotal(_boneRoot);
                int drawableZ = 0;
                for (int i = 0; i < visibleList.Count; i++)
                {
                    var nn = visibleList[i];
                    if (nn?.Transform == null) continue;
                    if (cam.WorldToScreenPoint(nn.Transform.position).z > 0f) drawableZ++;
                }
                bool justRebuilt = _agentBoneTreeJustRebuilt;
                _agentBoneTreeJustRebuilt = false;
                AgentDbgLog("H1_H2_H3", "OnGUI", "bone_dot_snapshot",
                    "{\"visibleListCount\":" + visibleList.Count.ToString(CultureInfo.InvariantCulture)
                    + ",\"showTrueTotal\":" + showTrue.ToString(CultureInfo.InvariantCulture)
                    + ",\"drawableZPositive\":" + drawableZ.ToString(CultureInfo.InvariantCulture)
                    + ",\"boneTreeRebuiltThisTick\":" + (justRebuilt ? "true" : "false")
                    + ",\"structureMode\":" + (_structureMode ? "true" : "false")
                    + ",\"anchorSet\":" + (_structureAnchorNode != null ? "true" : "false")
                    + ",\"chaInst\":" + chaCtrl.GetInstanceID().ToString(CultureInfo.InvariantCulture)
                    + ",\"storedChaInst\":" + (_boneTreeChaCtrl != null ? _boneTreeChaCtrl.GetInstanceID().ToString(CultureInfo.InvariantCulture) : "0")
                    + "}");
            }
            // #endregion
            float half = DotSize * 0.5f;
            float labelTopOffset = half + 4f;

            Vector2 guiMouse = Event.current.mousePosition;
            bool mouseInMenu = _boneMenuRect.Contains(guiMouse);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !mouseInMenu)
            {
                _bonePickMouseDownGui = guiMouse;
                _bonePickWaitingUp = true;
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && _bonePickWaitingUp)
            {
                _bonePickWaitingUp = false;
                if (!_boneMenuRect.Contains(guiMouse) &&
                    Vector2.Distance(guiMouse, _bonePickMouseDownGui) < BonePickMaxMovePx &&
                    _structureMode)
                {
                    BoneNode hit = PickBoneNodeAtGui(guiMouse, visibleList, cam, half);
                    ApplyStructurePick(hit);
                }
            }

            BoneNode hoverNode = null;
            if (!mouseInMenu)
                hoverNode = PickBoneNodeAtGui(guiMouse, visibleList, cam, half);

            foreach (var n in visibleList)
            {
                if (n.Transform == null) continue;
                Vector3 world = n.Transform.position;
                Vector3 screen = cam.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;
                float guiY = Screen.height - screen.y;
                Rect dotRect = new Rect(screen.x - half, guiY - half, DotSize, DotSize);
                GUI.Box(dotRect, "", _dotStyle);
            }

            if (hoverNode != null && !mouseInMenu)
            {
                Vector3 world = hoverNode.Transform.position;
                Vector3 screen = cam.WorldToScreenPoint(world);
                if (screen.z > 0f)
                {
                    float guiY = Screen.height - screen.y;
                    float labelY = guiY - half - labelTopOffset - LabelHeight;
                    Rect labelRect = new Rect(screen.x - LabelMaxWidth * 0.5f, labelY, LabelMaxWidth, LabelHeight);
                    if (labelRect.y >= 0 && labelRect.y < Screen.height - LabelHeight)
                    {
                        GUI.Box(labelRect, "", _labelBgStyle);
                        GUI.Label(labelRect, hoverNode.Name, _labelStyle);
                    }
                }
            }
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    tex.SetPixel(x, y, col);
            tex.Apply();
            return tex;
        }
    }
}
