using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class LuxWater_ProjectorRenderer : MonoBehaviour
{
	public enum BufferResolution
	{
		Full = 1,
		Half = 2,
		Quarter = 4,
		Eighth = 8
	}

	[Space(8f)]
	public BufferResolution FoamBufferResolution = BufferResolution.Full;

	public BufferResolution NormalBufferResolution = BufferResolution.Full;

	[Space(2f)]
	[Header("Debug")]
	[Space(4f)]
	public bool DebugFoamBuffer;

	public bool DebugNormalBuffer;

	public bool DebugStats;

	private int drawnFoamProjectors;

	private int drawnNormalProjectors;

	private static CommandBuffer cb_Foam;

	private static CommandBuffer cb_Normals;

	private Camera cam;

	private Transform camTransform;

	private RenderTexture ProjectedFoam;

	private RenderTexture ProjectedNormals;

	private Texture2D defaultBump;

	private Bounds tempBounds;

	private int _LuxWater_FoamOverlayPID;

	private int _LuxWater_NormalOverlayPID;

	private Plane[] frustumPlanes = new Plane[6];

	private Material DebugMat;

	private Material DebugNormalMat;

	private void OnEnable()
	{
		_LuxWater_FoamOverlayPID = Shader.PropertyToID("_LuxWater_FoamOverlay");
		_LuxWater_NormalOverlayPID = Shader.PropertyToID("_LuxWater_NormalOverlay");
		cb_Foam = new CommandBuffer();
		cb_Foam.name = "Lux Water: Foam Overlay Buffer";
		cb_Normals = new CommandBuffer();
		cb_Normals.name = "Lux Water: Normal Overlay Buffer";
	}

	private void OnDisable()
	{
		if (ProjectedFoam != null)
		{
			Object.DestroyImmediate(ProjectedFoam);
		}
		if (ProjectedNormals != null)
		{
			Object.DestroyImmediate(ProjectedNormals);
		}
		if (defaultBump != null)
		{
			Object.DestroyImmediate(defaultBump);
		}
		if (DebugMat != null)
		{
			Object.DestroyImmediate(DebugMat);
		}
		if (cb_Foam != null && cb_Foam.sizeInBytes > 0)
		{
			cb_Foam.Clear();
			cb_Foam.Dispose();
		}
		if (cb_Normals != null && cb_Normals.sizeInBytes > 0)
		{
			cb_Normals.Clear();
			cb_Normals.Dispose();
		}
		Shader.DisableKeyword("USINGWATERPROJECTORS");
	}

	private void OnPreCull()
	{
		cam = GetComponent<Camera>();
		int count = LuxWater_Projector.FoamProjectors.Count;
		int count2 = LuxWater_Projector.NormalProjectors.Count;
		if (count + count2 == 0)
		{
			if (cb_Foam != null)
			{
				cb_Foam.Clear();
			}
			if (cb_Normals != null)
			{
				cb_Normals.Clear();
			}
			Shader.DisableKeyword("USINGWATERPROJECTORS");
			return;
		}
		Shader.EnableKeyword("USINGWATERPROJECTORS");
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Matrix4x4 worldToProjectMatrix = projectionMatrix * worldToCameraMatrix;
		int pixelWidth = cam.pixelWidth;
		int pixelHeight = cam.pixelHeight;
		GeomUtil.CalculateFrustumPlanes(frustumPlanes, worldToProjectMatrix);
		int num = Mathf.FloorToInt(pixelWidth / (int)FoamBufferResolution);
		int height = Mathf.FloorToInt(pixelHeight / (int)FoamBufferResolution);
		if (!ProjectedFoam)
		{
			ProjectedFoam = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		else if (ProjectedFoam.width != num)
		{
			Object.DestroyImmediate(ProjectedFoam);
			ProjectedFoam = new RenderTexture(num, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		GL.PushMatrix();
		GL.modelview = worldToCameraMatrix;
		GL.LoadProjectionMatrix(projectionMatrix);
		cb_Foam.Clear();
		cb_Foam.SetRenderTarget(ProjectedFoam);
		cb_Foam.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f), 1f);
		drawnFoamProjectors = 0;
		for (int i = 0; i < count; i++)
		{
			LuxWater_Projector luxWater_Projector = LuxWater_Projector.FoamProjectors[i];
			tempBounds = luxWater_Projector.m_Rend.bounds;
			if (GeometryUtility.TestPlanesAABB(frustumPlanes, tempBounds))
			{
				cb_Foam.DrawRenderer(luxWater_Projector.m_Rend, luxWater_Projector.m_Mat);
				drawnFoamProjectors++;
			}
		}
		Graphics.ExecuteCommandBuffer(cb_Foam);
		Shader.SetGlobalTexture(_LuxWater_FoamOverlayPID, ProjectedFoam);
		num = Mathf.FloorToInt(pixelWidth / (int)NormalBufferResolution);
		height = Mathf.FloorToInt(pixelHeight / (int)NormalBufferResolution);
		if (!ProjectedNormals)
		{
			ProjectedNormals = new RenderTexture(num, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		}
		else if (ProjectedNormals.width != num)
		{
			Object.DestroyImmediate(ProjectedNormals);
			ProjectedNormals = new RenderTexture(num, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		}
		cb_Normals.Clear();
		cb_Normals.SetRenderTarget(ProjectedNormals);
		cb_Normals.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f), 1f);
		drawnNormalProjectors = 0;
		for (int j = 0; j < count2; j++)
		{
			LuxWater_Projector luxWater_Projector2 = LuxWater_Projector.NormalProjectors[j];
			tempBounds = luxWater_Projector2.m_Rend.bounds;
			if (GeometryUtility.TestPlanesAABB(frustumPlanes, tempBounds))
			{
				cb_Normals.DrawRenderer(luxWater_Projector2.m_Rend, luxWater_Projector2.m_Mat);
				drawnNormalProjectors++;
			}
		}
		Graphics.ExecuteCommandBuffer(cb_Normals);
		Shader.SetGlobalTexture(_LuxWater_NormalOverlayPID, ProjectedNormals);
		GL.PopMatrix();
	}

	private void OnDrawGizmos()
	{
		Camera component = GetComponent<Camera>();
		int num = 0;
		int num2 = (int)(component.aspect * 128f);
		if (DebugMat == null)
		{
			DebugMat = new Material(Shader.Find("Hidden/LuxWater_Debug"));
		}
		if (DebugNormalMat == null)
		{
			DebugNormalMat = new Material(Shader.Find("Hidden/LuxWater_DebugNormals"));
		}
		if (DebugFoamBuffer)
		{
			if (ProjectedFoam == null)
			{
				return;
			}
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, Screen.width, Screen.height, 0f);
			Graphics.DrawTexture(new Rect(num, 0f, num2, 128f), ProjectedFoam, DebugMat);
			GL.PopMatrix();
			num = num2;
		}
		if (DebugNormalBuffer && !(ProjectedNormals == null))
		{
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, Screen.width, Screen.height, 0f);
			Graphics.DrawTexture(new Rect(num, 0f, num2, 128f), ProjectedNormals, DebugNormalMat);
			GL.PopMatrix();
		}
	}

	private void OnGUI()
	{
		if (DebugStats)
		{
			int count = LuxWater_Projector.FoamProjectors.Count;
			int count2 = LuxWater_Projector.NormalProjectors.Count;
			TextAnchor alignment = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.Label(new Rect(10f, 0f, 300f, 40f), "Foam Projectors   [Registered] " + count + "  [Drawn] " + drawnFoamProjectors);
			GUI.Label(new Rect(10f, 18f, 300f, 40f), "Normal Projectors [Registered] " + count2 + "  [Drawn] " + drawnNormalProjectors);
			GUI.skin.label.alignment = alignment;
		}
	}
}
