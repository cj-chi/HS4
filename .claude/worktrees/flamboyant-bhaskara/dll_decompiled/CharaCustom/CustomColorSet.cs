using System;
using System.Linq;
using AIChara;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomColorSet : MonoBehaviour
{
	public Text title;

	public Button button;

	public Image image;

	public bool useAlpha;

	public Action<Color> actUpdateColor;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	public void SetColor(Color color)
	{
		image.color = color;
		customBase.customColorCtrl.SetColor(this, color);
	}

	public void EnableColorAlpha(bool enable)
	{
		useAlpha = enable;
		customBase.customColorCtrl.EnableAlpha(useAlpha);
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
			customBase.customColorCtrl.Setup(this, image.color, delegate(Color color)
			{
				image.color = color;
				actUpdateColor?.Invoke(color);
			}, useAlpha);
		});
	}
}
