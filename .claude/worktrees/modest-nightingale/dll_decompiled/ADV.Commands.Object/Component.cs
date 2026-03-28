using System;
using Illusion;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Object;

public class Component : CommandBase
{
	private enum Type
	{
		Add,
		Sub
	}

	public override string[] ArgsLabel => new string[5] { "Type", "ComponentType", "FindType", "ChildName", "RootName" };

	public override string[] ArgsDefault => new string[5]
	{
		Type.Add.ToString(),
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int num2 = args[num++].Check(ignoreCase: true, Enum.GetNames(typeof(Type)));
		string typeName = args[num++];
		string findType = args[num++];
		string childName = args[num++];
		string otherRootName = args[num++];
		GameObject gameObject = ObjectEx.FindGet(findType, childName, otherRootName, base.scenario.commandController).gameObject;
		System.Type type = Utils.Type.Get(typeName);
		switch ((Type)num2)
		{
		case Type.Add:
			gameObject.AddComponent(type);
			break;
		case Type.Sub:
			UnityEngine.Object.Destroy(gameObject.GetComponent(type));
			break;
		}
	}
}
