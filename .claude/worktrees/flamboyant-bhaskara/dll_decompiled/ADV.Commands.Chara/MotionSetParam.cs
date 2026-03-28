using Illusion;
using UnityEngine;

namespace ADV.Commands.Chara;

public class MotionSetParam : CommandBase
{
	public override string[] ArgsLabel => new string[3] { "No", "Name", "Value" };

	public override string[] ArgsDefault => new string[3]
	{
		int.MaxValue.ToString(),
		string.Empty,
		string.Empty
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		int no = int.Parse(args[num++]);
		string name = args[num++];
		string text = args[num++];
		Animator animBody = base.scenario.commandController.GetChara(no).chaCtrl.animBody;
		switch (Utils.Animator.GetAnimeParam(name, animBody).type)
		{
		case AnimatorControllerParameterType.Float:
			animBody.SetFloat(name, float.Parse(text));
			break;
		case AnimatorControllerParameterType.Int:
			animBody.SetInteger(name, int.Parse(text));
			break;
		case AnimatorControllerParameterType.Bool:
			animBody.SetBool(name, bool.Parse(text));
			break;
		case (AnimatorControllerParameterType)2:
			break;
		}
	}
}
