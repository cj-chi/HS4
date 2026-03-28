using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class VoiceUI : MonoBehaviour
{
	[SerializeField]
	protected int index;

	[SerializeField]
	protected Toggle toggle;

	[SerializeField]
	protected Slider slider;

	public void Refresh()
	{
		if (Voice._Config.chara.ContainsKey(index))
		{
			toggle.isOn = Voice._Config.chara[index].sound.Switch;
			slider.value = Voice._Config.chara[index].sound.Volume;
		}
	}

	protected void OnValueChangeToggle(bool _value)
	{
		Voice._Config.chara[index].sound.Switch = _value;
	}

	protected void OnValueChangeSlider(float _value)
	{
		Voice._Config.chara[index].sound.Volume = Mathf.FloorToInt(_value);
	}

	protected void Reset()
	{
		if (toggle == null)
		{
			toggle = GetComponentInChildren<Toggle>();
		}
		if (slider == null)
		{
			slider = GetComponentInChildren<Slider>();
		}
	}

	protected void Start()
	{
		if (Voice._Config.chara.ContainsKey(index))
		{
			Refresh();
			toggle.onValueChanged.AddListener(delegate(bool value)
			{
				OnValueChangeToggle(value);
			});
			slider.onValueChanged.AddListener(delegate(float value)
			{
				OnValueChangeSlider(value);
			});
		}
	}
}
