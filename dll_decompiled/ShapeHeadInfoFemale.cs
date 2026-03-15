using System;
using System.Collections.Generic;
using IllusionUtility.SetUtility;
using Manager;
using UnityEngine;

public class ShapeHeadInfoFemale : ShapeInfoBase
{
	public enum DstName
	{
		cf_J_CheekLow_L,
		cf_J_CheekLow_R,
		cf_J_CheekUp_L,
		cf_J_CheekUp_R,
		cf_J_Chin_rs,
		cf_J_ChinLow,
		cf_J_ChinTip_s,
		cf_J_EarBase_s_L,
		cf_J_EarBase_s_R,
		cf_J_EarLow_L,
		cf_J_EarLow_R,
		cf_J_EarRing_L,
		cf_J_EarRing_R,
		cf_J_EarUp_L,
		cf_J_EarUp_R,
		cf_J_Eye_r_L,
		cf_J_Eye_r_R,
		cf_J_Eye_s_L,
		cf_J_Eye_s_R,
		cf_J_Eye_t_L,
		cf_J_Eye_t_R,
		cf_J_Eye01_L,
		cf_J_Eye01_R,
		cf_J_Eye02_L,
		cf_J_Eye02_R,
		cf_J_Eye03_L,
		cf_J_Eye03_R,
		cf_J_Eye04_L,
		cf_J_Eye04_R,
		cf_J_EyePos_rz_L,
		cf_J_EyePos_rz_R,
		cf_J_FaceBase,
		cf_J_FaceLow_s,
		cf_J_FaceLowBase,
		cf_J_FaceUp_ty,
		cf_J_FaceUp_tz,
		cf_J_megane,
		cf_J_Mouth_L,
		cf_J_Mouth_R,
		cf_J_MouthLow,
		cf_J_Mouthup,
		cf_J_MouthBase_s,
		cf_J_MouthBase_tr,
		cf_J_Nose_t,
		cf_J_Nose_tip,
		cf_J_NoseBase_s,
		cf_J_NoseBase_trs,
		cf_J_NoseBridge_s,
		cf_J_NoseBridge_t,
		cf_J_NoseWing_tx_L,
		cf_J_NoseWing_tx_R,
		cf_J_MouthCavity
	}

	public enum SrcName
	{
		cf_s_CheekLow_tx_L,
		cf_s_CheekLow_tx_R,
		cf_s_CheekLow_ty,
		cf_s_CheekLow_tz,
		cf_s_CheekUp_tx_L,
		cf_s_CheekUp_tx_R,
		cf_s_CheekUp_ty,
		cf_s_CheekUp_tz_00,
		cf_s_CheekUp_tz_01,
		cf_s_Chin_rx,
		cf_s_Chin_sx,
		cf_s_Chin_ty,
		cf_s_Chin_tz,
		cf_s_ChinLow,
		cf_s_ChinTip_sx,
		cf_s_ChinTip_ty,
		cf_s_ChinTip_tz,
		cf_s_EarBase_ry_L,
		cf_s_EarBase_ry_R,
		cf_s_EarBase_rz_L,
		cf_s_EarBase_rz_R,
		cf_s_EarBase_s_L,
		cf_s_EarBase_s_R,
		cf_s_EarLow_L,
		cf_s_EarLow_R,
		cf_s_EarRing_L,
		cf_s_EarRing_R,
		cf_s_EarRing_rz_L,
		cf_s_EarRing_rz_R,
		cf_s_EarRing_s_L,
		cf_s_EarRing_s_R,
		cf_s_EarUp_L,
		cf_s_EarUp_R,
		cf_s_Eye_ry_L,
		cf_s_Eye_ry_R,
		cf_s_Eye_rz_L,
		cf_s_Eye_rz_R,
		cf_s_Eye_sx_L,
		cf_s_Eye_sx_R,
		cf_s_Eye_sy_L,
		cf_s_Eye_sy_R,
		cf_s_Eye_tx_L,
		cf_s_Eye_tx_R,
		cf_s_Eye_ty,
		cf_s_Eye_tz,
		cf_s_Eye01_L,
		cf_s_Eye01_R,
		cf_s_Eye01_rx_L,
		cf_s_Eye01_rx_R,
		cf_s_Eye02_L,
		cf_s_Eye02_R,
		cf_s_Eye02_ry_L,
		cf_s_Eye02_ry_R,
		cf_s_Eye03_L,
		cf_s_Eye03_R,
		cf_s_Eye03_rx_L,
		cf_s_Eye03_rx_R,
		cf_s_Eye04_L,
		cf_s_Eye04_R,
		cf_s_Eye04_ry_L,
		cf_s_Eye04_ry_R,
		cf_s_EyePos_rz_L,
		cf_s_EyePos_rz_R,
		cf_s_FaceBase_sx,
		cf_s_FaceLow_sx,
		cf_s_FaceLow_tz,
		cf_s_FaceUp_ty,
		cf_s_FaceUp_tz,
		cf_s_megane_rx_nosebridge,
		cf_s_megane_ty_eye,
		cf_s_megane_ty_nose,
		cf_s_megane_tz_nosebridge,
		cf_s_MouthBase_tz,
		cf_s_Mouthup,
		cf_s_Mouth_L,
		cf_s_Mouth_R,
		cf_s_MouthBase_sx,
		cf_s_MouthBase_sy,
		cf_s_MouthBase_ty,
		cf_s_MouthLow,
		cf_s_Nose_rx,
		cf_s_Nose_tip,
		cf_s_Nose_tz,
		cf_s_NoseBase,
		cf_s_NoseBase_rx,
		cf_s_NoseBase_sx,
		cf_s_NoseBase_ty,
		cf_s_NoseBase_tz,
		cf_s_NoseBridge_rx,
		cf_s_NoseBridge_sx,
		cf_s_NoseBridge_ty,
		cf_s_NoseBridge_tz_00,
		cf_s_NoseBridge_tz_01,
		cf_s_NoseWing_rx,
		cf_s_NoseWing_rz_L,
		cf_s_NoseWing_rz_R,
		cf_s_NoseWing_tx_L,
		cf_s_NoseWing_tx_R,
		cf_s_NoseWing_ty,
		cf_s_NoseWing_tz,
		cf_s_MouthC_ty,
		cf_s_MouthC_tz
	}

	public override void InitShapeInfo(string manifest, string assetBundleAnmKey, string assetBundleCategory, string anmKeyInfoPath, string cateInfoPath, Transform trfObj)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		DstName[] array = (DstName[])Enum.GetValues(typeof(DstName));
		for (int i = 0; i < array.Length; i++)
		{
			DstName value = array[i];
			dictionary[value.ToString()] = (int)value;
		}
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		SrcName[] array2 = (SrcName[])Enum.GetValues(typeof(SrcName));
		for (int i = 0; i < array2.Length; i++)
		{
			SrcName value2 = array2[i];
			dictionary2[value2.ToString()] = (int)value2;
		}
		InitShapeInfoBase(manifest, assetBundleAnmKey, assetBundleCategory, anmKeyInfoPath, cateInfoPath, trfObj, dictionary, dictionary2, Singleton<Character>.Instance.AddLoadAssetBundle);
		base.InitEnd = true;
	}

	public override void ForceUpdate()
	{
		Update();
	}

	public override void Update()
	{
		if (base.InitEnd && dictSrc.Count != 0)
		{
			dictDst[51].trfBone.SetLocalPositionY(dictSrc[100].vctPos.y);
			dictDst[51].trfBone.SetLocalPositionZ(dictSrc[101].vctPos.z + dictSrc[100].vctPos.z);
			dictDst[19].trfBone.SetLocalPositionX(dictSrc[41].vctPos.x);
			dictDst[19].trfBone.SetLocalPositionY(dictSrc[43].vctPos.y);
			dictDst[19].trfBone.SetLocalPositionZ(dictSrc[44].vctPos.z);
			dictDst[19].trfBone.SetLocalRotation(0f, 0f, dictSrc[35].vctRot.z);
			dictDst[20].trfBone.SetLocalPositionX(dictSrc[42].vctPos.x);
			dictDst[20].trfBone.SetLocalPositionY(dictSrc[43].vctPos.y);
			dictDst[20].trfBone.SetLocalPositionZ(dictSrc[44].vctPos.z);
			dictDst[20].trfBone.SetLocalRotation(0f, 0f, dictSrc[36].vctRot.z);
			dictDst[17].trfBone.SetLocalScale(dictSrc[37].vctScl.x, dictSrc[39].vctScl.y, 1f);
			dictDst[15].trfBone.SetLocalRotation(0f, dictSrc[33].vctRot.y, 0f);
			dictDst[18].trfBone.SetLocalScale(dictSrc[38].vctScl.x, dictSrc[40].vctScl.y, 1f);
			dictDst[16].trfBone.SetLocalRotation(0f, dictSrc[34].vctRot.y, 0f);
			dictDst[21].trfBone.SetLocalRotation(dictSrc[47].vctRot.x, dictSrc[45].vctRot.y + dictSrc[47].vctRot.y, 0f);
			dictDst[23].trfBone.SetLocalRotation(dictSrc[49].vctRot.x, dictSrc[51].vctRot.y, dictSrc[51].vctRot.z);
			dictDst[25].trfBone.SetLocalPositionX(dictSrc[53].vctPos.x);
			dictDst[25].trfBone.SetLocalRotation(dictSrc[55].vctRot.x, dictSrc[53].vctRot.y, 0f);
			dictDst[27].trfBone.SetLocalRotation(dictSrc[57].vctRot.x, dictSrc[59].vctRot.y, dictSrc[59].vctRot.z);
			dictDst[22].trfBone.SetLocalRotation(dictSrc[48].vctRot.x, dictSrc[46].vctRot.y + dictSrc[48].vctRot.y, 0f);
			dictDst[24].trfBone.SetLocalRotation(dictSrc[50].vctRot.x, dictSrc[52].vctRot.y, dictSrc[52].vctRot.z);
			dictDst[26].trfBone.SetLocalPositionX(dictSrc[54].vctPos.x);
			dictDst[26].trfBone.SetLocalRotation(dictSrc[56].vctRot.x, dictSrc[54].vctRot.y, 0f);
			dictDst[28].trfBone.SetLocalRotation(dictSrc[58].vctRot.x, dictSrc[60].vctRot.y, dictSrc[60].vctRot.z);
			dictDst[29].trfBone.SetLocalRotation(0f, 0f, dictSrc[61].vctRot.z);
			dictDst[30].trfBone.SetLocalRotation(0f, 0f, dictSrc[62].vctRot.z);
			dictDst[31].trfBone.SetLocalScale(dictSrc[63].vctScl.x, 1f, 1f);
			dictDst[34].trfBone.SetLocalPositionY(dictSrc[66].vctPos.y);
			dictDst[35].trfBone.SetLocalPositionZ(dictSrc[67].vctPos.z);
			dictDst[2].trfBone.SetLocalPositionX(dictSrc[4].vctPos.x);
			dictDst[2].trfBone.SetLocalPositionY(dictSrc[6].vctPos.y);
			dictDst[2].trfBone.SetLocalPositionZ(dictSrc[7].vctPos.z + dictSrc[8].vctPos.z);
			dictDst[3].trfBone.SetLocalPositionX(dictSrc[5].vctPos.x);
			dictDst[3].trfBone.SetLocalPositionY(dictSrc[6].vctPos.y);
			dictDst[3].trfBone.SetLocalPositionZ(dictSrc[7].vctPos.z + dictSrc[8].vctPos.z);
			dictDst[0].trfBone.SetLocalPositionX(dictSrc[0].vctPos.x);
			dictDst[0].trfBone.SetLocalPositionY(dictSrc[2].vctPos.y);
			dictDst[0].trfBone.SetLocalPositionZ(dictSrc[3].vctPos.z);
			dictDst[1].trfBone.SetLocalPositionX(dictSrc[1].vctPos.x);
			dictDst[1].trfBone.SetLocalPositionY(dictSrc[2].vctPos.y);
			dictDst[1].trfBone.SetLocalPositionZ(dictSrc[3].vctPos.z);
			dictDst[33].trfBone.SetLocalPositionZ(dictSrc[65].vctPos.z);
			dictDst[32].trfBone.SetLocalScale(dictSrc[64].vctScl.x, 1f, 1f);
			dictDst[42].trfBone.SetLocalPositionY(dictSrc[78].vctPos.y);
			dictDst[42].trfBone.SetLocalPositionZ(dictSrc[78].vctPos.z + dictSrc[72].vctPos.z);
			dictDst[41].trfBone.SetLocalScale(dictSrc[76].vctScl.x, dictSrc[77].vctScl.y, 1f);
			dictDst[40].trfBone.SetLocalPositionY(dictSrc[73].vctPos.y);
			dictDst[39].trfBone.SetLocalPositionY(dictSrc[79].vctPos.y);
			dictDst[39].trfBone.SetLocalPositionZ(dictSrc[79].vctPos.z);
			dictDst[39].trfBone.SetLocalScale(dictSrc[79].vctScl.x, 1f, 1f);
			dictDst[37].trfBone.SetLocalPositionY(dictSrc[74].vctPos.y);
			dictDst[37].trfBone.SetLocalRotation(0f, 0f, dictSrc[74].vctRot.z);
			dictDst[38].trfBone.SetLocalPositionY(dictSrc[75].vctPos.y);
			dictDst[38].trfBone.SetLocalRotation(0f, 0f, dictSrc[75].vctRot.z);
			dictDst[5].trfBone.SetLocalPositionY(dictSrc[13].vctPos.y);
			dictDst[4].trfBone.SetLocalPositionY(dictSrc[11].vctPos.y + dictSrc[9].vctPos.y);
			dictDst[4].trfBone.SetLocalPositionZ(dictSrc[12].vctPos.z + dictSrc[9].vctPos.z);
			dictDst[4].trfBone.SetLocalRotation(dictSrc[9].vctRot.x, 0f, 0f);
			dictDst[4].trfBone.SetLocalScale(dictSrc[10].vctScl.x, 1f, 1f);
			dictDst[6].trfBone.SetLocalPositionY(dictSrc[15].vctPos.y);
			dictDst[6].trfBone.SetLocalPositionZ(dictSrc[16].vctPos.z);
			dictDst[6].trfBone.SetLocalScale(dictSrc[14].vctScl.x, dictSrc[14].vctScl.y, dictSrc[14].vctScl.z);
			dictDst[48].trfBone.SetLocalPositionY(dictSrc[90].vctPos.y);
			dictDst[48].trfBone.SetLocalPositionZ(dictSrc[91].vctPos.z + dictSrc[92].vctPos.z + dictSrc[90].vctPos.z + dictSrc[88].vctPos.z);
			dictDst[48].trfBone.SetLocalRotation(dictSrc[88].vctRot.x, 0f, 0f);
			dictDst[47].trfBone.SetLocalScale(dictSrc[89].vctScl.x, 1f, 1f);
			dictDst[46].trfBone.SetLocalPositionY(dictSrc[84].vctPos.y + dictSrc[86].vctPos.y + dictSrc[83].vctPos.y);
			dictDst[46].trfBone.SetLocalPositionZ(dictSrc[84].vctPos.z + dictSrc[87].vctPos.z + dictSrc[83].vctPos.z);
			dictDst[45].trfBone.SetLocalRotation(dictSrc[84].vctRot.x + dictSrc[83].vctRot.x, 0f, 0f);
			dictDst[45].trfBone.SetLocalScale(dictSrc[85].vctScl.x, dictSrc[85].vctScl.y, dictSrc[85].vctScl.z);
			dictDst[49].trfBone.SetLocalPositionX(dictSrc[96].vctPos.x);
			dictDst[49].trfBone.SetLocalPositionY(dictSrc[98].vctPos.y);
			dictDst[49].trfBone.SetLocalPositionZ(dictSrc[99].vctPos.z);
			dictDst[49].trfBone.SetLocalRotation(dictSrc[93].vctRot.x, 0f, dictSrc[94].vctRot.z);
			dictDst[50].trfBone.SetLocalPositionX(dictSrc[97].vctPos.x);
			dictDst[50].trfBone.SetLocalPositionY(dictSrc[98].vctPos.y);
			dictDst[50].trfBone.SetLocalPositionZ(dictSrc[99].vctPos.z);
			dictDst[50].trfBone.SetLocalRotation(dictSrc[93].vctRot.x, 0f, dictSrc[95].vctRot.z);
			dictDst[44].trfBone.SetLocalPositionY(dictSrc[81].vctPos.y);
			dictDst[44].trfBone.SetLocalPositionZ(dictSrc[81].vctPos.z);
			dictDst[44].trfBone.SetLocalScale(dictSrc[81].vctScl.x, dictSrc[81].vctScl.y, dictSrc[81].vctScl.z);
			dictDst[43].trfBone.SetLocalPositionY(dictSrc[80].vctPos.y);
			dictDst[43].trfBone.SetLocalPositionZ(dictSrc[82].vctPos.z);
			dictDst[43].trfBone.SetLocalRotation(dictSrc[80].vctRot.x, 0f, 0f);
			dictDst[36].trfBone.SetLocalPositionY(dictSrc[70].vctPos.y + dictSrc[68].vctPos.y + dictSrc[69].vctPos.y);
			dictDst[36].trfBone.SetLocalPositionZ(dictSrc[70].vctPos.z + dictSrc[71].vctPos.z + dictSrc[69].vctPos.z);
			dictDst[36].trfBone.SetLocalRotation(dictSrc[70].vctRot.x + dictSrc[68].vctRot.x + dictSrc[69].vctRot.x, 0f, 0f);
			dictDst[7].trfBone.SetLocalRotation(0f, dictSrc[17].vctRot.y, dictSrc[19].vctRot.z);
			dictDst[7].trfBone.SetLocalScale(dictSrc[21].vctScl.x, dictSrc[21].vctScl.y, dictSrc[21].vctScl.z);
			dictDst[13].trfBone.SetLocalPositionX(dictSrc[31].vctPos.x);
			dictDst[13].trfBone.SetLocalPositionY(dictSrc[31].vctPos.y);
			dictDst[13].trfBone.SetLocalPositionZ(dictSrc[31].vctPos.z);
			dictDst[13].trfBone.SetLocalRotation(dictSrc[31].vctRot.x, dictSrc[31].vctRot.y, 0f);
			dictDst[13].trfBone.SetLocalScale(dictSrc[31].vctScl.x, dictSrc[31].vctScl.y, dictSrc[31].vctScl.z);
			dictDst[9].trfBone.SetLocalPositionY(dictSrc[23].vctPos.y);
			dictDst[9].trfBone.SetLocalPositionZ(dictSrc[23].vctPos.z);
			dictDst[9].trfBone.SetLocalScale(dictSrc[23].vctScl.x, dictSrc[23].vctScl.y, dictSrc[23].vctScl.z);
			dictDst[8].trfBone.SetLocalRotation(0f, dictSrc[18].vctRot.y, dictSrc[20].vctRot.z);
			dictDst[8].trfBone.SetLocalScale(dictSrc[22].vctScl.x, dictSrc[22].vctScl.y, dictSrc[22].vctScl.z);
			dictDst[14].trfBone.SetLocalPositionX(dictSrc[32].vctPos.x);
			dictDst[14].trfBone.SetLocalPositionY(dictSrc[32].vctPos.y);
			dictDst[14].trfBone.SetLocalPositionZ(dictSrc[32].vctPos.z);
			dictDst[14].trfBone.SetLocalRotation(dictSrc[32].vctRot.x, dictSrc[32].vctRot.y, 0f);
			dictDst[14].trfBone.SetLocalScale(dictSrc[32].vctScl.x, dictSrc[32].vctScl.y, dictSrc[32].vctScl.z);
			dictDst[10].trfBone.SetLocalPositionY(dictSrc[24].vctPos.y);
			dictDst[10].trfBone.SetLocalPositionZ(dictSrc[24].vctPos.z);
			dictDst[10].trfBone.SetLocalScale(dictSrc[24].vctScl.x, dictSrc[24].vctScl.y, dictSrc[24].vctScl.z);
			dictDst[11].trfBone.SetLocalPositionY(dictSrc[25].vctPos.y);
			dictDst[11].trfBone.SetLocalRotation(0f, 0f, dictSrc[27].vctRot.z);
			dictDst[11].trfBone.SetLocalScale(dictSrc[29].vctScl.x, dictSrc[29].vctScl.y, dictSrc[29].vctScl.z);
			dictDst[12].trfBone.SetLocalPositionY(dictSrc[26].vctPos.y);
			dictDst[12].trfBone.SetLocalRotation(0f, 0f, dictSrc[28].vctRot.z);
			dictDst[12].trfBone.SetLocalScale(dictSrc[30].vctScl.x, dictSrc[30].vctScl.y, dictSrc[30].vctScl.z);
		}
	}

	public override void UpdateAlways()
	{
		_ = base.InitEnd;
	}
}
