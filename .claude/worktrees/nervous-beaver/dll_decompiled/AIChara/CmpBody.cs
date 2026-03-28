using System;
using UnityEngine;

namespace AIChara;

[DisallowMultipleComponent]
public class CmpBody : CmpBase
{
	[Serializable]
	public class TargetCustom
	{
		public Renderer rendBody;
	}

	[Serializable]
	public class TargetEtc
	{
		public GameObject objBody;

		public GameObject objDanTop;

		public GameObject objDanTama;

		public GameObject objDanSao;

		public GameObject objTongue;

		public GameObject objMNPB;

		public Renderer rendTongue;
	}

	[Header("カスタムで使用")]
	public TargetCustom targetCustom = new TargetCustom();

	[Header("その他")]
	public TargetEtc targetEtc = new TargetEtc();

	public CmpBody()
		: base(_baseDB: false)
	{
	}

	public override void SetReferenceObject()
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		targetEtc.objBody = findAssist.GetObjectFromName("o_body_cm");
		if (null == targetEtc.objBody)
		{
			targetEtc.objBody = findAssist.GetObjectFromName("o_body_cf");
		}
		if (null == targetEtc.objBody)
		{
			targetEtc.objBody = findAssist.GetObjectFromName("o_silhouette_cm");
		}
		if (null == targetEtc.objBody)
		{
			targetEtc.objBody = findAssist.GetObjectFromName("o_silhouette_cf");
		}
		if (null != targetEtc.objBody)
		{
			targetCustom.rendBody = targetEtc.objBody.GetComponent<Renderer>();
		}
		targetEtc.objDanTop = findAssist.GetObjectFromName("N_dan");
		targetEtc.objDanTama = findAssist.GetObjectFromName("cm_o_dan_f");
		targetEtc.objDanSao = findAssist.GetObjectFromName("cm_o_dan00");
		targetEtc.objTongue = findAssist.GetObjectFromName("N_tang");
		if (null != targetEtc.objTongue)
		{
			targetEtc.rendTongue = targetEtc.objTongue.GetComponentInChildren<Renderer>();
		}
		targetEtc.objMNPB = findAssist.GetObjectFromName("N_mnpb");
	}
}
