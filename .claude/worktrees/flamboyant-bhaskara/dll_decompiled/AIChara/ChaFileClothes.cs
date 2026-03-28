using System;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileClothes
{
	[MessagePackObject(true)]
	public class PartsInfo
	{
		[MessagePackObject(true)]
		public class ColorInfo
		{
			public Color baseColor { get; set; }

			public int pattern { get; set; }

			public Vector4 layout { get; set; }

			public float rotation { get; set; }

			public Color patternColor { get; set; }

			public float glossPower { get; set; }

			public float metallicPower { get; set; }

			public ColorInfo()
			{
				MemberInit();
			}

			public void MemberInit()
			{
				baseColor = Color.white;
				pattern = 0;
				layout = new Vector4(1f, 1f, 0f, 0f);
				rotation = 0.5f;
				patternColor = Color.white;
				glossPower = 0.5f;
				metallicPower = 0f;
			}
		}

		public int id { get; set; }

		public ColorInfo[] colorInfo { get; set; }

		public float breakRate { get; set; }

		public bool[] hideOpt { get; set; }

		public PartsInfo()
		{
			MemberInit();
		}

		public void MemberInit()
		{
			id = 0;
			colorInfo = new ColorInfo[3];
			for (int i = 0; i < colorInfo.Length; i++)
			{
				colorInfo[i] = new ColorInfo();
			}
			breakRate = 0f;
			hideOpt = new bool[2];
		}
	}

	public Version version { get; set; }

	public PartsInfo[] parts { get; set; }

	public ChaFileClothes()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileClothesVersion;
		parts = new PartsInfo[Enum.GetValues(typeof(ChaFileDefine.ClothesKind)).Length];
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i] = new PartsInfo();
		}
	}

	public void ComplementWithVersion()
	{
		version = ChaFileDefine.ChaFileClothesVersion;
	}
}
