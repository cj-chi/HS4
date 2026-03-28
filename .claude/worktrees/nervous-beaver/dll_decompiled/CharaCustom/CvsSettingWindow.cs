using System.Linq;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsSettingWindow : MonoBehaviour
{
	public CanvasGroup cgbaseWindow;

	public Button[] btnClose;

	public virtual void Start()
	{
		if (!btnClose.Any())
		{
			return;
		}
		btnClose.Where((Button item) => null != item).ToList().ForEach(delegate(Button item)
		{
			item.OnClickAsObservable().Subscribe(delegate
			{
				if ((bool)cgbaseWindow)
				{
					cgbaseWindow.Enable(enable: false);
				}
			});
		});
	}
}
