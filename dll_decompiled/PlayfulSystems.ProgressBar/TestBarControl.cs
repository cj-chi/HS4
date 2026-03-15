using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSystems.ProgressBar;

public class TestBarControl : MonoBehaviour
{
	[SerializeField]
	private Transform barParent;

	[SerializeField]
	private Transform sizeButtonParent;

	[SerializeField]
	private TestSwitchBar[] barSelectors;

	private ProgressBarPro[] bars;

	private Button[] buttons;

	private Slider slider;

	private void Start()
	{
		if (barParent != null)
		{
			bars = barParent.GetComponentsInChildren<ProgressBarPro>(includeInactive: true);
		}
		if (sizeButtonParent != null)
		{
			buttons = sizeButtonParent.GetComponentsInChildren<Button>();
			slider = GetComponentInChildren<Slider>();
			SetupButtons();
		}
	}

	private void SetupButtons()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			float currentValue = (float)i / (float)(buttons.Length - 1);
			Button obj = buttons[i];
			obj.name = "Button_" + currentValue;
			obj.GetComponentInChildren<Text>().text = currentValue.ToString();
			obj.onClick.AddListener(delegate
			{
				SetSlider(currentValue);
			});
		}
	}

	private void SetSlider(float value)
	{
		if (slider != null)
		{
			slider.value = value;
		}
	}

	public void SetBars(float value)
	{
		if (bars != null)
		{
			for (int i = 0; i < bars.Length; i++)
			{
				bars[i].SetValue(value);
			}
		}
		if (barSelectors != null)
		{
			for (int j = 0; j < barSelectors.Length; j++)
			{
				barSelectors[j].SetValue(value);
			}
		}
	}

	public void SetRandomColor()
	{
		SetColor(new Color(Random.value, Random.value, Random.value));
	}

	public void SetColor(Color color)
	{
		if (bars != null)
		{
			for (int i = 0; i < bars.Length; i++)
			{
				bars[i].SetBarColor(color);
			}
		}
		if (barSelectors != null)
		{
			for (int j = 0; j < barSelectors.Length; j++)
			{
				barSelectors[j].SetBarColor(color);
			}
		}
	}
}
