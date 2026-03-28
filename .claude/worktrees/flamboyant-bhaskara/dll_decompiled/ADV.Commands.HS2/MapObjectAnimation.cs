using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

namespace ADV.Commands.HS2;

public class MapObjectAnimation : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "Find", "State" };

	public override string[] ArgsDefault => new string[2]
	{
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		BaseMap.mapRoot.transform.FindLoop(args[num++]).GetComponent<Animator>().Play(args[num++], 0, 0f);
	}
}
