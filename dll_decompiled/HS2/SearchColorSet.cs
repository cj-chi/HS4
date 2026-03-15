using System;
using System.Linq;
using AIChara;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SearchColorSet : MonoBehaviour
{
	public Text title;

	public Button button;

	public Image image;

	public bool useAlpha;

	public Action<Color> actUpdateColor;

	private SearchBase searchBase => Singleton<SearchBase>.Instance;

	protected ChaControl chaCtrl => searchBase.chaCtrl;

	public void SetColor(Color color)
	{
		image.color = color;
		searchBase.searchColorCtrl.SetColor(this, color);
	}

	public void EnableColorAlpha(bool enable)
	{
		useAlpha = enable;
		searchBase.searchColorCtrl.EnableAlpha(useAlpha);
	}

	public void Reset()
	{
		title = base.transform.GetComponentInChildren<Text>();
		Image[] componentsInChildren = base.transform.GetComponentsInChildren<Image>();
		if (componentsInChildren != null)
		{
			image = componentsInChildren.Where((Image x) => x.name == "imgColor").FirstOrDefault();
		}
		button = base.transform.GetComponentInChildren<Button>();
	}

	public void Start()
	{
		if (!button)
		{
			return;
		}
		button.OnClickAsObservable().Subscribe(delegate
		{
			searchBase.searchColorCtrl.Setup(this, image.color, delegate(Color color)
			{
				image.color = color;
				actUpdateColor?.Invoke(color);
			}, useAlpha);
		});
	}
}
