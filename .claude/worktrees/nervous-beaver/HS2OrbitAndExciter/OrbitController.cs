using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AIChara;
using HarmonyLib;
using Manager;
using UnityEngine;

namespace HS2OrbitAndExciter
{
    /// <summary>
    /// Drives orbit camera in H scene: hotkey Ctrl+Shift+O, 360° then reverse; optional random focus/angle, pose change, clothes.
    /// Runs LateUpdate before <see cref="CameraControl_Ver2"/> (default 0) so yaw is written to CamDat.Rot and CameraUpdate() applies transBase correctly.
    /// H-scene flag assist (auto action / checkpoint) runs in <see cref="OrbitHSceneLateAssist"/> after game proc.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class OrbitController : MonoBehaviour
    {
        private const KeyCode OrbitHotkey = KeyCode.O;
        private const KeyCode Modifier = KeyCode.LeftShift;
        private const KeyCode Modifier2 = KeyCode.LeftControl;

        private static readonly float[] AnglePresets = { 0f, 45f, 90f, 135f, 180f };

        /// <summary>True when user is holding any mouse button (used to pause orbit and yield to game camera).</summary>
        private static bool IsUserControllingCamera()
        {
            return Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        }

        /// <summary>
        /// NoCtrlCondition semantics: true = BLOCK mouse input (InputMouseProc skipped), false = ALLOW mouse input.
        /// During orbit: block mouse when user is NOT pressing any button (orbit controls camera);
        /// allow mouse when user IS pressing a button (game processes drag/pan/zoom).
        /// </summary>
        private static readonly BaseCameraControl_Ver2.NoCtrlFunc NoCtrlOrbit = () => !IsUserControllingCamera();

        private const float HotkeyCooldownSeconds = 0.25f;
        /// <summary>When choosing orbit focus option, prefer the game's default camera (option==maxFocus) to reduce distance surprises.</summary>
        private const float PreferGameDefaultCameraChance = 0.8f;

        private bool _orbitActive;
        /// <summary>Mirror of _orbitActive for Harmony patches (static read).</summary>
        private static bool _orbitActiveForPatches;
        private BaseCameraControl_Ver2.NoCtrlFunc? _savedNoCtrlCondition;
        private float _lastHotkeyTime = -999f;
        /// <summary>True when user held a mouse button last frame; used to resync orbit angle on release.</summary>
        private bool _wasUserControlling;

        // #region agent log
        private static void DebugLog(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-9961ad.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var line = $"{{\"sessionId\":\"9961ad\",\"location\":\"{location}\",\"message\":\"{message}\",\"data\":{dataJson},\"hypothesisId\":\"{hypothesisId}\",\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); } catch { }
        }
        private static void Debug341efeLog(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-341efe.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // Keep escaping minimal; these strings are controlled by code (no user input).
            string escLoc = (location ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
            string escMsg = (message ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
            string escHyp = (hypothesisId ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
            var line =
                $"{{\"sessionId\":\"341efe\",\"runId\":\"run1\",\"hypothesisId\":\"{escHyp}\",\"location\":\"{escLoc}\",\"message\":\"{escMsg}\",\"data\":{dataJson},\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); } catch { }
        }
        private static void DebugSessionLog(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-b1efc0.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var line = $"{{\"sessionId\":\"b1efc0\",\"runId\":\"run1\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"{location}\",\"message\":\"{message}\",\"data\":{dataJson},\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); Debug341efeLog(location, message, dataJson, hypothesisId); } catch { }
        }
        private static void DebugAutoAdvance(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-11c59c.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var line = $"{{\"sessionId\":\"11c59c\",\"runId\":\"run1\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"{location}\",\"message\":\"{message}\",\"data\":{dataJson},\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); Debug341efeLog(location, message, dataJson, hypothesisId); } catch { }
        }
        // #endregion

        private float _startOrbitY;
        private int _orbitPhase;
        private float _orbitAccumulatedDegrees;
        private int _orbitCycleCount;
        /// <summary>Current view option: 0..maxFocus-1 = body focus index, maxFocus = pose default camera. Total options = maxFocus + 1.</summary>
        private int _currentViewOption;
        /// <summary>Index into sequence [0,1,2,3,2,1]; next step is (_currentClothesSequenceIndex + 1) % 6.</summary>
        private int _currentClothesSequenceIndex;
        /// <summary>Track last nowAnimationInfo for pose-change detection; reapply view when it changes.</summary>
        private object? _lastNowAnimationInfoRef;
        /// <summary>When true, next LateUpdate will call ApplyCurrentViewOption once (e.g. after faintness toggle).</summary>
        private static bool _requestViewReapplyNextFrame;

        private static FieldInfo _feelFField;
        private static FieldInfo? _modeCtrlField;
        /// <summary>Seconds spent at checkpoint (Idle, no selection) while orbit is on; reset when we advance or leave checkpoint.</summary>
        private float _checkpointIdleTime;
        private static MethodInfo? _getAutoAnimationMethod;
        private static FieldInfo? _isAutoActionChangeField;
        private static PropertyInfo? _isAutoActionChangeProp;
        /// <summary>When orbit started in preparation (Idle + speed 0): wait this many seconds then set speed=1 to start; excitement only accumulates after start.</summary>
        private bool _waitingForPrepStart;
        private float _prepCountdownStart;
        private const float PrepWaitSeconds = 3f;

        private static float GetOrbitFeelAddPerSecond()
        {
            var v = HS2OrbitAndExciter.FeelAddPerSecondWhenOrbit?.Value ?? 0.1f;
            return v <= 0f ? 0f : v;
        }

        /// <summary>Animator state names where excitement is accumulated (action loop). Aibu/Houshi/Sonyu/Les/MultiPlay: W/S/O; Masturbation: W/M/S/O; Spnking: WIdle/SIdle/WAction/SAction.</summary>
        private static readonly HashSet<string> ActionLoopStateNames = new HashSet<string>
        {
            "WLoop", "SLoop", "OLoop", "D_WLoop", "D_SLoop", "D_OLoop",
            "MLoop",
            "WIdle", "SIdle", "WAction", "SAction", "D_Action"
        };

        private static readonly string[] OrbitDebugStateMatchNames = new[]
        {
            // Entry/loop states we care about for orbit auto-pass
            "Idle", "D_Idle",
            "WLoop", "SLoop", "OLoop", "D_WLoop", "D_SLoop", "D_OLoop",
            "Orgasm", "Orgasm_A", "D_Orgasm", "D_Orgasm_A",
            "Orgasm_OUT", "Orgasm_IN", "OrgasmF_IN", "OrgasmM_IN", "OrgasmS_IN",
            // Also include action-loop whitelist names (e.g. WIdle/SIdle/WAction/SAction)
            "WIdle", "SIdle", "WAction", "SAction", "D_Action"
        };

        private const float OrbitSpeedAddPerSecond = 0.35f;

        /// <summary>True when first female's animator is in an action loop state (only then we add feel_f / speed during orbit).</summary>
        private static bool IsInActionLoopState(HScene hScene)
        {
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || chaFemales.Length == 0) return false;
            var cha = chaFemales[0];
            if (cha == null) return false;
            var animBody = Traverse.Create(cha).Field("animBody").GetValue();
            if (animBody == null) return false;
            var animType = animBody.GetType();
            var getState = animType.GetMethod("GetCurrentAnimatorStateInfo", new[] { typeof(int) });
            if (getState == null) return false;
            var state = getState.Invoke(animBody, new object[] { 0 });
            if (state == null) return false;
            var isName = state.GetType().GetMethod("IsName", new[] { typeof(string) });
            if (isName == null) return false;
            foreach (string name in ActionLoopStateNames)
            {
                if ((bool)isName.Invoke(state, new object[] { name }))
                    return true;
            }
            return false;
        }

        /// <summary>Whether orbit is currently active (for Harmony patches).</summary>
        public static bool IsOrbitActive() => _orbitActiveForPatches;

        private static string GetFirstFemaleAnimatorStateLabel(HScene hScene)
        {
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || chaFemales.Length == 0) return "unknown";
            var cha = chaFemales[0];
            if (cha == null) return "unknown";
            var animBody = Traverse.Create(cha).Field("animBody").GetValue();
            if (animBody == null) return "unknown";
            var animType = animBody.GetType();
            var getState = animType.GetMethod("GetCurrentAnimatorStateInfo", new[] { typeof(int) });
            if (getState == null) return "unknown";
            var state = getState.Invoke(animBody, new object[] { 0 });
            if (state == null) return "unknown";
            var isName = state.GetType().GetMethod("IsName", new[] { typeof(string) });
            if (isName == null) return "unknown";
            foreach (var name in OrbitDebugStateMatchNames)
            {
                try
                {
                    if ((bool)isName.Invoke(state, new object[] { name }))
                        return name;
                }
                catch { }
            }
            return "unknown";
        }

        /// <summary>True when in preparation state: Idle/D_Idle and speed &lt;= 0.</summary>
        private static bool IsInPreparationState(HScene hScene)
        {
            var ctrlFlag = hScene?.ctrlFlag;
            if (ctrlFlag == null) return false;
            float speed = (float)(Traverse.Create(ctrlFlag).Field("speed").GetValue() ?? 0f);
            if (speed > 0.01f) return false;
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || chaFemales.Length == 0) return false;
            var cha = chaFemales[0];
            if (cha == null) return false;
            var animBody = Traverse.Create(cha).Field("animBody").GetValue();
            if (animBody == null) return false;
            var animType = animBody.GetType();
            var getState = animType.GetMethod("GetCurrentAnimatorStateInfo", new[] { typeof(int) });
            if (getState == null) return false;
            var state = getState.Invoke(animBody, new object[] { 0 });
            if (state == null) return false;
            var isName = state.GetType().GetMethod("IsName", new[] { typeof(string) });
            if (isName == null) return false;
            return (bool)isName.Invoke(state, new object[] { "Idle" }) || (bool)isName.Invoke(state, new object[] { "D_Idle" });
        }

        /// <summary>When orbit is active and in action loop only: add to excitement gauge and to speed so W/S/O segments advance without wheel.</summary>
        private void AccumulateFeelWhenOrbit(HScene hScene)
        {
            if (_waitingForPrepStart) return;
            if (!IsInActionLoopState(hScene)) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            float addPerSec = GetOrbitFeelAddPerSecond();
            if (addPerSec > 0f)
            {
                if (_feelFField == null)
                {
                    _feelFField = ctrlFlag.GetType().GetField("feel_f", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_feelFField == null) return;
                }
                float current = (float)(_feelFField.GetValue(ctrlFlag) ?? 0f);
                float next = Mathf.Clamp01(current + addPerSec * Time.deltaTime);
                if (next > current)
                    _feelFField.SetValue(ctrlFlag, next);
            }
            var speedField = Traverse.Create(ctrlFlag).Field("speed");
            if (speedField.FieldExists())
            {
                float speed = (float)(speedField.GetValue() ?? 0f);
                speed = Mathf.Clamp(speed + OrbitSpeedAddPerSecond * Time.deltaTime, 0f, 2f);
                speedField.SetValue(speed);
            }
        }

        /// <summary>
        /// Legacy: used to set isAutoActionChange=true and initiative=1 every frame.
        /// REMOVED: setting initiative=1 forces Auto mode which gates scroll wheel behind a timer;
        /// setting isAutoActionChange=true causes game to hide motion list (SetMotionListDraw(false)).
        /// Auto-advance is now handled solely by TryAutoAdvancePastCheckpoint which directly calls
        /// GetAutoAnimation after a timeout, without forcing Auto mode or hiding UI.
        /// </summary>
        private void ApplyOrbitAutoAction(HScene hScene)
        {
            bool enabled = HS2OrbitAndExciter.OrbitAutoActionEnabled?.Value == true;
            if (!enabled) return;
            // Ensure reflection for isAutoActionChange is initialized (used by RunLateHSceneAssist mouse-clear)
            if (_isAutoActionChangeField == null && _isAutoActionChangeProp == null)
            {
                var ctrlFlag = hScene.ctrlFlag;
                if (ctrlFlag != null)
                {
                    var flagType = ctrlFlag.GetType();
                    _isAutoActionChangeField = flagType.GetField("isAutoActionChange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_isAutoActionChangeField == null)
                        _isAutoActionChangeProp = flagType.GetProperty("isAutoActionChange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }
            // No longer set isAutoActionChange or initiative; TryAutoAdvancePastCheckpoint handles auto-advance.
        }

        /// <summary>When orbit is on and stuck at checkpoint (Idle, no selection) for OrbitCheckpointTimeoutSeconds, call HScene.GetAutoAnimation to advance.</summary>
        private static int _checkpointLogTicks;
        private static float _orbitDebugLastLogTime = -999f;
        private static string? _orbitDebugLastStateLabel;
        private static bool _orbitDebugLastSelectIsNull = true;
        private static bool _orbitDebugLastAutoActionChange;
        private static int _orbitDebugLastInitiative;
        private const float OrbitDebugLogIntervalSeconds = 0.5f;
        private void DebugOrbitIdlePass(HScene hScene)
        {
            if (!IsOrbitActive()) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            string stateLabel = GetFirstFemaleAnimatorStateLabel(hScene);

            bool selectIsNull = ctrlFlag.selectAnimationListInfo == null;
            bool autoChange = ctrlFlag.isAutoActionChange;
            int initiative = ctrlFlag.initiative;
            float speed = ctrlFlag.speed;

            float feelF = 0f;
            try
            {
                feelF = (float)(Traverse.Create(ctrlFlag).Field("feel_f").GetValue() ?? 0f);
            }
            catch { }

            float now = Time.unscaledTime;
            bool stateChanged = !string.Equals(stateLabel, _orbitDebugLastStateLabel, System.StringComparison.Ordinal);
            bool selectionChanged = selectIsNull != _orbitDebugLastSelectIsNull;
            bool autoChanged = autoChange != _orbitDebugLastAutoActionChange;
            bool initiativeChanged = initiative != _orbitDebugLastInitiative;
            bool intervalElapsed = now - _orbitDebugLastLogTime >= OrbitDebugLogIntervalSeconds;

            if (!intervalElapsed && !stateChanged && !selectionChanged && !autoChanged && !initiativeChanged)
                return;

            _orbitDebugLastLogTime = now;
            _orbitDebugLastStateLabel = stateLabel;
            _orbitDebugLastSelectIsNull = selectIsNull;
            _orbitDebugLastAutoActionChange = autoChange;
            _orbitDebugLastInitiative = initiative;

            string data = "{"
                + "\"state\":\"" + stateLabel + "\""
                + ",\"selectNull\":" + (selectIsNull ? "true" : "false")
                + ",\"autoActionChange\":" + (autoChange ? "true" : "false")
                + ",\"initiative\":" + initiative
                + ",\"speed\":" + speed.ToString("F2")
                + ",\"feel_f\":" + feelF.ToString("F2")
                + ",\"waitingForPrepStart\":" + (_waitingForPrepStart ? "true" : "false")
                + ",\"checkpointIdleTime\":" + _checkpointIdleTime.ToString("F2")
                + "}";
            DebugSessionLog("OrbitController.DebugOrbitIdlePass", "orbitStatus", data, "O0");
        }

        private void TryAutoAdvancePastCheckpoint(HScene hScene)
        {
            float timeout = HS2OrbitAndExciter.OrbitCheckpointTimeoutSeconds?.Value ?? 2f;
            if (timeout <= 0f) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            var sel = Traverse.Create(ctrlFlag).Property("selectAnimationListInfo").GetValue();
            bool hasSelection = sel != null;
            // #region agent log
            _checkpointLogTicks++;
            if (_checkpointLogTicks % 60 == 0) DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "tick", "{\"timeout\":" + timeout.ToString("F1") + ",\"hasSelection\":" + (hasSelection ? "true" : "false") + ",\"idleTime\":" + _checkpointIdleTime.ToString("F2") + "}", "H3");
            // #endregion
            if (hasSelection) { _checkpointIdleTime = 0f; return; }
            // Runtime evidence: animator state label can stay "unknown" while game still has no selection — do not gate on Idle/D_Idle only.
            _checkpointIdleTime += Time.deltaTime;
            if (_checkpointIdleTime < timeout) return;
            _checkpointIdleTime = 0f;
            if (_getAutoAnimationMethod == null)
            {
                _getAutoAnimationMethod = typeof(HScene).GetMethod("GetAutoAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_getAutoAnimationMethod == null) { DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "noMethod", "{}", "H4"); return; }
            }
            // #region agent log
            DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "invokeGetAutoAnimation", "{\"before\":true}", "H4");
            // #endregion
            try
            {
                _getAutoAnimationMethod.Invoke(hScene, new object[] { false });
                if (Traverse.Create(ctrlFlag).Property("selectAnimationListInfo").GetValue() == null)
                    _getAutoAnimationMethod.Invoke(hScene, new object[] { true });
            }
            catch (System.Exception e) { DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "invokeException", "{\"msg\":\"" + (e.Message ?? "").Replace("\"", "'") + "\"}", "H4"); }
            // #region agent log
            bool hasSelAfter = Traverse.Create(ctrlFlag).Property("selectAnimationListInfo").GetValue() != null;
            DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "afterInvoke", "{\"selectAnimationListInfoSet\":" + (hasSelAfter ? "true" : "false") + "}", "H4");
            // #endregion

            // Fallback: if we still can't pick next action, try to bump speed once (no feel_f change).
            if (!hasSelAfter)
            {
                try
                {
                    const float FallbackSpeedBump = 1.2f;
                    Traverse.Create(ctrlFlag).Field("speed").SetValue(FallbackSpeedBump);
                    DebugAutoAdvance("OrbitController.TryAutoAdvancePastCheckpoint", "fallbackSpeedBump", "{\"speed\":" + FallbackSpeedBump.ToString("F2") + "}", "H6");
                }
                catch { }
            }
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
            if (oDown && (mod2 || mod2R || mod || modR))
            {
                bool leftCombo = mod2 && mod && oDown;
                bool rightUsed = mod2R || modR;
                DebugSessionLog("OrbitController.cs:Update", "O+modifiers",
                    "{\"oDown\":true,\"leftCtrl\":" + (mod2 ? "true" : "false") + ",\"leftShift\":" + (mod ? "true" : "false") + ",\"rightCtrl\":" + (mod2R ? "true" : "false") + ",\"rightShift\":" + (modR ? "true" : "false") + ",\"leftComboSatisfied\":" + (leftCombo ? "true" : "false") + ",\"rightUsed\":" + (rightUsed ? "true" : "false") + "}",
                    leftCombo ? "H4" : "H1");
            }
            // #endregion
            if (mod2 && mod && oDown)
            {
                if (Time.unscaledTime - _lastHotkeyTime < HotkeyCooldownSeconds)
                {
                    // #region agent log
                    DebugSessionLog("OrbitController.cs:Update", "Cooldown skip", "{\"cooldownLeft\":" + cooldownLeft.ToString("F2") + ",\"_orbitActive\":" + (_orbitActive ? "true" : "false") + "}", "H2");
                    // #endregion
                    return;
                }
                _lastHotkeyTime = Time.unscaledTime;
                _orbitActive = !_orbitActive;
                _orbitActiveForPatches = _orbitActive;
                // #region agent log
                DebugSessionLog("OrbitController.cs:Update", "Toggle orbit", "{\"_orbitActive\":" + (_orbitActive ? "true" : "false") + "}", "H4");
                // #endregion
                OnOrbitToggled(_orbitActive);
            }
        }

        private void LateUpdate()
        {
            var hScene = GetHScene();
            if (hScene != null)
                ApplyFinishHotkeys(hScene);

            if (!_orbitActive)
            {
                _requestViewReapplyNextFrame = false;
                return;
            }

            if (hScene == null) return;

            var ctrl = hScene.ctrlFlag?.cameraCtrl as CameraControl_Ver2;
            if (ctrl == null) return;

            ApplyOrbitFocusHotkeys(hScene, ctrl);

            // If we started in preparation (Idle + speed 0): after 3 s set speed=1 to start motion; excitement only rises after that
            if (_waitingForPrepStart)
            {
                float elapsed = Time.unscaledTime - _prepCountdownStart;
                if (elapsed >= PrepWaitSeconds)
                {
                    _waitingForPrepStart = false;
                    var ctrlFlag = hScene.ctrlFlag;
                    if (ctrlFlag != null)
                        Traverse.Create(ctrlFlag).Field("speed").SetValue(1f);
                }
            }

            // Excitement gauge auto-accumulates while orbit is active (skipped during prep countdown)
            AccumulateFeelWhenOrbit(hScene);

            // ApplyOrbitAutoAction / DebugOrbitIdlePass / TryAutoAdvancePastCheckpoint: see OrbitHSceneLateAssist (runs after H proc)

            // Re-apply camera takeover every frame (e.g. after ChangeAnimation sets camera flag)
            ctrl.NoCtrlCondition = NoCtrlOrbit;

            // --- Mouse-held pause: when user holds any mouse button, skip ALL camera writes so vanilla mouse behaviour works identically ---
            bool userControlling = IsUserControllingCamera();
            if (userControlling)
            {
                _wasUserControlling = true;
                // Still track pose changes so we don't miss them
                _lastNowAnimationInfoRef = hScene.ctrlFlag?.nowAnimationInfo;
                return; // let the game handle camera entirely this frame
            }
            if (_wasUserControlling)
            {
                // User just released mouse — resync orbit start angle to where the camera is now so orbit continues seamlessly
                _wasUserControlling = false;
                float currentY = ((ctrl.CameraAngle.y % 360f) + 360f) % 360f;
                _startOrbitY = currentY;
                _orbitAccumulatedDegrees = 0f;
                _orbitPhase = 0;
            }

            // When pose changes (plugin or game), reapply current view so character stays in frame
            var nowInfo = hScene.ctrlFlag?.nowAnimationInfo;
            if (nowInfo != null && !ReferenceEquals(nowInfo, _lastNowAnimationInfoRef))
            {
                _lastNowAnimationInfoRef = nowInfo;
                ApplyCurrentViewOption(hScene, ctrl);
            }

            // After faintness toggle or other state change: reapply view once
            if (_requestViewReapplyNextFrame)
            {
                _requestViewReapplyNextFrame = false;
                ApplyCurrentViewOption(hScene, ctrl);
                _lastNowAnimationInfoRef = hScene.ctrlFlag?.nowAnimationInfo;
            }

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
            // Do NOT assign CameraAngle: its setter does transform.rotation = Euler(v) without transBase, breaking orbit vs CameraUpdate().
            // Match game autoCamera: only CamDat.Rot; CameraControl_Ver2.LateUpdate -> CameraUpdate applies transBase * Euler(CamDat.Rot).
            var rot = ctrl.CameraAngle;
            rot.y = rotY;
            ctrl.Rot = rot;
        }

        /// <summary>Minimum camera distance in body-height units (legacy configs often had 0.3; too tight for full-body framing).</summary>
        private const float OrbitDistanceMultMin = 1.35f;
        private const float OrbitDistanceMultMax = 3f;

        /// <summary>Set camera distance = body height × config multiplier for this focus. Call after setting TargetPos.</summary>
        private static void SetDistanceForFocus(CameraControl_Ver2 ctrl, ChaControl[]? chaFemales, int focusIndex)
        {
            if (chaFemales == null || chaFemales.Length == 0) return;
            int femaleIdx = focusIndex < 3 ? 0 : 1;
            float bodyHeight = OrbitHelpers.GetBodyHeight(chaFemales, femaleIdx);
            float mult = 1f;
            if (focusIndex == 0 || focusIndex == 3) mult = HS2OrbitAndExciter.OrbitDistanceHead?.Value ?? 1.4f;
            else if (focusIndex == 1 || focusIndex == 4) mult = HS2OrbitAndExciter.OrbitDistanceChest?.Value ?? 1.4f;
            else mult = HS2OrbitAndExciter.OrbitDistancePelvis?.Value ?? 1.4f;
            // Old cfg defaults (e.g. 0.3) mapped to 1× height and felt "inside" the model; pull back.
            if (mult < 1f)
                mult = OrbitDistanceMultMin;
            mult = Mathf.Clamp(mult, OrbitDistanceMultMin, OrbitDistanceMultMax);
            float d = bodyHeight * mult;
            d = Mathf.Clamp(d, OrbitDistanceMultMin * bodyHeight, OrbitDistanceMultMax * bodyHeight);
            ctrl.CameraDir = new Vector3(0f, 0f, -d);
        }

        /// <summary>
        /// Mirror vanilla <c>GlobalMethod.CameraKeyCtrl</c> in LateUpdate before orbit writes yaw so focus keys apply in the same frame as <see cref="CameraControl_Ver2.LateUpdate"/>.
        /// Vanilla only sets <see cref="CameraControl_Ver2.TargetPos"/>; we then set distance from body height so rapid Q/W/E reads clearly (matches plugin focus options 0..5).
        /// Skips when <c>HSceneFlagCtrl.inputForcus</c> (same as <c>HScene.ShortcutKey</c>).
        /// </summary>
        private void ApplyOrbitFocusHotkeys(HScene hScene, CameraControl_Ver2 ctrl)
        {
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag != null && ctrlFlag.inputForcus)
                return;

            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || chaFemales.Length == 0)
                return;

            bool ctrl2 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl2) return;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            int maxFocus = OrbitHelpers.GetMaxFocusIndex(chaFemales);
            int newOpt = -1;
            if (!shift)
            {
                if (Input.GetKeyDown(KeyCode.Q)) newOpt = 0;
                else if (Input.GetKeyDown(KeyCode.W)) newOpt = 1;
                else if (Input.GetKeyDown(KeyCode.E)) newOpt = 2;
            }
            else
            {
                if (chaFemales.Length > 1 && chaFemales[1] != null && chaFemales[1].objBodyBone != null)
                {
                    if (Input.GetKeyDown(KeyCode.Q)) newOpt = 3;
                    else if (Input.GetKeyDown(KeyCode.W)) newOpt = 4;
                    else if (Input.GetKeyDown(KeyCode.E)) newOpt = 5;
                }
            }

            if (newOpt < 0 || newOpt > maxFocus)
                return;

            SetDistanceForFocus(ctrl, chaFemales, newOpt);
            _currentViewOption = newOpt;
            // #region agent log
            Debug341efeLog("OrbitController.ApplyOrbitFocusHotkeys", "focusHotkey",
                "{\"newOpt\":" + newOpt + ",\"shift\":" + (shift ? "true" : "false") + "}", "QWE");
            // #endregion
        }

        /// <summary>
        /// Scene-aware Finish hotkeys (Path B: ctrlFlag.click). Keys mapped from P backwards by universality:
        /// P=FinishBefore(all), O=FinishInSide(H+S), I=FinishVomit(S), U=FinishOutSide(S3), Y=FinishSame(H2), T=FinishDrink(H2).
        /// Availability per modeCtrl from MultiPlay_F2M1.SetFinishCategoryEnable.
        /// </summary>
        private static void ApplyFinishHotkeys(HScene hScene)
        {
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null || ctrlFlag.inputForcus) return;

            // Get current modeCtrl via reflection (private field on HScene)
            if (_modeCtrlField == null)
                _modeCtrlField = typeof(HScene).GetField("modeCtrl", BindingFlags.NonPublic | BindingFlags.Instance);
            int mode = _modeCtrlField != null ? (int)_modeCtrlField.GetValue(hScene) : -1;
            if (mode < 0) return;

            // Map keys to ClickKind, filtered by scene availability
            HSceneFlagCtrl.ClickKind kind;
            if (Input.GetKeyDown(KeyCode.P))
                kind = HSceneFlagCtrl.ClickKind.FinishBefore;           // all modes
            else if (Input.GetKeyDown(KeyCode.O) && mode != 0)
                kind = HSceneFlagCtrl.ClickKind.FinishInSide;           // modeCtrl != 0
            else if (Input.GetKeyDown(KeyCode.I) && (mode == 3 || mode == 4))
                kind = HSceneFlagCtrl.ClickKind.FinishVomit;            // Sonyu (3,4)
            else if (Input.GetKeyDown(KeyCode.U) && mode == 3)
                kind = HSceneFlagCtrl.ClickKind.FinishOutSide;          // Sonyu sub-mode 3
            else if (Input.GetKeyDown(KeyCode.Y) && mode == 2)
                kind = HSceneFlagCtrl.ClickKind.FinishSame;             // Houshi sub-mode 2
            else if (Input.GetKeyDown(KeyCode.T) && mode == 2)
                kind = HSceneFlagCtrl.ClickKind.FinishDrink;            // Houshi sub-mode 2
            else return;

            ctrlFlag.click = kind;
        }

        /// <summary>Apply current view option: body focus (GetFocusPosition + SetDistanceForFocus) or pose default camera (setCameraLoad).</summary>
        private void ApplyCurrentViewOption(HScene hScene, CameraControl_Ver2 ctrl)
        {
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || ctrl == null) return;
            int maxFocus = OrbitHelpers.GetMaxFocusIndex(chaFemales);
            int totalOptions = maxFocus + 1;
            if (totalOptions <= 0) return;
            int option = _currentViewOption;
            if (option < 0 || option > maxFocus)
                option = 0;
            if (option < maxFocus)
            {
                var pos = OrbitHelpers.GetFocusPosition(chaFemales, option, ctrl.transBase);
                if (pos.HasValue)
                {
                    ctrl.TargetPos = pos.Value;
                    SetDistanceForFocus(ctrl, chaFemales, option);
                }
            }
            else
            {
                var ctrlFlag = hScene.ctrlFlag;
                if (ctrlFlag != null && ctrlFlag.nowAnimationInfo != null)
                    hScene.setCameraLoad(ctrlFlag.nowAnimationInfo, true);
            }
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

            if (nRandom > 0 && _orbitCycleCount % nRandom == 0)
            {
                int totalOptions = maxFocus + 1;
                if (totalOptions > 0)
                {
                    int current = _currentViewOption;
                    if (current < 0 || current > maxFocus) current = 0;
                    // Exclude current so we don't get the same option twice in a row
                    var candidates = new List<int>();
                    for (int i = 0; i <= maxFocus; i++)
                        if (i != current)
                            candidates.Add(i);
                    if (Random.value < PreferGameDefaultCameraChance)
                        _currentViewOption = maxFocus;
                    else if (candidates.Count > 0)
                        _currentViewOption = candidates[Random.Range(0, candidates.Count)];
                    else
                        _currentViewOption = current;
                    ApplyCurrentViewOption(hScene, ctrl);
                }
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
            // #region agent log
            var hScene = GetHScene();
            DebugSessionLog("OrbitController.cs:OnOrbitToggled", "entry", "{\"active\":" + (active ? "true" : "false") + ",\"hSceneNotNull\":" + (hScene != null ? "true" : "false") + "}", "H5");
            // #endregion
            if (hScene == null)
            {
                HS2OrbitAndExciter.Log?.LogInfo("Orbit: No H scene; orbit will start when entering H.");
                return;
            }

            var ctrl = hScene.ctrlFlag?.cameraCtrl;
            // #region agent log
            DebugSessionLog("OrbitController.cs:OnOrbitToggled", "ctrl check", "{\"ctrlNotNull\":" + (ctrl != null ? "true" : "false") + "}", "H5");
            // #endregion
            if (ctrl == null) return;

            if (active)
            {
                _orbitActiveForPatches = true;
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
                int maxFocus = OrbitHelpers.GetMaxFocusIndex(chaFemales);
                int totalOptions = maxFocus + 1;
                if (totalOptions > 0 && Random.value < PreferGameDefaultCameraChance)
                    _currentViewOption = maxFocus;
                else
                    _currentViewOption = totalOptions > 0 ? Random.Range(0, totalOptions) : 0;
                ApplyCurrentViewOption(hScene, (CameraControl_Ver2)ctrl);
                _lastNowAnimationInfoRef = hScene.ctrlFlag?.nowAnimationInfo;
                if (IsInPreparationState(hScene))
                {
                    _waitingForPrepStart = true;
                    _prepCountdownStart = Time.unscaledTime;
                }
                else
                    _waitingForPrepStart = false;
            }
            else
            {
                _orbitActiveForPatches = false;
                _waitingForPrepStart = false;
                // #region agent log
                DebugLog("OrbitController.cs:OnOrbitToggled", "Stopping orbit, restoring NoCtrlCondition", "{\"hasSaved\":" + (_savedNoCtrlCondition != null ? "true" : "false") + "}", "H5");
                // #endregion
                if (_savedNoCtrlCondition != null && _savedNoCtrlCondition != NoCtrlOrbit)
                    ctrl.NoCtrlCondition = _savedNoCtrlCondition;
                else
                    ctrl.NoCtrlCondition = () => true; // ensure player gets control back if we had no valid saved
            }
        }

        private static HScene? GetHScene() => TryGetHScene();

        /// <summary>For <see cref="OrbitHSceneLateAssist"/> and patches.</summary>
        internal static HScene? TryGetHScene()
        {
            if (!Singleton<HSceneManager>.IsInstance())
                return null;
            return Singleton<HSceneManager>.Instance.Hscene;
        }

        /// <summary>Runs after H-scene proc (see OrbitHSceneLateAssist). Do not call from early LateUpdate.</summary>
        internal void RunLateHSceneAssist(HScene hScene)
        {
            if (!_orbitActive || hScene == null) return;
            // When user is interacting (mouse held), pause auto-advance so game stays responsive.
            if (IsUserControllingCamera())
            {
                _checkpointIdleTime = 0f;
                return;
            }
            ApplyOrbitAutoAction(hScene);
            DebugOrbitIdlePass(hScene);
            TryAutoAdvancePastCheckpoint(hScene);
        }

        /// <summary>Request that the next LateUpdate reapplies the current orbit view (e.g. after faintness toggle).</summary>
        public static void RequestViewReapply()
        {
            _requestViewReapplyNextFrame = true;
        }
    }
}
