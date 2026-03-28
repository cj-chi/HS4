using HarmonyLib;
using UnityEngine;

namespace HS2OrbitAndExciter.Patches
{
    /// <summary>
    /// Previously set isAutoActionChange=true and initiative=1 after each Proc.
    /// REMOVED: initiative=1 forces Auto mode (gates scroll wheel behind timer);
    /// isAutoActionChange=true causes game to hide motion list.
    /// Auto-advance is now handled by TryAutoAdvancePastCheckpoint in OrbitController.
    /// Patch kept as placeholder to avoid Harmony unpatch issues; body is intentionally empty.
    /// </summary>
    [HarmonyPatch(typeof(ProcBase), nameof(ProcBase.Proc))]
    public static class OrbitAutoActionAfterProcPatches
    {
        [HarmonyPostfix]
        public static void Postfix(ProcBase __instance)
        {
            // Intentionally empty — see class summary.
        }
    }
}
