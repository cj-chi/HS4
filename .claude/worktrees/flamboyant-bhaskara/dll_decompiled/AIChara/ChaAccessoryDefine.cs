using System;
using System.Collections.Generic;

namespace AIChara;

public static class ChaAccessoryDefine
{
	public enum AccessoryType
	{
		None,
		Head,
		Ear,
		Glasses,
		Face,
		Neck,
		Shoulder,
		Chest,
		Waist,
		Back,
		Arm,
		Hand,
		Leg,
		Kokan
	}

	public enum AccessoryParentKey
	{
		none,
		N_Hair_pony,
		N_Hair_twin_L,
		N_Hair_twin_R,
		N_Hair_pin_L,
		N_Hair_pin_R,
		N_Head_top,
		N_Hitai,
		N_Head,
		N_Face,
		N_Earring_L,
		N_Earring_R,
		N_Megane,
		N_Nose,
		N_Mouth,
		N_Neck,
		N_Chest_f,
		N_Chest,
		N_Tikubi_L,
		N_Tikubi_R,
		N_Back,
		N_Back_L,
		N_Back_R,
		N_Waist,
		N_Waist_f,
		N_Waist_b,
		N_Waist_L,
		N_Waist_R,
		N_Leg_L,
		N_Knee_L,
		N_Ankle_L,
		N_Foot_L,
		N_Leg_R,
		N_Knee_R,
		N_Ankle_R,
		N_Foot_R,
		N_Shoulder_L,
		N_Elbo_L,
		N_Arm_L,
		N_Wrist_L,
		N_Shoulder_R,
		N_Elbo_R,
		N_Arm_R,
		N_Wrist_R,
		N_Hand_L,
		N_Index_L,
		N_Middle_L,
		N_Ring_L,
		N_Hand_R,
		N_Index_R,
		N_Middle_R,
		N_Ring_R,
		N_Dan,
		N_Kokan,
		N_Ana
	}

	public static readonly int[] AccessoryDefaultIndex;

	public static readonly string[] AccessoryTypeName;

	public static readonly string[] AccessoryParentName;

	public static Dictionary<int, string> dictAccessoryType { get; private set; }

	public static Dictionary<int, string> dictAccessoryParent { get; private set; }

	public static string GetAccessoryTypeName(ChaListDefine.CategoryNo cate)
	{
		return cate switch
		{
			ChaListDefine.CategoryNo.ao_none => AccessoryTypeName[0], 
			ChaListDefine.CategoryNo.ao_head => AccessoryTypeName[1], 
			ChaListDefine.CategoryNo.ao_ear => AccessoryTypeName[2], 
			ChaListDefine.CategoryNo.ao_glasses => AccessoryTypeName[3], 
			ChaListDefine.CategoryNo.ao_face => AccessoryTypeName[4], 
			ChaListDefine.CategoryNo.ao_neck => AccessoryTypeName[5], 
			ChaListDefine.CategoryNo.ao_shoulder => AccessoryTypeName[6], 
			ChaListDefine.CategoryNo.ao_chest => AccessoryTypeName[7], 
			ChaListDefine.CategoryNo.ao_waist => AccessoryTypeName[8], 
			ChaListDefine.CategoryNo.ao_back => AccessoryTypeName[9], 
			ChaListDefine.CategoryNo.ao_arm => AccessoryTypeName[10], 
			ChaListDefine.CategoryNo.ao_hand => AccessoryTypeName[11], 
			ChaListDefine.CategoryNo.ao_leg => AccessoryTypeName[12], 
			ChaListDefine.CategoryNo.ao_kokan => AccessoryTypeName[13], 
			_ => "不明", 
		};
	}

	static ChaAccessoryDefine()
	{
		AccessoryDefaultIndex = new int[14];
		AccessoryTypeName = new string[14]
		{
			"なし", "頭", "耳", "眼鏡", "顔", "首", "肩", "胸", "腰", "背中",
			"腕", "手", "脚", "下腹部"
		};
		AccessoryParentName = new string[55]
		{
			"未設定", "ポニー", "ツイン左", "ツイン右", "ヘアピン左", "ヘアピン右", "帽子", "額", "頭中心", "顔",
			"左耳", "右耳", "眼鏡", "鼻", "口", "首", "胸上", "胸上中央", "左胸", "右胸",
			"背中中央", "背中左", "背中右", "腰", "腰前", "腰後ろ", "腰左", "腰右", "左太もも", "左ひざ",
			"左足首", "かかと左", "右太もも", "右ひざ", "右足首", "かかと右", "左肩", "左肘", "左上腕", "左手首",
			"右肩", "右肘", "右上腕", "右手首", "左手", "左人差指", "左中指", "左薬指", "右手", "右人差指",
			"右中指", "右薬指", "下腹部①", "下腹部②", "下腹部③"
		};
		dictAccessoryType = new Dictionary<int, string>();
		int length = Enum.GetValues(typeof(AccessoryType)).Length;
		int num = AccessoryTypeName.Length;
		if (length == num)
		{
			for (int i = 0; i < length; i++)
			{
				dictAccessoryType[i] = AccessoryTypeName[i];
			}
		}
		dictAccessoryParent = new Dictionary<int, string>();
		length = Enum.GetValues(typeof(AccessoryParentKey)).Length;
		num = AccessoryParentName.Length;
		if (length == num)
		{
			for (int j = 0; j < length; j++)
			{
				dictAccessoryParent[j] = AccessoryParentName[j];
			}
		}
	}

	public static AccessoryParentKey GetReverseParent(AccessoryParentKey key)
	{
		return key switch
		{
			AccessoryParentKey.N_Hair_twin_L => AccessoryParentKey.N_Hair_twin_R, 
			AccessoryParentKey.N_Hair_pin_L => AccessoryParentKey.N_Hair_pin_R, 
			AccessoryParentKey.N_Earring_L => AccessoryParentKey.N_Earring_R, 
			AccessoryParentKey.N_Tikubi_L => AccessoryParentKey.N_Tikubi_R, 
			AccessoryParentKey.N_Back_L => AccessoryParentKey.N_Back_R, 
			AccessoryParentKey.N_Waist_L => AccessoryParentKey.N_Waist_R, 
			AccessoryParentKey.N_Leg_L => AccessoryParentKey.N_Leg_R, 
			AccessoryParentKey.N_Knee_L => AccessoryParentKey.N_Knee_R, 
			AccessoryParentKey.N_Ankle_L => AccessoryParentKey.N_Ankle_R, 
			AccessoryParentKey.N_Foot_L => AccessoryParentKey.N_Foot_R, 
			AccessoryParentKey.N_Shoulder_L => AccessoryParentKey.N_Shoulder_R, 
			AccessoryParentKey.N_Elbo_L => AccessoryParentKey.N_Elbo_R, 
			AccessoryParentKey.N_Arm_L => AccessoryParentKey.N_Arm_R, 
			AccessoryParentKey.N_Wrist_L => AccessoryParentKey.N_Wrist_R, 
			AccessoryParentKey.N_Hand_L => AccessoryParentKey.N_Hand_R, 
			AccessoryParentKey.N_Index_L => AccessoryParentKey.N_Index_R, 
			AccessoryParentKey.N_Middle_L => AccessoryParentKey.N_Middle_R, 
			AccessoryParentKey.N_Ring_L => AccessoryParentKey.N_Ring_R, 
			AccessoryParentKey.N_Hair_twin_R => AccessoryParentKey.N_Hair_twin_L, 
			AccessoryParentKey.N_Hair_pin_R => AccessoryParentKey.N_Hair_pin_L, 
			AccessoryParentKey.N_Earring_R => AccessoryParentKey.N_Earring_L, 
			AccessoryParentKey.N_Tikubi_R => AccessoryParentKey.N_Tikubi_L, 
			AccessoryParentKey.N_Back_R => AccessoryParentKey.N_Back_L, 
			AccessoryParentKey.N_Waist_R => AccessoryParentKey.N_Waist_L, 
			AccessoryParentKey.N_Leg_R => AccessoryParentKey.N_Leg_L, 
			AccessoryParentKey.N_Knee_R => AccessoryParentKey.N_Knee_L, 
			AccessoryParentKey.N_Ankle_R => AccessoryParentKey.N_Ankle_L, 
			AccessoryParentKey.N_Foot_R => AccessoryParentKey.N_Foot_L, 
			AccessoryParentKey.N_Shoulder_R => AccessoryParentKey.N_Shoulder_L, 
			AccessoryParentKey.N_Elbo_R => AccessoryParentKey.N_Elbo_L, 
			AccessoryParentKey.N_Arm_R => AccessoryParentKey.N_Arm_L, 
			AccessoryParentKey.N_Wrist_R => AccessoryParentKey.N_Wrist_L, 
			AccessoryParentKey.N_Hand_R => AccessoryParentKey.N_Hand_L, 
			AccessoryParentKey.N_Index_R => AccessoryParentKey.N_Index_L, 
			AccessoryParentKey.N_Middle_R => AccessoryParentKey.N_Middle_L, 
			AccessoryParentKey.N_Ring_R => AccessoryParentKey.N_Ring_L, 
			_ => AccessoryParentKey.none, 
		};
	}

	public static string GetReverseParent(string key)
	{
		return key switch
		{
			"N_Hair_twin_L" => "N_Hair_twin_R", 
			"N_Hair_pin_L" => "N_Hair_pin_R", 
			"N_Earring_L" => "N_Earring_R", 
			"N_Tikubi_L" => "N_Tikubi_R", 
			"N_Back_L" => "N_Back_R", 
			"N_Waist_L" => "N_Waist_R", 
			"N_Leg_L" => "N_Leg_R", 
			"N_Knee_L" => "N_Knee_R", 
			"N_Ankle_L" => "N_Ankle_R", 
			"N_Foot_L" => "N_Foot_R", 
			"N_Shoulder_L" => "N_Shoulder_R", 
			"N_Elbo_L" => "N_Elbo_R", 
			"N_Arm_L" => "N_Arm_R", 
			"N_Wrist_L" => "N_Wrist_R", 
			"N_Hand_L" => "N_Hand_R", 
			"N_Index_L" => "N_Index_R", 
			"N_Middle_L" => "N_Middle_R", 
			"N_Ring_L" => "N_Ring_R", 
			"N_Hair_twin_R" => "N_Hair_twin_L", 
			"N_Hair_pin_R" => "N_Hair_pin_L", 
			"N_Earring_R" => "N_Earring_L", 
			"N_Tikubi_R" => "N_Tikubi_L", 
			"N_Back_R" => "N_Back_L", 
			"N_Waist_R" => "N_Waist_L", 
			"N_Leg_R" => "N_Leg_L", 
			"N_Knee_R" => "N_Knee_L", 
			"N_Ankle_R" => "N_Ankle_L", 
			"N_Foot_R" => "N_Foot_L", 
			"N_Shoulder_R" => "N_Shoulder_L", 
			"N_Elbo_R" => "N_Elbo_L", 
			"N_Arm_R" => "N_Arm_L", 
			"N_Wrist_R" => "N_Wrist_L", 
			"N_Hand_R" => "N_Hand_L", 
			"N_Index_R" => "N_Index_L", 
			"N_Middle_R" => "N_Middle_L", 
			"N_Ring_R" => "N_Ring_L", 
			_ => "", 
		};
	}

	public static bool CheckPartsOfHead(string keyName)
	{
		if (Enum.TryParse<AccessoryParentKey>(keyName, out var result) && MathfEx.RangeEqualOn(1, (int)result, 14))
		{
			return true;
		}
		return false;
	}

	public static int GetAccessoryParentInt(string keyName)
	{
		if (Enum.TryParse<AccessoryParentKey>(keyName, out var result))
		{
			return (int)result;
		}
		return -1;
	}
}
