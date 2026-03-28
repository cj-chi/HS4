using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileParameter
{
	[IgnoreMember]
	public static readonly string BlockName = "Parameter";

	public Version version { get; set; }

	public byte sex { get; set; }

	public string fullname { get; set; }

	public int personality { get; set; }

	public byte birthMonth { get; set; }

	public byte birthDay { get; set; }

	[IgnoreMember]
	public string strBirthDay => ChaFileDefine.GetBirthdayStr(birthMonth, birthDay);

	public float voiceRate { get; set; }

	[IgnoreMember]
	public float voicePitch => Mathf.Lerp(0.94f, 1.06f, voiceRate);

	public HashSet<int> hsWish { get; set; }

	[IgnoreMember]
	public int wish01
	{
		get
		{
			if (hsWish.Count == 0)
			{
				return -1;
			}
			return hsWish.ToArray()[0];
		}
	}

	[IgnoreMember]
	public int wish02
	{
		get
		{
			if (1 >= hsWish.Count)
			{
				return -1;
			}
			return hsWish.ToArray()[1];
		}
	}

	[IgnoreMember]
	public int wish03
	{
		get
		{
			if (2 >= hsWish.Count)
			{
				return -1;
			}
			return hsWish.ToArray()[2];
		}
	}

	public bool futanari { get; set; }

	public ChaFileParameter()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileParameterVersion;
		sex = 0;
		fullname = "";
		personality = 0;
		birthMonth = 1;
		birthDay = 1;
		voiceRate = 0.5f;
		hsWish = new HashSet<int>();
		futanari = false;
	}

	public void Copy(ChaFileParameter src)
	{
		version = src.version;
		sex = src.sex;
		fullname = src.fullname;
		personality = src.personality;
		birthMonth = src.birthMonth;
		birthDay = src.birthDay;
		voiceRate = src.voiceRate;
		hsWish = new HashSet<int>(src.hsWish);
		futanari = src.futanari;
	}

	public void ComplementWithVersion()
	{
		if (version < new Version("0.0.1"))
		{
			hsWish = new HashSet<int>();
		}
		version = ChaFileDefine.ChaFileParameterVersion;
	}
}
