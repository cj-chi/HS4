using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using UnityEngine;

namespace Studio;

public class SceneInfo
{
	public class MapInfo
	{
		public int no = -1;

		public ChangeAmount ca = new ChangeAmount();

		public bool option = true;

		public bool light = true;
	}

	[MessagePackObject(true)]
	public class SkyInfo
	{
		public bool Enable { get; set; }

		public int Pattern { get; set; }

		public SkyInfo()
		{
			Enable = false;
			Pattern = 0;
		}
	}

	private readonly Version m_Version = new Version(1, 1, 1);

	public Dictionary<int, ObjectInfo> dicObject;

	public MapInfo mapInfo = new MapInfo();

	public int cgLookupTexture;

	public float cgBlend;

	public int cgSaturation;

	public int cgBrightness;

	public int cgContrast;

	public bool enableAmbientOcclusion;

	public float aoIntensity;

	public float aoThicknessModeifier;

	public Color aoColor;

	public bool enableBloom;

	public float bloomIntensity;

	public float bloomThreshold;

	public float bloomSoftKnee;

	public bool bloomClamp;

	public float bloomDiffusion;

	public Color bloomColor;

	public bool enableDepth;

	public int depthForcus;

	public float depthFocalSize;

	public float depthAperture;

	public bool enableVignette;

	public float vignetteIntensity;

	public bool enableSSR;

	public bool enableReflectionProbe;

	public int reflectionProbeCubemap;

	public float reflectionProbeIntensity;

	public bool enableFog;

	public bool fogExcludeFarPixels;

	public float fogHeight;

	public float fogHeightDensity;

	public Color fogColor;

	public float fogDensity;

	public bool enableSunShafts;

	public int sunCaster;

	public Color sunThresholdColor;

	public Color sunColor;

	public float sunDistanceFallOff;

	public float sunBlurSize;

	public float sunIntensity;

	public bool enableShadow;

	public Color environmentLightingSkyColor;

	public Color environmentLightingEquatorColor;

	public Color environmentLightingGroundColor;

	public SkyInfo skyInfo = new SkyInfo();

	public CameraControl.CameraData cameraSaveData;

	public CameraControl.CameraData[] cameraData;

	public CameraLightCtrl.LightInfo charaLight = new CameraLightCtrl.LightInfo();

	public CameraLightCtrl.MapLightInfo mapLight = new CameraLightCtrl.MapLightInfo();

	public BGMCtrl bgmCtrl = new BGMCtrl();

	public ENVCtrl envCtrl = new ENVCtrl();

	public OutsideSoundCtrl outsideSoundCtrl = new OutsideSoundCtrl();

	public string background = "";

	public string frame = "";

	private HashSet<int> hashIndex;

	private int lightCount;

	public Version version => m_Version;

	public Dictionary<int, ObjectInfo> dicImport { get; private set; }

	public Dictionary<int, int> dicChangeKey { get; private set; }

	public bool isLightCheck => lightCount < 2;

	public bool isLightLimitOver => lightCount > 2;

	public Version dataVersion { get; set; }

	public void Init()
	{
		dicObject.Clear();
		mapInfo.no = -1;
		mapInfo.ca.Reset();
		mapInfo.option = true;
		mapInfo.light = true;
		cgLookupTexture = ScreenEffectDefine.ColorGradingLookupTexture;
		cgBlend = ScreenEffectDefine.ColorGradingBlend;
		cgSaturation = ScreenEffectDefine.ColorGradingSaturation;
		cgBrightness = ScreenEffectDefine.ColorGradingBrightness;
		cgContrast = ScreenEffectDefine.ColorGradingContrast;
		enableAmbientOcclusion = ScreenEffectDefine.AmbientOcclusion;
		aoIntensity = ScreenEffectDefine.AmbientOcclusionIntensity;
		aoThicknessModeifier = ScreenEffectDefine.AmbientOcclusionThicknessModeifier;
		aoColor = ScreenEffectDefine.AmbientOcclusionColor;
		enableBloom = ScreenEffectDefine.Bloom;
		bloomIntensity = ScreenEffectDefine.BloomIntensity;
		bloomThreshold = ScreenEffectDefine.BloomThreshold;
		bloomSoftKnee = ScreenEffectDefine.BloomSoftKnee;
		bloomClamp = ScreenEffectDefine.BloomClamp;
		bloomDiffusion = ScreenEffectDefine.BloomDiffusion;
		bloomColor = ScreenEffectDefine.BloomColor;
		enableDepth = ScreenEffectDefine.DepthOfField;
		depthForcus = ScreenEffectDefine.DepthOfFieldForcus;
		depthFocalSize = ScreenEffectDefine.DepthOfFieldFocalSize;
		depthAperture = ScreenEffectDefine.DepthOfFieldAperture;
		enableVignette = ScreenEffectDefine.Vignette;
		vignetteIntensity = ScreenEffectDefine.VignetteIntensity;
		enableSSR = ScreenEffectDefine.ScreenSpaceReflections;
		enableReflectionProbe = ScreenEffectDefine.ReflectionProbe;
		reflectionProbeCubemap = ScreenEffectDefine.ReflectionProbeCubemap;
		reflectionProbeIntensity = ScreenEffectDefine.ReflectionProbeIntensity;
		enableFog = ScreenEffectDefine.Fog;
		fogExcludeFarPixels = ScreenEffectDefine.FogExcludeFarPixels;
		fogHeight = ScreenEffectDefine.FogHeight;
		fogHeightDensity = ScreenEffectDefine.FogHeightDensity;
		fogColor = ScreenEffectDefine.FogColor;
		fogDensity = ScreenEffectDefine.FogDensity;
		enableSunShafts = ScreenEffectDefine.SunShaft;
		sunCaster = ScreenEffectDefine.SunShaftCaster;
		sunThresholdColor = ScreenEffectDefine.SunShaftThresholdColor;
		sunColor = ScreenEffectDefine.SunShaftShaftsColor;
		sunDistanceFallOff = ScreenEffectDefine.SunShaftDistanceFallOff;
		sunBlurSize = ScreenEffectDefine.SunShaftBlurSize;
		sunIntensity = ScreenEffectDefine.SunShaftIntensity;
		enableShadow = true;
		environmentLightingSkyColor = ScreenEffectDefine.EnvironmentLightingSkyColor;
		environmentLightingEquatorColor = ScreenEffectDefine.EnvironmentLightingEquatorColor;
		environmentLightingGroundColor = ScreenEffectDefine.EnvironmentLightingGroundColor;
		skyInfo.Enable = false;
		skyInfo.Pattern = 0;
		cameraSaveData = null;
		cameraData = new CameraControl.CameraData[10];
		if (Singleton<Studio>.IsInstance())
		{
			for (int i = 0; i < 10; i++)
			{
				cameraData[i] = Singleton<Studio>.Instance.cameraCtrl.ExportResetData();
			}
		}
		charaLight.Init();
		mapLight.Init();
		bgmCtrl.play = false;
		bgmCtrl.repeat = BGMCtrl.Repeat.All;
		bgmCtrl.no = 0;
		envCtrl.play = false;
		envCtrl.repeat = BGMCtrl.Repeat.All;
		envCtrl.no = 0;
		outsideSoundCtrl.play = false;
		outsideSoundCtrl.repeat = BGMCtrl.Repeat.All;
		outsideSoundCtrl.fileName = "";
		background = "";
		frame = "";
		hashIndex.Clear();
		lightCount = 0;
		dataVersion = m_Version;
	}

	public int GetNewIndex()
	{
		for (int i = 0; MathfEx.RangeEqualOn(0, i, int.MaxValue); i++)
		{
			if (!hashIndex.Contains(i))
			{
				hashIndex.Add(i);
				return i;
			}
		}
		return -1;
	}

	public int CheckNewIndex()
	{
		for (int i = -1; MathfEx.RangeEqualOn(0, i, int.MaxValue); i++)
		{
			if (!hashIndex.Contains(i))
			{
				return i;
			}
		}
		return -1;
	}

	public bool SetNewIndex(int _index)
	{
		return hashIndex.Add(_index);
	}

	public bool DeleteIndex(int _index)
	{
		return hashIndex.Remove(_index);
	}

	public bool Save(string _path)
	{
		using (FileStream output = new FileStream(_path, FileMode.Create, FileAccess.Write))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			byte[] array = null;
			array = Singleton<Studio>.Instance.gameScreenShot.CreatePngScreen(320, 180);
			binaryWriter.Write(array);
			binaryWriter.Write(m_Version.ToString());
			Save(binaryWriter, dicObject);
			binaryWriter.Write(mapInfo.no);
			mapInfo.ca.Save(binaryWriter);
			binaryWriter.Write(mapInfo.option);
			binaryWriter.Write(mapInfo.light);
			binaryWriter.Write(cgLookupTexture);
			binaryWriter.Write(cgBlend);
			binaryWriter.Write(cgSaturation);
			binaryWriter.Write(cgBrightness);
			binaryWriter.Write(cgContrast);
			binaryWriter.Write(enableAmbientOcclusion);
			binaryWriter.Write(aoIntensity);
			binaryWriter.Write(aoThicknessModeifier);
			binaryWriter.Write(JsonUtility.ToJson(aoColor));
			binaryWriter.Write(enableBloom);
			binaryWriter.Write(bloomIntensity);
			binaryWriter.Write(bloomThreshold);
			binaryWriter.Write(bloomSoftKnee);
			binaryWriter.Write(bloomClamp);
			binaryWriter.Write(bloomDiffusion);
			binaryWriter.Write(JsonUtility.ToJson(bloomColor));
			binaryWriter.Write(enableDepth);
			binaryWriter.Write(depthForcus);
			binaryWriter.Write(depthFocalSize);
			binaryWriter.Write(depthAperture);
			binaryWriter.Write(enableVignette);
			binaryWriter.Write(vignetteIntensity);
			binaryWriter.Write(enableSSR);
			binaryWriter.Write(enableReflectionProbe);
			binaryWriter.Write(reflectionProbeCubemap);
			binaryWriter.Write(reflectionProbeIntensity);
			binaryWriter.Write(enableFog);
			binaryWriter.Write(fogExcludeFarPixels);
			binaryWriter.Write(fogHeight);
			binaryWriter.Write(fogHeightDensity);
			binaryWriter.Write(JsonUtility.ToJson(fogColor));
			binaryWriter.Write(fogDensity);
			binaryWriter.Write(enableSunShafts);
			binaryWriter.Write(sunCaster);
			binaryWriter.Write(JsonUtility.ToJson(sunThresholdColor));
			binaryWriter.Write(JsonUtility.ToJson(sunColor));
			binaryWriter.Write(sunDistanceFallOff);
			binaryWriter.Write(sunBlurSize);
			binaryWriter.Write(sunIntensity);
			binaryWriter.Write(enableShadow);
			binaryWriter.Write(JsonUtility.ToJson(environmentLightingSkyColor));
			binaryWriter.Write(JsonUtility.ToJson(environmentLightingEquatorColor));
			binaryWriter.Write(JsonUtility.ToJson(environmentLightingGroundColor));
			byte[] array2 = MessagePackSerializer.Serialize(skyInfo);
			binaryWriter.Write(array2.Length);
			binaryWriter.Write(array2);
			cameraSaveData.Save(binaryWriter);
			for (int i = 0; i < 10; i++)
			{
				cameraData[i].Save(binaryWriter);
			}
			charaLight.Save(binaryWriter, m_Version);
			mapLight.Save(binaryWriter, m_Version);
			bgmCtrl.Save(binaryWriter, m_Version);
			envCtrl.Save(binaryWriter, m_Version);
			outsideSoundCtrl.Save(binaryWriter, m_Version);
			binaryWriter.Write(background);
			binaryWriter.Write(frame);
			binaryWriter.Write("【StudioNEOV2】");
		}
		return true;
	}

	public void Save(BinaryWriter _writer, Dictionary<int, ObjectInfo> _dicObject)
	{
		int count = _dicObject.Count;
		_writer.Write(count);
		foreach (KeyValuePair<int, ObjectInfo> item in _dicObject)
		{
			_writer.Write(item.Key);
			item.Value.Save(_writer, m_Version);
		}
	}

	public bool Load(string _path)
	{
		Version _dataVersion;
		return Load(_path, out _dataVersion);
	}

	public bool Load(string _path, out Version _dataVersion)
	{
		using (FileStream input = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			PngFile.SkipPng(binaryReader);
			dataVersion = new Version(binaryReader.ReadString());
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				ObjectInfo objectInfo = null;
				switch (num3)
				{
				case 0:
					objectInfo = new OICharInfo(null, -1);
					break;
				case 1:
					objectInfo = new OIItemInfo(-1, -1, -1, -1);
					break;
				case 2:
					objectInfo = new OILightInfo(-1, -1);
					break;
				case 3:
					objectInfo = new OIFolderInfo(-1);
					break;
				case 4:
					objectInfo = new OIRouteInfo(-1);
					break;
				case 5:
					objectInfo = new OICameraInfo(-1);
					break;
				}
				objectInfo.Load(binaryReader, dataVersion, _import: false);
				dicObject.Add(num2, objectInfo);
				hashIndex.Add(num2);
			}
			mapInfo.no = binaryReader.ReadInt32();
			mapInfo.ca.Load(binaryReader);
			mapInfo.option = binaryReader.ReadBoolean();
			if (dataVersion.CompareTo(new Version(1, 1, 0)) >= 0)
			{
				mapInfo.light = binaryReader.ReadBoolean();
			}
			cgLookupTexture = binaryReader.ReadInt32();
			cgBlend = binaryReader.ReadSingle();
			cgSaturation = binaryReader.ReadInt32();
			cgBrightness = binaryReader.ReadInt32();
			cgContrast = binaryReader.ReadInt32();
			enableAmbientOcclusion = binaryReader.ReadBoolean();
			aoIntensity = binaryReader.ReadSingle();
			aoThicknessModeifier = binaryReader.ReadSingle();
			aoColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			enableBloom = binaryReader.ReadBoolean();
			bloomIntensity = binaryReader.ReadSingle();
			bloomThreshold = binaryReader.ReadSingle();
			bloomSoftKnee = binaryReader.ReadSingle();
			bloomClamp = binaryReader.ReadBoolean();
			bloomDiffusion = binaryReader.ReadSingle();
			bloomColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			enableDepth = binaryReader.ReadBoolean();
			depthForcus = binaryReader.ReadInt32();
			depthFocalSize = binaryReader.ReadSingle();
			depthAperture = binaryReader.ReadSingle();
			enableVignette = binaryReader.ReadBoolean();
			if (dataVersion.CompareTo(new Version(1, 1, 1)) >= 0)
			{
				vignetteIntensity = binaryReader.ReadSingle();
			}
			enableSSR = binaryReader.ReadBoolean();
			enableReflectionProbe = binaryReader.ReadBoolean();
			reflectionProbeCubemap = binaryReader.ReadInt32();
			reflectionProbeIntensity = binaryReader.ReadSingle();
			enableFog = binaryReader.ReadBoolean();
			fogExcludeFarPixels = binaryReader.ReadBoolean();
			fogHeight = binaryReader.ReadSingle();
			fogHeightDensity = binaryReader.ReadSingle();
			fogColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			fogDensity = binaryReader.ReadSingle();
			enableSunShafts = binaryReader.ReadBoolean();
			sunCaster = binaryReader.ReadInt32();
			sunThresholdColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			sunColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			sunDistanceFallOff = binaryReader.ReadSingle();
			sunBlurSize = binaryReader.ReadSingle();
			sunIntensity = binaryReader.ReadSingle();
			enableShadow = binaryReader.ReadBoolean();
			environmentLightingSkyColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			environmentLightingEquatorColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			environmentLightingGroundColor = JsonUtility.FromJson<Color>(binaryReader.ReadString());
			if (dataVersion.CompareTo(new Version(1, 1, 0)) >= 0)
			{
				skyInfo = MessagePackSerializer.Deserialize<SkyInfo>(binaryReader.ReadBytes(binaryReader.ReadInt32()));
			}
			if (cameraSaveData == null)
			{
				cameraSaveData = new CameraControl.CameraData();
			}
			cameraSaveData.Load(binaryReader);
			for (int j = 0; j < 10; j++)
			{
				CameraControl.CameraData cameraData = new CameraControl.CameraData();
				cameraData.Load(binaryReader);
				this.cameraData[j] = cameraData;
			}
			charaLight.Load(binaryReader, dataVersion);
			mapLight.Load(binaryReader, dataVersion);
			bgmCtrl.Load(binaryReader, dataVersion);
			envCtrl.Load(binaryReader, dataVersion);
			outsideSoundCtrl.Load(binaryReader, dataVersion);
			background = binaryReader.ReadString();
			if (dataVersion.CompareTo(new Version(1, 1, 0)) <= 0 && !background.IsNullOrEmpty())
			{
				background = BackgroundListAssist.GetFilePath(background);
			}
			frame = binaryReader.ReadString();
			_dataVersion = dataVersion;
		}
		return true;
	}

	public bool Import(string _path)
	{
		using (FileStream input = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			PngFile.SkipPng(binaryReader);
			Version version = new Version(binaryReader.ReadString());
			Import(binaryReader, version);
		}
		return true;
	}

	public void Import(BinaryReader _reader, Version _version)
	{
		dicImport = new Dictionary<int, ObjectInfo>();
		dicChangeKey = new Dictionary<int, int>();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int value = _reader.ReadInt32();
			int num2 = _reader.ReadInt32();
			ObjectInfo objectInfo = null;
			switch (num2)
			{
			case 0:
				objectInfo = new OICharInfo(null, Studio.GetNewIndex());
				break;
			case 1:
				objectInfo = new OIItemInfo(-1, -1, -1, Studio.GetNewIndex());
				break;
			case 2:
				objectInfo = new OILightInfo(-1, Studio.GetNewIndex());
				break;
			case 3:
				objectInfo = new OIFolderInfo(Studio.GetNewIndex());
				break;
			case 4:
				objectInfo = new OIRouteInfo(Studio.GetNewIndex());
				break;
			case 5:
				objectInfo = new OICameraInfo(Studio.GetNewIndex());
				break;
			}
			objectInfo.Load(_reader, _version, _import: true);
			dicObject.Add(objectInfo.dicKey, objectInfo);
			dicImport.Add(objectInfo.dicKey, objectInfo);
			dicChangeKey.Add(objectInfo.dicKey, value);
		}
	}

	public SceneInfo()
	{
		dicObject = new Dictionary<int, ObjectInfo>();
		if (mapInfo == null)
		{
			mapInfo = new MapInfo();
		}
		cameraData = new CameraControl.CameraData[10];
		for (int i = 0; i < cameraData.Length; i++)
		{
			cameraData[i] = new CameraControl.CameraData();
		}
		hashIndex = new HashSet<int>();
		ChangeAmount ca = mapInfo.ca;
		ca.onChangePos = (Action)Delegate.Combine(ca.onChangePos, new Action(Singleton<MapCtrl>.Instance.Reflect));
		ChangeAmount ca2 = mapInfo.ca;
		ca2.onChangeRot = (Action)Delegate.Combine(ca2.onChangeRot, new Action(Singleton<MapCtrl>.Instance.Reflect));
		Init();
	}
}
