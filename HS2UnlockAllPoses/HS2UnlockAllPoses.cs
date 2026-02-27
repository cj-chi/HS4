using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace HS2UnlockAllPoses
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class HS2UnlockAllPoses : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource? PluginLog;

        private void Awake()
        {
            PluginLog = Logger;
            Harmony.CreateAndPatchAll(typeof(Patches.HSceneSpritePatches));
            PluginLog.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
