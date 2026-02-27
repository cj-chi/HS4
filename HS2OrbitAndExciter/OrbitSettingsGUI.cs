using System;
using System.IO;
using UnityEngine;

namespace HS2OrbitAndExciter
{
    /// <summary>
    /// In-game settings window. Toggle with Ctrl+Shift+P. Values write to BepInEx config (auto-saved).
    /// </summary>
    public class OrbitSettingsGUI : MonoBehaviour
    {
        private const KeyCode MenuHotkey = KeyCode.P;
        private const KeyCode Modifier = KeyCode.LeftShift;
        private const KeyCode Modifier2 = KeyCode.LeftControl;
        private static readonly string DebugLogPath = System.IO.Path.Combine(@"d:\HS4", "debug-069a45.log");
        private static readonly string DebugLogPathFallback = System.IO.Path.Combine(@"D:\hs2", "BepInEx", "debug-069a45.log");

        private bool _visible;
        private Rect _windowRect = new Rect(100, 100, 420, 380);
        private GUIStyle _labelStyle;
        private bool _stylesInitialized;
        private int _onGuiCallCount;

        // #region agent log
        private static void DebugLog(string location, string message, object data, string hypothesisId)
        {
            try
            {
                var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var dataStr = data != null ? data.ToString() : "null";
                var line = $"{{\"sessionId\":\"069a45\",\"runId\":\"run1\",\"hypothesisId\":\"{hypothesisId}\",\"location\":\"{Escape(location)}\",\"message\":\"{Escape(message)}\",\"data\":\"{Escape(dataStr)}\",\"timestamp\":{ts}}}\n";
                try { File.AppendAllText(DebugLogPath, line); } catch { File.AppendAllText(DebugLogPathFallback, line); }
            }
            catch { }
        }
        private static string Escape(string s) { return s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? ""; }
        // #endregion

        private void Update()
        {
            // Hotkey moved to OnGUI (Event.current) so it works when game/UI has focus and after window is closed
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            // Handle hotkey in GUI event so it isn't consumed by game/UI; works even when window is closed
            if (Event.current != null && Event.current.type == EventType.KeyDown
                && Event.current.keyCode == MenuHotkey && Event.current.control && Event.current.shift)
            {
                _visible = !_visible;
                Event.current.Use();
            }
            if (!_visible) return;

            // #region agent log
            _onGuiCallCount++;
            if (_onGuiCallCount <= 3)
                DebugLog("OrbitSettingsGUI.cs:OnGUI", "Drawing window", new { _onGuiCallCount, rect = _windowRect.ToString(), screenW = Screen.width, screenH = Screen.height }, "B");
            // #endregion
            InitStyles();
            _windowRect = GUILayout.Window(9001, _windowRect, DrawWindow, "HS2 Orbit and Exciter — 設定");
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.Label("環視 (Orbit)", GUI.skin.box);
            if (HS2OrbitAndExciter.OrbitTimePer360 != null)
            {
                float ot = HS2OrbitAndExciter.OrbitTimePer360.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label("360° 一圈秒數 (OrbitTimePer360):", _labelStyle, GUILayout.Width(220));
                string s = GUILayout.TextField(ot.ToString("F1"), GUILayout.Width(60));
                if (float.TryParse(s, out float v) && v > 0.1f && v <= 120f)
                    HS2OrbitAndExciter.OrbitTimePer360.Value = v;
                GUILayout.EndHorizontal();
            }
            if (HS2OrbitAndExciter.OrbitCountBeforeRandom != null)
            {
                int n = HS2OrbitAndExciter.OrbitCountBeforeRandom.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label("幾次 360 後亂數焦點/角度 (0=不亂數):", _labelStyle, GUILayout.Width(220));
                string s = GUILayout.TextField(n.ToString(), GUILayout.Width(60));
                if (int.TryParse(s, out int v) && v >= 0 && v <= 99)
                    HS2OrbitAndExciter.OrbitCountBeforeRandom.Value = v;
                GUILayout.EndHorizontal();
            }
            if (HS2OrbitAndExciter.OrbitCountBeforePoseChange != null)
            {
                int n = HS2OrbitAndExciter.OrbitCountBeforePoseChange.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label("幾次 360 後換姿勢:", _labelStyle, GUILayout.Width(220));
                string s = GUILayout.TextField(n.ToString(), GUILayout.Width(60));
                if (int.TryParse(s, out int v) && v >= 1 && v <= 99)
                    HS2OrbitAndExciter.OrbitCountBeforePoseChange.Value = v;
                GUILayout.EndHorizontal();
            }
            if (HS2OrbitAndExciter.ChangePoseOnCycle != null)
                HS2OrbitAndExciter.ChangePoseOnCycle.Value = GUILayout.Toggle(HS2OrbitAndExciter.ChangePoseOnCycle.Value, " 每 n 次 360 後換姿勢 (ChangePoseOnCycle)");
            if (HS2OrbitAndExciter.ClothesChangeEnabled != null)
                HS2OrbitAndExciter.ClothesChangeEnabled.Value = GUILayout.Toggle(HS2OrbitAndExciter.ClothesChangeEnabled.Value, " 每完成 1 次 360 切換衣物階段 (ClothesChangeEnabled)");

            GUILayout.Space(8);
            GUILayout.Label("興奮劑 (Exciter)", GUI.skin.box);
            if (HS2OrbitAndExciter.ExcitementTriggerDelaySeconds != null)
            {
                float ex = HS2OrbitAndExciter.ExcitementTriggerDelaySeconds.Value;
                GUILayout.BeginHorizontal();
                GUILayout.Label("滿量表後幾秒才觸發 (0=馬上，點滑鼠仍馬上):", _labelStyle, GUILayout.Width(260));
                string s = GUILayout.TextField(ex.ToString("F1"), GUILayout.Width(60));
                if (float.TryParse(s, out float v) && v >= 0f && v <= 10f)
                {
                    HS2OrbitAndExciter.ExcitementTriggerDelaySeconds.Value = v;
                    Patches.ExciterState.DelaySecondsAtFull = v;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            GUILayout.Label("熱鍵：Ctrl+Shift+O 環視開關，Ctrl+Shift+P 本視窗", _labelStyle);
            if (GUILayout.Button("關閉"))
                _visible = false;

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        }
    }
}
