using System;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileAccessory
{
	[MessagePackObject(true)]
	public class PartsInfo
	{
		[MessagePackObject(true)]
		public class ColorInfo
		{
			public Color color { get; set; }

			public float glossPower { get; set; }

			public float metallicPower { get; set; }

			public float smoothnessPower { get; set; }

			public ColorInfo()
			{
				MemberInit();
			}

			public void MemberInit()
			{
				color = Color.white;
				glossPower = 0.5f;
				metallicPower = 0.5f;
				smoothnessPower = 0.5f;
			}
		}

		public int type { get; set; }

		public int id { get; set; }

		public string parentKey { get; set; }

		public Vector3[,] addMove { get; set; }

		public ColorInfo[] colorInfo { get; set; }

		public int hideCategory { get; set; }

		public int hideTiming { get; set; }

		public bool noShake { get; set; }

		[IgnoreMember]
		public bool partsOfHead { get; set; }

		public PartsInfo()
		{
			MemberInit();
		}

		public void MemberInit()
		{
			type = 120;
			id = 0;
			parentKey = "";
			addMove = new Vector3[2, 3];
			for (int i = 0; i < 2; i++)
			{
				addMove[i, 0] = Vector3.zero;
				addMove[i, 1] = Vector3.zero;
				addMove[i, 2] = Vector3.one;
			}
			colorInfo = new ColorInfo[4];
			for (int j = 0; j < colorInfo.Length; j++)
			{
				colorInfo[j] = new ColorInfo();
			}
			hideCategory = 0;
			hideTiming = 1;
			partsOfHead = false;
			noShake = false;
		}
	}

	public Version version { get; set; }

	public PartsInfo[] parts { get; set; }

	public ChaFileAccessory()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileAccessoryVersion;
		parts = new PartsInfo[20];
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i] = new PartsInfo();
		}
	}

	public void ComplementWithVersion()
	{
		version = ChaFileDefine.ChaFileAccessoryVersion;
	}
}
