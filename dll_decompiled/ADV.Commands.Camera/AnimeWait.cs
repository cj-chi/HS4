using UnityEngine;

namespace ADV.Commands.Camera;

public class AnimeWait : CommandBase
{
	private int layerNo;

	private float time;

	private Animator animator;

	public override string[] ArgsLabel => new string[2] { "LayerNo", "Time" };

	public override string[] ArgsDefault => new string[2] { "0", "1" };

	public override void Do()
	{
		base.Do();
		int num = 0;
		layerNo = int.Parse(args[num++]);
		time = float.Parse(args[num++]);
		animator = base.scenario.advScene.advCamera.GetComponent<Animator>();
	}

	public override bool Process()
	{
		base.Process();
		if (!(animator == null))
		{
			return animator.GetCurrentAnimatorStateInfo(layerNo).normalizedTime >= time;
		}
		return true;
	}
}
