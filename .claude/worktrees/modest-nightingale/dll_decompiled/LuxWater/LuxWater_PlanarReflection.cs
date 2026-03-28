using System.Collections.Generic;
using UnityEngine;

namespace LuxWater;

[ExecuteInEditMode]
public class LuxWater_PlanarReflection : MonoBehaviour
{
	public enum ReflectionResolution
	{
		Full = 1,
		Half = 2,
		Quarter = 4,
		Eighth = 8
	}

	public enum NumberOfShadowCascades
	{
		One = 1,
		Two = 2,
		Four = 4
	}

	[Space(6f)]
	[LuxWater_HelpBtn("h.5c3jy4qfh163")]
	public bool UpdateSceneView = true;

	[Space(5f)]
	public bool isMaster;

	public Material[] WaterMaterials;

	[Space(5f)]
	public LayerMask reflectionMask = -1;

	public ReflectionResolution Resolution = ReflectionResolution.Half;

	public Color clearColor = Color.black;

	public bool reflectSkybox = true;

	[Space(5f)]
	public bool disablePixelLights;

	[Space(5f)]
	public bool renderShadows = true;

	public float shadowDistance;

	public NumberOfShadowCascades ShadowCascades = NumberOfShadowCascades.One;

	[Space(5f)]
	public float WaterSurfaceOffset;

	public float clipPlaneOffset = 0.07f;

	private string reflectionSampler = "_LuxWater_ReflectionTex";

	private Vector3 m_Oldpos;

	private Camera m_ReflectionCamera;

	private Material m_SharedMaterial;

	private Dictionary<Camera, bool> m_HelperCameras;

	private RenderTexture m_reflectionMap;

	private void OnEnable()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Water");
		if (GetComponent<Renderer>() != null)
		{
			m_SharedMaterial = GetComponent<Renderer>().sharedMaterial;
		}
	}

	private void OnDisable()
	{
		if (m_ReflectionCamera != null)
		{
			Object.DestroyImmediate(m_ReflectionCamera.targetTexture);
			Object.DestroyImmediate(m_ReflectionCamera);
		}
		if (m_HelperCameras != null)
		{
			m_HelperCameras.Clear();
		}
	}

	private Camera CreateReflectionCameraFor(Camera cam)
	{
		string text = base.gameObject.name + "Reflection" + cam.name;
		GameObject gameObject = GameObject.Find(text);
		if (!gameObject)
		{
			gameObject = new GameObject(text, typeof(Camera));
			gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!gameObject.GetComponent(typeof(Camera)))
		{
			gameObject.AddComponent(typeof(Camera));
		}
		Camera component = gameObject.GetComponent<Camera>();
		component.backgroundColor = clearColor;
		component.clearFlags = (reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
		SetStandardCameraParameter(component, reflectionMask);
		if (!component.targetTexture)
		{
			component.targetTexture = CreateTextureFor(cam);
		}
		return component;
	}

	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.cullingMask = (int)mask & ~(1 << LayerMask.NameToLayer("Water"));
		cam.backgroundColor = Color.black;
		cam.enabled = false;
	}

	private RenderTexture CreateTextureFor(Camera cam)
	{
		int width = Mathf.FloorToInt(cam.pixelWidth / (int)Resolution);
		int height = Mathf.FloorToInt(cam.pixelHeight / (int)Resolution);
		return new RenderTexture(width, height, 24)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	public void RenderHelpCameras(Camera currentCam)
	{
		if (m_HelperCameras == null)
		{
			m_HelperCameras = new Dictionary<Camera, bool>();
		}
		if (!m_HelperCameras.ContainsKey(currentCam))
		{
			m_HelperCameras.Add(currentCam, value: false);
		}
		if (!m_HelperCameras[currentCam] && !currentCam.name.Contains("Reflection Probes"))
		{
			if (!m_ReflectionCamera)
			{
				m_ReflectionCamera = CreateReflectionCameraFor(currentCam);
			}
			RenderReflectionFor(currentCam, m_ReflectionCamera);
			m_HelperCameras[currentCam] = true;
		}
	}

	public void LateUpdate()
	{
		if (m_HelperCameras != null)
		{
			m_HelperCameras.Clear();
		}
	}

	public void WaterTileBeingRendered(Transform tr, Camera currentCam)
	{
		RenderHelpCameras(currentCam);
		if ((bool)m_ReflectionCamera && (bool)m_SharedMaterial)
		{
			m_SharedMaterial.SetTexture(reflectionSampler, m_ReflectionCamera.targetTexture);
		}
	}

	public void OnWillRenderObject()
	{
		WaterTileBeingRendered(base.transform, Camera.current);
	}

	private void RenderReflectionFor(Camera cam, Camera reflectCamera)
	{
		if (!reflectCamera || ((bool)m_SharedMaterial && !m_SharedMaterial.HasProperty(reflectionSampler)))
		{
			return;
		}
		reflectCamera.cullingMask = (int)reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));
		SaneCameraSettings(reflectCamera);
		reflectCamera.backgroundColor = clearColor;
		reflectCamera.clearFlags = (reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
		GL.invertCulling = true;
		Transform transform = base.transform;
		Vector3 eulerAngles = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
		reflectCamera.transform.position = cam.transform.position;
		reflectCamera.orthographic = cam.orthographic;
		reflectCamera.orthographicSize = cam.orthographicSize;
		Vector3 position = transform.transform.position;
		position.y = transform.position.y + WaterSurfaceOffset;
		Vector3 up = transform.transform.up;
		float w = 0f - Vector3.Dot(up, position) - clipPlaneOffset;
		Vector4 plane = new Vector4(up.x, up.y, up.z, w);
		Matrix4x4 zero = Matrix4x4.zero;
		zero = CalculateReflectionMatrix(zero, plane);
		m_Oldpos = cam.transform.position;
		Vector3 position2 = zero.MultiplyPoint(m_Oldpos);
		reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * zero;
		Vector4 clipPlane = CameraSpacePlane(reflectCamera, position, up, 1f);
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		projectionMatrix = CalculateObliqueMatrix(projectionMatrix, clipPlane);
		reflectCamera.projectionMatrix = projectionMatrix;
		reflectCamera.transform.position = position2;
		Vector3 eulerAngles2 = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
		int pixelLightCount = QualitySettings.pixelLightCount;
		if (disablePixelLights)
		{
			QualitySettings.pixelLightCount = 0;
		}
		float num = QualitySettings.shadowDistance;
		int shadowCascades = QualitySettings.shadowCascades;
		if (!renderShadows)
		{
			QualitySettings.shadowDistance = 0f;
		}
		else if (shadowDistance > 0f)
		{
			QualitySettings.shadowDistance = shadowDistance;
		}
		QualitySettings.shadowCascades = (int)ShadowCascades;
		reflectCamera.Render();
		GL.invertCulling = false;
		if (disablePixelLights)
		{
			QualitySettings.pixelLightCount = pixelLightCount;
		}
		if (!renderShadows || shadowDistance > 0f)
		{
			QualitySettings.shadowDistance = num;
		}
		QualitySettings.shadowCascades = shadowCascades;
		if (isMaster)
		{
			for (int i = 0; i < WaterMaterials.Length; i++)
			{
				WaterMaterials[i].SetTexture(reflectionSampler, reflectCamera.targetTexture);
			}
		}
	}

	private void SaneCameraSettings(Camera helperCam)
	{
		helperCam.depthTextureMode = DepthTextureMode.None;
		helperCam.backgroundColor = Color.black;
		helperCam.clearFlags = CameraClearFlags.Color;
		helperCam.renderingPath = RenderingPath.Forward;
	}

	private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(Sgn(clipPlane.x), Sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
		return projection;
	}

	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	private static float Sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * clipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}
}
