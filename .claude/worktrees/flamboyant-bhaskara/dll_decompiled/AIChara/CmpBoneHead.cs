using System;
using UnityEngine;

namespace AIChara;

[DisallowMultipleComponent]
public class CmpBoneHead : CmpBase
{
	[Serializable]
	public class TargetAccessory
	{
		public Transform acs_Hair_pony;

		public Transform acs_Hair_twin_L;

		public Transform acs_Hair_twin_R;

		public Transform acs_Hair_pin_L;

		public Transform acs_Hair_pin_R;

		public Transform acs_Head_top;

		public Transform acs_Head;

		public Transform acs_Hitai;

		public Transform acs_Face;

		public Transform acs_Megane;

		public Transform acs_Earring_L;

		public Transform acs_Earring_R;

		public Transform acs_Nose;

		public Transform acs_Mouth;
	}

	[Serializable]
	public class TargetEtc
	{
		public Transform trfHairParent;

		public Transform trfMouthAdjustWidth;
	}

	[Header("アクセサリの親")]
	public TargetAccessory targetAccessory = new TargetAccessory();

	[Header("その他ターゲット")]
	public TargetEtc targetEtc = new TargetEtc();

	public CmpBoneHead()
		: base(_baseDB: false)
	{
	}

	public override void SetReferenceObject()
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		targetAccessory.acs_Hair_pony = findAssist.GetTransformFromName("N_Hair_pony");
		targetAccessory.acs_Hair_twin_L = findAssist.GetTransformFromName("N_Hair_twin_L");
		targetAccessory.acs_Hair_twin_R = findAssist.GetTransformFromName("N_Hair_twin_R");
		targetAccessory.acs_Hair_pin_L = findAssist.GetTransformFromName("N_Hair_pin_L");
		targetAccessory.acs_Hair_pin_R = findAssist.GetTransformFromName("N_Hair_pin_R");
		targetAccessory.acs_Head_top = findAssist.GetTransformFromName("N_Head_top");
		targetAccessory.acs_Head = findAssist.GetTransformFromName("N_Head");
		targetAccessory.acs_Hitai = findAssist.GetTransformFromName("N_Hitai");
		targetAccessory.acs_Face = findAssist.GetTransformFromName("N_Face");
		targetAccessory.acs_Megane = findAssist.GetTransformFromName("N_Megane");
		targetAccessory.acs_Earring_L = findAssist.GetTransformFromName("N_Earring_L");
		targetAccessory.acs_Earring_R = findAssist.GetTransformFromName("N_Earring_R");
		targetAccessory.acs_Nose = findAssist.GetTransformFromName("N_Nose");
		targetAccessory.acs_Mouth = findAssist.GetTransformFromName("N_Mouth");
		targetEtc.trfHairParent = findAssist.GetTransformFromName("N_hair_Root");
		targetEtc.trfMouthAdjustWidth = findAssist.GetTransformFromName("cf_J_MouthMove");
	}
}
