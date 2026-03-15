using HarmonyLib;
using UnityEngine;

namespace HS2OrbitAndExciter.Patches
{
    /// <summary>
    /// When orbit is active, bypass wheel checkpoints: after a short delay inject a small wheel value so the game advances from Idle/orgasm without user input.
    /// </summary>
    internal static class OrbitBypassWheelState
    {
        public const float BypassWheelValue = 0.05f;
        public const float DelaySeconds = 2f;
        private static float _idleTimer;

        public static bool TryBypass(ref float wheel)
        {
            if (!OrbitController.IsOrbitActive())
            {
                _idleTimer = 0f;
                return false;
            }
            if (wheel != 0f)
            {
                _idleTimer = 0f;
                return false;
            }
            _idleTimer += Time.deltaTime;
            if (_idleTimer < DelaySeconds)
                return false;
            _idleTimer = 0f;
            wheel = BypassWheelValue;
            return true;
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartProcTrigger")]
    public static class OrbitBypass_StartProcTrigger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(ref _wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartAibuProc")]
    public static class OrbitBypass_StartAibuProc
    {
        [HarmonyPrefix]
        static void Prefix(ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartHoushiProc")]
    public static class OrbitBypass_StartHoushiProc
    {
        [HarmonyPrefix]
        static void Prefix(ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "FaintnessStartProcTrigger")]
    public static class OrbitBypass_FaintnessStartProcTrigger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(ref _wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "FaintnessStartAibuProc")]
    public static class OrbitBypass_FaintnessStartAibuProc
    {
        [HarmonyPrefix]
        static void Prefix(ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "AfterTheInsideWaitingProc")]
    public static class OrbitBypass_AfterTheInsideWaitingProc
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(ref _wheel);
        }
    }

    [HarmonyPatch(typeof(Masturbation), "StartProcTrriger")]
    public static class OrbitBypass_Masturbation_StartProcTrriger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(ref _wheel);
        }
    }
}
