using UnityEngine;
using System.Collections.Generic;
using AIChara;

namespace HS2SkeletonToggle
{
    /// <summary>
    /// Attached to Main Camera; draws skeleton lines in OnPostRender when skeleton mode is on.
    /// Head bone tree is drawn deep enough to include all ShapeHeadInfoFemale DstName bones; optional five ref distance lines.
    /// </summary>
    public class SkeletonCameraHelper : MonoBehaviour
    {
        private Material _lineMat;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        private static readonly Color ColorOtherLines = new Color(60f / 255f, 140f / 255f, 1f, 0.9f); // blue thin (skeleton + ref lines default)
        private static readonly Color ColorSelected = new Color(1f, 0f, 0f, 0.98f);
        private static readonly Color ColorJointSphere = new Color(1f, 0.92f, 0.2f, 0.95f);

        private struct HoverSegment
        {
            public Vector2 a; // screen-space point (Input.mousePosition coord system: bottom-left origin)
            public Vector2 b;
            public string childName;   // always show child.name
            public string sliderNames; // UI slider names joined by " / "
        }

        private struct HoverJoint
        {
            public Vector2 screen; // bottom-left origin, same as Input.mousePosition
            public string label;
        }

        private readonly List<HoverSegment> _hoverSegments = new List<HoverSegment>(512);
        private readonly List<HoverJoint> _hoverJoints = new List<HoverJoint>(256);
        // Menu items should include all segments we draw this frame,
        // even if they are off-screen / cannot be projected for hover.
        private readonly Dictionary<string, string> _menuKeyToLabel = new Dictionary<string, string>(1024);
        private const float HoverThresholdPx = 12f;
        /// <summary>Screen hit radius for joint spheres (scale with <see cref="JointSphereRadius"/>).</summary>
        private const float HoverJointThresholdPx = 44f;
        private const float JointSphereRadius = 0.014f * 3f; // 3x original ~0.014
        // Key = childName + "|" + sliderNames. Used to highlight all segments that share same label.
        private string _selectedSegmentKey = null; // null = none
        private Rect _refMenuRect = new Rect(20f, 20f, 320f, 220f);
        private Vector2 _refMenuScrollPos = Vector2.zero;
        private const int RefMenuWindowId = 0xBEE0; // arbitrary

        /// <summary>Quantized world position for merging overlapping joint spheres.</summary>
        private struct Int3 : System.IEquatable<Int3>
        {
            public int x, y, z;
            public bool Equals(Int3 o) => x == o.x && y == o.y && z == o.z;
            public override bool Equals(object obj) => obj is Int3 o && Equals(o);
            public override int GetHashCode() => unchecked((x * 73856093) ^ (y * 19349663) ^ (z * 83492791));
        }

        private sealed class JointAgg
        {
            public Vector3 world;
            public readonly HashSet<string> segKeys = new HashSet<string>();
        }

        private readonly Dictionary<Int3, JointAgg> _jointAggByQuant = new Dictionary<Int3, JointAgg>(512);
        private Mesh _sphereMesh;
        private Material _sphereMat;

        private void Awake()
        {
            CreateLineMaterial();
            CreateSphereResources();
        }

        private void CreateLineMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) return;
            _lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _lineMat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Off);
            _lineMat.SetInt(ZWrite, 0);
            // Draw on top to avoid being occluded by hair/face geometry.
            _lineMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        private void CreateSphereResources()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                _sphereMesh = mf.sharedMesh;
            Object.Destroy(go);

            // Prefer Unlit/Color; fall back for older Unity / HS2.
            var sh = Shader.Find("Unlit/Color");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Hidden/Internal-Colored");
            if (sh == null || _sphereMesh == null) return;

            _sphereMat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            _sphereMat.SetColor("_Color", ColorJointSphere);
            if (_sphereMat.HasProperty("_ZWrite"))
                _sphereMat.SetInt("_ZWrite", 0);
            // Try to draw on top like line overlay (shader may ignore unknown props).
            if (_sphereMat.HasProperty("_ZTest"))
                _sphereMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            _sphereMat.renderQueue = 4000;
        }

        private static Int3 QuantizeWorld(Vector3 p)
        {
            const float scale = 2000f;
            return new Int3
            {
                x = Mathf.RoundToInt(p.x * scale),
                y = Mathf.RoundToInt(p.y * scale),
                z = Mathf.RoundToInt(p.z * scale)
            };
        }

        private void RegisterLineEndpoints(Vector3 a, Vector3 b, string segKey)
        {
            RegisterEndpoint(a, segKey);
            RegisterEndpoint(b, segKey);
        }

        private void RegisterEndpoint(Vector3 world, string segKey)
        {
            if (string.IsNullOrEmpty(segKey)) return;
            var q = QuantizeWorld(world);
            if (!_jointAggByQuant.TryGetValue(q, out var agg))
            {
                agg = new JointAgg { world = world };
                _jointAggByQuant[q] = agg;
            }
            else
                agg.world = world; // keep latest
            agg.segKeys.Add(segKey);
        }

        private void DrawJointSpheres()
        {
            if (_sphereMesh == null || _sphereMat == null) return;
            float radius = JointSphereRadius;
            foreach (var kv in _jointAggByQuant)
            {
                var agg = kv.Value;
                bool sel = !string.IsNullOrEmpty(_selectedSegmentKey);
                if (sel)
                {
                    sel = false;
                    foreach (var k in agg.segKeys)
                    {
                        if (k == _selectedSegmentKey) { sel = true; break; }
                    }
                }
                var col = sel ? ColorSelected : ColorJointSphere;
                if (_sphereMat.HasProperty("_Color"))
                    _sphereMat.SetColor("_Color", col);
                var trs = Matrix4x4.TRS(agg.world, Quaternion.identity, Vector3.one * radius);
                _sphereMat.SetPass(0);
                Graphics.DrawMeshNow(_sphereMesh, trs);
            }
        }

        private string BuildJointHoverLabel(JointAgg agg)
        {
            var keys = new List<string>(agg.segKeys);
            keys.Sort(System.StringComparer.Ordinal);
            var parts = new List<string>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                string k = keys[i];
                if (_menuKeyToLabel.TryGetValue(k, out var lab))
                {
                    parts.Add(lab);
                    continue;
                }
                int pipe = k.IndexOf('|');
                if (pipe >= 0)
                {
                    string bone = k.Substring(0, pipe);
                    string sliders = pipe + 1 < k.Length ? k.Substring(pipe + 1) : "";
                    parts.Add(string.IsNullOrEmpty(sliders) ? bone : bone + " / " + sliders);
                }
                else
                    parts.Add(k);
            }
            return string.Join(" | ", parts);
        }

        private void CollectJointHoverTargets(Camera cam)
        {
            _hoverJoints.Clear();
            if (cam == null) return;
            foreach (var kv in _jointAggByQuant)
            {
                var agg = kv.Value;
                Vector2 scr;
                if (!TryProject(cam, agg.world, out scr)) continue;
                _hoverJoints.Add(new HoverJoint
                {
                    screen = scr,
                    label = BuildJointHoverLabel(agg)
                });
            }
        }

        private void OnPostRender()
        {
            if (!SkeletonToggleCore.SkeletonMode || _lineMat == null) return;

            // #region hover segments
            _hoverSegments.Clear();
            _hoverJoints.Clear();
            _menuKeyToLabel.Clear();
            _jointAggByQuant.Clear();
            // #endregion

            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(GL.GetGPUProjectionMatrix(Camera.main != null ? Camera.main.projectionMatrix : GetComponent<Camera>().projectionMatrix, false));
            GL.modelview = (Camera.main != null ? Camera.main : GetComponent<Camera>()).worldToCameraMatrix;
            GL.Begin(GL.LINES);
            GL.Color(ColorOtherLines);

            var chaControls = Object.FindObjectsOfType<ChaControl>();
            Camera cam = Camera.main != null ? Camera.main : GetComponent<Camera>();
            foreach (var cha in chaControls)
            {
                if (cha?.objHeadBone == null) continue;
                var t = cha.objHeadBone.transform;
                // Deep enough to draw all face DstName bones under head (ShapeHeadInfoFemale tree).
                DrawBoneHierarchyAndCollect(t, cam, 0, 32);
                if (cha.objBodyBone != null)
                    DrawBoneHierarchyAndCollect(cha.objBodyBone.transform, cam, 0, 6);
            }

            GL.End();

            // Five HS2 ref lines (head_width, eye_span, eye_size, chin_height, nose_height)
            if (SkeletonToggleCore.RefLinesVisible)
            {
                _lineMat.SetPass(0);
                GL.Begin(GL.LINES);
                foreach (var cha in chaControls)
                {
                    if (cha?.objBodyBone == null) continue;
                    DrawFiveRefLinesAndCollect(cha, cam);
                }
                GL.End();
            }

            GL.PopMatrix();

            DrawJointSpheres();
            CollectJointHoverTargets(cam);
        }

        private void OnGUI()
        {
            if (!SkeletonToggleCore.SkeletonMode) return;

            // #region ref line selection menu (Ctrl+Shift+F)
            if (SkeletonToggleCore.RefLineMenuVisible)
                DrawRefLineSelectionMenu();
            // #endregion

            var mouse = Input.mousePosition; // bottom-left origin
            HoverSegment bestSeg = default;
            float bestSegDist = float.MaxValue;
            bool hasSeg = false;

            foreach (var seg in _hoverSegments)
            {
                float d = DistancePointToSegment(mouse, seg.a, seg.b);
                if (d <= HoverThresholdPx && d < bestSegDist)
                {
                    bestSegDist = d;
                    bestSeg = seg;
                    hasSeg = true;
                }
            }

            HoverJoint bestJnt = default;
            float bestJntDist = float.MaxValue;
            bool hasJnt = false;
            foreach (var j in _hoverJoints)
            {
                float d = Vector2.Distance(mouse, j.screen);
                if (d <= HoverJointThresholdPx && d < bestJntDist)
                {
                    bestJntDist = d;
                    bestJnt = j;
                    hasJnt = true;
                }
            }

            if (!hasSeg && !hasJnt) return;

            // Convert to OnGUI (top-left origin)
            Vector2 posTopLeft = new Vector2(mouse.x + 12f, Screen.height - mouse.y + 12f);
            var style = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            float lineH = style.lineHeight > 1f ? style.lineHeight : 18f;
            GUI.color = Color.white;

            if (hasSeg && hasJnt)
            {
                string lineLabel = bestSeg.childName;
                if (!string.IsNullOrEmpty(bestSeg.sliderNames))
                    lineLabel += " / " + bestSeg.sliderNames;
                // Top = smaller y in OnGUI coords: joint (球) above line; draw joint second so it paints on top when overlapping.
                GUI.Label(new Rect(posTopLeft.x, posTopLeft.y + lineH, 560f, 120f), lineLabel, style);
                GUI.Label(new Rect(posTopLeft.x, posTopLeft.y, 560f, 120f), "[球] " + bestJnt.label, style);
            }
            else if (hasSeg)
            {
                string lineLabel = bestSeg.childName;
                if (!string.IsNullOrEmpty(bestSeg.sliderNames))
                    lineLabel += " / " + bestSeg.sliderNames;
                GUI.Label(new Rect(posTopLeft.x, posTopLeft.y, 560f, 80f), lineLabel, style);
            }
            else
            {
                GUI.Label(new Rect(posTopLeft.x, posTopLeft.y, 560f, 120f), "[球] " + bestJnt.label, style);
            }
        }

        // #region ref line selection menu
        private void DrawRefLineSelectionMenu()
        {
            _refMenuRect = GUI.Window(RefMenuWindowId, _refMenuRect, DrawRefLineMenuWindow, "Lines select");
        }

        private void DrawRefLineMenuWindow(int id)
        {
            const float dragH = 20f;
            const float noneButtonH = 22f;
            const float itemH = 22f;

            float x = 10f;
            float yNone = dragH + 2f;
            float w = _refMenuRect.width - 20f;

            // "None" button: clear selection -> all lines revert to thin blue.
            GUI.color = string.IsNullOrEmpty(_selectedSegmentKey) ? Color.red : Color.white;
            if (GUI.Button(new Rect(x, yNone, w, noneButtonH), "None"))
                _selectedSegmentKey = null;
            GUI.color = Color.white;

            // Build unique menu items from all segments we drew this frame.
            // (We populate _menuKeyToLabel during draw, not during hover projection.)
            var keys = new List<string>(_menuKeyToLabel.Keys);
            keys.Sort((a, b) => string.Compare(_menuKeyToLabel[a], _menuKeyToLabel[b], System.StringComparison.Ordinal));

            float scrollTop = yNone + noneButtonH + 4f;
            float scrollH = _refMenuRect.height - scrollTop - 6f;
            if (scrollH < 30f) scrollH = 30f;

            var viewRect = new Rect(0f, scrollTop, _refMenuRect.width, scrollH);
            var contentRect = new Rect(0f, 0f, _refMenuRect.width, keys.Count * itemH + 10f);

            _refMenuScrollPos = GUI.BeginScrollView(viewRect, _refMenuScrollPos, contentRect, false, true);
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                string label = _menuKeyToLabel[key];
                bool isSel = key == _selectedSegmentKey;
                GUI.color = isSel ? ColorSelected : Color.white;

                float y = i * itemH;
                if (GUI.Button(new Rect(x, y, w, itemH), label))
                    _selectedSegmentKey = key;
            }
            GUI.color = Color.white;
            GUI.EndScrollView();

            // Make menu movable by dragging the top bar.
            GUI.DragWindow(new Rect(0f, 0f, _refMenuRect.width, dragH));
        }
        // #endregion

        private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float ab2 = Vector2.Dot(ab, ab);
            if (ab2 <= 1e-6f) return Vector2.Distance(p, a);
            float t = Vector2.Dot(p - a, ab) / ab2;
            t = Mathf.Clamp01(t);
            Vector2 q = a + ab * t;
            return Vector2.Distance(p, q);
        }

        private static bool TryProject(Camera cam, Vector3 world, out Vector2 screen)
        {
            screen = default;
            if (cam == null) return false;
            var s = cam.WorldToScreenPoint(world);
            if (s.z <= 0f) return false;
            screen = new Vector2(s.x, s.y);
            return true;
        }

        // Preallocated slider index sets (cf_headshapename 0..58) to avoid GC in hot path.
        private static readonly int[] _scNone = System.Array.Empty<int>();
        private static readonly int[] _sc0 = { 0 };
        private static readonly int[] _sc1 = { 1 };
        private static readonly int[] _sc2 = { 2 };
        private static readonly int[] _sc3 = { 3 };
        private static readonly int[] _sc4 = { 4 };
        private static readonly int[] _sc5_8 = { 5, 6, 7, 8 };
        private static readonly int[] _sc5_9 = { 5, 9 };
        private static readonly int[] _sc10_12 = { 10, 11, 12 };
        private static readonly int[] _sc13_15 = { 13, 14, 15 };
        private static readonly int[] _sc16_18 = { 16, 17, 18 };
        private static readonly int[] _sc19 = { 19 };
        private static readonly int[] _sc19_23 = { 19, 20, 21, 22, 23 };
        private static readonly int[] _sc22_23_30_31 = { 22, 23, 30, 31 };
        private static readonly int[] _sc24 = { 24 };
        private static readonly int[] _sc24_25 = { 24, 25 };
        private static readonly int[] _sc32_33_44 = { 32, 33, 44 };
        private static readonly int[] _sc32_33_35 = { 32, 33, 35 };
        private static readonly int[] _sc32_44_45_46 = { 32, 44, 45, 46 };
        private static readonly int[] _sc36_38 = { 36, 37, 38 };
        private static readonly int[] _sc39_43 = { 39, 40, 41, 42, 43 };
        private static readonly int[] _sc47_48_49_53 = { 47, 48, 49, 53 };
        private static readonly int[] _sc47_49_51_52 = { 47, 49, 51, 52 };
        private static readonly int[] _sc50_52 = { 50, 51, 52 };
        private static readonly int[] _sc51_53 = { 51, 52, 53 };
        private static readonly int[] _sc54_56 = { 54, 55, 56 };
        private static readonly int[] _sc54_57 = { 54, 57 };
        private static readonly int[] _sc54_58 = { 54, 58 };

        /// <summary>Map head bone (child) to shapeValueFace indices for hover/menu labels. Heuristic from FaceShapeIdx / ShapeHeadInfoFemale DstName.</summary>
        private static int[] GetSliderCategoriesForBone(string childName)
        {
            switch (childName)
            {
                case "cf_J_FaceBase": return _sc0;
                case "cf_J_FaceUp_tz": return _sc1;
                case "cf_J_FaceUp_ty": return _sc2;
                case "cf_J_FaceLow_s": return _sc3;
                case "cf_J_FaceLowBase": return _sc4;

                case "cf_J_CheekLow_L":
                case "cf_J_CheekLow_R":
                    return _sc13_15;
                case "cf_J_CheekUp_L":
                case "cf_J_CheekUp_R":
                    return _sc16_18;

                case "cf_J_Chin_rs": return _sc5_8;
                case "cf_J_ChinLow": return _sc5_9;
                case "cf_J_ChinTip_s": return _sc10_12;

                case "cf_J_EarBase_s_L":
                case "cf_J_EarBase_s_R":
                    return _sc54_56;
                case "cf_J_EarUp_L":
                case "cf_J_EarUp_R":
                    return _sc54_57;
                case "cf_J_EarLow_L":
                case "cf_J_EarLow_R":
                    return _sc54_58;
                case "cf_J_EarRing_L":
                case "cf_J_EarRing_R":
                    return _sc54_56;

                case "cf_J_Eye_t_L":
                case "cf_J_Eye_t_R":
                    return _sc19;
                case "cf_J_Eye_s_L":
                case "cf_J_Eye_s_R":
                    return _sc19_23;
                case "cf_J_Eye_r_L":
                case "cf_J_Eye_r_R":
                    return _sc24_25;
                case "cf_J_Eye01_L":
                case "cf_J_Eye01_R":
                case "cf_J_Eye02_L":
                case "cf_J_Eye02_R":
                case "cf_J_Eye03_L":
                case "cf_J_Eye03_R":
                case "cf_J_Eye04_L":
                case "cf_J_Eye04_R":
                    return _sc22_23_30_31;
                case "cf_J_EyePos_rz_L":
                case "cf_J_EyePos_rz_R":
                    return _sc24;

                case "cf_J_megane": return _scNone;

                case "cf_J_Nose_t": return _sc32_33_44;
                case "cf_J_Nose_tip": return _sc32_44_45_46;
                case "cf_J_NoseBase_s":
                case "cf_J_NoseBase_trs":
                    return _sc32_33_35;
                case "cf_J_NoseBridge_s":
                case "cf_J_NoseBridge_t":
                    return _sc36_38;
                case "cf_J_NoseWing_tx_L":
                case "cf_J_NoseWing_tx_R":
                    return _sc39_43;

                case "cf_J_Mouth_L":
                case "cf_J_Mouth_R":
                    return _sc47_48_49_53;
                case "cf_J_Mouthup":
                case "cf_J_MouthLow":
                    return _sc47_49_51_52;
                case "cf_J_MouthBase_s":
                case "cf_J_MouthBase_tr":
                    return _sc50_52;
                case "cf_J_MouthCavity": return _sc51_53;

                default:
                    return _scNone;
            }
        }

        private static string JoinSliderNames(int[] categories)
        {
            if (categories == null || categories.Length == 0) return "";
            // Keep it simple: show all matching categories.
            var parts = new string[categories.Length];
            for (int i = 0; i < categories.Length; i++)
            {
                int idx = categories[i];
                if (idx >= 0 && idx < ChaFileDefine.cf_headshapename.Length)
                    parts[i] = ChaFileDefine.cf_headshapename[idx];
                else
                    parts[i] = idx.ToString();
            }
            return string.Join(" / ", parts);
        }

        private void DrawBoneHierarchyAndCollect(Transform t, Camera cam, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                // Match selection by label: child.name + sliderNames
                var cats = GetSliderCategoriesForBone(child.name);
                string sliderNames = JoinSliderNames(cats);
                string segKey = child.name + "|" + sliderNames;
                bool highlight = !string.IsNullOrEmpty(_selectedSegmentKey) && segKey == _selectedSegmentKey;
                // Populate menu even if this segment cannot be projected for hover.
                if (!_menuKeyToLabel.ContainsKey(segKey))
                {
                    string label = child.name;
                    if (!string.IsNullOrEmpty(sliderNames))
                        label += " / " + sliderNames;
                    _menuKeyToLabel[segKey] = label;
                }
                GL.Color(highlight ? ColorSelected : ColorOtherLines);
                GL.Vertex(t.position);
                GL.Vertex(child.position);
                RegisterLineEndpoints(t.position, child.position, segKey);
                // Collect for hover (only if both endpoints are in front of camera)
                Vector2 a2, b2;
                if (TryProject(cam, t.position, out a2) && TryProject(cam, child.position, out b2))
                {
                    var seg = new HoverSegment
                    {
                        a = a2,
                        b = b2,
                        childName = child.name,
                        sliderNames = sliderNames
                    };
                    _hoverSegments.Add(seg);
                }

                DrawBoneHierarchyAndCollect(child, cam, depth + 1, maxDepth);
            }
        }

        /// <summary>Recursive find transform by name (avoids dependency on IllusionUtility.GetUtility).</summary>
        private static Transform FindLoop(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name)) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindLoop(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Draw five reference lines (HS2 bone space) matching draw_top5_hs2_refs.py semantics.
        /// Also collects ref segments for hover label display.</summary>
        private void DrawFiveRefLinesAndCollect(ChaControl cha, Camera cam)
        {
            Transform root = cha.objBodyBone.transform;
            var segments = new List<(Vector3 a, Vector3 b, string childName, int[] sliderCats)>();

            // 1) head_width
            Transform cheekL = FindLoop(root, "cf_J_CheekUp_L");
            Transform cheekR = FindLoop(root, "cf_J_CheekUp_R");
            Transform faceUp = FindLoop(root, "cf_J_FaceUp_ty");
            Transform chinTip = FindLoop(root, "cf_J_ChinTip_s");
            if (cheekL != null && cheekR != null)
                segments.Add((cheekL.position, cheekR.position, cheekR.name, new[] { 0 }));
            if (faceUp != null && chinTip != null)
                segments.Add((faceUp.position, chinTip.position, chinTip.name, new[] { 0 }));

            // 2) eye_span
            Transform eyeL = FindLoop(root, "cf_J_Eye_s_L");
            Transform eyeR = FindLoop(root, "cf_J_Eye_s_R");
            if (eyeL != null && eyeR != null)
                segments.Add((eyeL.position, eyeR.position, eyeR.name, new[] { 20 }));

            // 3) eye_size
            Transform eye01L = FindLoop(root, "cf_J_Eye01_L");
            Transform eye03L = FindLoop(root, "cf_J_Eye03_L");
            Transform eye01R = FindLoop(root, "cf_J_Eye01_R");
            Transform eye03R = FindLoop(root, "cf_J_Eye03_R");
            if (eye01L != null && eye03L != null)
                segments.Add((eye01L.position, eye03L.position, eye03L.name, new[] { 22, 23 }));
            if (eye01R != null && eye03R != null)
                segments.Add((eye01R.position, eye03R.position, eye03R.name, new[] { 22, 23 }));

            // 4) chin_height
            Transform mouthUp = FindLoop(root, "cf_J_Mouthup");
            Transform mouthLow = FindLoop(root, "cf_J_MouthLow");
            if (chinTip != null && mouthUp != null && mouthLow != null)
            {
                Vector3 mouthCenter = (mouthUp.position + mouthLow.position) * 0.5f;
                segments.Add((chinTip.position, mouthCenter, mouthLow.name, new[] { 11 }));
            }

            // 5) nose_height
            Transform noseBridge = FindLoop(root, "cf_J_NoseBridge_t");
            Transform noseTip = FindLoop(root, "cf_J_Nose_tip");
            if (noseBridge != null && noseTip != null)
                segments.Add((noseBridge.position, noseTip.position, noseTip.name, new[] { 32 }));

            foreach (var (a, b, childName, sliderCats) in segments)
            {
                string sliderNames = JoinSliderNames(sliderCats);
                string segKey = childName + "|" + sliderNames;
                bool highlight = !string.IsNullOrEmpty(_selectedSegmentKey) && segKey == _selectedSegmentKey;

                // Populate menu even if this segment cannot be projected for hover.
                if (!_menuKeyToLabel.ContainsKey(segKey))
                {
                    string label = childName;
                    if (!string.IsNullOrEmpty(sliderNames))
                        label += " / " + sliderNames;
                    _menuKeyToLabel[segKey] = label;
                }

                // Collect hover segment in 2D
                Vector2 a2, b2;
                if (TryProject(cam, a, out a2) && TryProject(cam, b, out b2))
                {
                    _hoverSegments.Add(new HoverSegment
                    {
                        a = a2,
                        b = b2,
                        childName = childName,
                        sliderNames = sliderNames
                    });
                }
                // All segments default to thin blue; only menu selection turns red.
                GL.Color(highlight ? ColorSelected : ColorOtherLines);
                GL.Vertex(a);
                GL.Vertex(b);
                RegisterLineEndpoints(a, b, segKey);
            }
        }
    }
}
