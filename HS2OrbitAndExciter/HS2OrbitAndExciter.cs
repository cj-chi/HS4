using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HS2OrbitAndExciter
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class HS2OrbitAndExciter : BaseUnityPlugin
    {
        internal static ManualLogSource? Log;

        internal static ConfigEntry<float>? OrbitTimePer360;
        internal static ConfigEntry<float>? ExcitementTriggerDelaySeconds;
        /// <summary>When orbit is active, add this much to feel_f per second (0 = no auto accumulation).</summary>
        internal static ConfigEntry<float>? FeelAddPerSecondWhenOrbit;
        internal static ConfigEntry<int>? OrbitCountBeforeRandom;
        internal static ConfigEntry<int>? OrbitCountBeforePoseChange;
        internal static ConfigEntry<bool>? ChangePoseOnCycle;
        internal static ConfigEntry<bool>? ClothesChangeEnabled;
        /// <summary>When orbit is on: enable game auto action (isAutoActionChange + initiative) so user rarely needs to operate.</summary>
        internal static ConfigEntry<bool>? OrbitAutoActionEnabled;
        /// <summary>When orbit is on and stuck at checkpoint (Idle, no selection): auto-advance after this many seconds (0 = use game auto only).</summary>
        internal static ConfigEntry<float>? OrbitCheckpointTimeoutSeconds;
        /// <summary>Orbit camera distance = body height × this (head focus).</summary>
        internal static ConfigEntry<float>? OrbitDistanceHead;
        /// <summary>Orbit camera distance = body height × this (chest focus).</summary>
        internal static ConfigEntry<float>? OrbitDistanceChest;
        /// <summary>Orbit camera distance = body height × this (pelvis focus).</summary>
        internal static ConfigEntry<float>? OrbitDistancePelvis;
        /// <summary>Override faintness state in H scene (HScene.ctrlFlag.isFaintness). When toggled, pose list and camera view update.</summary>
        internal static ConfigEntry<bool>? OverrideFaintness;

        private static void PatchSafe(Harmony harmony, System.Type patchType)
        {
            try { harmony.PatchAll(patchType); }
            catch (System.Exception ex) { Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Patch skipped {patchType?.Name}: {ex.Message}"); }
        }

        private void Awake()
        {
            // #region agent log
            try
            {
                var logPath = System.IO.Path.Combine(@"d:\HS4", "debug-069a45.log");
                var fallback = System.IO.Path.Combine(@"D:\hs2", "BepInEx", "debug-069a45.log");
                var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var line = $"{{\"sessionId\":\"069a45\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"HS2OrbitAndExciter.cs:Awake\",\"message\":\"Awake started\",\"data\":\"early\",\"timestamp\":{ts}}}\n";
                try { System.IO.File.AppendAllText(logPath, line); } catch { System.IO.File.AppendAllText(fallback, line); }
            }
            catch { }
            // #endregion
            try
            {
                Log = Logger;

                OrbitTimePer360 = Config.Bind("Orbit", "OrbitTimePer360", 10f,
                    "Seconds for one full 360° rotation (one direction).");
                OrbitCountBeforeRandom = Config.Bind("Orbit", "OrbitCountBeforeRandom", 1,
                    "After this many full orbits (360°+reverse), randomize focus and angle. 0 = never.");
                OrbitCountBeforePoseChange = Config.Bind("Orbit", "OrbitCountBeforePoseChange", 2,
                    "After this many full orbits, change pose (if ChangePoseOnCycle is true).");
                ChangePoseOnCycle = Config.Bind("Orbit", "ChangePoseOnCycle", false,
                    "Whether to change pose after OrbitCountBeforePoseChange orbits.");
                ClothesChangeEnabled = Config.Bind("Orbit", "ClothesChangeEnabled", false,
                    "Whether to advance clothes stage (Full→Half→KeepAccessories→FullOff) each full orbit.");
                ExcitementTriggerDelaySeconds = Config.Bind("Exciter", "ExcitementTriggerDelaySeconds", 0f,
                    "Seconds at full gauge before auto trigger (0 = immediate, range 0–10). Mouse click still triggers immediately.");
                FeelAddPerSecondWhenOrbit = Config.Bind("Exciter", "FeelAddPerSecondWhenOrbit", 0.1f,
                    "When orbit (Ctrl+Shift+O) is active, add this much to excitement gauge per second (0 = only game default / mouse). 0.1 = fill in 10 s.");
                OrbitAutoActionEnabled = Config.Bind("Orbit", "OrbitAutoActionEnabled", true,
                    "When orbit is on: enable game auto action so next pose/action is chosen automatically (user rarely needs to operate).");
                OrbitCheckpointTimeoutSeconds = Config.Bind("Orbit", "OrbitCheckpointTimeoutSeconds", 5f,
                    "When orbit is on and stuck at checkpoint (Idle, no selection): auto-advance after this many seconds. 0 = only use game auto, no forced advance.");
                OrbitDistanceHead = Config.Bind("Orbit", "OrbitDistanceHead", 0.3f,
                    "Camera distance for head focus, in body-height units (0.1–3, avoid clipping).");
                OrbitDistanceChest = Config.Bind("Orbit", "OrbitDistanceChest", 0.3f,
                    "Camera distance for chest focus, in body-height units (0.1–3, avoid clipping).");
                OrbitDistancePelvis = Config.Bind("Orbit", "OrbitDistancePelvis", 0.3f,
                    "Camera distance for pelvis focus, in body-height units (0.1–3, avoid clipping).");
                OverrideFaintness = Config.Bind("State", "OverrideFaintness", false,
                    "In H scene: force faintness state on/off (ctrlFlag.isFaintness). Affects pose list and triggers camera reapply when orbit is on.");

                Patches.ExciterState.DelaySecondsAtFull = ExcitementTriggerDelaySeconds.Value;
                ExcitementTriggerDelaySeconds.SettingChanged += (_, __) => Patches.ExciterState.DelaySecondsAtFull = ExcitementTriggerDelaySeconds.Value;

                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                PatchSafe(harmony, typeof(Patches.FeelHitPatches));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F2M1_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F2M1_OLoopSonyuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F1M2_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F1M2_OLoopSonyuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Spnking_ActionProc));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_StartProcTrigger));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_StartAibuProc));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_StartHoushiProc));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_FaintnessStartProcTrigger));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_FaintnessStartAibuProc));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_AfterTheInsideWaitingProc));
                PatchSafe(harmony, typeof(Patches.OrbitBypass_Masturbation_StartProcTrriger));
                // Masturbation/Les/Sonyu/Aibu 不載入（此遊戲 build 無對應方法，避免警告）
                var go = new GameObject("HS2OrbitAndExciterController");
                DontDestroyOnLoad(go);
                go.AddComponent<OrbitController>();
                var gui = go.AddComponent<OrbitSettingsGUI>();
                // #region agent log
                try
                {
                    var logPath = System.IO.Path.Combine(@"d:\HS4", "debug-069a45.log");
                    var fallback = System.IO.Path.Combine(@"D:\hs2", "BepInEx", "debug-069a45.log");
                    var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var line = $"{{\"sessionId\":\"069a45\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"HS2OrbitAndExciter.cs:Awake\",\"message\":\"OrbitSettingsGUI added\",\"data\":\"enabled={gui.enabled} active={go.activeSelf} name={go.name}\",\"timestamp\":{ts}}}\n";
                    try { System.IO.File.AppendAllText(logPath, line); } catch { System.IO.File.AppendAllText(fallback, line); }
                }
                catch { }
                // #endregion
                Log.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded. Settings: Ctrl+Shift+P.");
            }
            catch (System.Exception ex)
            {
                // #region agent log
                try
                {
                    var logPath = System.IO.Path.Combine(@"d:\HS4", "debug-069a45.log");
                    var fallback = System.IO.Path.Combine(@"D:\hs2", "BepInEx", "debug-069a45.log");
                    var msg = (ex.Message ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
                    var stack = (ex.StackTrace ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
                    var ts = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var line = $"{{\"sessionId\":\"069a45\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"HS2OrbitAndExciter.cs:Awake\",\"message\":\"Exception\",\"data\":\"msg={msg} stack={stack}\",\"timestamp\":{ts}}}\n";
                    try { System.IO.File.AppendAllText(logPath, line); } catch { System.IO.File.AppendAllText(fallback, line); }
                }
                catch { }
                // #endregion
                throw;
            }
        }
    }
}
