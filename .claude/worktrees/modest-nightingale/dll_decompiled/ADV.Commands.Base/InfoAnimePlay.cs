namespace ADV.Commands.Base;

public class InfoAnimePlay : CommandBase
{
	public override string[] ArgsLabel => new string[5] { "crossFadeTime", "isCrossFade", "layerNo", "transitionDuration", "normalizedTime" };

	public override string[] ArgsDefault => null;

	public override void Do()
	{
		base.Do();
		int num = 0;
		Info.Anime.Play play = base.scenario.info.anime.play;
		args.SafeProc(num++, delegate(string s)
		{
			play.crossFadeTime = float.Parse(s);
		});
		args.SafeProc(num++, delegate(string s)
		{
			play.isCrossFade = bool.Parse(s);
		});
		args.SafeProc(num++, delegate(string s)
		{
			play.layerNo = int.Parse(s);
		});
		args.SafeProc(num++, delegate(string s)
		{
			play.transitionDuration = float.Parse(s);
		});
		args.SafeProc(num++, delegate(string s)
		{
			play.normalizedTime = float.Parse(s);
		});
	}
}
