using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater;

[RequireComponent(typeof(Camera))]
public class LuxWater_UnderWaterRendering : MonoBehaviour
{
	public static LuxWater_UnderWaterRendering instance;

	[Space(6f)]
	[LuxWater_HelpBtn("h.d0q6uguuxpy")]
	public Transform Sun;

	[Space(4f)]
	public bool FindSunOnEnable;

	public string SunGoName = "";

	public string SunTagName = "";

	private Light SunLight;

	[Space(2f)]
	[Header("Deep Water Lighting")]
	[Space(4f)]
	public bool EnableDeepwaterLighting;

	public float DefaultWaterSurfacePosition;

	public float DirectionalLightingFadeRange = 64f;

	public float FogLightingFadeRange = 64f;

	[Space(2f)]
	[Header("Advanced Deferred Fog")]
	[Space(4f)]
	public bool EnableAdvancedDeferredFog;

	public float FogDepthShift = 1f;

	public float FogEdgeBlending = 0.125f;

	[NonSerialized]
	[Space(8f)]
	public int activeWaterVolume = -1;

	[NonSerialized]
	public List<Camera> activeWaterVolumeCameras = new List<Camera>();

	[NonSerialized]
	public float activeGridSize;

	[NonSerialized]
	public float WaterSurfacePos;

	[NonSerialized]
	[Space(8f)]
	public List<int> RegisteredWaterVolumesIDs = new List<int>();

	[NonSerialized]
	public List<LuxWater_WaterVolume> RegisteredWaterVolumes = new List<LuxWater_WaterVolume>();

	private List<Mesh> WaterMeshes = new List<Mesh>();

	private List<Transform> WaterTransforms = new List<Transform>();

	private List<Material> WaterMaterials = new List<Material>();

	private List<bool> WaterIsOnScreen = new List<bool>();

	private List<bool> WaterUsesSlidingVolume = new List<bool>();

	private RenderTexture UnderWaterMask;

	[Space(2f)]
	[Header("Managed transparent Materials")]
	[Space(4f)]
	public List<Material> m_aboveWatersurface = new List<Material>();

	public List<Material> m_belowWatersurface = new List<Material>();

	[Space(2f)]
	[Header("Optimize")]
	[Space(4f)]
	public ShaderVariantCollection PrewarmedShaders;

	public int ListCapacity = 10;

	[Space(2f)]
	[Header("Debug")]
	[Space(4f)]
	public bool enableDebug;

	[Space(8f)]
	private Material mat;

	private Material blurMaterial;

	private Material blitMaterial;

	private Camera cam;

	private bool UnderwaterIsSetUp;

	private Transform camTransform;

	private Matrix4x4 frustumCornersArray = Matrix4x4.identity;

	private SphericalHarmonicsL2 ambientProbe;

	private Vector3[] directions = new Vector3[1]
	{
		new Vector3(0f, 1f, 0f)
	};

	private Color[] AmbientLightingSamples = new Color[1];

	private bool DoUnderWaterRendering;

	private Matrix4x4 camProj;

	private Vector3[] frustumCorners = new Vector3[4];

	private float Projection;

	private bool islinear;

	private Matrix4x4 WatervolumeMatrix;

	private int UnderWaterMaskPID;

	private int Lux_FrustumCornersWSPID;

	private int Lux_CameraWSPID;

	private int GerstnerEnabledPID;

	private int LuxWaterMask_GerstnerVertexIntensityPID;

	private int GerstnerVertexIntensityPID;

	private int LuxWaterMask_GAmplitudePID;

	private int GAmplitudePID;

	private int LuxWaterMask_GFinalFrequencyPID;

	private int GFinalFrequencyPID;

	private int LuxWaterMask_GSteepnessPID;

	private int GSteepnessPID;

	private int LuxWaterMask_GFinalSpeedPID;

	private int GFinalSpeedPID;

	private int LuxWaterMask_GDirectionABPID;

	private int GDirectionABPID;

	private int LuxWaterMask_GDirectionCDPID;

	private int GDirectionCDPID;

	private int LuxWaterMask_GerstnerSecondaryWaves;

	private int GerstnerSecondaryWaves;

	private int Lux_UnderWaterAmbientSkyLightPID;

	private int Lux_UnderWaterSunColorPID;

	private int Lux_UnderWaterSunDirPID;

	private int Lux_UnderWaterSunDirViewSpacePID;

	private int Lux_EdgeLengthPID;

	private int Lux_CausticsEnabledPID;

	private int Lux_CausticModePID;

	private int Lux_UnderWaterFogColorPID;

	private int Lux_UnderWaterFogDensityPID;

	private int Lux_UnderWaterFogAbsorptionCancellationPID;

	private int Lux_UnderWaterAbsorptionHeightPID;

	private int Lux_UnderWaterAbsorptionMaxHeightPID;

	private int Lux_MaxDirLightDepthPID;

	private int Lux_MaxFogLightDepthPID;

	private int Lux_UnderWaterAbsorptionDepthPID;

	private int Lux_UnderWaterAbsorptionColorStrengthPID;

	private int Lux_UnderWaterAbsorptionStrengthPID;

	private int Lux_UnderWaterUnderwaterScatteringPowerPID;

	private int Lux_UnderWaterUnderwaterScatteringIntensityPID;

	private int Lux_UnderWaterUnderwaterScatteringColorPID;

	private int Lux_UnderWaterUnderwaterScatteringOcclusionPID;

	private int Lux_UnderWaterCausticsPID;

	private int Lux_UnderWaterDeferredFogParams;

	private int CausticTexPID;

	private void OnEnable()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			instance = this;
		}
		mat = new Material(Shader.Find("Hidden/Lux Water/WaterMask"));
		blurMaterial = new Material(Shader.Find("Hidden/Lux Water/BlurEffectConeTap"));
		blitMaterial = new Material(Shader.Find("Hidden/Lux Water/UnderWaterPost"));
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		cam.depthTextureMode |= DepthTextureMode.Depth;
		camTransform = cam.transform;
		if (FindSunOnEnable)
		{
			if (SunGoName != "")
			{
				Sun = GameObject.Find(SunGoName).transform;
			}
			else if (SunTagName != "")
			{
				Sun = GameObject.FindWithTag(SunTagName).transform;
			}
		}
		if (SystemInfo.usesReversedZBuffer)
		{
			Projection = -1f;
		}
		else
		{
			Projection = 1f;
		}
		UnderWaterMaskPID = Shader.PropertyToID("_UnderWaterMask");
		Lux_FrustumCornersWSPID = Shader.PropertyToID("_Lux_FrustumCornersWS");
		Lux_CameraWSPID = Shader.PropertyToID("_Lux_CameraWS");
		GerstnerEnabledPID = Shader.PropertyToID("_GerstnerEnabled");
		LuxWaterMask_GerstnerVertexIntensityPID = Shader.PropertyToID("_LuxWaterMask_GerstnerVertexIntensity");
		GerstnerVertexIntensityPID = Shader.PropertyToID("_GerstnerVertexIntensity");
		LuxWaterMask_GAmplitudePID = Shader.PropertyToID("_LuxWaterMask_GAmplitude");
		GAmplitudePID = Shader.PropertyToID("_GAmplitude");
		LuxWaterMask_GFinalFrequencyPID = Shader.PropertyToID("_LuxWaterMask_GFinalFrequency");
		GFinalFrequencyPID = Shader.PropertyToID("_GFinalFrequency");
		LuxWaterMask_GSteepnessPID = Shader.PropertyToID("_LuxWaterMask_GSteepness");
		GSteepnessPID = Shader.PropertyToID("_GSteepness");
		LuxWaterMask_GFinalSpeedPID = Shader.PropertyToID("_LuxWaterMask_GFinalSpeed");
		GFinalSpeedPID = Shader.PropertyToID("_GFinalSpeed");
		LuxWaterMask_GDirectionABPID = Shader.PropertyToID("_LuxWaterMask_GDirectionAB");
		GDirectionABPID = Shader.PropertyToID("_GDirectionAB");
		LuxWaterMask_GDirectionCDPID = Shader.PropertyToID("_LuxWaterMask_GDirectionCD");
		GDirectionCDPID = Shader.PropertyToID("_GDirectionCD");
		LuxWaterMask_GerstnerSecondaryWaves = Shader.PropertyToID("_LuxWaterMask_GerstnerSecondaryWaves");
		GerstnerSecondaryWaves = Shader.PropertyToID("_GerstnerSecondaryWaves");
		Lux_UnderWaterAmbientSkyLightPID = Shader.PropertyToID("_Lux_UnderWaterAmbientSkyLight");
		Lux_UnderWaterSunColorPID = Shader.PropertyToID("_Lux_UnderWaterSunColor");
		Lux_UnderWaterSunDirPID = Shader.PropertyToID("_Lux_UnderWaterSunDir");
		Lux_UnderWaterSunDirViewSpacePID = Shader.PropertyToID("_Lux_UnderWaterSunDirViewSpace");
		Lux_EdgeLengthPID = Shader.PropertyToID("_LuxWater_EdgeLength");
		Lux_MaxDirLightDepthPID = Shader.PropertyToID("_MaxDirLightDepth");
		Lux_MaxFogLightDepthPID = Shader.PropertyToID("_MaxFogLightDepth");
		Lux_CausticsEnabledPID = Shader.PropertyToID("_CausticsEnabled");
		Lux_CausticModePID = Shader.PropertyToID("_CausticMode");
		Lux_UnderWaterFogColorPID = Shader.PropertyToID("_Lux_UnderWaterFogColor");
		Lux_UnderWaterFogDensityPID = Shader.PropertyToID("_Lux_UnderWaterFogDensity");
		Lux_UnderWaterFogAbsorptionCancellationPID = Shader.PropertyToID("_Lux_UnderWaterFogAbsorptionCancellation");
		Lux_UnderWaterAbsorptionHeightPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionHeight");
		Lux_UnderWaterAbsorptionMaxHeightPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionMaxHeight");
		Lux_UnderWaterAbsorptionDepthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionDepth");
		Lux_UnderWaterAbsorptionColorStrengthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionColorStrength");
		Lux_UnderWaterAbsorptionStrengthPID = Shader.PropertyToID("_Lux_UnderWaterAbsorptionStrength");
		Lux_UnderWaterUnderwaterScatteringPowerPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringPower");
		Lux_UnderWaterUnderwaterScatteringIntensityPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringIntensity");
		Lux_UnderWaterUnderwaterScatteringColorPID = Shader.PropertyToID("_Lux_UnderWaterUnderwaterScatteringColor");
		Lux_UnderWaterUnderwaterScatteringOcclusionPID = Shader.PropertyToID("_Lux_UnderwaterScatteringOcclusion");
		Lux_UnderWaterCausticsPID = Shader.PropertyToID("_Lux_UnderWaterCaustics");
		Lux_UnderWaterDeferredFogParams = Shader.PropertyToID("_LuxUnderWaterDeferredFogParams");
		CausticTexPID = Shader.PropertyToID("_CausticTex");
		islinear = QualitySettings.desiredColorSpace == ColorSpace.Linear;
		if (PrewarmedShaders != null && !PrewarmedShaders.isWarmedUp)
		{
			PrewarmedShaders.WarmUp();
		}
		if (Sun != null)
		{
			SunLight = Sun.GetComponent<Light>();
		}
		RegisteredWaterVolumesIDs.Capacity = ListCapacity;
		RegisteredWaterVolumes.Capacity = ListCapacity;
		WaterMeshes.Capacity = ListCapacity;
		WaterTransforms.Capacity = ListCapacity;
		WaterMaterials.Capacity = ListCapacity;
		WaterIsOnScreen.Capacity = ListCapacity;
		WaterUsesSlidingVolume.Capacity = ListCapacity;
		activeWaterVolumeCameras.Capacity = 2;
		SetDeepwaterLighting();
		SetDeferredFogParams();
	}

	private void CleanUp()
	{
		instance = null;
		if (UnderWaterMask != null)
		{
			UnderWaterMask.Release();
			UnityEngine.Object.Destroy(UnderWaterMask);
		}
		if ((bool)mat)
		{
			UnityEngine.Object.Destroy(mat);
		}
		if ((bool)blurMaterial)
		{
			UnityEngine.Object.Destroy(blurMaterial);
		}
		if ((bool)blitMaterial)
		{
			UnityEngine.Object.Destroy(blitMaterial);
		}
		Shader.DisableKeyword("LUXWATER_DEEPWATERLIGHTING");
		Shader.DisableKeyword("LUXWATER_DEFERREDFOG");
	}

	private void OnDisable()
	{
		CleanUp();
	}

	private void OnDestroy()
	{
		CleanUp();
	}

	private void OnValidate()
	{
		SetDeepwaterLighting();
		SetDeferredFogParams();
	}

	public void SetDeferredFogParams()
	{
		if (EnableAdvancedDeferredFog)
		{
			Shader.EnableKeyword("LUXWATER_DEFERREDFOG");
			Vector4 value = new Vector4(DoUnderWaterRendering ? 1 : 0, FogDepthShift, FogEdgeBlending, 0f);
			Shader.SetGlobalVector(Lux_UnderWaterDeferredFogParams, value);
		}
		else
		{
			Shader.DisableKeyword("LUXWATER_DEFERREDFOG");
		}
	}

	public void SetDeepwaterLighting()
	{
		if (EnableDeepwaterLighting)
		{
			Shader.EnableKeyword("LUXWATER_DEEPWATERLIGHTING");
			if (activeWaterVolume != -1)
			{
				Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", WaterSurfacePos);
			}
			else
			{
				Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", DefaultWaterSurfacePosition);
			}
			Shader.SetGlobalFloat("_Lux_UnderWaterDirLightingDepth", DirectionalLightingFadeRange);
			Shader.SetGlobalFloat("_Lux_UnderWaterFogLightingDepth", FogLightingFadeRange);
		}
		else
		{
			Shader.DisableKeyword("LUXWATER_DEEPWATERLIGHTING");
		}
	}

	public void RegisterWaterVolume(LuxWater_WaterVolume item, int ID, bool visible, bool SlidingVolume)
	{
		RegisteredWaterVolumesIDs.Add(ID);
		RegisteredWaterVolumes.Add(item);
		WaterMeshes.Add(item.WaterVolumeMesh);
		WaterMaterials.Add(item.transform.GetComponent<Renderer>().sharedMaterial);
		WaterTransforms.Add(item.transform);
		WaterIsOnScreen.Add(visible);
		WaterUsesSlidingVolume.Add(SlidingVolume);
		int num = WaterMaterials.Count - 1;
		Shader.SetGlobalTexture(Lux_UnderWaterCausticsPID, WaterMaterials[num].GetTexture(CausticTexPID));
		SetGerstnerWaves(num);
	}

	public void DeRegisterWaterVolume(LuxWater_WaterVolume item, int ID)
	{
		int num = RegisteredWaterVolumesIDs.IndexOf(ID);
		if (activeWaterVolume == num)
		{
			activeWaterVolume = -1;
		}
		RegisteredWaterVolumesIDs.RemoveAt(num);
		RegisteredWaterVolumes.RemoveAt(num);
		WaterMeshes.RemoveAt(num);
		WaterMaterials.RemoveAt(num);
		WaterTransforms.RemoveAt(num);
		WaterIsOnScreen.RemoveAt(num);
		WaterUsesSlidingVolume.RemoveAt(num);
	}

	public void SetWaterVisible(int ID)
	{
		int index = RegisteredWaterVolumesIDs.IndexOf(ID);
		WaterIsOnScreen[index] = true;
	}

	public void SetWaterInvisible(int ID)
	{
		int index = RegisteredWaterVolumesIDs.IndexOf(ID);
		WaterIsOnScreen[index] = false;
	}

	public void EnteredWaterVolume(LuxWater_WaterVolume item, int ID, Camera triggerCam, float GridSize)
	{
		DoUnderWaterRendering = true;
		int num = RegisteredWaterVolumesIDs.IndexOf(ID);
		if (num != activeWaterVolume)
		{
			activeWaterVolume = num;
			activeGridSize = GridSize;
			WaterSurfacePos = WaterTransforms[activeWaterVolume].position.y;
			for (int i = 0; i < m_aboveWatersurface.Count; i++)
			{
				m_aboveWatersurface[i].renderQueue = 2997;
			}
			for (int j = 0; j < m_belowWatersurface.Count; j++)
			{
				m_belowWatersurface[j].renderQueue = 3001;
			}
		}
		if (!activeWaterVolumeCameras.Contains(triggerCam))
		{
			activeWaterVolumeCameras.Add(triggerCam);
		}
	}

	public void LeftWaterVolume(LuxWater_WaterVolume item, int ID, Camera triggerCam)
	{
		DoUnderWaterRendering = false;
		int num = RegisteredWaterVolumesIDs.IndexOf(ID);
		if (activeWaterVolume == num)
		{
			activeWaterVolume = -1;
			for (int i = 0; i < m_aboveWatersurface.Count; i++)
			{
				m_aboveWatersurface[i].renderQueue = 3000;
			}
			for (int j = 0; j < m_belowWatersurface.Count; j++)
			{
				m_belowWatersurface[j].renderQueue = 2997;
			}
		}
		if (activeWaterVolumeCameras.Contains(triggerCam))
		{
			activeWaterVolumeCameras.Remove(triggerCam);
		}
	}

	private void OnPreCull()
	{
		SetDeferredFogParams();
		RenderWaterMask(cam, SecondaryCameraRendering: false);
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		RenderUnderWater(src, dest, cam, SecondaryCameraRendering: false);
	}

	public void SetGerstnerWaves(int index)
	{
		if (WaterMaterials[index].GetFloat(GerstnerEnabledPID) == 1f)
		{
			mat.EnableKeyword("GERSTNERENABLED");
			mat.SetVector(LuxWaterMask_GerstnerVertexIntensityPID, WaterMaterials[index].GetVector(GerstnerVertexIntensityPID));
			mat.SetVector(LuxWaterMask_GAmplitudePID, WaterMaterials[index].GetVector(GAmplitudePID));
			mat.SetVector(LuxWaterMask_GFinalFrequencyPID, WaterMaterials[index].GetVector(GFinalFrequencyPID));
			mat.SetVector(LuxWaterMask_GSteepnessPID, WaterMaterials[index].GetVector(GSteepnessPID));
			mat.SetVector(LuxWaterMask_GFinalSpeedPID, WaterMaterials[index].GetVector(GFinalSpeedPID));
			mat.SetVector(LuxWaterMask_GDirectionABPID, WaterMaterials[index].GetVector(GDirectionABPID));
			mat.SetVector(LuxWaterMask_GDirectionCDPID, WaterMaterials[index].GetVector(GDirectionCDPID));
			mat.SetVector(LuxWaterMask_GerstnerSecondaryWaves, WaterMaterials[index].GetVector(GerstnerSecondaryWaves));
		}
		else
		{
			mat.DisableKeyword("GERSTNERENABLED");
		}
	}

	public void RenderWaterMask(Camera currentCamera, bool SecondaryCameraRendering)
	{
		Shader.SetGlobalFloat("_Lux_Time", Time.timeSinceLevelLoad);
		currentCamera.depthTextureMode |= DepthTextureMode.Depth;
		camTransform = currentCamera.transform;
		if (!UnderWaterMask)
		{
			UnderWaterMask = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		else if (UnderWaterMask.width != currentCamera.pixelWidth && !SecondaryCameraRendering)
		{
			UnderWaterMask = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}
		Shader.SetGlobalTexture(UnderWaterMaskPID, UnderWaterMask);
		Graphics.SetRenderTarget(UnderWaterMask);
		currentCamera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), currentCamera.farClipPlane, currentCamera.stereoActiveEye, frustumCorners);
		Vector3 vector = camTransform.TransformVector(frustumCorners[0]);
		Vector3 vector2 = camTransform.TransformVector(frustumCorners[1]);
		Vector3 vector3 = camTransform.TransformVector(frustumCorners[2]);
		Vector3 vector4 = camTransform.TransformVector(frustumCorners[3]);
		frustumCornersArray.SetRow(0, vector);
		frustumCornersArray.SetRow(1, vector4);
		frustumCornersArray.SetRow(2, vector2);
		frustumCornersArray.SetRow(3, vector3);
		Shader.SetGlobalMatrix(Lux_FrustumCornersWSPID, frustumCornersArray);
		Shader.SetGlobalVector(Lux_CameraWSPID, camTransform.position);
		ambientProbe = RenderSettings.ambientProbe;
		ambientProbe.Evaluate(directions, AmbientLightingSamples);
		if (islinear)
		{
			Shader.SetGlobalColor(Lux_UnderWaterAmbientSkyLightPID, (AmbientLightingSamples[0] * RenderSettings.ambientIntensity).linear);
		}
		else
		{
			Shader.SetGlobalColor(Lux_UnderWaterAmbientSkyLightPID, AmbientLightingSamples[0] * RenderSettings.ambientIntensity);
		}
		if (!activeWaterVolumeCameras.Contains(currentCamera))
		{
			_ = EnableAdvancedDeferredFog;
		}
		if (activeWaterVolume > -1)
		{
			Shader.EnableKeyword("LUXWATERENABLED");
			if (!EnableDeepwaterLighting)
			{
				Shader.SetGlobalFloat("_Lux_UnderWaterDirLightingDepth", WaterMaterials[activeWaterVolume].GetFloat(Lux_MaxDirLightDepthPID));
				Shader.SetGlobalFloat("_Lux_UnderWaterFogLightingDepth", WaterMaterials[activeWaterVolume].GetFloat(Lux_MaxFogLightDepthPID));
			}
			Shader.SetGlobalFloat("_Lux_UnderWaterWaterSurfacePos", WaterSurfacePos);
		}
		else
		{
			Shader.DisableKeyword("LUXWATERENABLED");
		}
		GL.PushMatrix();
		GL.Clear(clearDepth: true, clearColor: true, Color.black, 1f);
		camProj = currentCamera.projectionMatrix;
		GL.LoadProjectionMatrix(camProj);
		Shader.SetGlobalVector("_WorldSpaceCameraPos", camTransform.position);
		Shader.SetGlobalVector("_ProjectionParams", new Vector4(Projection, currentCamera.nearClipPlane, currentCamera.farClipPlane, 1f / currentCamera.farClipPlane));
		Shader.SetGlobalVector("_ScreenParams", new Vector4(currentCamera.pixelWidth, currentCamera.pixelHeight, 1f + 1f / (float)currentCamera.pixelWidth, 1f + 1f / (float)currentCamera.pixelHeight));
		for (int i = 0; i < RegisteredWaterVolumes.Count; i++)
		{
			if ((!WaterIsOnScreen[i] && i != activeWaterVolume) || (!EnableAdvancedDeferredFog && i != activeWaterVolume))
			{
				continue;
			}
			WatervolumeMatrix = WaterTransforms[i].localToWorldMatrix;
			if (WaterUsesSlidingVolume[i])
			{
				Vector3 position = camTransform.position;
				Vector4 column = WatervolumeMatrix.GetColumn(3);
				Vector3 lossyScale = WaterTransforms[i].lossyScale;
				Vector2 vector5 = new Vector2(activeGridSize * lossyScale.x, activeGridSize * lossyScale.z);
				float num = (float)Math.Round(position.x / vector5.x);
				float num2 = vector5.x * num;
				num = (float)Math.Round(position.z / vector5.y);
				float num3 = vector5.y * num;
				column.x = num2 + column.x % vector5.x;
				column.z = num3 + column.z % vector5.y;
				WatervolumeMatrix.SetColumn(3, column);
			}
			Material material = WaterMaterials[i];
			if (material.GetFloat(GerstnerEnabledPID) == 1f)
			{
				mat.EnableKeyword("GERSTNERENABLED");
				mat.SetVector(LuxWaterMask_GerstnerVertexIntensityPID, material.GetVector(GerstnerVertexIntensityPID));
				mat.SetVector(LuxWaterMask_GAmplitudePID, material.GetVector(GAmplitudePID));
				mat.SetVector(LuxWaterMask_GFinalFrequencyPID, material.GetVector(GFinalFrequencyPID));
				mat.SetVector(LuxWaterMask_GSteepnessPID, material.GetVector(GSteepnessPID));
				mat.SetVector(LuxWaterMask_GFinalSpeedPID, material.GetVector(GFinalSpeedPID));
				mat.SetVector(LuxWaterMask_GDirectionABPID, material.GetVector(GDirectionABPID));
				mat.SetVector(LuxWaterMask_GDirectionCDPID, material.GetVector(GDirectionCDPID));
				mat.SetVector(LuxWaterMask_GerstnerSecondaryWaves, material.GetVector(GerstnerSecondaryWaves));
			}
			else
			{
				mat.DisableKeyword("GERSTNERENABLED");
			}
			bool flag = material.HasProperty(Lux_EdgeLengthPID) && SystemInfo.graphicsShaderLevel >= 46;
			if (flag)
			{
				mat.SetFloat(Lux_EdgeLengthPID, material.GetFloat(Lux_EdgeLengthPID));
			}
			if (i == activeWaterVolume && activeWaterVolumeCameras.Contains(currentCamera))
			{
				if (WaterUsesSlidingVolume[i] && flag)
				{
					mat.SetPass(5);
				}
				else
				{
					mat.SetPass(0);
				}
				Graphics.DrawMeshNow(WaterMeshes[i], WatervolumeMatrix, 0);
			}
			if ((i != activeWaterVolume || !activeWaterVolumeCameras.Contains(currentCamera)) && !EnableAdvancedDeferredFog)
			{
				continue;
			}
			if (flag)
			{
				if (i == activeWaterVolume)
				{
					mat.SetPass(3);
				}
				else
				{
					mat.SetPass(4);
				}
			}
			else if (i == activeWaterVolume)
			{
				mat.SetPass(1);
			}
			else
			{
				mat.SetPass(2);
			}
			Graphics.DrawMeshNow(WaterMeshes[i], WatervolumeMatrix, 1);
		}
		GL.PopMatrix();
	}

	public void RenderUnderWater(RenderTexture src, RenderTexture dest, Camera currentCamera, bool SecondaryCameraRendering)
	{
		if (activeWaterVolumeCameras.Contains(currentCamera))
		{
			if (DoUnderWaterRendering && activeWaterVolume > -1)
			{
				if (!UnderwaterIsSetUp)
				{
					if ((bool)Sun)
					{
						Vector3 vector = -Sun.forward;
						Color color = SunLight.color * SunLight.intensity;
						if (islinear)
						{
							color = color.linear;
						}
						Shader.SetGlobalColor(Lux_UnderWaterSunColorPID, color * Mathf.Clamp01(Vector3.Dot(vector, Vector3.up)));
						Shader.SetGlobalVector(Lux_UnderWaterSunDirPID, -vector);
						Shader.SetGlobalVector(Lux_UnderWaterSunDirViewSpacePID, currentCamera.WorldToViewportPoint(currentCamera.transform.position - vector * 1000f));
					}
					if (WaterMaterials[activeWaterVolume].GetFloat(Lux_CausticsEnabledPID) == 1f)
					{
						blitMaterial.EnableKeyword("GEOM_TYPE_FROND");
						if (WaterMaterials[activeWaterVolume].GetFloat(Lux_CausticModePID) == 1f)
						{
							blitMaterial.EnableKeyword("GEOM_TYPE_LEAF");
						}
						else
						{
							blitMaterial.DisableKeyword("GEOM_TYPE_LEAF");
						}
					}
					else
					{
						blitMaterial.DisableKeyword("GEOM_TYPE_FROND");
					}
					if (islinear)
					{
						Shader.SetGlobalColor(Lux_UnderWaterFogColorPID, WaterMaterials[activeWaterVolume].GetColor("_Color").linear);
					}
					else
					{
						Shader.SetGlobalColor(Lux_UnderWaterFogColorPID, WaterMaterials[activeWaterVolume].GetColor("_Color"));
					}
					Shader.SetGlobalFloat(Lux_UnderWaterFogDensityPID, WaterMaterials[activeWaterVolume].GetFloat("_Density"));
					Shader.SetGlobalFloat(Lux_UnderWaterFogAbsorptionCancellationPID, WaterMaterials[activeWaterVolume].GetFloat("_FogAbsorptionCancellation"));
					Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionHeightPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionHeight"));
					Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionMaxHeightPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionMaxHeight"));
					Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionDepthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionDepth"));
					Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionColorStrengthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionColorStrength"));
					Shader.SetGlobalFloat(Lux_UnderWaterAbsorptionStrengthPID, WaterMaterials[activeWaterVolume].GetFloat("_AbsorptionStrength"));
					Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringPowerPID, WaterMaterials[activeWaterVolume].GetFloat("_ScatteringPower"));
					Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringIntensityPID, WaterMaterials[activeWaterVolume].GetFloat("_UnderwaterScatteringIntensity"));
					if (islinear)
					{
						Shader.SetGlobalColor(Lux_UnderWaterUnderwaterScatteringColorPID, WaterMaterials[activeWaterVolume].GetColor("_TranslucencyColor").linear);
					}
					else
					{
						Shader.SetGlobalColor(Lux_UnderWaterUnderwaterScatteringColorPID, WaterMaterials[activeWaterVolume].GetColor("_TranslucencyColor"));
					}
					Shader.SetGlobalFloat(Lux_UnderWaterUnderwaterScatteringOcclusionPID, WaterMaterials[activeWaterVolume].GetFloat("_ScatterOcclusion"));
					Shader.SetGlobalTexture(Lux_UnderWaterCausticsPID, WaterMaterials[activeWaterVolume].GetTexture(CausticTexPID));
					Shader.SetGlobalFloat("_Lux_UnderWaterCausticsTiling", WaterMaterials[activeWaterVolume].GetFloat("_CausticsTiling"));
					Shader.SetGlobalFloat("_Lux_UnderWaterCausticsScale", WaterMaterials[activeWaterVolume].GetFloat("_CausticsScale"));
					Shader.SetGlobalFloat("_Lux_UnderWaterCausticsSpeed", WaterMaterials[activeWaterVolume].GetFloat("_CausticsSpeed"));
					Shader.SetGlobalFloat("_Lux_UnderWaterCausticsTiling", WaterMaterials[activeWaterVolume].GetFloat("_CausticsTiling"));
					Shader.SetGlobalFloat("_Lux_UnderWaterCausticsSelfDistortion", WaterMaterials[activeWaterVolume].GetFloat("_CausticsSelfDistortion"));
					Shader.SetGlobalVector("_Lux_UnderWaterFinalBumpSpeed01", WaterMaterials[activeWaterVolume].GetVector("_FinalBumpSpeed01"));
					Shader.SetGlobalVector("_Lux_UnderWaterFogDepthAtten", WaterMaterials[activeWaterVolume].GetVector("_DepthAtten"));
				}
				Graphics.Blit(src, dest, blitMaterial, 0);
			}
			else
			{
				Graphics.Blit(src, dest);
			}
		}
		else
		{
			Graphics.Blit(src, dest);
		}
	}
}
