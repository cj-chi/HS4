using System;
using System.Reflection;
using HarmonyLib;

namespace HS2UnlockAllPoses.Patches
{
    /// <summary>
    /// Postfix: when the original returns false, allow the pose if only "safe" checks failed
    /// (state, achievement, pain, faintness). We must still pass: number, EventNo==19, CheckEventLimit, CheckPlace, CheckAppendEV.
    /// </summary>
    [HarmonyPatch]
    public static class HSceneSpritePatches
    {
        private static MethodBase _checkEventLimitMethod;

        static HSceneSpritePatches()
        {
            _checkEventLimitMethod = AccessTools.Method(typeof(HSceneSprite), "CheckEventLimit");
        }

        private static int GetEventNo(HSceneSprite instance)
        {
            return Traverse.Create(instance).Field("EventNo").GetValue<int>();
        }

        private static int GetEventPeep(HSceneSprite instance)
        {
            return Traverse.Create(instance).Field("EventPeep").GetValue<int>();
        }

        private static bool WouldUnsafeChecksPass(HSceneSprite instance, HScene.AnimationListInfo lstAnimInfo, int eventNo, int eventPeep)
        {
            // 1. Number: need second female for Item1==4,5; need second male for Item1==6 (use Traverse: fields may be private in game DLL)
            var chaFemales = Traverse.Create(instance).Field("chaFemales").GetValue() as Array;
            var chaMales = Traverse.Create(instance).Field("chaMales").GetValue() as Array;
            if (chaFemales != null && chaFemales.Length > 1 && chaFemales.GetValue(1) == null &&
                (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5))
                return false;
            if (chaMales != null && chaMales.Length > 1 && chaMales.GetValue(1) == null && lstAnimInfo.ActionCtrl.Item1 == 6)
                return false;

            // 2. EventNo == 19 special case
            if (eventNo == 19)
            {
                if (lstAnimInfo.ActionCtrl.Item1 == 4 || lstAnimInfo.ActionCtrl.Item1 == 5 || lstAnimInfo.ActionCtrl.Item1 == 6)
                    return false;
                if (lstAnimInfo.ActionCtrl.Item1 == 3 && lstAnimInfo.id == 0)
                    return false;
            }

            // 3. CheckEventLimit (private)
            if (_checkEventLimitMethod != null)
            {
                var ev = lstAnimInfo.Event;
                var checkEvent = _checkEventLimitMethod.Invoke(instance, new object[] { ev });
                if (checkEvent is bool b && !b)
                    return false;
            }

            // 4. Item1==3 and Item2==5 or 6: event must match (inlined from original)
            if (lstAnimInfo.ActionCtrl.Item1 == 3)
            {
                int item2 = lstAnimInfo.ActionCtrl.Item2;
                if (item2 == 5 || item2 == 6)
                {
                    bool flag = false;
                    foreach (int[] item in lstAnimInfo.Event)
                    {
                        if (item[1] == -1)
                        {
                            if (item[0] == eventNo) flag = true;
                        }
                        else if (item[0] == eventNo && item[1] == eventPeep)
                            flag = true;
                        if (flag) break;
                    }
                    if (!flag) return false;
                }
            }

            // 5. CheckPlace (public)
            if (!instance.CheckPlace(lstAnimInfo))
                return false;

            // 6. CheckAppendEV (public)
            if (!instance.CheckAppendEV(lstAnimInfo, eventNo))
                return false;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.CheckMotionLimit))]
        public static void CheckMotionLimit_Postfix(HSceneSprite __instance, HScene.AnimationListInfo lstAnimInfo, ref bool __result)
        {
            if (__result) return;
            int eventNo = GetEventNo(__instance);
            int eventPeep = GetEventPeep(__instance);
            if (WouldUnsafeChecksPass(__instance, lstAnimInfo, eventNo, eventPeep))
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.CheckMotionLimitRecover))]
        public static void CheckMotionLimitRecover_Postfix(HSceneSprite __instance, HScene.AnimationListInfo lstAnimInfo, ref bool __result)
        {
            if (__result) return;
            int eventNo = GetEventNo(__instance);
            int eventPeep = GetEventPeep(__instance);
            if (WouldUnsafeChecksPass(__instance, lstAnimInfo, eventNo, eventPeep))
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), "CheckAutoMotionLimit")]
        public static void CheckAutoMotionLimit_Postfix(HSceneSprite __instance, HScene.AnimationListInfo lstAnimInfo, ref bool __result)
        {
            if (__result) return;
            int eventNo = GetEventNo(__instance);
            int eventPeep = GetEventPeep(__instance);
            if (WouldUnsafeChecksPass(__instance, lstAnimInfo, eventNo, eventPeep))
                __result = true;
        }
    }
}
