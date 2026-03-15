using System;
using System.Collections.Generic;
using System.IO;
using Manager;
using MessagePack;
using UnityEngine;

namespace AIChara;

public class ChaFileControl : ChaFile
{
	public bool skipRangeCheck;

	public static bool CheckDataRangeFace(ChaFile chafile, List<string> checkInfo = null)
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		ChaFileFace face = chafile.custom.face;
		byte sex = chafile.parameter.sex;
		bool result = false;
		for (int i = 0; i < face.shapeValueFace.Length; i++)
		{
			if (!MathfEx.RangeEqualOn(0f, face.shapeValueFace[i], 1f))
			{
				checkInfo?.Add($"【{ChaFileDefine.cf_headshapename[i]}】値:{face.shapeValueFace[i]}");
				result = true;
				face.shapeValueFace[i] = Mathf.Clamp(face.shapeValueFace[i], 0f, 1f);
			}
		}
		if (face.shapeValueFace.Length > ChaFileDefine.cf_headshapename.Length)
		{
			float[] array = new float[ChaFileDefine.cf_headshapename.Length];
			Array.Copy(face.shapeValueFace, array, array.Length);
			face.shapeValueFace = new float[ChaFileDefine.cf_headshapename.Length];
			Array.Copy(array, face.shapeValueFace, array.Length);
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mo_head : ChaListDefine.CategoryNo.fo_head).ContainsKey(face.headId))
		{
			checkInfo?.Add($"【頭の種類】値:{face.headId}");
			result = true;
			face.headId = ((sex == 0) ? 0 : 0);
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mt_skin_f : ChaListDefine.CategoryNo.ft_skin_f).ContainsKey(face.skinId))
		{
			checkInfo?.Add($"【肌の種類】値:{face.skinId}");
			result = true;
			face.skinId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mt_detail_f : ChaListDefine.CategoryNo.ft_detail_f).ContainsKey(face.detailId))
		{
			checkInfo?.Add($"【彫りの種類】値:{face.detailId}");
			result = true;
			face.detailId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyebrow).ContainsKey(face.eyebrowId))
		{
			checkInfo?.Add($"【眉毛の種類】値:{face.eyebrowId}");
			result = true;
			face.eyebrowId = 0;
		}
		for (int j = 0; j < 2; j++)
		{
			if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eye).ContainsKey(face.pupil[j].pupilId))
			{
				checkInfo?.Add($"【瞳の種類】値:{face.pupil[j].pupilId}");
				result = true;
				face.pupil[j].pupilId = 0;
			}
			if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyeblack).ContainsKey(face.pupil[j].blackId))
			{
				checkInfo?.Add($"【黒目の種類】値:{face.pupil[j].blackId}");
				result = true;
				face.pupil[j].blackId = 0;
			}
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eye_hl).ContainsKey(face.hlId))
		{
			checkInfo?.Add($"【ハイライト種類】値:{face.hlId}");
			result = true;
			face.hlId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyelash).ContainsKey(face.eyelashesId))
		{
			checkInfo?.Add($"【睫毛の種類】値:{face.eyelashesId}");
			result = true;
			face.eyelashesId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_mole).ContainsKey(face.moleId))
		{
			checkInfo?.Add($"【ホクロの種類】値:{face.moleId}");
			result = true;
			face.moleId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_eyeshadow).ContainsKey(face.makeup.eyeshadowId))
		{
			checkInfo?.Add($"【アイシャドウ種類】値:{face.makeup.eyeshadowId}");
			result = true;
			face.makeup.eyeshadowId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_cheek).ContainsKey(face.makeup.cheekId))
		{
			checkInfo?.Add($"【チークの種類】値:{face.makeup.cheekId}");
			result = true;
			face.makeup.cheekId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_lip).ContainsKey(face.makeup.lipId))
		{
			checkInfo?.Add($"【リップの種類】値:{face.makeup.lipId}");
			result = true;
			face.makeup.lipId = 0;
		}
		for (int k = 0; k < 2; k++)
		{
			if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_paint).ContainsKey(face.makeup.paintInfo[k].id))
			{
				checkInfo?.Add($"【ペイント種類】値:{face.makeup.paintInfo[k].id}");
				result = true;
				face.makeup.paintInfo[k].id = 0;
			}
		}
		if (sex == 0 && !chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.mt_beard).ContainsKey(face.beardId))
		{
			checkInfo?.Add($"【ヒゲの種類】値:{face.beardId}");
			result = true;
			face.beardId = 0;
		}
		return result;
	}

	public static bool CheckDataRangeBody(ChaFile chafile, List<string> checkInfo = null)
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		ChaFileBody body = chafile.custom.body;
		byte sex = chafile.parameter.sex;
		bool result = false;
		for (int i = 0; i < body.shapeValueBody.Length; i++)
		{
			if (!MathfEx.RangeEqualOn(0f, body.shapeValueBody[i], 1f))
			{
				checkInfo?.Add($"【{ChaFileDefine.cf_bodyshapename[i]}】値:{body.shapeValueBody[i]}");
				result = true;
				body.shapeValueBody[i] = Mathf.Clamp(body.shapeValueBody[i], 0f, 1f);
			}
		}
		if (!MathfEx.RangeEqualOn(0f, body.bustSoftness, 1f))
		{
			checkInfo?.Add($"【胸の柔らかさ】値:{body.bustSoftness}");
			result = true;
			body.bustSoftness = Mathf.Clamp(body.bustSoftness, 0f, 1f);
		}
		if (!MathfEx.RangeEqualOn(0f, body.bustWeight, 1f))
		{
			checkInfo?.Add($"【胸の重さ】値:{body.bustWeight}");
			result = true;
			body.bustWeight = Mathf.Clamp(body.bustWeight, 0f, 1f);
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mt_skin_b : ChaListDefine.CategoryNo.ft_skin_b).ContainsKey(body.skinId))
		{
			checkInfo?.Add($"【肌の種類】値:{body.skinId}");
			result = true;
			body.skinId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mt_detail_b : ChaListDefine.CategoryNo.ft_detail_b).ContainsKey(body.detailId))
		{
			checkInfo?.Add($"【筋肉の種類】値:{body.detailId}");
			result = true;
			body.detailId = 0;
		}
		if (!chaListCtrl.GetCategoryInfo((sex == 0) ? ChaListDefine.CategoryNo.mt_sunburn : ChaListDefine.CategoryNo.ft_sunburn).ContainsKey(body.sunburnId))
		{
			checkInfo?.Add($"【日焼けの種類】値:{body.sunburnId}");
			result = true;
			body.sunburnId = 0;
		}
		for (int j = 0; j < 2; j++)
		{
			if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_paint).ContainsKey(body.paintInfo[j].id))
			{
				checkInfo?.Add($"【ペイントの種類】値:{body.paintInfo[j].id}");
				result = true;
				body.paintInfo[j].id = 0;
			}
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_nip).ContainsKey(body.nipId))
		{
			checkInfo?.Add($"【乳首の種類】値:{body.nipId}");
			result = true;
			body.nipId = 0;
		}
		if (!MathfEx.RangeEqualOn(0f, body.areolaSize, 1f))
		{
			checkInfo?.Add($"【乳輪のサイズ】値:{body.areolaSize}");
			result = true;
			body.areolaSize = Mathf.Clamp(body.areolaSize, 0f, 1f);
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_underhair).ContainsKey(body.underhairId))
		{
			checkInfo?.Add($"【アンダーヘア種類】値:{body.underhairId}");
			result = true;
			body.underhairId = 0;
		}
		return result;
	}

	public static bool CheckDataRangeHair(ChaFile chafile, List<string> checkInfo = null)
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		ChaFileHair hair = chafile.custom.hair;
		byte sex = chafile.parameter.sex;
		bool result = false;
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.so_hair_b).ContainsKey(hair.parts[0].id))
		{
			checkInfo?.Add($"【後ろ髪】値:{hair.parts[0].id}");
			result = true;
			hair.parts[0].id = ((sex == 0) ? 0 : 0);
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.so_hair_f).ContainsKey(hair.parts[1].id))
		{
			checkInfo?.Add($"【前髪】値:{hair.parts[1].id}");
			result = true;
			hair.parts[1].id = ((sex != 0) ? 1 : 2);
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.so_hair_s).ContainsKey(hair.parts[2].id))
		{
			checkInfo?.Add($"【横髪】値:{hair.parts[2].id}");
			result = true;
			hair.parts[2].id = ((sex == 0) ? 0 : 0);
		}
		if (!chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.so_hair_o).ContainsKey(hair.parts[3].id))
		{
			checkInfo?.Add($"【エクステ】値:{hair.parts[3].id}");
			result = true;
			hair.parts[3].id = ((sex == 0) ? 0 : 0);
		}
		return result;
	}

	public static bool CheckDataRangeCoordinate(ChaFile chafile, List<string> checkInfo = null)
	{
		return CheckDataRangeCoordinate(chafile.coordinate, chafile.parameter.sex, checkInfo);
	}

	public static bool CheckDataRangeCoordinate(ChaFileCoordinate coorde, int sex, List<string> checkInfo = null)
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		bool result = false;
		string[] array = new string[8] { "トップス", "ボトムス", "インナー上", "インナー下", "手袋", "パンスト", "靴下", "靴" };
		ChaListDefine.CategoryNo[] array2 = new ChaListDefine.CategoryNo[8]
		{
			(sex == 0) ? ChaListDefine.CategoryNo.mo_top : ChaListDefine.CategoryNo.fo_top,
			(sex == 0) ? ChaListDefine.CategoryNo.mo_bot : ChaListDefine.CategoryNo.fo_bot,
			(sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_inner_t,
			(sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_inner_b,
			(sex == 0) ? ChaListDefine.CategoryNo.mo_gloves : ChaListDefine.CategoryNo.fo_gloves,
			(sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_panst,
			(sex == 0) ? ChaListDefine.CategoryNo.unknown : ChaListDefine.CategoryNo.fo_socks,
			(sex == 0) ? ChaListDefine.CategoryNo.mo_shoes : ChaListDefine.CategoryNo.fo_shoes
		};
		int[] array3 = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
		int[] array4 = new int[8]
		{
			(sex == 0) ? 0 : 0,
			(sex == 0) ? 0 : 0,
			(sex == 0) ? (-1) : 0,
			(sex == 0) ? (-1) : 0,
			(sex == 0) ? 0 : 0,
			(sex == 0) ? (-1) : 0,
			(sex == 0) ? (-1) : 0,
			(sex == 0) ? 0 : 0
		};
		Dictionary<int, ListInfoBase> dictionary = null;
		ChaFileClothes clothes = coorde.clothes;
		for (int i = 0; i < array3.Length; i++)
		{
			if (ChaListDefine.CategoryNo.unknown == array2[i])
			{
				continue;
			}
			dictionary = chaListCtrl.GetCategoryInfo(array2[i]);
			if (!dictionary.ContainsKey(clothes.parts[array3[i]].id))
			{
				checkInfo?.Add($"【{array[i]}】値:{clothes.parts[array3[i]].id}");
				result = true;
				clothes.parts[array3[i]].id = array4[i];
			}
			dictionary = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.st_pattern);
			for (int j = 0; j < clothes.parts[array3[i]].colorInfo.Length; j++)
			{
				if (!dictionary.ContainsKey(clothes.parts[array3[i]].colorInfo[j].pattern))
				{
					checkInfo?.Add($"【柄】値:{clothes.parts[array3[i]].colorInfo[j].pattern}");
					result = true;
					clothes.parts[array3[i]].colorInfo[j].pattern = 0;
				}
			}
		}
		ChaFileAccessory accessory = coorde.accessory;
		for (int k = 0; k < accessory.parts.Length; k++)
		{
			int type = accessory.parts[k].type;
			_ = accessory.parts[k].id;
			dictionary = chaListCtrl.GetCategoryInfo((ChaListDefine.CategoryNo)type);
			if (dictionary != null && !dictionary.ContainsKey(accessory.parts[k].id))
			{
				checkInfo?.Add($"【{ChaAccessoryDefine.GetAccessoryTypeName((ChaListDefine.CategoryNo)type)}】値:{accessory.parts[k].id}");
				result = true;
				accessory.parts[k].MemberInit();
			}
		}
		return result;
	}

	public static bool CheckDataRangeParameter(ChaFile chafile, List<string> checkInfo = null)
	{
		_ = Singleton<Character>.Instance.chaListCtrl;
		ChaFileParameter chaFileParameter = chafile.parameter;
		ChaFileParameter2 chaFileParameter2 = chafile.parameter2;
		bool result = false;
		if (chaFileParameter.sex == 0)
		{
			return false;
		}
		if (!Voice.infoTable.ContainsKey(chaFileParameter2.personality))
		{
			checkInfo?.Add($"【性格】値:{chaFileParameter2.personality}");
			result = true;
			chaFileParameter2.personality = 0;
		}
		return result;
	}

	public static bool CheckDataRange(ChaFile chafile, bool chk_custom, bool chk_clothes, bool chk_parameter, List<string> checkInfo = null)
	{
		bool flag = false;
		if (chk_custom)
		{
			flag |= CheckDataRangeFace(chafile, checkInfo);
			flag |= CheckDataRangeBody(chafile, checkInfo);
			flag |= CheckDataRangeHair(chafile, checkInfo);
		}
		if (chk_clothes)
		{
			flag |= CheckDataRangeCoordinate(chafile, checkInfo);
		}
		if (chk_parameter)
		{
			flag |= CheckDataRangeParameter(chafile, checkInfo);
		}
		return flag;
	}

	public bool InitGameInfoParam()
	{
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		gameinfo2.MemberInit();
		ListInfoBase listInfo = chaListCtrl.GetListInfo(ChaListDefine.CategoryNo.init_mind_param, parameter2.mind);
		if (listInfo != null)
		{
			gameinfo2.Favor = listInfo.GetInfoInt(ChaListDefine.KeyType.FAVOR);
			gameinfo2.Enjoyment = listInfo.GetInfoInt(ChaListDefine.KeyType.ENJOYMENT);
			gameinfo2.Aversion = listInfo.GetInfoInt(ChaListDefine.KeyType.AVERSION);
			gameinfo2.Slavery = listInfo.GetInfoInt(ChaListDefine.KeyType.SLAVERY);
		}
		if (gameinfo2.Favor != 0)
		{
			gameinfo2.nowState = ChaFileDefine.State.Favor;
			if (gameinfo2.Favor >= 50)
			{
				gameinfo2.nowDrawState = ChaFileDefine.State.Favor;
			}
		}
		else if (gameinfo2.Enjoyment != 0)
		{
			gameinfo2.nowState = ChaFileDefine.State.Enjoyment;
			if (gameinfo2.Enjoyment >= 50)
			{
				gameinfo2.nowDrawState = ChaFileDefine.State.Enjoyment;
			}
		}
		else if (gameinfo2.Aversion != 0)
		{
			gameinfo2.nowState = ChaFileDefine.State.Aversion;
			if (gameinfo2.Aversion >= 50)
			{
				gameinfo2.nowDrawState = ChaFileDefine.State.Aversion;
			}
		}
		else if (gameinfo2.Slavery != 0)
		{
			gameinfo2.nowState = ChaFileDefine.State.Slavery;
			if (gameinfo2.Slavery >= 50)
			{
				gameinfo2.nowDrawState = ChaFileDefine.State.Slavery;
			}
		}
		return true;
	}

	public bool SaveCharaFile(string filename, byte sex = byte.MaxValue, bool newFile = false)
	{
		string path = ConvertCharaFilePath(filename, sex, newFile);
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		base.charaFileName = Path.GetFileName(path);
		string text = userID;
		string text2 = dataID;
		if (userID != Singleton<GameSystem>.Instance.UserUUID)
		{
			dataID = YS_Assist.CreateUUID();
		}
		else if (!File.Exists(path))
		{
			dataID = YS_Assist.CreateUUID();
		}
		userID = Singleton<GameSystem>.Instance.UserUUID;
		using FileStream st = new FileStream(path, FileMode.Create, FileAccess.Write);
		bool result = SaveCharaFile(st, savePng: true);
		userID = text;
		dataID = text2;
		return result;
	}

	public bool SaveCharaFile(Stream st, bool savePng)
	{
		using BinaryWriter bw = new BinaryWriter(st);
		return SaveCharaFile(bw, savePng);
	}

	public bool SaveCharaFile(BinaryWriter bw, bool savePng)
	{
		return SaveFile(bw, savePng, (int)Singleton<GameSystem>.Instance.language);
	}

	public bool LoadCharaFile(string filename, byte sex = byte.MaxValue, bool noLoadPng = false, bool noLoadStatus = true)
	{
		if (filename.IsNullOrEmpty())
		{
			return false;
		}
		base.charaFileName = Path.GetFileName(filename);
		string path = ConvertCharaFilePath(filename, sex);
		if (!File.Exists(path))
		{
			return false;
		}
		using FileStream st = new FileStream(path, FileMode.Open, FileAccess.Read);
		return LoadCharaFile(st, noLoadPng, noLoadStatus);
	}

	public bool LoadCharaFile(Stream st, bool noLoadPng = false, bool noLoadStatus = true)
	{
		using BinaryReader br = new BinaryReader(st);
		return LoadCharaFile(br, noLoadPng, noLoadStatus);
	}

	public bool LoadCharaFile(BinaryReader br, bool noLoadPng = false, bool noLoadStatus = true)
	{
		bool result = LoadFile(br, (int)Singleton<GameSystem>.Instance.language, noLoadPng, noLoadStatus);
		if (!skipRangeCheck)
		{
			Singleton<Character>.Instance.isMod = CheckDataRange(this, chk_custom: true, chk_clothes: true, chk_parameter: true);
		}
		return result;
	}

	public bool LoadFileLimited(string filename, byte sex = byte.MaxValue, bool face = true, bool body = true, bool hair = true, bool parameter = true, bool coordinate = true)
	{
		string path = ConvertCharaFilePath(filename, sex);
		ChaFileControl chaFileControl = new ChaFileControl();
		if (chaFileControl.LoadFile(path, (int)Singleton<GameSystem>.Instance.language))
		{
			if (!skipRangeCheck)
			{
				Singleton<Character>.Instance.isMod = CheckDataRange(chaFileControl, chk_custom: true, chk_clothes: true, chk_parameter: true);
			}
			byte[] array = null;
			if (face)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.face);
				custom.face = MessagePackSerializer.Deserialize<ChaFileFace>(array);
			}
			if (body)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.body);
				custom.body = MessagePackSerializer.Deserialize<ChaFileBody>(array);
			}
			if (hair)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.hair);
				custom.hair = MessagePackSerializer.Deserialize<ChaFileHair>(array);
			}
			if (parameter)
			{
				base.parameter.Copy(chaFileControl.parameter);
				gameinfo.Copy(chaFileControl.gameinfo);
				parameter2.Copy(chaFileControl.parameter2);
				gameinfo2.Copy(chaFileControl.gameinfo2);
			}
			if (coordinate)
			{
				CopyCoordinate(chaFileControl.coordinate);
			}
			if (face && body && hair && parameter && coordinate)
			{
				userID = chaFileControl.userID;
				dataID = chaFileControl.dataID;
			}
		}
		return false;
	}

	public bool LoadMannequinFile(string assetBundleName, string assetName, bool face = true, bool body = true, bool hair = true, bool parameter = true, bool coordinate = true)
	{
		ChaFileControl chaFileControl = new ChaFileControl();
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, assetName);
		if (null == textAsset)
		{
			return false;
		}
		if (chaFileControl.LoadFromTextAsset(textAsset, noSetPNG: true))
		{
			byte[] array = null;
			if (face)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.face);
				custom.face = MessagePackSerializer.Deserialize<ChaFileFace>(array);
			}
			if (body)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.body);
				custom.body = MessagePackSerializer.Deserialize<ChaFileBody>(array);
			}
			if (hair)
			{
				array = MessagePackSerializer.Serialize(chaFileControl.custom.hair);
				custom.hair = MessagePackSerializer.Deserialize<ChaFileHair>(array);
			}
			if (parameter)
			{
				base.parameter.Copy(chaFileControl.parameter);
			}
			if (coordinate)
			{
				CopyCoordinate(chaFileControl.coordinate);
			}
		}
		return false;
	}

	public bool LoadFromAssetBundle(string assetBundleName, string assetName, bool noSetPNG = false, bool noLoadStatus = true)
	{
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, assetName);
		if (null == textAsset)
		{
			return false;
		}
		bool result = LoadFromTextAsset(textAsset, noSetPNG, noLoadStatus);
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: true, null, unloadAllLoadedObjects: true);
		return result;
	}

	public bool LoadFromTextAsset(TextAsset ta, bool noSetPNG = false, bool noLoadStatus = true)
	{
		if (null == ta)
		{
			return false;
		}
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(ta.bytes, 0, ta.bytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return LoadCharaFile(memoryStream, noSetPNG, noLoadStatus);
	}

	public bool LoadFromBytes(byte[] bytes, bool noSetPNG = false, bool noLoadStatus = true)
	{
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(bytes, 0, bytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		bool result = LoadCharaFile(memoryStream, noSetPNG, noLoadStatus);
		if (!skipRangeCheck)
		{
			Singleton<Character>.Instance.isMod = CheckDataRange(this, chk_custom: true, chk_clothes: true, chk_parameter: true);
		}
		return result;
	}

	public void SaveFace(string path)
	{
		if (custom != null)
		{
			custom.SaveFace(path);
		}
	}

	public void LoadFace(string path)
	{
		if (custom != null)
		{
			custom.LoadFace(path);
		}
	}

	public void LoadFacePreset()
	{
		ListInfoBase listInfo = Singleton<Character>.Instance.chaListCtrl.GetListInfo((parameter.sex == 0) ? ChaListDefine.CategoryNo.mo_head : ChaListDefine.CategoryNo.fo_head, custom.face.headId);
		string info = listInfo.GetInfo(ChaListDefine.KeyType.MainManifest);
		string info2 = listInfo.GetInfo(ChaListDefine.KeyType.MainAB);
		string info3 = listInfo.GetInfo(ChaListDefine.KeyType.Preset);
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(info2, info3, clone: false, info);
		if (!(null == textAsset))
		{
			custom.LoadFace(textAsset.bytes);
			AssetBundleManager.UnloadAssetBundle(info2, isUnloadForceRefCount: true);
		}
	}

	public string ConvertCharaFilePath(string path, byte _sex, bool newFile = false)
	{
		byte b = ((byte.MaxValue == _sex) ? parameter.sex : _sex);
		string text = "";
		string text2 = "";
		if (path != "")
		{
			text = Path.GetDirectoryName(path);
			text2 = Path.GetFileName(path);
			text = ((!(text == "")) ? (text + "/") : (UserData.Path + ((b == 0) ? "chara/male/" : "chara/female/")));
			if (text2 == "")
			{
				text2 = ((!newFile && !(base.charaFileName == "")) ? base.charaFileName : ((b != 0) ? ("HS2ChaF_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) : ("HS2ChaM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"))));
			}
			if (Path.GetExtension(text2).IsNullOrEmpty())
			{
				return text + Path.GetFileNameWithoutExtension(text2) + ".png";
			}
			return text + text2;
		}
		return "";
	}

	public static string ConvertCoordinateFilePath(string path, byte sex)
	{
		string text = "";
		string text2 = "";
		if (path != "")
		{
			text = Path.GetDirectoryName(path);
			text2 = Path.GetFileName(path);
			text = ((!(text == "")) ? (text + "/") : (UserData.Path + ((sex == 0) ? "coordinate/male/" : "coordinate/female/")));
			if (text2 == "")
			{
				text2 = ((sex != 0) ? ("HS2CoordeF_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) : ("HS2CoordeM_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")));
			}
			if (Path.GetExtension(text2).IsNullOrEmpty())
			{
				return text + Path.GetFileNameWithoutExtension(text2) + ".png";
			}
			return text + text2;
		}
		return "";
	}
}
