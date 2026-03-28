using Config;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class ConfigCtrl : MonoBehaviour
{
	[SerializeField]
	private Button buttonColor;

	[SerializeField]
	private Toggle toggleShield;

	[SerializeField]
	private Toggle[] togglesTexture;

	[SerializeField]
	private Toggle[] toggleSound;

	[SerializeField]
	private Slider[] sliderSound;

	private SoundData[] soundData { get; set; }

	private void OnClickColor()
	{
		if (Singleton<Studio>.Instance.colorPalette.Check("背景色"))
		{
			Singleton<Studio>.Instance.colorPalette.visible = false;
		}
		else
		{
			Singleton<Studio>.Instance.colorPalette.Setup("背景色", Manager.Config.GraphicData.BackColor, OnValueChangeColor, _useAlpha: false);
		}
	}

	private void OnValueChangeColor(Color _color)
	{
		Manager.Config.GraphicData.BackColor = _color;
		buttonColor.image.color = _color;
		Camera.main.backgroundColor = _color;
	}

	private void OnOnValueChangedTexture(int _no)
	{
	}

	private void OnValueChangedMute(bool _value, int _idx)
	{
		soundData[_idx].Switch = _value;
		sliderSound[_idx].interactable = _value;
	}

	private void OnValueChangedVolume(float _value, int _idx)
	{
		soundData[_idx].Volume = Mathf.FloorToInt(_value);
	}

	private void Start()
	{
		soundData = new SoundData[6]
		{
			Manager.Config.SoundData.Master,
			Manager.Config.SoundData.BGM,
			Manager.Config.SoundData.GameSE,
			Manager.Config.SoundData.SystemSE,
			Manager.Config.SoundData.ENV,
			Voice._Config.PCM
		};
		buttonColor.image.color = Manager.Config.GraphicData.BackColor;
		toggleShield.isOn = Manager.Config.GraphicData.Shield;
		for (int i = 0; i < 6; i++)
		{
			toggleSound[i].isOn = soundData[i].Switch;
			sliderSound[i].interactable = soundData[i].Switch;
			sliderSound[i].value = soundData[i].Volume;
		}
		buttonColor.onClick.AddListener(OnClickColor);
		toggleShield.onValueChanged.AddListener(delegate(bool v)
		{
			Manager.Config.GraphicData.Shield = v;
			Singleton<Studio>.Instance.cameraCtrl.isConfigVanish = v;
		});
		for (int num = 0; num < 3; num++)
		{
			byte limit = (byte)num;
			togglesTexture[num].onValueChanged.AddListener(delegate(bool _b)
			{
				if (_b)
				{
					Manager.Config.GraphicData.GraphicQuality = limit;
					if (QualitySettings.masterTextureLimit != limit)
					{
						QualitySettings.masterTextureLimit = limit;
					}
				}
			});
		}
		for (int num2 = 0; num2 < 6; num2++)
		{
			int no = num2;
			toggleSound[num2].onValueChanged.AddListener(delegate(bool v)
			{
				OnValueChangedMute(v, no);
			});
			sliderSound[num2].onValueChanged.AddListener(delegate(float v)
			{
				OnValueChangedVolume(v, no);
			});
		}
	}
}
