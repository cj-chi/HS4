using System;
using UnityEngine;

namespace AIChara;

[DisallowMultipleComponent]
public class CmpFace : CmpBase
{
	[Serializable]
	public class TargetCustom
	{
		public Renderer[] rendEyes;

		public Renderer rendEyelashes;

		public Renderer rendShadow;

		public Renderer rendHead;
	}

	[Serializable]
	public class TargetEtc
	{
		public Renderer rendTears;

		public GameObject objTongue;
	}

	[Header("カスタムで使用")]
	public TargetCustom targetCustom = new TargetCustom();

	[Header("その他")]
	public TargetEtc targetEtc = new TargetEtc();

	public CmpFace()
		: base(_baseDB: false)
	{
	}

	public override void SetReferenceObject()
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		targetCustom.rendEyes = new Renderer[2];
		GameObject objectFromName = findAssist.GetObjectFromName("o_eyebase_L");
		if (null != objectFromName)
		{
			targetCustom.rendEyes[0] = objectFromName.GetComponent<Renderer>();
		}
		objectFromName = findAssist.GetObjectFromName("o_eyebase_R");
		if (null != objectFromName)
		{
			targetCustom.rendEyes[1] = objectFromName.GetComponent<Renderer>();
		}
		objectFromName = findAssist.GetObjectFromName("o_eyelashes");
		if (null != objectFromName)
		{
			targetCustom.rendEyelashes = objectFromName.GetComponent<Renderer>();
		}
		objectFromName = findAssist.GetObjectFromName("o_eyeshadow");
		if (null != objectFromName)
		{
			targetCustom.rendShadow = objectFromName.GetComponent<Renderer>();
		}
		objectFromName = findAssist.GetObjectFromName("o_head");
		if (null != objectFromName)
		{
			targetCustom.rendHead = objectFromName.GetComponent<Renderer>();
		}
		objectFromName = findAssist.GetObjectFromName("o_namida");
		if (null != objectFromName)
		{
			targetEtc.rendTears = objectFromName.GetComponent<Renderer>();
		}
		targetEtc.objTongue = findAssist.GetObjectFromName("o_tang");
	}
}
