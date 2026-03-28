using Illusion.Anime;

namespace Manager;

public sealed class ADVManager : Singleton<ADVManager>
{
	public class ADVDelivery
	{
		public int charaID { get; private set; }

		public int adv_category { get; private set; }

		public string asset { get; private set; }

		public ADVDelivery()
		{
		}

		public ADVDelivery(string asset, int charaID, int adv_category)
		{
			this.charaID = charaID;
			this.adv_category = adv_category;
			this.asset = asset;
		}

		public void Set(string asset, int charaID, int adv_category)
		{
			this.charaID = charaID;
			this.adv_category = adv_category;
			this.asset = asset;
		}
	}

	public string[] filenames = new string[2] { "", "" };

	public ADVDelivery advDelivery { get; } = new ADVDelivery();

	protected override void Awake()
	{
		if (CheckInstance())
		{
			Loader.LoadAnimePlayStateTable(AssetBundleNames.AdvActionPath);
			Loader.LoadMotionIKDataTable(AssetBundleNames.AdvAction_IkPath);
			Loader.LoadEventItemTable(AssetBundleNames.AdvItemObjectPath);
			Loader.LoadEventItemScaleTable(AssetBundleNames.AdvItemScalePath);
			Loader.LoadEventItemKeyTable(AssetBundleNames.AdvItemVisiblePath);
		}
	}
}
