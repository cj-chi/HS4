using UnityEngine;
using System.Collections.Generic;
using AIChara;

namespace HS2SkeletonToggle
{
    /// <summary>
    /// Attached to Main Camera; draws skeleton lines in OnPostRender when skeleton mode is on.
    /// When head is hidden, also draws the five HS2 ref lines (head_width, eye_span, eye_size, chin_height, nose_height).
    /// </summary>
    public class SkeletonCameraHelper : MonoBehaviour
    {
        private Material _lineMat;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        /// <summary>Five HS2 ref lines: same semantics as draw_top5_hs2_refs.py (MediaPipe → HS2 bone mapping).</summary>
        private static readonly Color RefLineColor = new Color(1f, 0.85f, 0.2f, 0.95f); // yellow

        /// <summary>Key bone dot color (related to 17 MediaPipe ratios).</summary>
        private static readonly Color KeyBoneDotColor = new Color(0f, 1f, 1f, 0.95f); // cyan
        /// <summary>Other bone dot color.</summary>
        private static readonly Color OtherBoneDotColor = new Color(1f, 0.4f, 1f, 0.7f); // magenta
        /// <summary>World-space half-size of bone dots.</summary>
        private const float BoneDotRadius = 0.004f;

        /// <summary>Bones related to 17 MediaPipe ratios (key bones shown in cyan).</summary>
        private static readonly HashSet<string> KeyBoneNames = new HashSet<string>
        {
            "cf_J_CheekUp_L", "cf_J_CheekUp_R",     // head_width
            "cf_J_FaceUp_ty", "cf_J_ChinTip_s",     // face_height
            "cf_J_Eye_s_L", "cf_J_Eye_s_R",         // eye_span
            "cf_J_Eye01_L", "cf_J_Eye03_L",         // eye_size L
            "cf_J_Eye01_R", "cf_J_Eye03_R",         // eye_size R
            "cf_J_Mouthup", "cf_J_MouthLow",        // chin_height (mouth center)
            "cf_J_Mouth_L", "cf_J_Mouth_R",         // mouth_width
            "cf_J_NoseBridge_t", "cf_J_Nose_tip",   // nose_height
            "cf_J_NoseBase_trs",                     // nose_width
            "cf_J_ChinLow",                          // jaw
        };

        private void Awake()
        {
            CreateLineMaterial();
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

        private void OnPostRender()
        {
            if (!SkeletonToggleCore.SkeletonMode || _lineMat == null) return;

            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(GL.GetGPUProjectionMatrix(Camera.main != null ? Camera.main.projectionMatrix : GetComponent<Camera>().projectionMatrix, false));
            GL.modelview = (Camera.main != null ? Camera.main : GetComponent<Camera>()).worldToCameraMatrix;
            GL.Begin(GL.LINES);
            GL.Color(new Color(0.2f, 0.8f, 0.2f, 0.9f));

            var chaControls = Object.FindObjectsOfType<ChaControl>();
            foreach (var cha in chaControls)
            {
                if (cha?.objHeadBone == null) continue;
                var t = cha.objHeadBone.transform;
                DrawBoneHierarchy(t, 0, 4);
                if (cha.objBodyBone != null)
                    DrawBoneHierarchy(cha.objBodyBone.transform, 0, 6);
            }

            GL.End();

            Camera cam = Camera.main != null ? Camera.main : GetComponent<Camera>();

            // Five HS2 ref lines (head_width, eye_span, eye_size, chin_height, nose_height)
            if (SkeletonToggleCore.RefLinesVisible)
            {
                _lineMat.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Color(RefLineColor);
                foreach (var cha in chaControls)
                {
                    if (cha?.objBodyBone == null) continue;
                    DrawFiveRefLines(cha, cam);
                }
                GL.End();
            }

            // Face bone points (all head bones as GL.QUADS dots, key bones in cyan, others in magenta)
            if (SkeletonToggleCore.BonePointsVisible)
            {
                _lineMat.SetPass(0);
                GL.Begin(GL.QUADS);
                foreach (var cha in chaControls)
                {
                    if (cha?.objHeadBone == null) continue;
                    DrawFaceBonePoints(cha.objHeadBone.transform, cam);
                }
                GL.End();
            }

            GL.PopMatrix();
        }

        private static void DrawBoneHierarchy(Transform t, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                GL.Vertex(t.position);
                GL.Vertex(child.position);
                DrawBoneHierarchy(child, depth + 1, maxDepth);
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

        /// <summary>Recursively draw all face bone positions as camera-facing quads. Key bones in cyan, others in magenta.</summary>
        private static void DrawFaceBonePoints(Transform root, Camera cam)
        {
            if (root == null || cam == null) return;
            Vector3 right = cam.transform.right * BoneDotRadius;
            Vector3 up = cam.transform.up * BoneDotRadius;
            DrawBonePointRecursive(root, cam, right, up);
        }

        private static void DrawBonePointRecursive(Transform t, Camera cam, Vector3 right, Vector3 up)
        {
            if (t == null) return;
            // Draw this bone as a camera-facing quad
            bool isKey = KeyBoneNames.Contains(t.name);
            GL.Color(isKey ? KeyBoneDotColor : OtherBoneDotColor);
            Vector3 pos = t.position;
            // Key bones are drawn larger for visibility
            float scale = isKey ? 2f : 1f;
            Vector3 r = right * scale;
            Vector3 u = up * scale;
            GL.Vertex(pos - r - u);
            GL.Vertex(pos + r - u);
            GL.Vertex(pos + r + u);
            GL.Vertex(pos - r + u);

            for (int i = 0; i < t.childCount; i++)
                DrawBonePointRecursive(t.GetChild(i), cam, right, up);
        }

        /// <summary>World-space offset for thick lines (multiple passes). Logs showed segments=7 drawn but invisible; 0.003 was too thin at typical camera distance.</summary>
        private const float RefLineThicknessWorld = 0.018f;
        private const int RefLinePasses = 9;

        /// <summary>Draw five reference lines (HS2 bone space) matching draw_top5_hs2_refs.py semantics. Draws thick by multiple offset passes.</summary>
        private static void DrawFiveRefLines(ChaControl cha, Camera cam)
        {
            Transform root = cha.objBodyBone.transform;
            var segments = new List<(Vector3 a, Vector3 b)>();

            // 1) head_width
            Transform cheekL = FindLoop(root, "cf_J_CheekUp_L");
            Transform cheekR = FindLoop(root, "cf_J_CheekUp_R");
            Transform faceUp = FindLoop(root, "cf_J_FaceUp_ty");
            Transform chinTip = FindLoop(root, "cf_J_ChinTip_s");
            if (cheekL != null && cheekR != null) { segments.Add((cheekL.position, cheekR.position)); }
            if (faceUp != null && chinTip != null) { segments.Add((faceUp.position, chinTip.position)); }

            // 2) eye_span
            Transform eyeL = FindLoop(root, "cf_J_Eye_s_L");
            Transform eyeR = FindLoop(root, "cf_J_Eye_s_R");
            if (eyeL != null && eyeR != null) { segments.Add((eyeL.position, eyeR.position)); }

            // 3) eye_size
            Transform eye01L = FindLoop(root, "cf_J_Eye01_L");
            Transform eye03L = FindLoop(root, "cf_J_Eye03_L");
            Transform eye01R = FindLoop(root, "cf_J_Eye01_R");
            Transform eye03R = FindLoop(root, "cf_J_Eye03_R");
            if (eye01L != null && eye03L != null) { segments.Add((eye01L.position, eye03L.position)); }
            if (eye01R != null && eye03R != null) { segments.Add((eye01R.position, eye03R.position)); }

            // 4) chin_height
            Transform mouthUp = FindLoop(root, "cf_J_Mouthup");
            Transform mouthLow = FindLoop(root, "cf_J_MouthLow");
            if (chinTip != null && mouthUp != null && mouthLow != null)
            {
                Vector3 mouthCenter = (mouthUp.position + mouthLow.position) * 0.5f;
                segments.Add((chinTip.position, mouthCenter));
            }

            // 5) nose_height
            Transform noseBridge = FindLoop(root, "cf_J_NoseBridge_t");
            Transform noseTip = FindLoop(root, "cf_J_Nose_tip");
            if (noseBridge != null && noseTip != null) { segments.Add((noseBridge.position, noseTip.position)); }

            Vector3 right = (cam != null ? cam.transform.right : Vector3.right) * RefLineThicknessWorld;
            int half = RefLinePasses / 2;
            foreach (var (a, b) in segments)
            {
                for (int k = -half; k <= half; k++)
                {
                    Vector3 off = right * k;
                    GL.Vertex(a + off);
                    GL.Vertex(b + off);
                }
            }
        }
    }
}
