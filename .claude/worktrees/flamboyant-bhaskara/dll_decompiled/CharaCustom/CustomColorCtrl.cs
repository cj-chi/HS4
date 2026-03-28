using System;
using Illusion.Component.UI.ColorPicker;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomColorCtrl : MonoBehaviour
{
	[Tooltip("このキャンバスグループ")]
	[SerializeField]
	private CanvasGroup cgWindow;

	[Tooltip("閉じるボタン")]
	[SerializeField]
	private Button btnClose;

	[Tooltip("サンプルカラーScript")]
	[SerializeField]
	private UI_SampleColor sampleColor;

	[Tooltip("PickerのRect")]
	[SerializeField]
	private PickerRectA cmpPickerRect;

	[Tooltip("PickerのSlider")]
	[SerializeField]
	private PickerSliderInput cmpPickerSliderI;

	public CustomColorSet linkCustomColorSet;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	public bool isOpen
	{
		get
		{
			if (0f != cgWindow.alpha)
			{
				return true;
			}
			return false;
		}
	}

	public void Setup(CustomColorSet ccSet, Color color, Action<Color> _actUpdateColor, bool _useAlpha)
	{
		linkCustomColorSet = ccSet;
		if (null != sampleColor)
		{
			sampleColor.SetColor(color);
			sampleColor.actUpdateColor = _actUpdateColor;
		}
		customBase.customCtrl.showColorCvs = true;
		cmpPickerRect.isAlpha.Value = _useAlpha;
		cmpPickerSliderI.useAlpha.Value = _useAlpha;
	}

	public void EnableAlpha(bool enable)
	{
		cmpPickerRect.isAlpha.Value = enable;
		cmpPickerSliderI.useAlpha.Value = enable;
	}

	public void SetColor(CustomColorSet ccSet, Color color)
	{
		if (!(ccSet != linkCustomColorSet) && null != sampleColor)
		{
			sampleColor.SetColor(color);
		}
	}

	public void Close()
	{
		if (null != sampleColor)
		{
			sampleColor.actUpdateColor = null;
		}
		customBase.customCtrl.showColorCvs = false;
		linkCustomColorSet = null;
	}

	private void Start()
	{
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			});
		}
	}
}
