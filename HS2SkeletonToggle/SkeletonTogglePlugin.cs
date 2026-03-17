using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using AIChara;

namespace HS2SkeletonToggle
{
    public static class SkeletonToggleCore
    {
        public static bool SkeletonMode { get; set; }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkeletonTogglePlugin : BaseUnityPlugin
    {
        private ConfigEntry<KeyCode> _hotkeyMod1;
        private ConfigEntry<KeyCode> _hotkeyMod2;
        private ConfigEntry<KeyCode> _hotkeyKey;
        private ConfigEntry<bool> _skeletonOnlySaved;
        private readonly List<SkinnedMeshRenderer> _hiddenRenderers = new List<SkinnedMeshRenderer>();
        private Camera _mainCam;
        private bool _skeletonMode;

        private void Awake()
        {
            _hotkeyMod1 = Config.Bind("Hotkey", "Modifier1", KeyCode.LeftControl, "First modifier (Ctrl)");
            _hotkeyMod2 = Config.Bind("Hotkey", "Modifier2", KeyCode.LeftShift, "Second modifier (Shift)");
            _hotkeyKey = Config.Bind("Hotkey", "Key", KeyCode.S, "Key (S)");
            _skeletonOnlySaved = Config.Bind("State", "SkeletonOnly", false, "Last skeleton-only state");
            _skeletonMode = _skeletonOnlySaved.Value;
            SkeletonToggleCore.SkeletonMode = _skeletonMode;
        }

        private void OnEnable()
        {
            EnsureCameraHelper();
        }

        private void EnsureCameraHelper()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;
            if (_mainCam.GetComponent<SkeletonCameraHelper>() == null)
                _mainCam.gameObject.AddComponent<SkeletonCameraHelper>();
        }

        private void Update()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam != null && _mainCam.GetComponent<SkeletonCameraHelper>() == null)
                _mainCam.gameObject.AddComponent<SkeletonCameraHelper>();

            if (Input.GetKey(_hotkeyMod1.Value) && Input.GetKey(_hotkeyMod2.Value) && Input.GetKeyDown(_hotkeyKey.Value))
            {
                _skeletonMode = !_skeletonMode;
                _skeletonOnlySaved.Value = _skeletonMode;
                SkeletonToggleCore.SkeletonMode = _skeletonMode;
                ApplySkeletonMode(_skeletonMode);
            }
        }

        private void ApplySkeletonMode(bool skeletonOnly)
        {
            foreach (var r in _hiddenRenderers)
            {
                if (r != null) r.enabled = true;
            }
            _hiddenRenderers.Clear();

            if (!skeletonOnly) return;

            // 只隱藏「頭」的 mesh，身體保留顯示；骨骼線由 SkeletonCameraHelper 繪製
            var chaControls = Object.FindObjectsOfType<ChaControl>();
            foreach (var cha in chaControls)
            {
                if (cha?.objHead == null) continue;
                var headRenderers = cha.objHead.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var r in headRenderers)
                {
                    if (r != null && r.enabled) { r.enabled = false; _hiddenRenderers.Add(r); }
                }
            }
        }
    }
}
