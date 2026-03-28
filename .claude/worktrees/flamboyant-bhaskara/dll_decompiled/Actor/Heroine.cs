using System;
using System.Collections.Generic;
using System.IO;
using ADV;
using AIChara;
using Illusion.Extensions;
using Manager;

namespace Actor;

[Serializable]
public class Heroine : CharaData
{
	public int fixCharaID;

	private int? cachedVoiceNo;

	private CharaParams _param;

	public bool isConcierge => fixCharaID <= -1;

	public int FixCharaIDOrPersonality
	{
		get
		{
			if (fixCharaID == 0)
			{
				return base.personality;
			}
			return fixCharaID;
		}
	}

	public string ChaName => "c" + FixCharaIDOrPersonality.MinusThroughToString("00");

	public string ChaVoice => "c" + voiceNo.MinusThroughToString("00");

	public override int voiceNo
	{
		get
		{
			VoiceInfo.Param value;
			int? num = (cachedVoiceNo = cachedVoiceNo ?? (Voice.infoTable.TryGetValue(FixCharaIDOrPersonality, out value) ? value.No : base.personality));
			return num.Value;
		}
	}

	public override CharaParams param => this.GetCache(ref _param, () => new CharaParams(this, "H"));

	public override IEnumerable<CommandData> CreateCommandData(string head)
	{
		List<CommandData> list = new List<CommandData>();
		string head2 = head + "gameinfo2.";
		base.gameinfo2.AddList(list, head2);
		return list;
	}

	public override void Randomize()
	{
	}

	public override void Initialize()
	{
	}

	public override void ChaFileUpdate()
	{
		base.chaFile.parameter.sex = 1;
	}

	public Heroine(ChaFileControl chaFile, bool isRandomize)
		: base(chaFile, isRandomize)
	{
	}

	public Heroine(bool isRandomize)
		: base(isRandomize)
	{
	}

	public Heroine(Version version, BinaryReader r)
		: base(isRandomize: false)
	{
	}
}
