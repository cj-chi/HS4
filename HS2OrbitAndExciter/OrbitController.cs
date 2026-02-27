using System.Collections.Generic;
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

        private bool _orbitActive;
        private BaseCameraControl_Ver2.NoCtrlFunc _savedNoCtrlCondition;

        private float _startOrbitY;
        private int _orbitPhase;
        private float _orbitAccumulatedDegrees;
        private int _orbitCycleCount;
        private int _currentFocusIndex;
        /// <summary>Index into sequence [0,1,2,3,2,1]; next step is (_currentClothesSequenceIndex + 1) % 6.</summary>
        private int _currentClothesSequenceIndex;

        private void Update()
        {
            if (Input.GetKey(Modifier2) && Input.GetKey(Modifier) && Input.GetKeyDown(OrbitHotkey))
            {
                _orbitActive = !_orbitActive;
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

            // Re-apply camera takeover every frame (e.g. after ChangeAnimation sets camera flag)
            ctrl.NoCtrlCondition = () => false;

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
                _savedNoCtrlCondition = ctrl.NoCtrlCondition;
                ctrl.NoCtrlCondition = () => false;
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
                if (_savedNoCtrlCondition != null)
                    ctrl.NoCtrlCondition = _savedNoCtrlCondition;
            }
        }

        private static HScene GetHScene()
        {
            if (!Singleton<HSceneManager>.IsInstance())
                return null;
            return Singleton<HSceneManager>.Instance.Hscene;
        }
    }
}
