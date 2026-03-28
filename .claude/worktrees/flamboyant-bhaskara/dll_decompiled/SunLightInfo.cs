using System;
using System.Collections.Generic;
using Illusion.CustomAttributes;
using IllusionUtility.GetUtility;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SunLightInfo : MonoBehaviour
{
	[Serializable]
	public class Info
	{
		[Header("表示オブジェクトの設定")]
		public List<GameObject> visibleList = new List<GameObject>();

		[Header("被写界深度の設定")]
		[Label("使用するか")]
		public bool dofUse;

		[Label("焦点幅")]
		public float dofFocalSize;

		[Label("絞り")]
		public float dofAperture;

		[Header("フォグの設定")]
		[Label("使用するか")]
		public bool fogUse;

		[Label("背景フォグ")]
		public bool fogExcludeFarPixels;

		[Label("フォグ高さ")]
		public float fogHeight;

		[Label("フォグ高さ濃度")]
		public float fogHeightDensity;

		[Label("色")]
		public Color fogColor;

		[Label("フォグ強さ")]
		public float fogDensity;

		[Header("カメラのサンシャフト(SunShafts)設定")]
		[Label("使用するか")]
		public bool sunShaftsUse;

		[Label("Shafts caster")]
		public Transform sunShaftsTransform;

		[Label("しきい色")]
		public Color sunShaftsThresholdColor;

		[Label("色")]
		public Color sunShaftsColor;

		[Label("範囲")]
		public float sunShaftsMaxRadius;

		[Label("伸び")]
		public float sunShaftsBlurSize;

		[Label("強さ")]
		public float sunShaftsIntensity;

		[Header("ReflectProbe設定")]
		[Label("ReflectProbeの初期テクスチャ")]
		public Texture reflectProbeTexture;

		[Label("強さ")]
		public float reflectProbeintensity;

		[Header("Hキャラライト設定")]
		[Label("Front(Key)の色")]
		public Color charaLightFrontColor;

		[Label("Front(Key)の強さ")]
		public float charaLightFrontintensity = 1f;

		[Label("Backの色")]
		public Color charaLightBackColor;

		[Label("Backの強さ")]
		public float charaLightBackintensity = 1f;

		[Header("ADVキャラライト設定")]
		[Label("Front(Key)の色")]
		public Color charaLightADVFrontColor;

		[Label("Front(Key)の強さ")]
		public float charaLightADVFrontintensity = 1f;

		[Label("Backの色")]
		public Color charaLightADVBackColor;

		[Label("Backの強さ")]
		public float charaLightADVBackintensity = 1f;

		public void VisibleChange(bool visible)
		{
			visibleList.ForEach(delegate(GameObject p)
			{
				p.SetActive(visible);
			});
		}

		public void SetDoF(DepthOfField dof)
		{
			if (!(dof == null))
			{
				dof.enabled = dofUse;
				dof.focalSize = dofFocalSize;
				dof.aperture = dofAperture;
			}
		}

		public void CopyFog(DepthOfField dof)
		{
			if (!(dof == null))
			{
				dofUse = dof.enabled;
				dofFocalSize = dof.focalSize;
				dofAperture = dof.aperture;
			}
		}

		public void SetFog(GlobalFog fog)
		{
			if (!(fog == null))
			{
				fog.enabled = fogUse;
				fog.excludeFarPixels = fogExcludeFarPixels;
				fog.height = fogHeight;
				fog.heightDensity = fogHeightDensity;
				RenderSettings.fog = fogUse;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogDensity = fogDensity;
			}
		}

		public void CopyFog(GlobalFog fog)
		{
			if (!(fog == null))
			{
				fogUse = fog.enabled;
				fogExcludeFarPixels = fog.excludeFarPixels;
				fogHeight = fog.height;
				fogHeightDensity = fog.heightDensity;
				fogUse = RenderSettings.fog;
				fogColor = RenderSettings.fogColor;
				fogDensity = RenderSettings.fogDensity;
			}
		}

		public void SetSunShafts(SunShafts sunShafts)
		{
			if (!(sunShafts == null))
			{
				sunShafts.enabled = sunShaftsUse;
				sunShafts.sunTransform = sunShaftsTransform;
				sunShafts.sunThreshold = sunShaftsThresholdColor;
				sunShafts.sunColor = sunShaftsColor;
				sunShafts.maxRadius = 1f - sunShaftsMaxRadius;
				sunShafts.sunShaftBlurRadius = sunShaftsBlurSize;
				sunShafts.sunShaftIntensity = sunShaftsIntensity;
			}
		}

		public void CopySunShafts(SunShafts sunShafts)
		{
			if (!(sunShafts == null))
			{
				sunShaftsUse = sunShafts.enabled;
				sunShaftsTransform = sunShafts.sunTransform;
				sunShaftsThresholdColor = sunShafts.sunThreshold;
				sunShaftsColor = sunShafts.sunColor;
				sunShaftsMaxRadius = sunShafts.maxRadius;
				sunShaftsBlurSize = sunShafts.sunShaftBlurRadius;
				sunShaftsIntensity = sunShafts.sunShaftIntensity;
			}
		}

		public void SetReflectProbe(ReflectionProbe reflectionProbe)
		{
			if (!(reflectionProbe == null))
			{
				reflectionProbe.customBakedTexture = reflectProbeTexture;
				reflectionProbe.intensity = reflectProbeintensity;
			}
		}

		public void SetReflectProbeTextrue(ReflectionProbe reflectionProbe)
		{
			if (!(reflectionProbe == null))
			{
				reflectionProbe.customBakedTexture = reflectProbeTexture;
			}
		}

		public void CopyReflectProbe(ReflectionProbe reflectionProbe)
		{
			if (!(reflectionProbe == null))
			{
				reflectProbeintensity = reflectionProbe.intensity;
			}
		}

		public void SetCharaLight(Light _lightFront, Light _lightBack)
		{
			if ((bool)_lightFront)
			{
				_lightFront.color = charaLightFrontColor;
				_lightFront.intensity = charaLightFrontintensity;
			}
			if ((bool)_lightBack)
			{
				_lightBack.color = charaLightBackColor;
				_lightBack.intensity = charaLightBackintensity;
			}
		}

		public void SetCharaLightADV(Light _lightFront, Light _lightBack)
		{
			if ((bool)_lightFront)
			{
				_lightFront.color = charaLightADVFrontColor;
				_lightFront.intensity = charaLightADVFrontintensity;
			}
			if ((bool)_lightBack)
			{
				_lightBack.color = charaLightADVBackColor;
				_lightBack.intensity = charaLightADVBackintensity;
			}
		}
	}

	[SerializeField]
	private Info _info;

	[SerializeField]
	private List<GameObject> rootObjectMaps;

	[SerializeField]
	private List<GameObject> environmentLightObjects;

	public Info info => _info;

	public List<GameObject> RootObjectMaps => rootObjectMaps;

	public List<GameObject> EnvironmentLightObjects => environmentLightObjects;

	public bool Set(Camera cam, int _charaLight)
	{
		if (cam == null)
		{
			return false;
		}
		if (info != null)
		{
			info.VisibleChange(visible: true);
			info.SetDoF(cam.GetComponent<DepthOfField>());
			info.SetFog(cam.GetComponent<GlobalFog>());
			info.SetSunShafts(cam.GetComponent<SunShafts>());
			info.SetReflectProbe(GetComponentInChildren<ReflectionProbe>(includeInactive: true));
			Transform transform = cam.transform.FindLoop("Directional Light Key");
			Light lightFront = (transform ? transform.GetComponent<Light>() : null);
			Transform transform2 = cam.transform.FindLoop("Directional Light Back");
			Light lightBack = (transform2 ? transform2.GetComponent<Light>() : null);
			switch (_charaLight)
			{
			case 0:
				info.SetCharaLight(lightFront, lightBack);
				break;
			case 1:
				info.SetCharaLightADV(lightFront, lightBack);
				break;
			}
			return true;
		}
		return false;
	}

	public bool SetDefaultReflectProbe()
	{
		info.SetReflectProbeTextrue(GetComponentInChildren<ReflectionProbe>(includeInactive: true));
		return false;
	}
}
