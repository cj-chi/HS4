using System;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace HS2OrbitAndExciter.Patches
{
    /// <summary>
    /// When orbit is active, bypass wheel checkpoints: after a short delay inject a small wheel value so the game advances from Idle/orgasm without user input.
    /// </summary>
    internal static class OrbitBypassWheelState
    {
        public const float BypassWheelValue = 0.10f;
        public const float DelaySeconds = 2f;
        private static float _bypassStartTimeUnscaled = -1f;

        // Avoid log spam: only log when we "start waiting" and when we "inject".
        private static float _lastStartLogTime;
        private static float _lastInjectLogTime;

    private static string EscapeJson(string s)
    {
        if (s == null) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static void Debug341efeLog(string location, string message, string dataJson, string hypothesisId)
    {
        var logPath = Path.Combine(@"d:\HS4", "debug-341efe.log");
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var line =
            $"{{\"sessionId\":\"341efe\",\"runId\":\"run1\",\"hypothesisId\":\"{EscapeJson(hypothesisId)}\",\"location\":\"{EscapeJson(location)}\",\"message\":\"{EscapeJson(message)}\",\"data\":{dataJson},\"timestamp\":{ts}}}\n";
        try { File.AppendAllText(logPath, line); } catch { }
    }

        public static bool TryBypass(string patchName, ref float wheel)
        {
            bool orbitActive = OrbitController.IsOrbitActive();
            float wheelIn = wheel;

            if (!orbitActive || wheel != 0f)
            {
                _bypassStartTimeUnscaled = -1f;
                return false;
            }

            // wheel==0 && orbitActive
            if (_bypassStartTimeUnscaled < 0f)
            {
                _bypassStartTimeUnscaled = Time.unscaledTime;

                // Log "waiting start"
                if (Time.unscaledTime - _lastStartLogTime >= 0.5f)
                {
                    _lastStartLogTime = Time.unscaledTime;
                    var logger = HS2OrbitAndExciter.Log;
                    logger?.LogInfo($"[OrbitBypassWheel] start patch={patchName} wheelIn={wheelIn:F2} delay={DelaySeconds:F1}");
                    Debug341efeLog(
                        "OrbitBypassWheelState.TryBypass",
                        "wheelBypassStart",
                        "{\"patchName\":\"" + EscapeJson(patchName) + "\",\"wheelIn\":" + wheelIn.ToString("F2") + ",\"delay\":" + DelaySeconds.ToString("F1") + "}",
                        "WB1");
                }
            }

            float elapsed = Time.unscaledTime - _bypassStartTimeUnscaled;
            if (elapsed < DelaySeconds)
                return false;

            _bypassStartTimeUnscaled = -1f;
            wheel = BypassWheelValue;

            // Log "injected"
            if (Time.unscaledTime - _lastInjectLogTime >= 0.2f)
            {
                _lastInjectLogTime = Time.unscaledTime;
                var logger = HS2OrbitAndExciter.Log;
                logger?.LogInfo($"[OrbitBypassWheel] inject patch={patchName} wheelIn={wheelIn:F2} wheelOut={wheel:F2} elapsed={elapsed:F2}");
                Debug341efeLog(
                    "OrbitBypassWheelState.TryBypass",
                    "wheelBypassInject",
                    "{\"patchName\":\"" + EscapeJson(patchName) + "\",\"wheelIn\":" + wheelIn.ToString("F2") + ",\"wheelOut\":" + wheel.ToString("F2") + ",\"elapsed\":" + elapsed.ToString("F2") + "}",
                    "WB2");
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartProcTrigger")]
    public static class OrbitBypass_StartProcTrigger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_StartProcTrigger), ref _wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartAibuProc")]
    public static class OrbitBypass_StartAibuProc
    {
        [HarmonyPrefix]
        static void Prefix(bool _isReStart, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_StartAibuProc), ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartHoushiProc")]
    public static class OrbitBypass_StartHoushiProc
    {
        [HarmonyPrefix]
        static void Prefix(int _state, bool _restart, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_StartHoushiProc), ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "FaintnessStartProcTrigger")]
    public static class OrbitBypass_FaintnessStartProcTrigger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_FaintnessStartProcTrigger), ref _wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "FaintnessStartAibuProc")]
    public static class OrbitBypass_FaintnessStartAibuProc
    {
        [HarmonyPrefix]
        static void Prefix(bool _start, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_FaintnessStartAibuProc), ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "AfterTheInsideWaitingProc")]
    public static class OrbitBypass_AfterTheInsideWaitingProc
    {
        [HarmonyPrefix]
        static void Prefix(int _state, ref float _wheel, int _modeCtrl)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AfterTheInsideWaitingProc), ref _wheel);
        }
    }

    [HarmonyPatch(typeof(Masturbation), "StartProcTrriger")]
    public static class OrbitBypass_Masturbation_StartProcTrriger
    {
        [HarmonyPrefix]
        static void Prefix(ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_Masturbation_StartProcTrriger), ref _wheel);
        }
    }

    // Aibu Idle/D_Idle auto-entry (wheel==0 gate) bypass
    [HarmonyPatch(typeof(MultiPlay_F2M1), "AutoStartProcTrigger")]
    public static class OrbitBypass_AutoStartProcTrigger
    {
        [HarmonyPrefix]
        static void Prefix(bool _start, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AutoStartProcTrigger), ref wheel);
        }
    }

    [HarmonyPatch(typeof(MultiPlay_F2M1), "AutoStartAibuProc")]
    public static class OrbitBypass_AutoStartAibuProc
    {
        [HarmonyPrefix]
        static void Prefix(bool _isReStart, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AutoStartAibuProc), ref wheel);
        }
    }

    // Houshi Idle/D_Idle auto-entry bypass (wheel==0 gate)
    [HarmonyPatch(typeof(MultiPlay_F2M1), "AutoStartHoushiProc")]
    public static class OrbitBypass_AutoStartHoushiProc
    {
        [HarmonyPrefix]
        static void Prefix(int _state, bool _restart, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AutoStartHoushiProc), ref wheel);
        }
    }

    // Sonyu Idle auto-entry bypass (wheel==0 gate)
    [HarmonyPatch(typeof(MultiPlay_F2M1), "AutoStartSonyuProc")]
    public static class OrbitBypass_AutoStartSonyuProc
    {
        [HarmonyPrefix]
        static void Prefix(bool _restart, int _state, int _modeCtrl, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AutoStartSonyuProc), ref wheel);
        }
    }

    // Sonyu D_Idle bypass uses StartSonyuProc (not AutoStartSonyuProc)
    [HarmonyPatch(typeof(MultiPlay_F2M1), "StartSonyuProc")]
    public static class OrbitBypass_StartSonyuProc
    {
        [HarmonyPrefix]
        static void Prefix(bool _restart, int _state, int _modeCtrl, ref float wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_StartSonyuProc), ref wheel);
        }
    }

    // Inside-waiting follow-up bypass (wheel==0 gate)
    [HarmonyPatch(typeof(MultiPlay_F2M1), "AutoAfterTheInsideWaitingProc")]
    public static class OrbitBypass_AutoAfterTheInsideWaitingProc
    {
        [HarmonyPrefix]
        static void Prefix(int _state, ref float _wheel)
        {
            OrbitBypassWheelState.TryBypass(nameof(OrbitBypass_AutoAfterTheInsideWaitingProc), ref _wheel);
        }
    }
}
