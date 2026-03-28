using System.Linq;
using UnityEngine;

namespace ADV.Commands.Camera;

public class SetParent : Base
{
	public override string[] ArgsLabel => new string[3] { "ParentName", "ChildName", "isWorldPositionStays" };

	public override string[] ArgsDefault => new string[3]
	{
		string.Empty,
		string.Empty,
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		UnityEngine.Camera advCamera = base.scenario.advScene.advCamera;
		int num = 0;
		string text = args[num++];
		string childName = args[num++];
		GameObject gameObject = null;
		if (!int.TryParse(text, out var _))
		{
			gameObject = GameObject.Find(text);
		}
		Transform parent = gameObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform p) => p.name == childName);
		bool worldPositionStays = bool.Parse(args[num++]);
		advCamera.transform.SetParent(parent, worldPositionStays);
	}
}
