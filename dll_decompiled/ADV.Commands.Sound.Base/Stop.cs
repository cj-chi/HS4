using Manager;
using UnityEngine;

namespace ADV.Commands.Sound.Base;

public abstract class Stop : CommandBase
{
	private Manager.Sound.Type type;

	private float stopTime;

	private float timer;

	public override string[] ArgsLabel => new string[1] { "Time" };

	public override string[] ArgsDefault => new string[1] { "0" };

	public Stop(Manager.Sound.Type type)
	{
		this.type = type;
	}

	public override void Do()
	{
		base.Do();
		float.TryParse(args.SafeGet(0), out stopTime);
	}

	public override bool Process()
	{
		base.Process();
		if (timer >= stopTime)
		{
			return true;
		}
		timer += Time.deltaTime;
		return false;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		Manager.Sound.Stop(type);
	}
}
