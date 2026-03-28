using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileHair
{
	[MessagePackObject(true)]
	public class PartsInfo
	{
		[MessagePackObject(true)]
		public class BundleInfo
		{
			public Vector3 moveRate { get; set; }

			public Vector3 rotRate { get; set; }

			public bool noShake { get; set; }

			public BundleInfo()
			{
				MemberInit();
			}

			public void MemberInit()
			{
				moveRate = Vector3.zero;
				rotRate = Vector3.zero;
				noShake = false;
			}
		}

		[MessagePackObject(true)]
		public class ColorInfo
		{
			public Color color { get; set; }

			public ColorInfo()
			{
				MemberInit();
			}

			public void MemberInit()
			{
				color = Color.white;
			}
		}

		public int id { get; set; }

		public Color baseColor { get; set; }

		public Color topColor { get; set; }

		public Color underColor { get; set; }

		public Color specular { get; set; }

		public float metallic { get; set; }

		public float smoothness { get; set; }

		public ColorInfo[] acsColorInfo { get; set; }

		public int bundleId { get; set; }

		public Dictionary<int, BundleInfo> dictBundle { get; set; }

		public int meshType { get; set; }

		public Color meshColor { get; set; }

		public Vector4 meshLayout { get; set; }

		public PartsInfo()
		{
			MemberInit();
		}

		public void MemberInit()
		{
			id = 0;
			baseColor = new Color(0.2f, 0.2f, 0.2f);
			topColor = new Color(0.039f, 0.039f, 0.039f);
			underColor = new Color(0.565f, 0.565f, 0.565f);
			specular = new Color(0.3f, 0.3f, 0.3f);
			metallic = 0f;
			smoothness = 0f;
			acsColorInfo = new ColorInfo[4];
			for (int i = 0; i < acsColorInfo.Length; i++)
			{
				acsColorInfo[i] = new ColorInfo();
			}
			bundleId = -1;
			dictBundle = new Dictionary<int, BundleInfo>();
			meshType = 0;
			meshColor = new Color(1f, 1f, 1f, 1f);
			meshLayout = new Vector4(1f, 1f, 0f, 0f);
		}
	}

	public Version version { get; set; }

	public bool sameSetting { get; set; }

	public bool autoSetting { get; set; }

	public bool ctrlTogether { get; set; }

	public PartsInfo[] parts { get; set; }

	public int kind { get; set; }

	public int shaderType { get; set; }

	public ChaFileHair()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileHairVersion;
		sameSetting = true;
		autoSetting = true;
		ctrlTogether = false;
		parts = new PartsInfo[Enum.GetValues(typeof(ChaFileDefine.HairKind)).Length];
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i] = new PartsInfo();
		}
		kind = 0;
		shaderType = 0;
	}

	public void ComplementWithVersion()
	{
		if (version < new Version("0.0.1"))
		{
			for (int i = 0; i < parts.Length; i++)
			{
				parts[i].acsColorInfo = new PartsInfo.ColorInfo[4];
				for (int j = 0; j < parts[i].acsColorInfo.Length; j++)
				{
					parts[i].acsColorInfo[j] = new PartsInfo.ColorInfo();
				}
			}
		}
		if (version < new Version("0.0.2"))
		{
			sameSetting = true;
			autoSetting = true;
			ctrlTogether = false;
		}
		if (version < new Version("0.0.3"))
		{
			PartsInfo.BundleInfo value;
			if (4 == parts[0].id)
			{
				if (parts[0].dictBundle.TryGetValue(0, out value))
				{
					value.rotRate = new Vector3(value.rotRate.x, 1f - value.rotRate.y, value.rotRate.z);
				}
			}
			else if (5 == parts[0].id)
			{
				if (parts[0].dictBundle.TryGetValue(0, out value))
				{
					float value2 = Mathf.Lerp(3f, 30f, value.rotRate.z);
					float z = Mathf.InverseLerp(-30f, 30f, value2);
					value.rotRate = new Vector3(value.rotRate.x, value.rotRate.y, z);
				}
				if (parts[0].dictBundle.TryGetValue(1, out value))
				{
					float value3 = Mathf.Lerp(0.1f, -0.1f, value.moveRate.z);
					float z2 = Mathf.InverseLerp(0.1f, -0.4f, value3);
					value.moveRate = new Vector3(value.moveRate.x, value.moveRate.y, z2);
				}
				if (parts[0].dictBundle.TryGetValue(2, out value))
				{
					float value4 = Mathf.Lerp(-25f, 45f, value.rotRate.x);
					float x = Mathf.InverseLerp(-25f, 50f, value4);
					value.rotRate = new Vector3(x, value.rotRate.y, value.rotRate.z);
				}
				if (parts[0].dictBundle.TryGetValue(3, out value))
				{
					float value5 = Mathf.Lerp(-0.1f, -0.4f, value.moveRate.z);
					float z3 = Mathf.InverseLerp(-0.1f, 0.4f, value5);
					value.moveRate = new Vector3(value.moveRate.x, value.moveRate.y, z3);
					value5 = Mathf.Lerp(-22.5f, 45f, value.rotRate.x);
					z3 = Mathf.InverseLerp(45f, -22.5f, value5);
					value.rotRate = new Vector3(z3, value.rotRate.y, value.rotRate.z);
				}
				if (parts[0].dictBundle.TryGetValue(4, out value))
				{
					float value6 = Mathf.Lerp(-22.5f, 45f, value.rotRate.x);
					float x2 = Mathf.InverseLerp(45f, -22.5f, value6);
					value.rotRate = new Vector3(x2, value.rotRate.y, value.rotRate.z);
				}
			}
			else if (8 == parts[0].id && parts[0].dictBundle.TryGetValue(0, out value))
			{
				value.rotRate = new Vector3(value.rotRate.x, 1f - value.rotRate.y, value.rotRate.z);
			}
			if (7 == parts[1].id && parts[0].dictBundle.TryGetValue(0, out value))
			{
				float value7 = Mathf.Lerp(-70f, 35f, value.rotRate.y);
				float y = Mathf.InverseLerp(70f, -35f, value7);
				value.rotRate = new Vector3(value.rotRate.x, y, value.rotRate.z);
			}
		}
		version = ChaFileDefine.ChaFileHairVersion;
	}
}
