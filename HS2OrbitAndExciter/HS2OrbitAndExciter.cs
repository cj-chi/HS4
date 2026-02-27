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
        internal static ManualLogSource Log;

        internal static ConfigEntry<float> OrbitTimePer360;
        internal static ConfigEntry<float> ExcitementTriggerDelaySeconds;
        internal static ConfigEntry<int> OrbitCountBeforeRandom;
        internal static ConfigEntry<int> OrbitCountBeforePoseChange;
        internal static ConfigEntry<bool> ChangePoseOnCycle;
        internal static ConfigEntry<bool> ClothesChangeEnabled;

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

                Patches.ExciterState.DelaySecondsAtFull = ExcitementTriggerDelaySeconds.Value;
                ExcitementTriggerDelaySeconds.SettingChanged += (_, __) => Patches.ExciterState.DelaySecondsAtFull = ExcitementTriggerDelaySeconds.Value;

                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                PatchSafe(harmony, typeof(Patches.FeelHitPatches));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F2M1_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F2M1_OLoopSonyuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F1M2_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_F1M2_OLoopSonyuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Masturbation_OLoopProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Les_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Spnking_ActionProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Sonyu_OLoopAibuProc));
                PatchSafe(harmony, typeof(Patches.ExciterTranspiler_Aibu_OLoopAibuProc));
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
