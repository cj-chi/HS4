using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class GraphicSetting : BaseSetting
{
	[Header("描画レベルスライダー")]
	[SerializeField]
	private Slider qualitySlider;

	[Header("描画レベルスライダー数字")]
	[SerializeField]
	private Text[] qualityNumberText;

	[Header("数字の色")]
	[SerializeField]
	private Color[] qualitySelectColor;

	[Header("セルフシャドウ")]
	[SerializeField]
	private Toggle selfShadowToggle;

	[Header("被写界深度")]
	[SerializeField]
	private Toggle depthOfFieldToggle;

	[Header("ブルーム")]
	[SerializeField]
	private Toggle bloomToggle;

	[Header("大気表現")]
	[SerializeField]
	private Toggle atmosphereToggle;

	[Header("SSAO")]
	[SerializeField]
	private Toggle ssaoToggle;

	[Header("ビグネット")]
	[SerializeField]
	private Toggle vignetteToggle;

	[Header("SSR")]
	[SerializeField]
	private Toggle ssrToggle;

	[Header("リフレクションプローブ")]
	[SerializeField]
	private Toggle rpToggle;

	[Header("サンシャフト")]
	[SerializeField]
	private Toggle sunShaftToggle;

	[Header("雨の描画")]
	[SerializeField]
	private Toggle rainToggle;

	[Header("解像度")]
	[SerializeField]
	private Toggle[] levalToggles;

	[Header("環境ライト")]
	[SerializeField]
	private Toggle[] ambientToggles;

	[Header("マップ")]
	[SerializeField]
	private Toggle[] mapToggles;

	[Header("遮蔽")]
	[SerializeField]
	private Toggle[] shieldToggles;

	[Header("背景色")]
	[SerializeField]
	private UI_SampleColor backGroundCololr;

	private bool ChangeSlider = true;

	private bool[][] effectEnables = new bool[4][]
	{
		new bool[10] { true, false, false, false, false, false, false, false, false, false },
		new bool[10] { true, false, true, true, true, true, false, false, false, false },
		new bool[10] { true, true, true, true, true, true, true, true, true, false },
		new bool[10] { true, true, true, true, true, true, true, true, true, true }
	};

	private Dictionary<int, List<bool>> easySettingInfo = new Dictionary<int, List<bool>>();

	public override void Init()
	{
		for (int i = 0; i < 4; i++)
		{
			easySettingInfo.Add(i + 1, effectEnables[i].ToList());
		}
		GraphicSystem data = Manager.Config.GraphicData;
		LinkToggle(selfShadowToggle, delegate(bool isOn)
		{
			QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel() / 2 * 2 + ((!isOn) ? 1 : 0));
			data.SelfShadow = isOn;
			SetEasySlider();
		});
		LinkToggle(depthOfFieldToggle, delegate(bool isOn)
		{
			data.DepthOfField = isOn;
			SetEasySlider();
		});
		LinkToggle(bloomToggle, delegate(bool isOn)
		{
			data.Bloom = isOn;
			SetEasySlider();
		});
		LinkToggle(atmosphereToggle, delegate(bool isOn)
		{
			data.Fog = isOn;
			SetEasySlider();
		});
		LinkToggle(ssaoToggle, delegate(bool isOn)
		{
			data.SSAO = isOn;
			SetEasySlider();
		});
		LinkToggle(vignetteToggle, delegate(bool isOn)
		{
			data.Vignette = isOn;
			SetEasySlider();
		});
		LinkToggle(ssrToggle, delegate(bool isOn)
		{
			data.SSR = isOn;
			SetEasySlider();
		});
		LinkToggle(rpToggle, delegate(bool isOn)
		{
			data.RP = isOn;
			SetEasySlider();
		});
		LinkToggle(sunShaftToggle, delegate(bool isOn)
		{
			data.SunShaft = isOn;
			SetEasySlider();
		});
		LinkToggle(rainToggle, delegate(bool isOn)
		{
			data.Rain = isOn;
			SetEasySlider();
		});
		qualitySlider.onValueChanged.AddListener(delegate(float value)
		{
			SetNumberColor((int)value);
			if (!ChangeSlider)
			{
				ChangeSlider = true;
			}
			else
			{
				switch ((int)value)
				{
				case 1:
					data.SelfShadow = true;
					data.DepthOfField = false;
					data.Bloom = false;
					data.Fog = false;
					data.SSAO = false;
					data.Vignette = false;
					data.SSR = false;
					data.RP = false;
					data.SunShaft = false;
					data.Rain = false;
					break;
				case 2:
					data.SelfShadow = true;
					data.DepthOfField = false;
					data.Bloom = true;
					data.Fog = true;
					data.SSAO = true;
					data.Vignette = true;
					data.SSR = false;
					data.RP = false;
					data.SunShaft = false;
					data.Rain = false;
					break;
				case 3:
					data.SelfShadow = true;
					data.DepthOfField = true;
					data.Bloom = true;
					data.Fog = true;
					data.SSAO = true;
					data.Vignette = true;
					data.SSR = true;
					data.RP = true;
					data.SunShaft = true;
					data.Rain = false;
					break;
				case 4:
					data.SelfShadow = true;
					data.DepthOfField = true;
					data.Bloom = true;
					data.Fog = true;
					data.SSAO = true;
					data.Vignette = true;
					data.SSR = true;
					data.RP = true;
					data.SunShaft = true;
					data.Rain = true;
					break;
				}
				UIPresenter();
			}
		});
		LinkToggleArray(levalToggles, delegate(int num)
		{
			data.GraphicQuality = (byte)num;
		});
		LinkToggleArray(ambientToggles, delegate(int num)
		{
			data.AmbientLight = num == 0;
		});
		LinkToggleArray(shieldToggles, delegate(int num)
		{
			data.Shield = num == 0;
		});
		LinkToggleArray(mapToggles, delegate(int num)
		{
			data.Map = num == 0;
		});
		backGroundCololr.actUpdateColor = delegate(Color c)
		{
			data.BackColor = c;
		};
		ChangeSlider = true;
	}

	protected override void ValueToUI()
	{
		GraphicSystem data = Manager.Config.GraphicData;
		selfShadowToggle.isOn = data.SelfShadow;
		depthOfFieldToggle.isOn = data.DepthOfField;
		bloomToggle.isOn = data.Bloom;
		atmosphereToggle.isOn = data.Fog;
		ssaoToggle.isOn = data.SSAO;
		vignetteToggle.isOn = data.Vignette;
		ssrToggle.isOn = data.SSR;
		rpToggle.isOn = data.RP;
		sunShaftToggle.isOn = data.SunShaft;
		rainToggle.isOn = data.Rain;
		SetEasySlider();
		SetToggleUIArray(levalToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = index == data.GraphicQuality;
		});
		SetToggleUIArray(ambientToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? data.AmbientLight : (!data.AmbientLight));
		});
		SetToggleUIArray(mapToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? data.Map : (!data.Map));
		});
		SetToggleUIArray(shieldToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? data.Shield : (!data.Shield));
		});
		backGroundCololr.SetColor(data.BackColor);
	}

	private void SetEasySlider()
	{
		GraphicSystem graphicData = Manager.Config.GraphicData;
		bool[] array = new bool[10] { graphicData.SelfShadow, graphicData.DepthOfField, graphicData.Bloom, graphicData.Fog, graphicData.SSAO, graphicData.Vignette, graphicData.SSR, graphicData.RP, graphicData.SunShaft, graphicData.Rain };
		List<bool> list = new List<bool>();
		int num = array.Length;
		foreach (KeyValuePair<int, List<bool>> item in easySettingInfo)
		{
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				if (item.Value[i] && array[i] == item.Value[i])
				{
					num2++;
				}
			}
			list.Add(num2 == item.Value.Count((bool b) => b));
		}
		int num3 = list.FindLastIndex((bool v) => v) + 1;
		if (qualitySlider.value != (float)num3 && num3 != 0)
		{
			ChangeSlider = false;
			qualitySlider.value = num3;
		}
	}

	private void SetNumberColor(int _value)
	{
		for (int i = 0; i < qualityNumberText.Length; i++)
		{
			qualityNumberText[i].color = ((_value == i + 1) ? qualitySelectColor[0] : qualitySelectColor[1]);
		}
	}
}
