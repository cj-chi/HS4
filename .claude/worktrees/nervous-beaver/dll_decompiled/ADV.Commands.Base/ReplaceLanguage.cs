using Manager;

namespace ADV.Commands.Base;

public class ReplaceLanguage : CommandBase
{
	public override string[] ArgsLabel => new string[2] { "Src", "Dst" };

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		int num = 0;
		string key = args[num++];
		string dst = string.Empty;
		num += Singleton<GameSystem>.Instance.languageInt;
		args.SafeProc(num++, delegate(string s)
		{
			dst = s;
		});
		base.scenario.Replaces[key] = dst;
	}
}
