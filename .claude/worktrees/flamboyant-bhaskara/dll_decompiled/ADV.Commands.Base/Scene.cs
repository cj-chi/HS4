using Manager;
using UniRx.Async;

namespace ADV.Commands.Base;

public class Scene : CommandBase
{
	public override string[] ArgsLabel => new string[6] { "Bundle", "Asset", "isLoad", "isAsync", "isFade", "isImageDraw" };

	public override string[] ArgsDefault => new string[7]
	{
		string.Empty,
		string.Empty,
		bool.FalseString,
		bool.TrueString,
		bool.TrueString,
		bool.FalseString,
		bool.FalseString
	};

	public override void Do()
	{
		base.Do();
		int num = 0;
		string bundleName = args[num++];
		string levelName = args[num++];
		bool flag = bool.Parse(args[num++]);
		bool num2 = bool.Parse(args[num++]);
		bool isFade = bool.Parse(args[num++]);
		bool isLoadingImageDraw = bool.Parse(args[num++]);
		if (!num2)
		{
			Manager.Scene.LoadReserve(new Manager.Scene.Data
			{
				bundleName = bundleName,
				levelName = levelName,
				isAdd = !flag,
				isFade = isFade
			}, isLoadingImageDraw);
		}
		else
		{
			Manager.Scene.LoadReserveAsync(new Manager.Scene.Data
			{
				bundleName = bundleName,
				levelName = levelName,
				isAdd = !flag,
				isFade = isFade
			}, isLoadingImageDraw).Forget();
		}
	}
}
