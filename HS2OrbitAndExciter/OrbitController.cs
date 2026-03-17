using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AIChara;
using HarmonyLib;
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

        /// <summary>When true, game gives camera to player; when false, orbit keeps control. User can always intervene (mouse drag or Q/W/E).</summary>
        private static bool IsUserControllingCamera()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) return true;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.E)) return true;
            return false;
        }

        private static readonly BaseCameraControl_Ver2.NoCtrlFunc NoCtrlOrbit = () => IsUserControllingCamera();

        private const float HotkeyCooldownSeconds = 0.25f;

        private bool _orbitActive;
        /// <summary>Mirror of _orbitActive for Harmony patches (static read).</summary>
        private static bool _orbitActiveForPatches;
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
        private static void DebugSessionLog(string location, string message, string dataJson, string hypothesisId)
        {
            var logPath = Path.Combine(@"d:\HS4", "debug-b1efc0.log");
            var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var line = $"{{\"sessionId\":\"b1efc0\",\"runId\":\"run1\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"{location}\",\"message\":\"{message}\",\"data\":{dataJson},\"timestamp\":{ts}}}\n";
            try { File.AppendAllText(logPath, line); } catch { }
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
        /// <summary>Seconds spent at checkpoint (Idle, no selection) while orbit is on; reset when we advance or leave checkpoint.</summary>
        private float _checkpointIdleTime;
        private static MethodInfo? _getAutoAnimationMethod;
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

        /// <summary>When orbit is on and OrbitAutoActionEnabled: set isAutoActionChange and initiative so game auto-picks next action.</summary>
        private void ApplyOrbitAutoAction(HScene hScene)
        {
            if (HS2OrbitAndExciter.OrbitAutoActionEnabled?.Value != true) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            var t = Traverse.Create(ctrlFlag);
            t.Property("isAutoActionChange").SetValue(true);
            t.Field("initiative").SetValue(1);
        }

        /// <summary>When orbit is on and stuck at checkpoint (Idle, no selection) for OrbitCheckpointTimeoutSeconds, call HScene.GetAutoAnimation to advance.</summary>
        private void TryAutoAdvancePastCheckpoint(HScene hScene)
        {
            float timeout = HS2OrbitAndExciter.OrbitCheckpointTimeoutSeconds?.Value ?? 5f;
            if (timeout <= 0f) return;
            var ctrlFlag = hScene.ctrlFlag;
            if (ctrlFlag == null) return;
            if (Traverse.Create(ctrlFlag).Property("selectAnimationListInfo").GetValue() != null) { _checkpointIdleTime = 0f; return; }
            var chaFemales = OrbitHelpers.GetChaFemales(hScene);
            if (chaFemales == null || chaFemales.Length == 0) return;
            var cha = chaFemales[0];
            if (cha == null) return;
            var animBody = Traverse.Create(cha).Field("animBody").GetValue();
            if (animBody == null) return;
            var animType = animBody.GetType();
            var getState = animType.GetMethod("GetCurrentAnimatorStateInfo", new[] { typeof(int) });
            if (getState == null) return;
            var state = getState.Invoke(animBody, new object[] { 0 });
            if (state == null) return;
            var isName = state.GetType().GetMethod("IsName", new[] { typeof(string) });
            if (isName == null) return;
            bool isIdle = (bool)isName.Invoke(state, new object[] { "Idle" }) || (bool)isName.Invoke(state, new object[] { "D_Idle" });
            if (!isIdle) { _checkpointIdleTime = 0f; return; }
            _checkpointIdleTime += Time.deltaTime;
            if (_checkpointIdleTime < timeout) return;
            _checkpointIdleTime = 0f;
            if (_getAutoAnimationMethod == null)
            {
                _getAutoAnimationMethod = typeof(HScene).GetMethod("GetAutoAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_getAutoAnimationMethod == null) return;
            }
            try
            {
                _getAutoAnimationMethod.Invoke(hScene, new object[] { false });
                if (Traverse.Create(ctrlFlag).Property("selectAnimationListInfo").GetValue() == null)
                    _getAutoAnimationMethod.Invoke(hScene, new object[] { true });
            }
            catch { /* ignore */ }
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
            if (!_orbitActive)
            {
                _requestViewReapplyNextFrame = false;
                return;
            }

            var hScene = GetHScene();
            if (hScene == null) return;

            var ctrl = hScene.ctrlFlag?.cameraCtrl as CameraControl_Ver2;
            if (ctrl == null) return;

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

            // When orbit is on: enable game auto action so user rarely needs to operate (auto-enter action loop / auto-pick next)
            ApplyOrbitAutoAction(hScene);

            // When stuck at checkpoint (Idle, no selection) for N seconds, force advance to next phase
            TryAutoAdvancePastCheckpoint(hScene);

            // Re-apply camera takeover every frame (e.g. after ChangeAnimation sets camera flag)
            ctrl.NoCtrlCondition = NoCtrlOrbit;

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
            var rot = ctrl.CameraAngle;
            rot.y = rotY;
            ctrl.CameraAngle = rot;
            ctrl.Rot = rot;
        }

        /// <summary>Set camera distance = body height × config multiplier for this focus. Call after setting TargetPos.</summary>
        private static void SetDistanceForFocus(CameraControl_Ver2 ctrl, ChaControl[]? chaFemales, int focusIndex)
        {
            if (chaFemales == null || chaFemales.Length == 0) return;
            int femaleIdx = focusIndex < 3 ? 0 : 1;
            float bodyHeight = OrbitHelpers.GetBodyHeight(chaFemales, femaleIdx);
            float mult = 1f;
            if (focusIndex == 0 || focusIndex == 3) mult = HS2OrbitAndExciter.OrbitDistanceHead?.Value ?? 0.3f;
            else if (focusIndex == 1 || focusIndex == 4) mult = HS2OrbitAndExciter.OrbitDistanceChest?.Value ?? 0.3f;
            else mult = HS2OrbitAndExciter.OrbitDistancePelvis?.Value ?? 0.3f;
            float d = bodyHeight * mult;
            d = Mathf.Clamp(d, 0.7f * bodyHeight, 3f * bodyHeight);
            ctrl.CameraDir = new Vector3(0f, 0f, -d);
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
                    if (candidates.Count > 0)
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

        private static HScene? GetHScene()
        {
            if (!Singleton<HSceneManager>.IsInstance())
                return null;
            return Singleton<HSceneManager>.Instance.Hscene;
        }

        /// <summary>Request that the next LateUpdate reapplies the current orbit view (e.g. after faintness toggle).</summary>
        public static void RequestViewReapply()
        {
            _requestViewReapplyNextFrame = true;
        }
    }
}
