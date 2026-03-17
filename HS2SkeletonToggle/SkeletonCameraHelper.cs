using UnityEngine;
using System.Collections.Generic;
using AIChara;

namespace HS2SkeletonToggle
{
    /// <summary>
    /// Attached to Main Camera; draws skeleton lines in OnPostRender when skeleton mode is on.
    /// </summary>
    public class SkeletonCameraHelper : MonoBehaviour
    {
        private Material _lineMat;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

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
        }

        private void OnPostRender()
        {
            if (!SkeletonToggleCore.SkeletonMode || _lineMat == null) return;

            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(GL.GetGPUProjectionMatrix(Camera.main != null ? Camera.main.projectionMatrix : GetComponent<Camera>().projectionMatrix, false));
            GL.modelview = (Camera.main != null ? Camera.main : GetComponent<Camera>()).worldToCameraMatrix;
            GL.LoadIdentity();
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
    }
}
