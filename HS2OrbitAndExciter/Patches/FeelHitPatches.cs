using HarmonyLib;

namespace HS2OrbitAndExciter.Patches
{
    /// <summary>
    /// When speed > 0, always count as hit so gauge rises (plan §4).
    /// </summary>
    [HarmonyPatch(typeof(FeelHit), nameof(FeelHit.isHit))]
    public static class FeelHitPatches
    {
        [HarmonyPrefix]
        public static bool Prefix(int _state, int _loop, float _power, int _resist, ref bool __result)
        {
            if (_power > 0f)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
