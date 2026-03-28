using System;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Object;

public class Rotation : CommandBase
{
	private enum Type
	{
		Add,
		Set
	}

	public override string[] ArgsLabel => new string[5] { "Name", "Type", "Pitch", "Yaw", "Roll" };

	public override string[] ArgsDefault => new string[5]
	{
		string.Empty,
		Type.Add.ToString(),
		string.Empty,
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int cnt = 0;
		string key = args[cnt++];
		int num = args[cnt++].Check(ignoreCase: true, Enum.GetNames(typeof(Type)));
		Vector3 v = Vector3.zero;
		CommandBase.CountAddV3(args, ref cnt, ref v);
		switch ((Type)num)
		{
		case Type.Add:
			base.scenario.commandController.Objects[key].transform.eulerAngles += v;
			break;
		case Type.Set:
			base.scenario.commandController.Objects[key].transform.eulerAngles = v;
			break;
		}
	}
}
