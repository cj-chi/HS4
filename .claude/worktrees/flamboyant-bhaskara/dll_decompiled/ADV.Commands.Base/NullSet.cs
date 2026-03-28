using Illusion;
using Illusion.Extensions;
using UnityEngine;

namespace ADV.Commands.Base;

public class NullSet : CommandBase
{
	private enum Type
	{
		Base,
		Camera,
		Chara
	}

	public override string[] ArgsLabel => new string[2] { "Name", "Type" };

	public override string[] ArgsDefault => new string[2]
	{
		string.Empty,
		Type.Base.ToString()
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		string name = args[num++];
		switch ((Type)args[num++].Check(ignoreCase: true, Utils.Enum<Type>.Names))
		{
		case Type.Base:
			Set(base.scenario.commandController.BasePositon, name);
			break;
		case Type.Camera:
		{
			Transform cameraPosition = base.scenario.commandController.CameraPosition;
			Vector3 position = cameraPosition.position;
			Quaternion rotation = cameraPosition.rotation;
			cameraPosition.position = Vector3.zero;
			cameraPosition.rotation = Quaternion.identity;
			Set(base.scenario.advScene.advCamera.transform, name);
			cameraPosition.SetPositionAndRotation(position, rotation);
			break;
		}
		case Type.Chara:
			Set(base.scenario.commandController.Character, name);
			break;
		}
	}

	private void Set(Transform transform, string name)
	{
		if (base.scenario.commandController.NullTable.TryGetValue(name, out var value))
		{
			transform.SetPositionAndRotation(value.position, value.rotation);
		}
	}
}
