using System;
using Illusion.Component.UI.ColorPicker;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SearchColorCtrl : MonoBehaviour
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

	public SearchColorSet linkSearchColorSet;

	protected SearchBase searchBase => Singleton<SearchBase>.Instance;

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

	public void Setup(SearchColorSet ccSet, Color color, Action<Color> _actUpdateColor, bool _useAlpha)
	{
		linkSearchColorSet = ccSet;
		if (null != sampleColor)
		{
			sampleColor.SetColor(color);
			sampleColor.actUpdateColor = _actUpdateColor;
		}
		searchBase.searchCtrl.showColorCvs = true;
		cmpPickerRect.isAlpha.Value = _useAlpha;
		cmpPickerSliderI.useAlpha.Value = _useAlpha;
	}

	public void EnableAlpha(bool enable)
	{
		cmpPickerRect.isAlpha.Value = enable;
		cmpPickerSliderI.useAlpha.Value = enable;
	}

	public void SetColor(SearchColorSet ccSet, Color color)
	{
		if (!(ccSet != linkSearchColorSet) && null != sampleColor)
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
		searchBase.searchCtrl.showColorCvs = false;
		linkSearchColorSet = null;
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
