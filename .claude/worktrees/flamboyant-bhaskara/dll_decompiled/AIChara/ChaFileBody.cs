using System;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileBody
{
	public Version version { get; set; }

	public float[] shapeValueBody { get; set; }

	public float bustSoftness { get; set; }

	public float bustWeight { get; set; }

	public int skinId { get; set; }

	public int detailId { get; set; }

	public float detailPower { get; set; }

	public Color skinColor { get; set; }

	public float skinGlossPower { get; set; }

	public float skinMetallicPower { get; set; }

	public int sunburnId { get; set; }

	public Color sunburnColor { get; set; }

	public PaintInfo[] paintInfo { get; set; }

	public int nipId { get; set; }

	public Color nipColor { get; set; }

	public float nipGlossPower { get; set; }

	public float areolaSize { get; set; }

	public int underhairId { get; set; }

	public Color underhairColor { get; set; }

	public Color nailColor { get; set; }

	public float nailGlossPower { get; set; }

	public ChaFileBody()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileBodyVersion;
		shapeValueBody = new float[ChaFileDefine.cf_bodyshapename.Length];
		for (int i = 0; i < shapeValueBody.Length; i++)
		{
			shapeValueBody[i] = ChaFileDefine.cf_bodyInitValue[i];
		}
		bustSoftness = 0.5f;
		bustWeight = 0.5f;
		skinId = 0;
		detailId = 0;
		detailPower = 0.5f;
		skinColor = new Color(0.8f, 0.7f, 0.64f);
		skinGlossPower = 0.7f;
		skinMetallicPower = 0f;
		sunburnId = 0;
		sunburnColor = Color.white;
		paintInfo = new PaintInfo[2];
		for (int j = 0; j < 2; j++)
		{
			paintInfo[j] = new PaintInfo();
		}
		nipId = 0;
		nipColor = new Color(0.76f, 0.52f, 0.52f);
		nipGlossPower = 0.6f;
		areolaSize = 0.7f;
		underhairId = 0;
		underhairColor = new Color(0.05f, 0.05f, 0.05f);
		nailColor = new Color(1f, 0.92f, 0.92f);
		nailGlossPower = 0.6f;
	}

	public void ComplementWithVersion()
	{
		if (version < new Version("0.0.1"))
		{
			bustWeight *= 0.1f;
		}
		version = ChaFileDefine.ChaFileBodyVersion;
	}
}
