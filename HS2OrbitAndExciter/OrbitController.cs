using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Manager;
using UnityEngine;

namespace HS2OrbitAndExciter
{
    [DefaultExecutionOrder(-100)]
    /// <summary>
    /// Drives orbit camera in H scene: hotkey Ctrl+Shift+O, 360° then reverse; optional random focus/angle, pose change, clothes.
    /// </summary>
    public class OrbitController : MonoBehaviour
    {
        private const KeyCode OrbitHotkey = KeyCode.O;
        private const KeyCode Modifier = KeyCode.LeftShift;
        private const KeyCode Modifier2 = KeyCode.LeftControl;

        private static readonly float[] AnglePresets = { 0f, 45f, 90f, 135f, 180f };

        /// <summary>Single delegate we set during orbit so we never save/restore our own and get stuck.</summary>
        private static readonly BaseCameraControl_Ver2.NoCtrlFunc NoCtrlOrbit = () => false;

        private const float HotkeyCooldownSeconds = 0.25f;

        private bool _orbitActive;
        private BaseCameraControl_Ver2.NoCtrlFunc? _savedNoCtrlCondition;
        private float _lastHotkeyTime = -999f;

        // #region agent log
        private static void DebugLog(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-9961ad.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var line = $"{{\"sessionId\":\"9961ad\",\"location\":\"{location}\",\"message\":\"{message}\",\"data\":{dataJson},\"hypothesisId\":\"{hypothesisId}\",\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); } catch { }
        }
        // #endregion

        private float _startOrbitY;
        private int _orbitPhase;
        private float _orbitAccumulatedDegrees;
        private int _orbitCycleCount;
        private int _currentFocusIndex;
        /// <summary>Index into sequence [0,1,2,3,2,1]; next step is (_currentClothesSequenceIndex + 1) % 6.</summary>
        private int _currentClothesSequenceIndex;

        private static FieldInfo _feelFField;

        private static float GetOrbitFeelAddPerSecond()
        {
            var v = HS2OrbitAndExciter.FeelAddPerSecondWhenOrbit?.Value ?? 0.1f;
            return v <= 0f ? 0f : v;
        }

        /// <summary>When orbit is active, add to excitement gauge each frame so it fills without mouse.</summary>
        private void AccumulateFeelWhenOrbit(HScene hScene)
        {
            float addPerSec = GetOrbitFeelAddPerSecond();
            if (addPerSec <= 0f) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            if (_feelFField == null)
            {
                _feelFField = ctrlFlag.GetType().GetField("feel_f", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (_feelFField == null) return;
            }
            float current = (float)(_feelFField.GetValue(ctrlFlag) ?? 0f);
            float next = Mathf.Clamp01(current + addPerSec * Time.deltaTime);
            if (next <= current) return;
            _feelFField.SetValue(ctrlFlag, next);
        }

        private void Update()
        {
            bool mod2 = Input.GetKey(Modifier2);
            bool mod = Input.GetKey(Modifier);
            bool mod2R = Input.GetKey(KeyCode.RightControl);
            bool modR = Input.GetKey(KeyCode.RightShift);
            bool oDown = Input.GetKeyDown(OrbitHotkey);
            bool oKey = Input.GetKey(OrbitHotkey);
            float cooldownLeft = HotkeyCooldownSeconds - (Time.unscaledTime - _lastHotkeyTime);
            // #region agent log
            if (_orbitActive && (mod2 || mod2R) && (mod || modR) && (oKey || oDown))
            {
                bool leftCombo = mod2 && mod && oDown;
                DebugLog("OrbitController.cs:Update", "Orbit active, modifiers+O", "{\"oDown\":" + (oDown ? "true" : "false") + ",\"oKey\":" + (oKey ? "true" : "false") + ",\"cooldownLeft\":" + cooldownLeft.ToString("F2") + ",\"leftCtrl\":" + (mod2 ? "true" : "false") + ",\"leftShift\":" + (mod ? "true" : "false") + ",\"rightCtrl\":" + (mod2R ? "true" : "false") + ",\"rightShift\":" + (modR ? "true" : "false") + ",\"leftComboWouldFire\":" + (leftCombo ? "true" : "false") + "}", "H1");
            }
            // #endregion
            if (mod2 && mod && oDown)
            {
                if (Time.unscaledTime - _lastHotkeyTime < HotkeyCooldownSeconds)
                {
                    // #region agent log
                    DebugLog("OrbitController.cs:Update", "Cooldown skip", "{\"cooldownLeft\":" + cooldownLeft.ToString("F2") + ",\"_orbitActive\":" + (_orbitActive ? "true" : "false") + "}", "H2");
                    // #endregion
                    return;
                }
                _lastHotkeyTime = Time.unscaledTime;
                _orbitActive = !_orbitActive;
                // #region agent log
                DebugLog("OrbitController.cs:Update", "Toggle", "{\"_orbitActive\":" + (_orbitActive ? "true" : "false") + "}", "H4");
                // #endregion
                OnOrbitToggled(_orbitActive);
            }
        }

        private void LateUpdate()
        {
            if (!_orbitActive) return;

            var hScene = GetHScene();
            if (hScene == null) return;

            var ctrl = hScene.ctrlFlag?.cameraCtrl as CameraControl_Ver2;
            if (ctrl == null) return;

            // Excitement gauge auto-accumulates while orbit is active (no mouse required)
            AccumulateFeelWhenOrbit(hScene);

            // Re-apply camera takeover every frame (e.g. after ChangeAnimation sets camera flag)
            ctrl.NoCtrlCondition = NoCtrlOrbit;

            float orbitTime = HS2OrbitAndExciter.OrbitTimePer360?.Value ?? 10f;
            if (orbitTime <= 0f) orbitTime = 10f;
            float speedDegPerSec = 360f / orbitTime;
            float dt = Time.deltaTime;

            if (_orbitPhase == 0)
            {
                _orbitAccumulatedDegrees += speedDegPerSec * dt;
                if (_orbitAccumulatedDegrees >= 360f)
                {
                    _orbitAccumulatedDegrees = 360f;
                    _orbitPhase = 1;
                }
            }
            else
            {
                _orbitAccumulatedDegrees -= speedDegPerSec * dt;
                if (_orbitAccumulatedDegrees <= 0f)
                {
                    _orbitAccumulatedDegrees = 0f;
                    _orbitPhase = 0;
                    OnOrbitCycleComplete(hScene, ctrl);
                }
            }

            float rotY = _startOrbitY + (_orbitPhase == 0 ? _orbitAccumulatedDegrees : 360f - _orbitAccumulatedDegrees);
            rotY = ((rotY % 360f) + 360f) % 360f;
            var rot = ctrl.CameraAngle;
            rot.y = rotY;
            ctrl.CameraAngle = rot;
            ctrl.Rot = rot;
        }

        private void OnOrbitCycleComplete(HScene hScene, CameraControl_Ver2 ctrl)
        {
            _orbitCycleCount++;
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            int maxFocus = OrbitHelpers.GetMaxFocusIndex(chaFemales);
            int nRandom = HS2OrbitAndExciter.OrbitCountBeforeRandom?.Value ?? 1;
            int nPose = HS2OrbitAndExciter.OrbitCountBeforePoseChange?.Value ?? 2;
            bool changePose = HS2OrbitAndExciter.ChangePoseOnCycle?.Value ?? false;
            bool clothesEnabled = HS2OrbitAndExciter.ClothesChangeEnabled?.Value ?? false;

            if (nRandom > 0 && _orbitCycleCount % nRandom == 0 && maxFocus > 0)
            {
                _currentFocusIndex = Random.Range(0, maxFocus);
                var pos = OrbitHelpers.GetFocusPosition(chaFemales, _currentFocusIndex, ctrl.transBase);
                if (pos.HasValue) ctrl.TargetPos = pos.Value;
                _startOrbitY = AnglePresets[Random.Range(0, AnglePresets.Length)];
            }

            if (clothesEnabled)
            {
                _currentClothesSequenceIndex = (_currentClothesSequenceIndex + 1) % 6;
                int stage = OrbitHelpers.ClothesSequenceStage(_currentClothesSequenceIndex);
                var chaMales = OrbitHelpers.GetChaMales(hScene);
                OrbitHelpers.SetClothesStage(chaFemales, chaMales, stage);
            }

            if (changePose && nPose > 0 && _orbitCycleCount % nPose == 0)
            {
                var all = OrbitHelpers.GetAllPoseList();
                if (all.Count > 0)
                {
                    var current = hScene.ctrlFlag?.nowAnimationInfo;
                    var next = OrbitHelpers.PickNextPose(current, all);
                    if (next != null)
                        hScene.StartCoroutine(hScene.ChangeAnimation(next, _isForceResetCamera: false, _isForceLoopAction: false, _UseFade: true));
                }
            }
        }

        private void OnOrbitToggled(bool active)
        {
            var hScene = GetHScene();
            if (hScene == null)
            {
                HS2OrbitAndExciter.Log?.LogInfo("Orbit: No H scene; orbit will start when entering H.");
                return;
            }

            var ctrl = hScene.ctrlFlag?.cameraCtrl;
            if (ctrl == null) return;

            if (active)
            {
                // Only save game's delegate; never save our own or we restore it on stop and camera stays locked
                if (ctrl.NoCtrlCondition != NoCtrlOrbit)
                    _savedNoCtrlCondition = ctrl.NoCtrlCondition;
                ctrl.NoCtrlCondition = NoCtrlOrbit;
                _startOrbitY = ((ctrl.CameraAngle.y % 360f) + 360f) % 360f;
                _orbitPhase = 0;
                _orbitAccumulatedDegrees = 0f;
                _orbitCycleCount = 0;
                var chaFemales = OrbitHelpers.GetChaFemales(hScene);
                _currentClothesSequenceIndex = OrbitHelpers.GetClothesSequenceIndexFromCurrent(chaFemales);
                _currentFocusIndex = 0;
                if (chaFemales != null && chaFemales.Length > 0)
                {
                    var pos = OrbitHelpers.GetFocusPosition(chaFemales, 0, ctrl.transBase);
                    if (pos.HasValue) ctrl.TargetPos = pos.Value;
                }
            }
            else
            {
                // #region agent log
                DebugLog("OrbitController.cs:OnOrbitToggled", "Stopping orbit, restoring NoCtrlCondition", "{\"hasSaved\":" + (_savedNoCtrlCondition != null ? "true" : "false") + "}", "H5");
                // #endregion
                if (_savedNoCtrlCondition != null && _savedNoCtrlCondition != NoCtrlOrbit)
                    ctrl.NoCtrlCondition = _savedNoCtrlCondition;
                else
                    ctrl.NoCtrlCondition = () => true; // ensure player gets control back if we had no valid saved
            }
        }

        private static HScene? GetHScene()
        {
            if (!Singleton<HSceneManager>.IsInstance())
                return null;
            return Singleton<HSceneManager>.Instance.Hscene;
        }
    }
}
