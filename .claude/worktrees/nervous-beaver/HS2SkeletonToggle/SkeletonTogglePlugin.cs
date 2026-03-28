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
        public static bool RefLinesVisible { get; set; }
        public static bool RefLineMenuVisible { get; set; }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkeletonTogglePlugin : BaseUnityPlugin
    {
        private ConfigEntry<KeyCode> _hotkeyMod1;
        private ConfigEntry<KeyCode> _hotkeyMod2;
        private ConfigEntry<KeyCode> _hotkeyKey;
        private ConfigEntry<KeyCode> _refLinesKey;
        private ConfigEntry<KeyCode> _refMenuKey;
        private ConfigEntry<bool> _skeletonOnlySaved;
        private ConfigEntry<bool> _refLinesVisibleSaved;
        private ConfigEntry<bool> _refLineMenuVisibleSaved;
        /// <summary>Head + hair mesh renderers disabled while skeleton mode is on.</summary>
        private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();
        private Camera _mainCam;
        private bool _skeletonMode;
        private bool _refLinesVisible;
        private bool _refLineMenuVisible;

        private void Awake()
        {
            _hotkeyMod1 = Config.Bind("Hotkey", "Modifier1", KeyCode.LeftControl, "First modifier (Ctrl)");
            _hotkeyMod2 = Config.Bind("Hotkey", "Modifier2", KeyCode.LeftShift, "Second modifier (Shift)");
            _hotkeyKey = Config.Bind("Hotkey", "Key", KeyCode.S, "Key (S)");
            _refLinesKey = Config.Bind("Hotkey", "RefLinesKey", KeyCode.D, "Toggle ref lines key (D)");
            _refMenuKey = Config.Bind("Hotkey", "RefMenuKey", KeyCode.F, "Toggle ref line selection menu key (F)");
            _skeletonOnlySaved = Config.Bind("State", "SkeletonOnly", false, "Last skeleton-only state");
            _refLinesVisibleSaved = Config.Bind("State", "RefLinesVisible", true, "Last ref lines visible state");
            _refLineMenuVisibleSaved = Config.Bind("State", "RefLineMenuVisible", false, "Last ref line selection menu visible state (Ctrl+Shift+F); default hidden");
            _skeletonMode = _skeletonOnlySaved.Value;
            SkeletonToggleCore.SkeletonMode = _skeletonMode;
            _refLinesVisible = _refLinesVisibleSaved.Value;
            SkeletonToggleCore.RefLinesVisible = _refLinesVisible;
            _refLineMenuVisible = _refLineMenuVisibleSaved.Value;
            SkeletonToggleCore.RefLineMenuVisible = _refLineMenuVisible;
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

            // Ctrl+Shift+D toggles ref line visibility (independent of skeleton mode)
            if (Input.GetKey(_hotkeyMod1.Value) && Input.GetKey(_hotkeyMod2.Value) && Input.GetKeyDown(_refLinesKey.Value))
            {
                _refLinesVisible = !_refLinesVisible;
                _refLinesVisibleSaved.Value = _refLinesVisible;
                SkeletonToggleCore.RefLinesVisible = _refLinesVisible;
            }

            // Ctrl+Shift+F toggles ref line selection menu visibility (independent of skeleton mode)
            if (Input.GetKey(_hotkeyMod1.Value) && Input.GetKey(_hotkeyMod2.Value) && Input.GetKeyDown(_refMenuKey.Value))
            {
                _refLineMenuVisible = !_refLineMenuVisible;
                _refLineMenuVisibleSaved.Value = _refLineMenuVisible;
                SkeletonToggleCore.RefLineMenuVisible = _refLineMenuVisible;
            }

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

            // 隱藏臉部／頭部 mesh 與所有頭髮 slot 的 mesh；身體保留；骨骼線由 SkeletonCameraHelper 繪製
            var chaControls = Object.FindObjectsOfType<ChaControl>();
            foreach (var cha in chaControls)
            {
                if (cha == null) continue;

                if (cha.objHead != null)
                {
                    foreach (var r in cha.objHead.GetComponentsInChildren<Renderer>(true))
                    {
                        if (r != null && r.enabled)
                        {
                            r.enabled = false;
                            _hiddenRenderers.Add(r);
                        }
                    }
                }

                if (cha.objHair != null)
                {
                    foreach (var hairRoot in cha.objHair)
                    {
                        if (hairRoot == null) continue;
                        foreach (var r in hairRoot.GetComponentsInChildren<Renderer>(true))
                        {
                            if (r != null && r.enabled)
                            {
                                r.enabled = false;
                                _hiddenRenderers.Add(r);
                            }
                        }
                    }
                }
            }
        }
    }
}
