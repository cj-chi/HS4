namespace Config;

public class TitleSystem : BaseSystem
{
	public bool isTitleCharaLoad;

	public bool isUseUserCharaCard;

	public string charaCardFileNameFullPath = "";

	public bool isCustomBGMChange;

	public int customBGMNo = 3;

	public TitleSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		isTitleCharaLoad = false;
		isUseUserCharaCard = false;
		charaCardFileNameFullPath = "";
		isCustomBGMChange = false;
		customBGMNo = 3;
	}
}
