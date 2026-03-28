using System;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileParameter2
{
	[IgnoreMember]
	public static readonly string BlockName = "Parameter2";

	public Version version { get; set; }

	public int personality { get; set; }

	public float voiceRate { get; set; }

	[IgnoreMember]
	public float voicePitch => Mathf.Lerp(0.94f, 1.06f, voiceRate);

	public byte trait { get; set; }

	public byte mind { get; set; }

	public byte hAttribute { get; set; }

	public ChaFileParameter2()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileParameterVersion2;
		personality = 0;
		voiceRate = 0.5f;
		trait = 0;
		mind = 0;
		hAttribute = 0;
	}

	public void Copy(ChaFileParameter2 src)
	{
		version = src.version;
		personality = src.personality;
		voiceRate = src.voiceRate;
		trait = src.trait;
		mind = src.mind;
		hAttribute = src.hAttribute;
	}

	public void ComplementWithVersion()
	{
		version = ChaFileDefine.ChaFileParameterVersion2;
	}
}
