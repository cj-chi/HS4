using System.Collections;
using System.Linq;
using UniRx;

namespace CharaCustom;

public class CvsSelectAccessory : CvsSelectWindow
{
	public override IEnumerator Start()
	{
		yield return base.Start();
		Singleton<CustomBase>.Instance.ChangeAcsSlotColor(0);
		if (!items.Any())
		{
			yield break;
		}
		(from item in items.Select((ItemInfo val, int idx) => new { val, idx })
			where item.val != null && item.val.btnItem != null
			select item).ToList().ForEach(item =>
		{
			item.val.btnItem.OnClickAsObservable().Subscribe(delegate
			{
				int idx = item.idx;
				if (idx < 0 || 19 < idx)
				{
					idx = -1;
				}
				Singleton<CustomBase>.Instance.ChangeAcsSlotColor(item.idx);
			});
		});
	}
}
