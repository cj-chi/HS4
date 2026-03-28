using System;
using System.Collections.Generic;
using ADV;
using AIChara;

namespace Actor;

[Serializable]
public class Player : CharaData
{
	private int? cachedVoiceNo;

	private CharaParams _param;

	public override int voiceNo
	{
		get
		{
			int? num = (cachedVoiceNo = cachedVoiceNo ?? base.personality);
			return num.Value;
		}
	}

	public override CharaParams param => this.GetCache(ref _param, () => new CharaParams(this, "P"));

	public override IEnumerable<CommandData> CreateCommandData(string head)
	{
		List<CommandData> list = new List<CommandData>();
		string key = head + "sex";
		list.Add(new CommandData(CommandData.Command.Int, key, () => (int)base.parameter.sex));
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
	}

	public Player()
		: base(isRandomize: false)
	{
	}

	public Player(ChaFileControl chaFile, bool isRandomize)
		: base(chaFile, isRandomize)
	{
	}

	public Player(bool isRandomize)
		: base(isRandomize)
	{
	}
}
