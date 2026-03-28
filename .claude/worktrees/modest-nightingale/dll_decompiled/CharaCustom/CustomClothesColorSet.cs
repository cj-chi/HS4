using System;
using System.Collections.Generic;
using AIChara;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomClothesColorSet : MonoBehaviour
{
	public class ClothesInfo
	{
		public float gloss;

		public float metallic;

		public Vector4 layout = Vector4.zero;

		public float rot;
	}

	[SerializeField]
	private Text title;

	[SerializeField]
	private CustomColorSet csMainColor;

	[SerializeField]
	private CustomSliderSet ssGloss;

	[SerializeField]
	private CustomSliderSet ssMetallic;

	[SerializeField]
	private CustomClothesPatternSelect clothesPtnSel;

	[SerializeField]
	private GameObject objPatternSet;

	[SerializeField]
	private Button btnPatternWin;

	[SerializeField]
	private Image imgPattern;

	[SerializeField]
	private CustomColorSet csPatternColor;

	[SerializeField]
	private CustomSliderSet ssPatternW;

	[SerializeField]
	private CustomSliderSet ssPatternH;

	[SerializeField]
	private CustomSliderSet ssPatternX;

	[SerializeField]
	private CustomSliderSet ssPatternY;

	[SerializeField]
	private CustomSliderSet ssPatternRot;

	private List<IDisposable> lstDisposable = new List<IDisposable>();

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl => customBase.chaCtrl;

	private ChaFileClothes nowClothes => chaCtrl.nowCoordinate.clothes;

	private ChaFileClothes orgClothes => chaCtrl.chaFile.coordinate.clothes;

	public int parts { get; set; } = -1;

	public int idx { get; set; } = -1;

	private ChaFileClothes.PartsInfo.ColorInfo nowColorInfo => nowClothes.parts[parts].colorInfo[idx];

	private ChaFileClothes.PartsInfo.ColorInfo orgColorInfo => orgClothes.parts[parts].colorInfo[idx];

	public void UpdateCustomUI()
	{
		if (-1 != parts && -1 != idx)
		{
			ChaFileClothes.PartsInfo.ColorInfo colorInfo = nowClothes.parts[parts].colorInfo[idx];
			csMainColor.SetColor(colorInfo.baseColor);
			ssGloss.SetSliderValue(colorInfo.glossPower);
			ssMetallic.SetSliderValue(colorInfo.metallicPower);
			ChangePatternImage();
			if ((bool)objPatternSet)
			{
				objPatternSet.SetActiveIfDifferent(colorInfo.pattern != 0);
			}
			csPatternColor.SetColor(colorInfo.patternColor);
			ssPatternW.SetSliderValue(colorInfo.layout.x);
			ssPatternH.SetSliderValue(colorInfo.layout.y);
			ssPatternX.SetSliderValue(colorInfo.layout.z);
			ssPatternY.SetSliderValue(colorInfo.layout.w);
			ssPatternRot.SetSliderValue(colorInfo.rotation);
		}
	}

	public void EnableColorAlpha(bool enable)
	{
		if ((bool)csMainColor)
		{
			csMainColor.EnableColorAlpha(enable);
		}
	}

	public void ChangePatternImage()
	{
		ListInfoBase listInfo = chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.st_pattern, nowClothes.parts[parts].colorInfo[idx].pattern);
		Texture2D texture2D = CommonLib.LoadAsset<Texture2D>(listInfo.GetInfo(ChaListDefine.KeyType.ThumbAB), listInfo.GetInfo(ChaListDefine.KeyType.ThumbTex));
		if ((bool)texture2D)
		{
			imgPattern.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
	}

	public ClothesInfo GetDefaultClothesInfo()
	{
		float[] array = new float[3];
		float[] array2 = new float[3];
		Vector4[] array3 = new Vector4[3]
		{
			Vector4.zero,
			Vector4.zero,
			Vector4.zero
		};
		float[] array4 = new float[3];
		if (null != chaCtrl.cmpClothes[parts])
		{
			array[0] = chaCtrl.cmpClothes[parts].defGloss01;
			array[1] = chaCtrl.cmpClothes[parts].defGloss02;
			array[2] = chaCtrl.cmpClothes[parts].defGloss03;
			array2[0] = chaCtrl.cmpClothes[parts].defMetallic01;
			array2[1] = chaCtrl.cmpClothes[parts].defMetallic02;
			array2[2] = chaCtrl.cmpClothes[parts].defMetallic03;
			Vector4 vector = default(Vector4);
			vector.x = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout01.x);
			vector.y = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout01.y);
			vector.z = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout01.z);
			vector.w = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout01.w);
			array3[0] = vector;
			vector.x = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout02.x);
			vector.y = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout02.y);
			vector.z = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout02.z);
			vector.w = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout02.w);
			array3[1] = vector;
			vector.x = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout03.x);
			vector.y = Mathf.InverseLerp(20f, 1f, chaCtrl.cmpClothes[parts].defLayout03.y);
			vector.z = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout03.z);
			vector.w = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defLayout03.w);
			array3[2] = vector;
			array4[0] = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defRotation01);
			array4[1] = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defRotation02);
			array4[2] = Mathf.InverseLerp(-1f, 1f, chaCtrl.cmpClothes[parts].defRotation03);
		}
		return new ClothesInfo
		{
			gloss = array[idx],
			metallic = array2[idx],
			layout = array3[idx],
			rot = array4[idx]
		};
	}

	public void Initialize(int _parts, int _idx)
	{
		parts = _parts;
		idx = _idx;
		if (-1 == parts || -1 == idx)
		{
			return;
		}
		if ((bool)title)
		{
			title.text = CharaCustomDefine.CustomColorTitle[Singleton<GameSystem>.Instance.languageInt] + (idx + 1).ToString("00");
		}
		if (lstDisposable != null && lstDisposable.Count != 0)
		{
			int count = lstDisposable.Count;
			for (int i = 0; i < count; i++)
			{
				lstDisposable[i].Dispose();
			}
		}
		IDisposable disposable = null;
		csMainColor.actUpdateColor = delegate(Color color)
		{
			nowColorInfo.baseColor = color;
			orgColorInfo.baseColor = color;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssGloss.onChange = delegate(float value)
		{
			nowColorInfo.glossPower = value;
			orgColorInfo.glossPower = value;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssGloss.onSetDefaultValue = () => GetDefaultClothesInfo().gloss;
		ssMetallic.onChange = delegate(float value)
		{
			nowColorInfo.metallicPower = value;
			orgColorInfo.metallicPower = value;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssMetallic.onSetDefaultValue = () => GetDefaultClothesInfo().metallic;
		disposable = btnPatternWin.OnClickAsObservable().Subscribe(delegate
		{
			customBase.customCtrl.showPattern = true;
			clothesPtnSel.ChangeLink(0, parts, idx);
			clothesPtnSel.onSelect = delegate
			{
				ChangePatternImage();
				if ((bool)objPatternSet)
				{
					objPatternSet.SetActiveIfDifferent(nowColorInfo.pattern != 0);
				}
			};
		});
		lstDisposable.Add(disposable);
		csPatternColor.actUpdateColor = delegate(Color color)
		{
			nowColorInfo.patternColor = color;
			orgColorInfo.patternColor = color;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternW.onChange = delegate(float value)
		{
			nowColorInfo.layout = new Vector4(value, nowColorInfo.layout.y, nowColorInfo.layout.z, nowColorInfo.layout.w);
			orgColorInfo.layout = nowColorInfo.layout;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternW.onSetDefaultValue = () => GetDefaultClothesInfo().layout.x;
		ssPatternH.onChange = delegate(float value)
		{
			nowColorInfo.layout = new Vector4(nowColorInfo.layout.x, value, nowColorInfo.layout.z, nowColorInfo.layout.w);
			orgColorInfo.layout = nowColorInfo.layout;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternH.onSetDefaultValue = () => GetDefaultClothesInfo().layout.y;
		ssPatternX.onChange = delegate(float value)
		{
			nowColorInfo.layout = new Vector4(nowColorInfo.layout.x, nowColorInfo.layout.y, value, nowColorInfo.layout.w);
			orgColorInfo.layout = nowColorInfo.layout;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternX.onSetDefaultValue = () => GetDefaultClothesInfo().layout.z;
		ssPatternY.onChange = delegate(float value)
		{
			nowColorInfo.layout = new Vector4(nowColorInfo.layout.x, nowColorInfo.layout.y, nowColorInfo.layout.z, value);
			orgColorInfo.layout = nowColorInfo.layout;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternY.onSetDefaultValue = () => GetDefaultClothesInfo().layout.w;
		ssPatternRot.onChange = delegate(float value)
		{
			nowColorInfo.rotation = value;
			orgColorInfo.rotation = value;
			chaCtrl.ChangeCustomClothes(parts, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false);
		};
		ssPatternRot.onSetDefaultValue = () => GetDefaultClothesInfo().rot;
		UpdateCustomUI();
		ssGloss.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.glossPower));
		ssMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.metallicPower));
		ssPatternW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.layout.x));
		ssPatternH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.layout.y));
		ssPatternX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.layout.z));
		ssPatternY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.layout.w));
		ssPatternRot.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, nowColorInfo.rotation));
	}
}
