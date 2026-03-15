using Manager;
using UnityEngine;

namespace Config;

public class AppendSetting : BaseSetting
{
	[Header("男Mobカラー")]
	[SerializeField]
	private UI_SampleColor mobCololr_M;

	[Header("女Mobカラー")]
	[SerializeField]
	private UI_SampleColor mobCololr_F;

	public override void Init()
	{
		AppendSystem append = Manager.Config.AppendData;
		mobCololr_M.actUpdateColor = delegate(Color c)
		{
			append.MobMColor_M = c;
		};
		mobCololr_F.actUpdateColor = delegate(Color c)
		{
			append.MobMColor_F = c;
		};
	}

	protected override void ValueToUI()
	{
		AppendSystem appendData = Manager.Config.AppendData;
		mobCololr_M.SetColor(appendData.MobMColor_M);
		mobCololr_F.SetColor(appendData.MobMColor_F);
	}
}
