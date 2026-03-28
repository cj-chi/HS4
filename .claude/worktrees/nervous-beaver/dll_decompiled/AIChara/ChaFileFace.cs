using System;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileFace
{
	[MessagePackObject(true)]
	public class EyesInfo
	{
		public Color whiteColor { get; set; }

		public int pupilId { get; set; }

		public Color pupilColor { get; set; }

		public float pupilW { get; set; }

		public float pupilH { get; set; }

		public float pupilEmission { get; set; }

		public int blackId { get; set; }

		public Color blackColor { get; set; }

		public float blackW { get; set; }

		public float blackH { get; set; }

		public EyesInfo()
		{
			MemberInit();
		}

		public void MemberInit()
		{
			whiteColor = new Color(0.846f, 0.846f, 0.846f);
			pupilId = 0;
			pupilColor = new Color(0.33f, 0.33f, 0.33f);
			pupilW = 0.666f;
			pupilH = 0.666f;
			pupilEmission = 0f;
			blackId = 0;
			blackColor = Color.black;
			blackW = 0.8333f;
			blackH = 0.8333f;
		}

		public void Copy(EyesInfo src)
		{
			whiteColor = src.whiteColor;
			pupilId = src.pupilId;
			pupilColor = src.pupilColor;
			pupilW = src.pupilW;
			pupilH = src.pupilH;
			pupilEmission = src.pupilEmission;
			blackId = src.blackId;
			blackColor = src.blackColor;
			blackW = src.blackW;
			blackH = src.blackH;
		}
	}

	[MessagePackObject(true)]
	public class MakeupInfo
	{
		public int eyeshadowId { get; set; }

		public Color eyeshadowColor { get; set; }

		public float eyeshadowGloss { get; set; }

		public int cheekId { get; set; }

		public Color cheekColor { get; set; }

		public float cheekGloss { get; set; }

		public int lipId { get; set; }

		public Color lipColor { get; set; }

		public float lipGloss { get; set; }

		public PaintInfo[] paintInfo { get; set; }

		public MakeupInfo()
		{
			MemberInit();
		}

		public void MemberInit()
		{
			eyeshadowId = 0;
			eyeshadowColor = Color.white;
			cheekId = 0;
			cheekColor = Color.white;
			lipId = 0;
			lipColor = Color.white;
			paintInfo = new PaintInfo[2];
			for (int i = 0; i < 2; i++)
			{
				paintInfo[i] = new PaintInfo();
			}
		}
	}

	public Version version { get; set; }

	public float[] shapeValueFace { get; set; }

	public int headId { get; set; }

	public int skinId { get; set; }

	public int detailId { get; set; }

	public float detailPower { get; set; }

	public int eyebrowId { get; set; }

	public Color eyebrowColor { get; set; }

	public Vector4 eyebrowLayout { get; set; }

	public float eyebrowTilt { get; set; }

	public EyesInfo[] pupil { get; set; }

	public bool pupilSameSetting { get; set; }

	public float pupilY { get; set; }

	public int hlId { get; set; }

	public Color hlColor { get; set; }

	public Vector4 hlLayout { get; set; }

	public float hlTilt { get; set; }

	public float whiteShadowScale { get; set; }

	public int eyelashesId { get; set; }

	public Color eyelashesColor { get; set; }

	public int moleId { get; set; }

	public Color moleColor { get; set; }

	public Vector4 moleLayout { get; set; }

	public MakeupInfo makeup { get; set; }

	public int beardId { get; set; }

	public Color beardColor { get; set; }

	public ChaFileFace()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileFaceVersion;
		shapeValueFace = new float[ChaFileDefine.cf_headshapename.Length];
		for (int i = 0; i < shapeValueFace.Length; i++)
		{
			shapeValueFace[i] = ChaFileDefine.cf_faceInitValue[i];
		}
		headId = 0;
		skinId = 0;
		detailId = 0;
		detailPower = 1f;
		eyebrowId = 0;
		eyebrowColor = new Color(0.05f, 0.05f, 0.05f);
		eyebrowLayout = new Vector4(0.5f, 0.375f, 0.666f, 0.666f);
		eyebrowTilt = 0.5f;
		pupil = new EyesInfo[2];
		for (int j = 0; j < pupil.Length; j++)
		{
			pupil[j] = new EyesInfo();
		}
		pupilSameSetting = true;
		pupilY = 0.5f;
		hlId = 0;
		hlColor = Color.white;
		hlLayout = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
		hlTilt = 0.5f;
		whiteShadowScale = 0.5f;
		eyelashesId = 0;
		eyelashesColor = new Color(0.19f, 0.19f, 0.19f);
		moleId = 0;
		moleColor = Color.black;
		moleLayout = new Vector4(0.5f, 0.5f, 0.25f, 0.5f);
		makeup = new MakeupInfo();
		beardId = 0;
		beardColor = new Color(0.19f, 0.19f, 0.19f);
	}

	public void ComplementWithVersion()
	{
		if (version < new Version("0.0.1"))
		{
			hlLayout = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
			hlTilt = 0.5f;
		}
		if (version < new Version("0.0.2"))
		{
			beardId = 0;
			beardColor = new Color(0.19f, 0.19f, 0.19f);
		}
		version = ChaFileDefine.ChaFileFaceVersion;
	}
}
