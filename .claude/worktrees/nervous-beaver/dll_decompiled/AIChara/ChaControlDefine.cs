using UnityEngine;

namespace AIChara;

public static class ChaControlDefine
{
	public enum ExtraAccessoryParts
	{
		Head,
		Back,
		Neck,
		Waist
	}

	public enum DynamicBoneKind
	{
		BreastL,
		BreastR,
		HipL,
		HipR
	}

	public const string headBoneName = "cf_J_FaceRoot";

	public const string bodyBoneName = "cf_J_Root";

	public const string bodyTopName = "BodyTop";

	public const string AnimeMannequinState = "mannequin";

	public const string AnimeMannequinState02 = "mannequin02";

	public const string objHeadName = "ct_head";

	public const int FaceTexSize = 2048;

	public const int BodyTexSize = 4096;

	public static readonly Bounds bounds = new Bounds(new Vector3(0f, -2f, 0f), new Vector3(20f, 20f, 20f));

	public static readonly string[] extraAcsNames = new string[4] { "mapAcsHead", "mapAcsBack", "mapAcsNeck", "mapAcsWaist" };
}
