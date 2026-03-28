using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using MessagePack;
using UnityEngine;

namespace HS2;

public class ChaFemaleRandom : MonoBehaviour
{
	private readonly int[] shapeEyeIdxs = new int[13]
	{
		19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
		29, 30, 31
	};

	private readonly int[] shapeMouseIdxs = new int[1] { 53 };

	private readonly int[] shapeBodyIdxs = new int[22]
	{
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31
	};

	private Dictionary<int, ChaFileControl>[] fileSkinColors = new Dictionary<int, ChaFileControl>[3];

	private Dictionary<int, ChaFileControl>[] fileSkins = new Dictionary<int, ChaFileControl>[4];

	private Dictionary<int, ChaFileControl>[] fileBodys = new Dictionary<int, ChaFileControl>[3];

	private Dictionary<int, ChaFileControl>[] fileFaces = new Dictionary<int, ChaFileControl>[3];

	private Dictionary<int, ChaFileControl>[] fileHairs = new Dictionary<int, ChaFileControl>[5];

	private Dictionary<int, ChaFileControl> fileHairColors = new Dictionary<int, ChaFileControl>();

	private IReadOnlyDictionary<int, SearchAccessoryInfoData.Param> accesoryInfos = new Dictionary<int, SearchAccessoryInfoData.Param>();

	private Dictionary<int, TextAsset> fileCoordinates = new Dictionary<int, TextAsset>();

	private ChaFileControl defChaFile = new ChaFileControl();

	private void Awake()
	{
		for (int i = 0; i < fileSkinColors.Length; i++)
		{
			fileSkinColors[i] = new Dictionary<int, ChaFileControl>();
		}
		for (int j = 0; j < fileSkins.Length; j++)
		{
			fileSkins[j] = new Dictionary<int, ChaFileControl>();
		}
		for (int k = 0; k < fileBodys.Length; k++)
		{
			fileBodys[k] = new Dictionary<int, ChaFileControl>();
		}
		for (int l = 0; l < fileHairs.Length; l++)
		{
			fileHairs[l] = new Dictionary<int, ChaFileControl>();
		}
		for (int m = 0; m < fileFaces.Length; m++)
		{
			fileFaces[m] = new Dictionary<int, ChaFileControl>();
		}
		fileHairColors = new Dictionary<int, ChaFileControl>();
	}

	public IEnumerator Load()
	{
		yield return StartCoroutine(LoadSkinColorPreset());
		yield return StartCoroutine(LoadSkinPreset());
		yield return StartCoroutine(LoadBodyPreset());
		yield return StartCoroutine(LoadHairPreset());
		yield return StartCoroutine(LoadFacePreset());
		yield return StartCoroutine(LoadHairColorPreset());
		yield return StartCoroutine(LoadAccesoryInfosPreset());
		yield return StartCoroutine(LoaCoordinatePreset());
		defChaFile.LoadFromAssetBundle("custom/00/presets_f_00.unity3d", "ill_Default_Female");
	}

	private IEnumerator LoadSkinColorPreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetSkin_ColorPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int num2 = 0;
				int num3 = -1;
				if (text.Contains("def"))
				{
					num2 = 1;
				}
				else
				{
					num3 = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
					if (num3 > 9)
					{
						num2 = 2;
					}
				}
				if (!fileSkinColors[num2].ContainsKey(num3))
				{
					ChaFileControl chaFileControl = new ChaFileControl();
					if (chaFileControl.LoadFromAssetBundle(item, text, noSetPNG: true))
					{
						fileSkinColors[num2].Add(num3, chaFileControl);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadSkinPreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetSkinPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int num2 = 0;
				int num3 = -1;
				if (text.Contains("def"))
				{
					num2 = 3;
				}
				else
				{
					num3 = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
					if (num3 > 19)
					{
						num2 = 2;
					}
					else if (num3 > 9)
					{
						num2 = 1;
					}
				}
				if (!fileSkins[num2].ContainsKey(num3))
				{
					ChaFileControl chaFileControl = new ChaFileControl();
					if (chaFileControl.LoadFromAssetBundle(item, text, noSetPNG: true))
					{
						fileSkins[num2].Add(num3, chaFileControl);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadBodyPreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetBodyPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int num2 = 0;
				int num3 = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
				if (num3 > 19)
				{
					num2 = 2;
				}
				else if (num3 > 9)
				{
					num2 = 1;
				}
				if (!fileBodys[num2].ContainsKey(num3))
				{
					ChaFileControl chaFileControl = new ChaFileControl();
					if (chaFileControl.LoadFromAssetBundle(item, text, noSetPNG: true))
					{
						fileBodys[num2].Add(num3, chaFileControl);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadHairPreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetHairPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int num2 = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
				int num3 = ((num2 > 39) ? 3 : ((num2 > 29) ? 2 : ((num2 > 19) ? 1 : ((num2 <= 9) ? 4 : 0))));
				if (!fileHairs[num3].ContainsKey(num2))
				{
					ChaFileControl chaFileControl = new ChaFileControl();
					if (chaFileControl.LoadFromAssetBundle(item, text, noSetPNG: true))
					{
						fileHairs[num3].Add(num2, chaFileControl);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadFacePreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetFacePath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int num2 = 0;
				int num3 = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
				if (num3 > 19)
				{
					num2 = 2;
				}
				else if (num3 > 9)
				{
					num2 = 1;
				}
				if (!fileFaces[num2].ContainsKey(num3))
				{
					TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(item, text);
					if (!(null == textAsset))
					{
						ChaFileControl chaFileControl = new ChaFileControl();
						chaFileControl.custom.LoadFace(textAsset.bytes);
						fileFaces[num2].Add(num3, chaFileControl);
						AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: true);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadHairColorPreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetHair_ColorPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int key = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
				if (!fileHairColors.ContainsKey(key))
				{
					ChaFileControl chaFileControl = new ChaFileControl();
					if (chaFileControl.LoadFromAssetBundle(item, text, noSetPNG: true))
					{
						fileHairColors.Add(key, chaFileControl);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator LoadAccesoryInfosPreset()
	{
		List<string> list = (from b in CommonLib.GetAssetBundleNameListFromPath(AssetBundleNames.SearchListAccessoryPath, subdirCheck: true)
			orderby b descending
			select b).ToList();
		Dictionary<int, SearchAccessoryInfoData.Param> dic = new Dictionary<int, SearchAccessoryInfoData.Param>();
		list.ForEach(delegate(string file)
		{
			if (GameSystem.IsPathAdd50(file))
			{
				foreach (List<SearchAccessoryInfoData.Param> item in from p in AssetBundleManager.LoadAllAsset(file, typeof(SearchAccessoryInfoData)).GetAllAssets<SearchAccessoryInfoData>()
					select p.param)
				{
					foreach (SearchAccessoryInfoData.Param item2 in item)
					{
						dic[item2.id] = item2;
					}
				}
				AssetBundleManager.UnloadAssetBundle(file, isUnloadForceRefCount: false);
			}
		});
		accesoryInfos = dic.ToDictionary((KeyValuePair<int, SearchAccessoryInfoData.Param> d) => d.Key, (KeyValuePair<int, SearchAccessoryInfoData.Param> d) => d.Value);
		yield return null;
	}

	private IEnumerator LoaCoordinatePreset()
	{
		foreach (string item in (from b in GlobalMethod.GetAssetBundleNameListFromPath(AssetBundleNames.SearchPresetClothPath, subdirCheck: true)
			orderby b descending
			select b).ToList())
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(item, _WithExtension: false);
			foreach (string text in allAssetName)
			{
				int key = int.Parse(YS_Assist.GetStringRight(Path.GetFileNameWithoutExtension(text), 2));
				if (!fileCoordinates.ContainsKey(key))
				{
					TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(item, text);
					if (!(textAsset == null))
					{
						fileCoordinates.Add(key, textAsset);
					}
				}
			}
		}
		yield return null;
	}

	public void RandomHair(ChaControl _chara, int _hairPtn)
	{
		if (!(_chara == null))
		{
			Dictionary<int, ChaFileControl> dictionary = null;
			if (fileHairs.Length > _hairPtn && _hairPtn >= 0)
			{
				dictionary = fileHairs[_hairPtn];
			}
			else
			{
				int num = Random.Range(0, fileHairs.Length);
				dictionary = fileHairs[num];
			}
			byte[] bytes = MessagePackSerializer.Serialize(dictionary.Shuffle().First().Value.custom.hair);
			_chara.fileCustom.hair = MessagePackSerializer.Deserialize<ChaFileHair>(bytes);
			ChaFileHair fileHair = _chara.fileHair;
			ChaFileHair hair = fileHairColors.Shuffle().First().Value.custom.hair;
			for (int i = 0; i < fileHair.parts.Length; i++)
			{
				fileHair.parts[i].baseColor = hair.parts[i].baseColor;
				fileHair.parts[i].topColor = hair.parts[i].topColor;
				fileHair.parts[i].underColor = hair.parts[i].underColor;
				fileHair.parts[i].specular = hair.parts[i].specular;
				fileHair.parts[i].metallic = hair.parts[i].metallic;
				fileHair.parts[i].smoothness = hair.parts[i].smoothness;
			}
		}
	}

	public void RandomFace(ChaControl _chara, bool _mole, bool _elf)
	{
		if (_chara == null)
		{
			return;
		}
		int num = Random.Range(0, 3);
		Dictionary<int, ChaFileControl> self = fileFaces[num];
		ChaFileFace fileFace = _chara.fileFace;
		ChaFileFace face = self.Shuffle().First().Value.custom.face;
		fileFace.headId = face.headId;
		face = self.Shuffle().First().Value.custom.face;
		int[] array = shapeEyeIdxs;
		foreach (int num2 in array)
		{
			fileFace.shapeValueFace[num2] = face.shapeValueFace[num2];
		}
		face = self.Shuffle().First().Value.custom.face;
		array = shapeMouseIdxs;
		foreach (int num3 in array)
		{
			fileFace.shapeValueFace[num3] = face.shapeValueFace[num3];
		}
		face = self.Shuffle().First().Value.custom.face;
		fileFace.eyebrowLayout = new Vector4(face.eyebrowLayout.x, fileFace.eyebrowLayout.y, fileFace.eyebrowLayout.z, fileFace.eyebrowLayout.w);
		fileFace.eyebrowLayout = new Vector4(fileFace.eyebrowLayout.x, face.eyebrowLayout.y, fileFace.eyebrowLayout.z, fileFace.eyebrowLayout.w);
		fileFace.eyebrowTilt = face.eyebrowTilt;
		fileFace.eyebrowColor = _chara.fileHair.parts[0].baseColor;
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		Dictionary<int, ListInfoBase> categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.ft_skin_f);
		fileFace.skinId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
		fileFace.pupilSameSetting = 1 == GetRandomIndex(5, 95);
		for (int j = 0; j < 2; j++)
		{
			categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eye);
			fileFace.pupil[j].pupilId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
			fileFace.pupil[j].pupilColor = HsvColor.ToRgb(Random.Range(0f, 359f), 0.11f, 0.39f);
		}
		if (fileFace.pupilSameSetting)
		{
			fileFace.pupil[1].Copy(fileFace.pupil[0]);
		}
		categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eye_hl);
		fileFace.hlId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
		categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyelash);
		fileFace.eyelashesId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
		categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyeshadow);
		if (GetRandomIndex(50, 50) == 0)
		{
			fileFace.makeup.eyeshadowId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
		}
		else
		{
			fileFace.makeup.eyeshadowId = 0;
		}
		categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_lip);
		if (GetRandomIndex(50, 50) == 0)
		{
			fileFace.makeup.lipId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
			Color lipColor = fileFace.makeup.lipColor;
			lipColor.a = Random.Range(0.2f, 0.5f);
			fileFace.makeup.lipColor = lipColor;
		}
		else
		{
			fileFace.makeup.lipId = 0;
		}
		if (_mole)
		{
			fileFace.moleId = 1;
			Dictionary<int, ListInfoBase> categoryInfo2 = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.mole_layout);
			Dictionary<int, Vector4> dictionary = null;
			List<int> setMoleIDs = new List<int> { 0, 1, 4, 5 };
			dictionary = categoryInfo2.Where((KeyValuePair<int, ListInfoBase> d) => setMoleIDs.Contains(d.Value.Id)).Select((KeyValuePair<int, ListInfoBase> val, int idx) => new
			{
				idx = idx,
				x = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
				y = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
				z = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosX),
				w = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosY)
			}).ToDictionary(v => v.idx, v => new Vector4
			{
				x = v.x,
				y = v.y,
				z = v.z,
				w = v.w
			});
			fileFace.moleLayout = dictionary[Random.Range(0, dictionary.Count)];
		}
		else
		{
			fileFace.moleId = 0;
		}
		if (_elf)
		{
			fileFace.shapeValueFace[54] = Random.Range(0.2f, 0.8f);
			fileFace.shapeValueFace[55] = Random.value;
			fileFace.shapeValueFace[56] = Random.value;
			fileFace.shapeValueFace[57] = Random.Range(0.8f, 0.9f);
		}
		else
		{
			ChaFileFace face2 = defChaFile.custom.face;
			fileFace.shapeValueFace[54] = face2.shapeValueFace[54];
			fileFace.shapeValueFace[55] = face2.shapeValueFace[55];
			fileFace.shapeValueFace[56] = face2.shapeValueFace[56];
			fileFace.shapeValueFace[57] = face2.shapeValueFace[57];
		}
	}

	public void RandomBody(ChaControl _chara, int _skin, int _skinColor, int _height, int _breast, int _shape)
	{
		if (!(_chara == null))
		{
			ChaFileBody fileBody = _chara.fileBody;
			int num = ((_skin >= fileSkins.Length || _skin < 0) ? Random.Range(0, fileSkins.Length) : _skin);
			ChaFileBody body = fileSkins[num].Shuffle().First().Value.custom.body;
			fileBody.skinId = body.skinId;
			fileBody.detailId = body.detailId;
			fileBody.detailPower = body.detailPower;
			int num2 = ((_skinColor >= fileSkinColors.Length || _skinColor < 0) ? Random.Range(0, fileSkinColors.Length) : _skinColor);
			body = fileSkinColors[num2].Shuffle().First().Value.custom.body;
			fileBody.skinColor = body.skinColor;
			int num3 = ((_height >= fileBodys.Length || _height < 0) ? Random.Range(0, fileBodys.Length) : _height);
			body = fileBodys[num3].Shuffle().First().Value.custom.body;
			fileBody.shapeValueBody[0] = body.shapeValueBody[0];
			int num4 = ((_breast >= fileBodys.Length || _breast < 0) ? Random.Range(0, fileBodys.Length) : _breast);
			body = fileBodys[num4].Shuffle().First().Value.custom.body;
			fileBody.shapeValueBody[1] = body.shapeValueBody[1];
			fileBody.bustSoftness = body.bustSoftness;
			fileBody.bustWeight = body.bustWeight;
			int num5 = ((_shape >= fileBodys.Length || _shape < 0) ? Random.Range(0, fileBodys.Length) : _shape);
			body = fileBodys[num5].Shuffle().First().Value.custom.body;
			int[] array = shapeBodyIdxs;
			foreach (int num6 in array)
			{
				fileBody.shapeValueBody[num6] = body.shapeValueBody[num6];
			}
			Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_nip);
			fileBody.nipId = categoryInfo.Keys.ElementAt(Random.Range(0, categoryInfo.Keys.Count));
			fileBody.underhairColor = _chara.fileHair.parts[0].baseColor;
		}
	}

	public void RandomClothAndAccessory(ChaControl _chara, bool _glasses)
	{
		if (_chara == null)
		{
			return;
		}
		_chara.nowCoordinate.LoadFile(fileCoordinates.Shuffle().First().Value);
		if (_glasses)
		{
			int slotNo = -1;
			for (int i = 0; i < _chara.nowCoordinate.accessory.parts.Length; i++)
			{
				if (_chara.nowCoordinate.accessory.parts[i].type == 350)
				{
					slotNo = i;
					break;
				}
			}
			SearchAccessoryInfoData.Param value = accesoryInfos.Shuffle().First().Value;
			_chara.loadWithDefaultColorAndPtn = true;
			_chara.ChangeAccessory(slotNo, value.category, value.accessoryID, "", forceChange: true);
			_chara.loadWithDefaultColorAndPtn = false;
		}
		_chara.AssignCoordinate();
	}

	public void RandomPersonal(ChaControl _chara, int _positive)
	{
		if (_chara == null)
		{
			return;
		}
		ChaFileParameter fileParam = _chara.fileParam;
		ChaRandomName chaRandomName = new ChaRandomName();
		chaRandomName.Initialize();
		fileParam.fullname = chaRandomName.GetRandName(1);
		ChaFileParameter fileParam2 = _chara.fileParam;
		ChaFileParameter2 fileParam3 = _chara.fileParam2;
		List<VoiceInfo.Param> list = (from info in Voice.infoTable
			where info.Value.No >= 0
			select info.Value).ToList();
		List<int> list2 = new List<int>();
		foreach (VoiceInfo.Param item in list)
		{
			if (_positive == 0)
			{
				if (!item.isPositive)
				{
					list2.Add(item.No);
				}
			}
			else if (1 == _positive)
			{
				if (item.isPositive)
				{
					list2.Add(item.No);
				}
			}
			else
			{
				list2.Add(item.No);
			}
		}
		fileParam3.personality = list2.Shuffle().FirstOrDefault();
		fileParam3.voiceRate = Random.value;
		int max = Game.infoTraitTable.Count();
		fileParam3.trait = (byte)Random.Range(0, max);
		max = Game.infoMindTable.Count();
		fileParam3.mind = (byte)Random.Range(0, max);
		max = Game.infoHAttributeTable.Count();
		fileParam3.hAttribute = (byte)Random.Range(0, max);
		fileParam2.birthMonth = (byte)Random.Range(1, 13);
		int[] array = new int[12]
		{
			31, 29, 31, 30, 31, 30, 31, 31, 30, 31,
			30, 31
		};
		fileParam2.birthDay = (byte)Random.Range(1, array[fileParam2.birthMonth - 1] + 1);
	}

	public int GetRandomIndex(params int[] weightTable)
	{
		int num = weightTable.Sum();
		int num2 = Random.Range(1, num + 1);
		int result = -1;
		for (int i = 0; i < weightTable.Length; i++)
		{
			if (weightTable[i] >= num2)
			{
				result = i;
				break;
			}
			num2 -= weightTable[i];
		}
		return result;
	}
}
