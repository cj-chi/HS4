using System;
using System.Collections.Generic;
using System.IO;
using Illusion;
using Manager;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class ScreenEffect : MonoBehaviour
{
	public struct BloomValue
	{
		public bool enable;

		public float intensity;

		public float threshold;

		public float softKnee;

		public bool clamp;

		public float diffusion;

		public Color color;
	}

	public struct AOValue
	{
		public bool enable;

		public float intensity;

		public float thicknessModifier;

		public Color color;
	}

	public struct DOFValue
	{
		public bool enable;

		public float focalSize;

		public float aperture;
	}

	public struct ColorGradingValue
	{
		public int kind;

		public float blend;

		public float saturation;

		public float brightness;

		public float contrast;
	}

	public struct FogValue
	{
		public bool enable;

		public bool excludeFarPixels;

		public float height;

		public float heightDensity;

		public Color color;

		public float density;
	}

	public struct SunShaftsValue
	{
		public bool enable;

		public Color sunThreshold;

		public Color sunColor;

		public float maxRadius;

		public float sunShaftBlurRadius;

		public float sunShaftIntensity;
	}

	public struct EnvironmentValue
	{
		public Color sky;

		public Color equator;

		public Color ground;
	}

	public struct ReflectionProbeValue
	{
		public bool enable;

		public int kind;

		public float intensity;
	}

	public struct VignetValue
	{
		public bool enable;

		public float intensity;
	}

	public class PresetInfo
	{
		private int id;

		public string showName;

		public AssetBundleInfo assetbundle;

		public int ID => id;

		public PresetInfo(int _id, string _name, string _assetbundle, string _assetname, string _manifest)
		{
			id = _id;
			showName = _name;
			assetbundle = new AssetBundleInfo("", _assetbundle, _assetname, _manifest);
		}
	}

	private ColorGrading colorGrading;

	private AmbientOcclusion ao;

	[SerializeField]
	private UnityStandardAssets.ImageEffects.DepthOfField dof;

	private Vignette vignette;

	private ScreenSpaceReflections ssr;

	[SerializeField]
	private ReflectionProbe reflectionProbe;

	[SerializeField]
	private GlobalFog fog;

	private UnityEngine.Rendering.PostProcessing.Bloom bloom;

	[SerializeField]
	private SunShafts sunShafts;

	private PostProcessVolume processVolume;

	[SerializeField]
	private PostProcessVolume processVolumeColor;

	private PostProcessVolume processVolumeDef;

	private ColorGradingValue initColorValue;

	private AOValue initAmbientValue;

	private DOFValue initDoFValue;

	private ReflectionProbeValue initRPValue;

	private FogValue initFogValue;

	private BloomValue initBloomValue;

	private SunShaftsValue initSunShaftsValue;

	private EnvironmentValue initEnvironmentValue;

	private VignetValue initVignetValue;

	private bool initSSRValue;

	private bool initSelfShadowValue;

	private ColorGradingValue loadColorValue;

	private AOValue loadAmbientValue;

	private DOFValue loadDoFValue;

	private ReflectionProbeValue loadRPValue;

	private FogValue loadFogValue;

	private BloomValue loadBloomValue;

	private SunShaftsValue loadSunShaftsValue;

	private EnvironmentValue loadEnvironmentValue;

	private VignetValue loadVignetValue;

	private bool loadSSRValue;

	private bool loadSelfShadowValue;

	private string loadEffectName;

	private bool canOverride;

	public int NowColoGradingKind;

	public int NowReflectionKind;

	public bool NowSelfShadowEnable = true;

	private bool loadLastEffect;

	private List<string> userFileNames;

	private SunLightInfo lightInfo;

	public ColorGrading ColorGrading => colorGrading;

	public AmbientOcclusion AO => ao;

	public ReflectionProbe ReflectionProbe
	{
		get
		{
			return reflectionProbe;
		}
		set
		{
			reflectionProbe = value;
		}
	}

	public UnityEngine.Rendering.PostProcessing.Bloom Bloom => bloom;

	public PostProcessVolume ProcessVolume => processVolume;

	public PostProcessVolume ProcessVolumeColor => processVolumeColor;

	public PostProcessVolume ProcessVolumeDef => processVolumeDef;

	public ColorGradingValue InitColorValue => initColorValue;

	public AOValue InitAmbientValue => initAmbientValue;

	public DOFValue InitDoFValue => initDoFValue;

	public ReflectionProbeValue InitRPValue => initRPValue;

	public FogValue InitFogValue => initFogValue;

	public BloomValue InitBloomValue => initBloomValue;

	public SunShaftsValue InitSunShaftsValue => initSunShaftsValue;

	public EnvironmentValue InitEnvironmentValue => initEnvironmentValue;

	public VignetValue InitVignetValue => initVignetValue;

	public bool InitSSRValue => initSSRValue;

	public bool InitSelfShadowValue => initSelfShadowValue;

	public bool LoadLastEffect => loadLastEffect;

	public List<string> UserFileNames => userFileNames;

	public void Init()
	{
		processVolumeDef = BaseMap.postProcessVolume_default;
		processVolume = BaseMap.postProcessVolume_user;
		processVolumeColor = BaseMap.postProcessVolume_color;
		reflectionProbe = BaseMap.reflectionProbe;
		loadLastEffect = false;
		int no = BaseMap.no;
		loadLastEffect = LoadBefore($"tmp_Map{no:00}");
		if (!(processVolume == null))
		{
			lightInfo = BaseMap.sunLightInfo;
			bloom = processVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.Bloom>();
			UnityEngine.Rendering.PostProcessing.Bloom setting = processVolumeDef.profile.GetSetting<UnityEngine.Rendering.PostProcessing.Bloom>();
			if (bloom != null)
			{
				initBloomValue.enable = setting.active;
				initBloomValue.intensity = setting.intensity.value;
				initBloomValue.threshold = setting.threshold.value;
				initBloomValue.softKnee = setting.softKnee.value;
				initBloomValue.clamp = setting.clamp.overrideState;
				initBloomValue.diffusion = setting.diffusion.value;
				Color value = setting.color.value;
				value.r = Mathf.Clamp01(value.r);
				value.g = Mathf.Clamp01(value.g);
				value.b = Mathf.Clamp01(value.b);
				value.a = 1f;
				initBloomValue.color = value;
			}
			ao = processVolume.profile.GetSetting<AmbientOcclusion>();
			AmbientOcclusion setting2 = processVolumeDef.profile.GetSetting<AmbientOcclusion>();
			if (ao != null)
			{
				initAmbientValue.enable = setting2.active;
				initAmbientValue.intensity = setting2.intensity.value;
				initAmbientValue.thicknessModifier = setting2.thicknessModifier.value;
				initAmbientValue.color = setting2.color.value;
			}
			if (dof != null)
			{
				initDoFValue.enable = lightInfo.info.dofUse;
				initDoFValue.focalSize = lightInfo.info.dofFocalSize;
				initDoFValue.aperture = lightInfo.info.dofAperture;
				dof.enabled = InitDoFValue.enable;
				dof.focalSize = InitDoFValue.focalSize;
				dof.aperture = InitDoFValue.aperture;
			}
			colorGrading = processVolumeColor.profile.GetSetting<ColorGrading>();
			if (colorGrading != null)
			{
				initColorValue.kind = NowColoGradingKind;
				initColorValue.blend = processVolumeColor.weight;
				initColorValue.saturation = colorGrading.saturation.value;
				initColorValue.brightness = colorGrading.brightness.value;
				initColorValue.contrast = colorGrading.contrast.value;
			}
			if (fog != null)
			{
				initFogValue.enable = lightInfo.info.fogUse;
				initFogValue.excludeFarPixels = lightInfo.info.fogExcludeFarPixels;
				initFogValue.height = lightInfo.info.fogHeight;
				initFogValue.heightDensity = lightInfo.info.fogHeightDensity;
				initFogValue.color = new Color(lightInfo.info.fogColor.r, lightInfo.info.fogColor.g, lightInfo.info.fogColor.b, 1f);
				initFogValue.density = lightInfo.info.fogDensity;
				fog.enabled = InitFogValue.enable;
				fog.excludeFarPixels = InitFogValue.excludeFarPixels;
				fog.height = InitFogValue.height;
				fog.heightDensity = InitFogValue.heightDensity;
				RenderSettings.fogColor = InitFogValue.color;
				RenderSettings.fogDensity = InitFogValue.density;
			}
			if (sunShafts != null)
			{
				initSunShaftsValue.enable = lightInfo.info.sunShaftsUse;
				initSunShaftsValue.sunThreshold = new Color(lightInfo.info.sunShaftsThresholdColor.r, lightInfo.info.sunShaftsThresholdColor.g, lightInfo.info.sunShaftsThresholdColor.b, 1f);
				initSunShaftsValue.sunColor = new Color(lightInfo.info.sunShaftsColor.r, lightInfo.info.sunShaftsColor.g, lightInfo.info.sunShaftsColor.b, 1f);
				initSunShaftsValue.maxRadius = lightInfo.info.sunShaftsMaxRadius;
				initSunShaftsValue.sunShaftBlurRadius = lightInfo.info.sunShaftsBlurSize;
				initSunShaftsValue.sunShaftIntensity = lightInfo.info.sunShaftsIntensity;
				sunShafts.enabled = InitSunShaftsValue.enable;
				sunShafts.sunThreshold = InitSunShaftsValue.sunThreshold;
				sunShafts.sunColor = InitSunShaftsValue.sunColor;
				sunShafts.maxRadius = InitSunShaftsValue.maxRadius;
				sunShafts.sunShaftBlurRadius = InitSunShaftsValue.sunShaftBlurRadius;
				sunShafts.sunShaftIntensity = InitSunShaftsValue.sunShaftIntensity;
			}
			initRPValue.kind = 0;
			if (reflectionProbe != null)
			{
				initRPValue.intensity = lightInfo.info.reflectProbeintensity;
				initRPValue.enable = true;
			}
			else
			{
				initRPValue.enable = false;
			}
			initEnvironmentValue.sky = new Color(RenderSettings.ambientSkyColor.r, RenderSettings.ambientSkyColor.g, RenderSettings.ambientSkyColor.b, 1f);
			initEnvironmentValue.equator = new Color(RenderSettings.ambientEquatorColor.r, RenderSettings.ambientEquatorColor.g, RenderSettings.ambientEquatorColor.b, 1f);
			initEnvironmentValue.ground = new Color(RenderSettings.ambientGroundColor.r, RenderSettings.ambientGroundColor.g, RenderSettings.ambientGroundColor.b, 1f);
			vignette = processVolume.profile.GetSetting<Vignette>();
			Vignette setting3 = processVolumeDef.profile.GetSetting<Vignette>();
			if (vignette != null)
			{
				initVignetValue.enable = setting3.active;
				initVignetValue.intensity = setting3.intensity;
			}
			ssr = processVolume.profile.GetSetting<ScreenSpaceReflections>();
			ScreenSpaceReflections setting4 = processVolumeDef.profile.GetSetting<ScreenSpaceReflections>();
			initSSRValue = ssr != null && setting4.active;
			initSelfShadowValue = Manager.Config.GraphicData.SelfShadow;
			NowSelfShadowEnable = initSelfShadowValue;
		}
	}

	public void Save(string name, string overrideName = "")
	{
		string text = UserData.Create("ScreenEffect");
		string text2 = ".bytes";
		if (overrideName.IsNullOrEmpty())
		{
			string text3 = DateTime.Now.Year.ToString("0000");
			text3 += DateTime.Now.Month.ToString("00");
			text3 += DateTime.Now.Day.ToString("00");
			text3 += DateTime.Now.Hour.ToString("00");
			text3 += DateTime.Now.Minute.ToString("00");
			text3 += DateTime.Now.Second.ToString("00");
			text3 += DateTime.Now.Millisecond.ToString("000");
			text = text + text3 + text2;
		}
		else
		{
			text = text + overrideName + text2;
			if (!Load(overrideName) || !canOverride)
			{
				return;
			}
		}
		BloomValue saveBloomValue = default(BloomValue);
		if (bloom != null)
		{
			saveBloomValue.enable = bloom.active;
			saveBloomValue.intensity = bloom.intensity.value;
			saveBloomValue.threshold = bloom.threshold.value;
			saveBloomValue.softKnee = bloom.softKnee.value;
			saveBloomValue.clamp = bloom.clamp.overrideState;
			saveBloomValue.diffusion = bloom.diffusion.value;
			saveBloomValue.color = bloom.color.value;
		}
		else
		{
			saveBloomValue.enable = initBloomValue.enable;
			saveBloomValue.intensity = initBloomValue.intensity;
			saveBloomValue.threshold = initBloomValue.threshold;
			saveBloomValue.softKnee = initBloomValue.softKnee;
			saveBloomValue.clamp = initBloomValue.clamp;
			saveBloomValue.diffusion = initBloomValue.diffusion;
			saveBloomValue.color = initBloomValue.color;
		}
		AOValue saveAmbientValue = default(AOValue);
		if (ao != null)
		{
			saveAmbientValue.enable = ao.active;
			saveAmbientValue.intensity = ao.intensity.value;
			saveAmbientValue.thicknessModifier = ao.thicknessModifier.value;
			saveAmbientValue.color = ao.color.value;
		}
		else
		{
			saveAmbientValue.enable = initAmbientValue.enable;
			saveAmbientValue.intensity = initAmbientValue.intensity;
			saveAmbientValue.thicknessModifier = initAmbientValue.thicknessModifier;
			saveAmbientValue.color = initAmbientValue.color;
		}
		DOFValue saveDoFValue = default(DOFValue);
		if (dof != null)
		{
			saveDoFValue.enable = dof.enabled;
			saveDoFValue.focalSize = dof.focalSize;
			saveDoFValue.aperture = dof.aperture;
		}
		else
		{
			saveDoFValue.enable = initDoFValue.enable;
			saveDoFValue.focalSize = initDoFValue.focalSize;
			saveDoFValue.aperture = initDoFValue.aperture;
		}
		ColorGradingValue saveColorValue = default(ColorGradingValue);
		if (colorGrading != null)
		{
			saveColorValue.kind = NowColoGradingKind;
			saveColorValue.blend = processVolumeColor.weight;
			saveColorValue.saturation = colorGrading.saturation.value;
			saveColorValue.brightness = colorGrading.brightness.value;
			saveColorValue.contrast = colorGrading.contrast.value;
		}
		else
		{
			saveColorValue.kind = initColorValue.kind;
			saveColorValue.blend = initColorValue.blend;
			saveColorValue.saturation = initColorValue.saturation;
			saveColorValue.brightness = initColorValue.brightness;
			saveColorValue.contrast = initColorValue.contrast;
		}
		FogValue saveFogValue = default(FogValue);
		if (fog != null)
		{
			saveFogValue.enable = fog.enabled;
			saveFogValue.excludeFarPixels = fog.excludeFarPixels;
			saveFogValue.height = fog.height;
			saveFogValue.heightDensity = fog.heightDensity;
			saveFogValue.color = RenderSettings.fogColor;
			saveFogValue.density = RenderSettings.fogDensity;
		}
		else
		{
			saveFogValue.enable = initFogValue.enable;
			saveFogValue.excludeFarPixels = initFogValue.excludeFarPixels;
			saveFogValue.height = initFogValue.height;
			saveFogValue.heightDensity = initFogValue.heightDensity;
			saveFogValue.color = initFogValue.color;
			saveFogValue.density = initFogValue.density;
		}
		SunShaftsValue saveSunShaftsValue = default(SunShaftsValue);
		if (sunShafts != null)
		{
			saveSunShaftsValue.enable = sunShafts.enabled;
			saveSunShaftsValue.sunThreshold = sunShafts.sunThreshold;
			saveSunShaftsValue.sunColor = sunShafts.sunColor;
			saveSunShaftsValue.maxRadius = sunShafts.maxRadius;
			saveSunShaftsValue.sunShaftBlurRadius = sunShafts.sunShaftBlurRadius;
			saveSunShaftsValue.sunShaftIntensity = sunShafts.sunShaftIntensity;
		}
		else
		{
			saveSunShaftsValue.enable = initSunShaftsValue.enable;
			saveSunShaftsValue.sunThreshold = initSunShaftsValue.sunThreshold;
			saveSunShaftsValue.sunColor = initSunShaftsValue.sunColor;
			saveSunShaftsValue.maxRadius = initSunShaftsValue.maxRadius;
			saveSunShaftsValue.sunShaftBlurRadius = initSunShaftsValue.sunShaftBlurRadius;
			saveSunShaftsValue.sunShaftIntensity = initSunShaftsValue.sunShaftIntensity;
		}
		ReflectionProbeValue saveRPValue = default(ReflectionProbeValue);
		if (reflectionProbe != null)
		{
			saveRPValue.kind = NowReflectionKind;
			saveRPValue.intensity = reflectionProbe.intensity;
			saveRPValue.enable = reflectionProbe.gameObject.activeSelf;
		}
		else
		{
			saveRPValue.kind = initRPValue.kind;
			saveRPValue.intensity = initRPValue.intensity;
			saveRPValue.enable = initRPValue.enable;
		}
		EnvironmentValue saveEnvironmentValue = default(EnvironmentValue);
		saveEnvironmentValue.sky = RenderSettings.ambientSkyColor;
		saveEnvironmentValue.equator = RenderSettings.ambientEquatorColor;
		saveEnvironmentValue.ground = RenderSettings.ambientGroundColor;
		VignetValue saveVignetteValue = default(VignetValue);
		if (vignette != null)
		{
			saveVignetteValue.enable = vignette.active;
			saveVignetteValue.intensity = vignette.intensity.value;
		}
		else
		{
			saveVignetteValue.enable = initVignetValue.enable;
			saveVignetteValue.intensity = initVignetValue.intensity;
		}
		bool saveSSRValue;
		if (ssr != null)
		{
			saveSSRValue = ssr.active;
		}
		else
		{
			saveSSRValue = initSSRValue;
		}
		bool saveSelfShadowValue = NowSelfShadowEnable;
		Utils.File.OpenWrite(text, isAppend: false, delegate(FileStream stream)
		{
			using BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(1);
			binaryWriter.Write(name);
			binaryWriter.Write(saveColorValue.kind);
			binaryWriter.Write(saveColorValue.blend);
			binaryWriter.Write(saveColorValue.saturation);
			binaryWriter.Write(saveColorValue.brightness);
			binaryWriter.Write(saveColorValue.contrast);
			binaryWriter.Write(saveAmbientValue.enable);
			binaryWriter.Write(saveAmbientValue.intensity);
			binaryWriter.Write(saveAmbientValue.thicknessModifier);
			binaryWriter.Write(saveAmbientValue.color.r);
			binaryWriter.Write(saveAmbientValue.color.g);
			binaryWriter.Write(saveAmbientValue.color.b);
			binaryWriter.Write(saveAmbientValue.color.a);
			binaryWriter.Write(saveBloomValue.enable);
			binaryWriter.Write(saveBloomValue.intensity);
			binaryWriter.Write(saveBloomValue.threshold);
			binaryWriter.Write(saveBloomValue.softKnee);
			binaryWriter.Write(saveBloomValue.clamp);
			binaryWriter.Write(saveBloomValue.diffusion);
			binaryWriter.Write(saveBloomValue.color.r);
			binaryWriter.Write(saveBloomValue.color.g);
			binaryWriter.Write(saveBloomValue.color.b);
			binaryWriter.Write(saveBloomValue.color.a);
			binaryWriter.Write(saveDoFValue.enable);
			binaryWriter.Write(saveDoFValue.focalSize);
			binaryWriter.Write(saveDoFValue.aperture);
			binaryWriter.Write(saveVignetteValue.enable);
			binaryWriter.Write(saveVignetteValue.intensity);
			binaryWriter.Write(saveSSRValue);
			binaryWriter.Write(saveRPValue.enable);
			binaryWriter.Write(saveRPValue.kind);
			binaryWriter.Write(saveRPValue.intensity);
			binaryWriter.Write(saveFogValue.enable);
			binaryWriter.Write(saveFogValue.excludeFarPixels);
			binaryWriter.Write(saveFogValue.height);
			binaryWriter.Write(saveFogValue.heightDensity);
			binaryWriter.Write(saveFogValue.color.r);
			binaryWriter.Write(saveFogValue.color.g);
			binaryWriter.Write(saveFogValue.color.b);
			binaryWriter.Write(saveFogValue.color.a);
			binaryWriter.Write(saveFogValue.density);
			binaryWriter.Write(saveSunShaftsValue.enable);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.r);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.g);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.b);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.a);
			binaryWriter.Write(saveSunShaftsValue.sunColor.r);
			binaryWriter.Write(saveSunShaftsValue.sunColor.g);
			binaryWriter.Write(saveSunShaftsValue.sunColor.b);
			binaryWriter.Write(saveSunShaftsValue.sunColor.a);
			binaryWriter.Write(saveSunShaftsValue.maxRadius);
			binaryWriter.Write(saveSunShaftsValue.sunShaftBlurRadius);
			binaryWriter.Write(saveSunShaftsValue.sunShaftIntensity);
			binaryWriter.Write(saveSelfShadowValue);
			binaryWriter.Write(saveEnvironmentValue.sky.r);
			binaryWriter.Write(saveEnvironmentValue.sky.g);
			binaryWriter.Write(saveEnvironmentValue.sky.b);
			binaryWriter.Write(saveEnvironmentValue.sky.a);
			binaryWriter.Write(saveEnvironmentValue.equator.r);
			binaryWriter.Write(saveEnvironmentValue.equator.g);
			binaryWriter.Write(saveEnvironmentValue.equator.b);
			binaryWriter.Write(saveEnvironmentValue.equator.a);
			binaryWriter.Write(saveEnvironmentValue.ground.r);
			binaryWriter.Write(saveEnvironmentValue.ground.g);
			binaryWriter.Write(saveEnvironmentValue.ground.b);
			binaryWriter.Write(saveEnvironmentValue.ground.a);
		});
	}

	private bool Load(string fileName)
	{
		string text = UserData.Create("ScreenEffect");
		string text2 = ".bytes";
		return Utils.File.OpenRead(text + fileName + text2, delegate(FileStream stream)
		{
			using BinaryReader binaryReader = new BinaryReader(stream);
			canOverride = binaryReader.ReadInt32() == 1;
			loadEffectName = binaryReader.ReadString();
			loadColorValue.kind = binaryReader.ReadInt32();
			loadColorValue.blend = binaryReader.ReadSingle();
			loadColorValue.saturation = binaryReader.ReadSingle();
			loadColorValue.brightness = binaryReader.ReadSingle();
			loadColorValue.contrast = binaryReader.ReadSingle();
			loadAmbientValue.enable = binaryReader.ReadBoolean();
			loadAmbientValue.intensity = binaryReader.ReadSingle();
			loadAmbientValue.thicknessModifier = binaryReader.ReadSingle();
			loadAmbientValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadBloomValue.enable = binaryReader.ReadBoolean();
			loadBloomValue.intensity = binaryReader.ReadSingle();
			loadBloomValue.threshold = binaryReader.ReadSingle();
			loadBloomValue.softKnee = binaryReader.ReadSingle();
			loadBloomValue.clamp = binaryReader.ReadBoolean();
			loadBloomValue.diffusion = binaryReader.ReadSingle();
			loadBloomValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadDoFValue.enable = binaryReader.ReadBoolean();
			loadDoFValue.focalSize = binaryReader.ReadSingle();
			loadDoFValue.aperture = binaryReader.ReadSingle();
			loadVignetValue.enable = binaryReader.ReadBoolean();
			loadVignetValue.intensity = binaryReader.ReadSingle();
			loadSSRValue = binaryReader.ReadBoolean();
			loadRPValue.enable = binaryReader.ReadBoolean();
			loadRPValue.kind = binaryReader.ReadInt32();
			loadRPValue.intensity = binaryReader.ReadSingle();
			loadFogValue.enable = binaryReader.ReadBoolean();
			loadFogValue.excludeFarPixels = binaryReader.ReadBoolean();
			loadFogValue.height = binaryReader.ReadSingle();
			loadFogValue.heightDensity = binaryReader.ReadSingle();
			loadFogValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadFogValue.density = binaryReader.ReadSingle();
			loadSunShaftsValue.enable = binaryReader.ReadBoolean();
			loadSunShaftsValue.sunThreshold = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadSunShaftsValue.sunColor = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadSunShaftsValue.maxRadius = binaryReader.ReadSingle();
			loadSunShaftsValue.sunShaftBlurRadius = binaryReader.ReadSingle();
			loadSunShaftsValue.sunShaftIntensity = binaryReader.ReadSingle();
			loadSelfShadowValue = binaryReader.ReadBoolean();
			loadEnvironmentValue.sky = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadEnvironmentValue.equator = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadEnvironmentValue.ground = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		});
	}

	private bool Load(string bundle, string fileName, string manifest = "")
	{
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(bundle, fileName, clone: false, manifest);
		AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: true);
		if (textAsset == null)
		{
			return false;
		}
		bool flag = false;
		using MemoryStream input = new MemoryStream(textAsset.bytes);
		using BinaryReader binaryReader = new BinaryReader(input);
		canOverride = binaryReader.ReadInt32() == 1;
		loadEffectName = binaryReader.ReadString();
		loadColorValue.kind = binaryReader.ReadInt32();
		loadColorValue.blend = binaryReader.ReadSingle();
		loadColorValue.saturation = binaryReader.ReadSingle();
		loadColorValue.brightness = binaryReader.ReadSingle();
		loadColorValue.contrast = binaryReader.ReadSingle();
		loadAmbientValue.enable = binaryReader.ReadBoolean();
		loadAmbientValue.intensity = binaryReader.ReadSingle();
		loadAmbientValue.thicknessModifier = binaryReader.ReadSingle();
		loadAmbientValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadBloomValue.enable = binaryReader.ReadBoolean();
		loadBloomValue.intensity = binaryReader.ReadSingle();
		loadBloomValue.threshold = binaryReader.ReadSingle();
		loadBloomValue.softKnee = binaryReader.ReadSingle();
		loadBloomValue.clamp = binaryReader.ReadBoolean();
		loadBloomValue.diffusion = binaryReader.ReadSingle();
		loadBloomValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadDoFValue.enable = binaryReader.ReadBoolean();
		loadDoFValue.focalSize = binaryReader.ReadSingle();
		loadDoFValue.aperture = binaryReader.ReadSingle();
		loadVignetValue.enable = binaryReader.ReadBoolean();
		loadVignetValue.intensity = binaryReader.ReadSingle();
		loadSSRValue = binaryReader.ReadBoolean();
		loadRPValue.enable = binaryReader.ReadBoolean();
		loadRPValue.kind = binaryReader.ReadInt32();
		loadRPValue.intensity = binaryReader.ReadSingle();
		loadFogValue.enable = binaryReader.ReadBoolean();
		loadFogValue.excludeFarPixels = binaryReader.ReadBoolean();
		loadFogValue.height = binaryReader.ReadSingle();
		loadFogValue.heightDensity = binaryReader.ReadSingle();
		loadFogValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadFogValue.density = binaryReader.ReadSingle();
		loadSunShaftsValue.enable = binaryReader.ReadBoolean();
		loadSunShaftsValue.sunThreshold = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadSunShaftsValue.sunColor = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadSunShaftsValue.maxRadius = binaryReader.ReadSingle();
		loadSunShaftsValue.sunShaftBlurRadius = binaryReader.ReadSingle();
		loadSunShaftsValue.sunShaftIntensity = binaryReader.ReadSingle();
		loadSelfShadowValue = binaryReader.ReadBoolean();
		loadEnvironmentValue.sky = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadEnvironmentValue.equator = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		loadEnvironmentValue.ground = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		return true;
	}

	private bool LoadBefore(string fileName)
	{
		string text = DefaultData.Create("ScreenEffect");
		string text2 = ".bytes";
		return Utils.File.OpenRead(text + fileName + text2, delegate(FileStream stream)
		{
			using BinaryReader binaryReader = new BinaryReader(stream);
			canOverride = binaryReader.ReadInt32() == 1;
			loadEffectName = binaryReader.ReadString();
			loadColorValue.kind = binaryReader.ReadInt32();
			loadColorValue.blend = binaryReader.ReadSingle();
			loadColorValue.saturation = binaryReader.ReadSingle();
			loadColorValue.brightness = binaryReader.ReadSingle();
			loadColorValue.contrast = binaryReader.ReadSingle();
			loadAmbientValue.enable = binaryReader.ReadBoolean();
			loadAmbientValue.intensity = binaryReader.ReadSingle();
			loadAmbientValue.thicknessModifier = binaryReader.ReadSingle();
			loadAmbientValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadBloomValue.enable = binaryReader.ReadBoolean();
			loadBloomValue.intensity = binaryReader.ReadSingle();
			loadBloomValue.threshold = binaryReader.ReadSingle();
			loadBloomValue.softKnee = binaryReader.ReadSingle();
			loadBloomValue.clamp = binaryReader.ReadBoolean();
			loadBloomValue.diffusion = binaryReader.ReadSingle();
			loadBloomValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadDoFValue.enable = binaryReader.ReadBoolean();
			loadDoFValue.focalSize = binaryReader.ReadSingle();
			loadDoFValue.aperture = binaryReader.ReadSingle();
			loadVignetValue.enable = binaryReader.ReadBoolean();
			loadVignetValue.intensity = binaryReader.ReadSingle();
			loadSSRValue = binaryReader.ReadBoolean();
			loadRPValue.enable = binaryReader.ReadBoolean();
			loadRPValue.kind = binaryReader.ReadInt32();
			loadRPValue.intensity = binaryReader.ReadSingle();
			loadFogValue.enable = binaryReader.ReadBoolean();
			loadFogValue.excludeFarPixels = binaryReader.ReadBoolean();
			loadFogValue.height = binaryReader.ReadSingle();
			loadFogValue.heightDensity = binaryReader.ReadSingle();
			loadFogValue.color = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadFogValue.density = binaryReader.ReadSingle();
			loadSunShaftsValue.enable = binaryReader.ReadBoolean();
			loadSunShaftsValue.sunThreshold = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadSunShaftsValue.sunColor = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadSunShaftsValue.maxRadius = binaryReader.ReadSingle();
			loadSunShaftsValue.sunShaftBlurRadius = binaryReader.ReadSingle();
			loadSunShaftsValue.sunShaftIntensity = binaryReader.ReadSingle();
			loadSelfShadowValue = binaryReader.ReadBoolean();
			loadEnvironmentValue.sky = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadEnvironmentValue.equator = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			loadEnvironmentValue.ground = new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		});
	}

	private string GetEffectName(string fileName, string bundle = "", string manifest = "", bool fullPath = false)
	{
		string ret = "";
		if (bundle == "")
		{
			string text = "";
			if (!fullPath)
			{
				text = UserData.Create("ScreenEffect");
				string text2 = ".bytes";
				text = text + fileName + text2;
			}
			else
			{
				text = fileName;
			}
			Utils.File.OpenRead(text, delegate(FileStream stream)
			{
				using BinaryReader binaryReader2 = new BinaryReader(stream);
				binaryReader2.ReadInt32();
				ret = binaryReader2.ReadString();
			});
		}
		else
		{
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(bundle, fileName, clone: false, manifest);
			AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: true);
			if (textAsset != null)
			{
				using MemoryStream input = new MemoryStream(textAsset.bytes);
				using BinaryReader binaryReader = new BinaryReader(input);
				binaryReader.ReadInt32();
				ret = binaryReader.ReadString();
			}
		}
		return ret;
	}

	public List<string> GetAllEffectDataName()
	{
		List<string> list = new List<string>();
		List<string> files = new List<string>();
		Utils.File.GetAllFiles(UserData.Create("ScreenEffect"), "*.bytes", ref files);
		foreach (string item in files)
		{
			if (!GlobalMethod.StartsWith(Path.GetFileName(item), "tmp"))
			{
				list.Add(GetEffectName(item, "", "", fullPath: true));
			}
		}
		userFileNames = new List<string>();
		foreach (string item2 in files)
		{
			if (!GlobalMethod.StartsWith(Path.GetFileName(item2), "tmp"))
			{
				userFileNames.Add(Path.GetFileNameWithoutExtension(item2));
			}
		}
		return list;
	}

	public void SetEffectSetting(string fileName, string bundle = "", string manifest = "")
	{
		if (bundle == "")
		{
			if (Load(fileName))
			{
				SetEffectSetting();
			}
		}
		else if (Load(bundle, fileName, manifest))
		{
			SetEffectSetting();
		}
	}

	public void SetEffectSetting()
	{
		if (bloom != null)
		{
			bloom.active = loadBloomValue.enable;
			bloom.intensity.value = loadBloomValue.intensity;
			bloom.threshold.value = loadBloomValue.threshold;
			bloom.softKnee.value = loadBloomValue.softKnee;
			bloom.clamp.overrideState = loadBloomValue.clamp;
			bloom.diffusion.value = loadBloomValue.diffusion;
			bloom.color.value = loadBloomValue.color;
		}
		if (ao != null)
		{
			ao.active = loadAmbientValue.enable;
			ao.intensity.value = loadAmbientValue.intensity;
			ao.thicknessModifier.value = loadAmbientValue.thicknessModifier;
			ao.color.value = loadAmbientValue.color;
		}
		if (dof != null)
		{
			dof.enabled = loadDoFValue.enable;
			dof.focalSize = loadDoFValue.focalSize;
			dof.aperture = loadDoFValue.aperture;
		}
		if (colorGrading != null)
		{
			NowColoGradingKind = loadColorValue.kind;
			processVolumeColor.weight = loadColorValue.blend;
			colorGrading.saturation.value = loadColorValue.saturation;
			colorGrading.brightness.value = loadColorValue.brightness;
			colorGrading.contrast.value = loadColorValue.contrast;
		}
		if (fog != null)
		{
			fog.enabled = loadFogValue.enable;
			fog.excludeFarPixels = loadFogValue.excludeFarPixels;
			fog.height = loadFogValue.height;
			fog.heightDensity = loadFogValue.heightDensity;
			RenderSettings.fogColor = loadFogValue.color;
			RenderSettings.fogDensity = loadFogValue.density;
		}
		if (sunShafts != null)
		{
			sunShafts.enabled = loadSunShaftsValue.enable;
			sunShafts.sunThreshold = loadSunShaftsValue.sunThreshold;
			sunShafts.sunColor = loadSunShaftsValue.sunColor;
			sunShafts.maxRadius = loadSunShaftsValue.maxRadius;
			sunShafts.sunShaftBlurRadius = loadSunShaftsValue.sunShaftBlurRadius;
			sunShafts.sunShaftIntensity = loadSunShaftsValue.sunShaftIntensity;
		}
		if (reflectionProbe != null)
		{
			if (loadRPValue.kind != 0)
			{
				if (HSceneManager.HResourceTables.ProbeTexInfos.TryGetValue(loadRPValue.kind, out var value))
				{
					Texture customBakedTexture = CommonLib.LoadAsset<Texture>(value.assetbundle, value.asset);
					AssetBundleManager.UnloadAssetBundle(value.assetbundle, isUnloadForceRefCount: true);
					reflectionProbe.customBakedTexture = customBakedTexture;
				}
			}
			else
			{
				SingletonInitializer<BaseMap>.instance.SetDefaultReflectProbeTextrure();
			}
			NowReflectionKind = loadRPValue.kind;
			reflectionProbe.intensity = loadRPValue.intensity;
			reflectionProbe.gameObject.SetActive(loadRPValue.enable);
		}
		RenderSettings.ambientSkyColor = loadEnvironmentValue.sky;
		RenderSettings.ambientEquatorColor = loadEnvironmentValue.equator;
		RenderSettings.ambientGroundColor = loadEnvironmentValue.ground;
		if (vignette != null)
		{
			vignette.active = loadVignetValue.enable;
			vignette.intensity.value = loadVignetValue.intensity;
		}
		if (ssr != null)
		{
			ssr.active = loadSSRValue;
		}
		NowSelfShadowEnable = loadSelfShadowValue;
	}

	public void ResetAllSetting()
	{
		if (bloom != null)
		{
			bloom.active = initBloomValue.enable;
			bloom.intensity.value = initBloomValue.intensity;
			bloom.threshold.value = initBloomValue.threshold;
			bloom.softKnee.value = initBloomValue.softKnee;
			bloom.clamp.overrideState = initBloomValue.clamp;
			bloom.diffusion.value = initBloomValue.diffusion;
			bloom.color.value = initBloomValue.color;
		}
		if (ao != null)
		{
			ao.active = initAmbientValue.enable;
			ao.intensity.value = initAmbientValue.intensity;
			ao.thicknessModifier.value = initAmbientValue.thicknessModifier;
			ao.color.value = initAmbientValue.color;
		}
		if (dof != null)
		{
			dof.enabled = initDoFValue.enable;
			dof.focalSize = initDoFValue.focalSize;
			dof.aperture = initDoFValue.aperture;
		}
		if (colorGrading != null)
		{
			NowColoGradingKind = initColorValue.kind;
			processVolumeColor.weight = initColorValue.blend;
			colorGrading.saturation.value = initColorValue.saturation;
			colorGrading.brightness.value = initColorValue.brightness;
			colorGrading.contrast.value = initColorValue.contrast;
		}
		if (fog != null)
		{
			fog.enabled = initFogValue.enable;
			fog.excludeFarPixels = initFogValue.excludeFarPixels;
			fog.height = initFogValue.height;
			fog.heightDensity = initFogValue.heightDensity;
			RenderSettings.fogColor = initFogValue.color;
			RenderSettings.fogDensity = initFogValue.density;
		}
		if (sunShafts != null)
		{
			sunShafts.enabled = initSunShaftsValue.enable;
			sunShafts.sunThreshold = initSunShaftsValue.sunThreshold;
			sunShafts.sunColor = initSunShaftsValue.sunColor;
			sunShafts.maxRadius = initSunShaftsValue.maxRadius;
			sunShafts.sunShaftBlurRadius = initSunShaftsValue.sunShaftBlurRadius;
			sunShafts.sunShaftIntensity = initSunShaftsValue.sunShaftIntensity;
		}
		if (reflectionProbe != null)
		{
			if (initRPValue.kind != 0)
			{
				if (HSceneManager.HResourceTables.ProbeTexInfos.TryGetValue(initRPValue.kind, out var value))
				{
					Texture customBakedTexture = CommonLib.LoadAsset<Texture>(value.assetbundle, value.asset);
					AssetBundleManager.UnloadAssetBundle(value.assetbundle, isUnloadForceRefCount: true);
					reflectionProbe.customBakedTexture = customBakedTexture;
				}
			}
			else
			{
				SingletonInitializer<BaseMap>.instance.SetDefaultReflectProbeTextrure();
			}
			NowReflectionKind = initRPValue.kind;
			reflectionProbe.intensity = initRPValue.intensity;
			reflectionProbe.gameObject.SetActive(initRPValue.enable);
		}
		RenderSettings.ambientSkyColor = initEnvironmentValue.sky;
		RenderSettings.ambientEquatorColor = initEnvironmentValue.equator;
		RenderSettings.ambientGroundColor = initEnvironmentValue.ground;
		if (vignette != null)
		{
			vignette.active = initVignetValue.enable;
			vignette.intensity.value = initVignetValue.intensity;
		}
		if (ssr != null)
		{
			ssr.active = initSSRValue;
		}
		NowSelfShadowEnable = initSelfShadowValue;
	}

	public void endSave()
	{
		string text = DefaultData.Create("ScreenEffect");
		string text2 = ".bytes";
		string filePath = text + $"tmp_Map{Singleton<HSceneManager>.Instance.mapID:00}" + text2;
		BloomValue saveBloomValue = default(BloomValue);
		if (bloom != null)
		{
			saveBloomValue.enable = bloom.active;
			saveBloomValue.intensity = bloom.intensity.value;
			saveBloomValue.threshold = bloom.threshold.value;
			saveBloomValue.softKnee = bloom.softKnee.value;
			saveBloomValue.clamp = bloom.clamp.overrideState;
			saveBloomValue.diffusion = bloom.diffusion.value;
			saveBloomValue.color = bloom.color.value;
		}
		else
		{
			saveBloomValue.enable = initBloomValue.enable;
			saveBloomValue.intensity = initBloomValue.intensity;
			saveBloomValue.threshold = initBloomValue.threshold;
			saveBloomValue.softKnee = initBloomValue.softKnee;
			saveBloomValue.clamp = initBloomValue.clamp;
			saveBloomValue.diffusion = initBloomValue.diffusion;
			saveBloomValue.color = initBloomValue.color;
		}
		AOValue saveAmbientValue = default(AOValue);
		if (ao != null)
		{
			saveAmbientValue.enable = ao.active;
			saveAmbientValue.intensity = ao.intensity.value;
			saveAmbientValue.thicknessModifier = ao.thicknessModifier.value;
			saveAmbientValue.color = ao.color.value;
		}
		else
		{
			saveAmbientValue.enable = initAmbientValue.enable;
			saveAmbientValue.intensity = initAmbientValue.intensity;
			saveAmbientValue.thicknessModifier = initAmbientValue.thicknessModifier;
			saveAmbientValue.color = initAmbientValue.color;
		}
		DOFValue saveDoFValue = default(DOFValue);
		if (dof != null)
		{
			saveDoFValue.enable = dof.enabled;
			saveDoFValue.focalSize = dof.focalSize;
			saveDoFValue.aperture = dof.aperture;
		}
		else
		{
			saveDoFValue.enable = initDoFValue.enable;
			saveDoFValue.focalSize = initDoFValue.focalSize;
			saveDoFValue.aperture = initDoFValue.aperture;
		}
		ColorGradingValue saveColorValue = default(ColorGradingValue);
		if (colorGrading != null)
		{
			saveColorValue.kind = NowColoGradingKind;
			saveColorValue.blend = processVolumeColor.weight;
			saveColorValue.saturation = colorGrading.saturation.value;
			saveColorValue.brightness = colorGrading.brightness.value;
			saveColorValue.contrast = colorGrading.contrast.value;
		}
		else
		{
			saveColorValue.kind = initColorValue.kind;
			saveColorValue.blend = initColorValue.blend;
			saveColorValue.saturation = initColorValue.saturation;
			saveColorValue.brightness = initColorValue.brightness;
			saveColorValue.contrast = initColorValue.contrast;
		}
		FogValue saveFogValue = default(FogValue);
		if (fog != null)
		{
			saveFogValue.enable = fog.enabled;
			saveFogValue.excludeFarPixels = fog.excludeFarPixels;
			saveFogValue.height = fog.height;
			saveFogValue.heightDensity = fog.heightDensity;
			saveFogValue.color = RenderSettings.fogColor;
			saveFogValue.density = RenderSettings.fogDensity;
		}
		else
		{
			saveFogValue.enable = initFogValue.enable;
			saveFogValue.excludeFarPixels = initFogValue.excludeFarPixels;
			saveFogValue.height = initFogValue.height;
			saveFogValue.heightDensity = initFogValue.heightDensity;
			saveFogValue.color = initFogValue.color;
			saveFogValue.density = initFogValue.density;
		}
		SunShaftsValue saveSunShaftsValue = default(SunShaftsValue);
		if (sunShafts != null)
		{
			saveSunShaftsValue.enable = sunShafts.enabled;
			saveSunShaftsValue.sunThreshold = sunShafts.sunThreshold;
			saveSunShaftsValue.sunColor = sunShafts.sunColor;
			saveSunShaftsValue.maxRadius = sunShafts.maxRadius;
			saveSunShaftsValue.sunShaftBlurRadius = sunShafts.sunShaftBlurRadius;
			saveSunShaftsValue.sunShaftIntensity = sunShafts.sunShaftIntensity;
		}
		else
		{
			saveSunShaftsValue.enable = initSunShaftsValue.enable;
			saveSunShaftsValue.sunThreshold = initSunShaftsValue.sunThreshold;
			saveSunShaftsValue.sunColor = initSunShaftsValue.sunColor;
			saveSunShaftsValue.maxRadius = initSunShaftsValue.maxRadius;
			saveSunShaftsValue.sunShaftBlurRadius = initSunShaftsValue.sunShaftBlurRadius;
			saveSunShaftsValue.sunShaftIntensity = initSunShaftsValue.sunShaftIntensity;
		}
		ReflectionProbeValue saveRPValue = default(ReflectionProbeValue);
		if (reflectionProbe != null)
		{
			saveRPValue.kind = NowReflectionKind;
			saveRPValue.intensity = reflectionProbe.intensity;
			saveRPValue.enable = reflectionProbe.gameObject.activeSelf;
		}
		else
		{
			saveRPValue.kind = initRPValue.kind;
			saveRPValue.intensity = initRPValue.intensity;
			saveRPValue.enable = initRPValue.enable;
		}
		EnvironmentValue saveEnvironmentValue = default(EnvironmentValue);
		saveEnvironmentValue.sky = RenderSettings.ambientSkyColor;
		saveEnvironmentValue.equator = RenderSettings.ambientEquatorColor;
		saveEnvironmentValue.ground = RenderSettings.ambientGroundColor;
		VignetValue saveVignetteValue = default(VignetValue);
		if (vignette != null)
		{
			saveVignetteValue.enable = vignette.active;
			saveVignetteValue.intensity = vignette.intensity.value;
		}
		else
		{
			saveVignetteValue = initVignetValue;
			saveVignetteValue = initVignetValue;
		}
		bool saveSSRValue;
		if (ssr != null)
		{
			saveSSRValue = ssr.active;
		}
		else
		{
			saveSSRValue = initSSRValue;
		}
		bool saveSelfShadowValue = NowSelfShadowEnable;
		Utils.File.OpenWrite(filePath, isAppend: false, delegate(FileStream stream)
		{
			using BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(1);
			binaryWriter.Write("前回の終了時");
			binaryWriter.Write(saveColorValue.kind);
			binaryWriter.Write(saveColorValue.blend);
			binaryWriter.Write(saveColorValue.saturation);
			binaryWriter.Write(saveColorValue.brightness);
			binaryWriter.Write(saveColorValue.contrast);
			binaryWriter.Write(saveAmbientValue.enable);
			binaryWriter.Write(saveAmbientValue.intensity);
			binaryWriter.Write(saveAmbientValue.thicknessModifier);
			binaryWriter.Write(saveAmbientValue.color.r);
			binaryWriter.Write(saveAmbientValue.color.g);
			binaryWriter.Write(saveAmbientValue.color.b);
			binaryWriter.Write(saveAmbientValue.color.a);
			binaryWriter.Write(saveBloomValue.enable);
			binaryWriter.Write(saveBloomValue.intensity);
			binaryWriter.Write(saveBloomValue.threshold);
			binaryWriter.Write(saveBloomValue.softKnee);
			binaryWriter.Write(saveBloomValue.clamp);
			binaryWriter.Write(saveBloomValue.diffusion);
			binaryWriter.Write(saveBloomValue.color.r);
			binaryWriter.Write(saveBloomValue.color.g);
			binaryWriter.Write(saveBloomValue.color.b);
			binaryWriter.Write(saveBloomValue.color.a);
			binaryWriter.Write(saveDoFValue.enable);
			binaryWriter.Write(saveDoFValue.focalSize);
			binaryWriter.Write(saveDoFValue.aperture);
			binaryWriter.Write(saveVignetteValue.enable);
			binaryWriter.Write(saveVignetteValue.intensity);
			binaryWriter.Write(saveSSRValue);
			binaryWriter.Write(saveRPValue.enable);
			binaryWriter.Write(saveRPValue.kind);
			binaryWriter.Write(saveRPValue.intensity);
			binaryWriter.Write(saveFogValue.enable);
			binaryWriter.Write(saveFogValue.excludeFarPixels);
			binaryWriter.Write(saveFogValue.height);
			binaryWriter.Write(saveFogValue.heightDensity);
			binaryWriter.Write(saveFogValue.color.r);
			binaryWriter.Write(saveFogValue.color.g);
			binaryWriter.Write(saveFogValue.color.b);
			binaryWriter.Write(saveFogValue.color.a);
			binaryWriter.Write(saveFogValue.density);
			binaryWriter.Write(saveSunShaftsValue.enable);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.r);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.g);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.b);
			binaryWriter.Write(saveSunShaftsValue.sunThreshold.a);
			binaryWriter.Write(saveSunShaftsValue.sunColor.r);
			binaryWriter.Write(saveSunShaftsValue.sunColor.g);
			binaryWriter.Write(saveSunShaftsValue.sunColor.b);
			binaryWriter.Write(saveSunShaftsValue.sunColor.a);
			binaryWriter.Write(saveSunShaftsValue.maxRadius);
			binaryWriter.Write(saveSunShaftsValue.sunShaftBlurRadius);
			binaryWriter.Write(saveSunShaftsValue.sunShaftIntensity);
			binaryWriter.Write(saveSelfShadowValue);
			binaryWriter.Write(saveEnvironmentValue.sky.r);
			binaryWriter.Write(saveEnvironmentValue.sky.g);
			binaryWriter.Write(saveEnvironmentValue.sky.b);
			binaryWriter.Write(saveEnvironmentValue.sky.a);
			binaryWriter.Write(saveEnvironmentValue.equator.r);
			binaryWriter.Write(saveEnvironmentValue.equator.g);
			binaryWriter.Write(saveEnvironmentValue.equator.b);
			binaryWriter.Write(saveEnvironmentValue.equator.a);
			binaryWriter.Write(saveEnvironmentValue.ground.r);
			binaryWriter.Write(saveEnvironmentValue.ground.g);
			binaryWriter.Write(saveEnvironmentValue.ground.b);
			binaryWriter.Write(saveEnvironmentValue.ground.a);
		});
	}
}
