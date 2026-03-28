using UnityEngine;

namespace ADV.Commands.Movie;

public class Play : CommandBase
{
	private MoviePlayerGUI player;

	private bool skipReg;

	public override string[] ArgsLabel => new string[3] { "Bundle", "Asset", "isSkipReg" };

	public override string[] ArgsDefault => new string[3]
	{
		string.Empty,
		string.Empty,
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		player = null;
		int num = 0;
		GameObject gameObject = CommonLib.LoadAsset<GameObject>(args[num++], args[num++], clone: true);
		if (!bool.TryParse(args.SafeGet(num++), out skipReg))
		{
			skipReg = false;
		}
		if (skipReg)
		{
			MainScenario mainScenario = base.scenario as MainScenario;
			if (mainScenario != null && mainScenario.mode == MainScenario.Mode.Movie)
			{
				base.scenario.regulate.AddRegulate(Regulate.Control.Auto);
			}
			else
			{
				base.scenario.regulate.AddRegulate((Regulate.Control)13);
			}
		}
		player = gameObject.GetComponent<MoviePlayerGUI>();
	}

	public override bool Process()
	{
		base.Process();
		return !player.player.isPlaying;
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		Destroy();
	}

	private void Destroy()
	{
		UnityEngine.Object.Destroy(player.gameObject);
		if (skipReg)
		{
			MainScenario mainScenario = base.scenario as MainScenario;
			if (mainScenario != null && mainScenario.mode == MainScenario.Mode.Movie)
			{
				mainScenario.regulate.SubRegulate(Regulate.Control.Auto);
			}
			else
			{
				base.scenario.regulate.SetRegulate((Regulate.Control)0);
			}
		}
	}
}
