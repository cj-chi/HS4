using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsH_Hair : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscHairType;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csBaseColor;

	[SerializeField]
	private CustomColorSet csTopColor;

	[SerializeField]
	private CustomColorSet csUnderColor;

	[SerializeField]
	private CustomColorSet csSpecular;

	[SerializeField]
	private CustomSliderSet ssMetallic;

	[SerializeField]
	private CustomSliderSet ssSmoothness;

	[SerializeField]
	private CustomHairColorPreset hcPreset;

	[SerializeField]
	private Button btnSameSkinColor;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomColorSet[] csAcsColor;

	[Header("【設定04】----------------------")]
	[SerializeField]
	private Transform trfContent;

	[SerializeField]
	private GameObject tmpBundleObj;

	private List<CustomHairBundleSet> lstHairBundleSet = new List<CustomHairBundleSet>();

	[SerializeField]
	private Button btnCorrectAllReset;

	[Header("【設定05】----------------------")]
	[SerializeField]
	private CustomClothesPatternSelect hairMeshPtnSel;

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
	private Toggle tglGuidDraw;

	[SerializeField]
	private Toggle[] tglGuidType;

	[SerializeField]
	private Slider sldGuidSpeed;

	[SerializeField]
	private Slider sldGuidScale;

	private int backSNo = -1;

	private List<IDisposable> lstDisposable = new List<IDisposable>();

	public bool allReset { get; set; }

	private bool sameSetting => base.hair.sameSetting;

	private bool autoSetting => base.hair.autoSetting;

	private bool ctrlTogether => base.hair.ctrlTogether;

	private CustomBase.CustomSettingSave.HairCtrlSetting hairCtrlSetting => base.customBase.customSettingSave.hairCtrlSetting;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	public void UpdateHairList()
	{
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList((new ChaListDefine.CategoryNo[4]
		{
			ChaListDefine.CategoryNo.so_hair_b,
			ChaListDefine.CategoryNo.so_hair_f,
			ChaListDefine.CategoryNo.so_hair_s,
			ChaListDefine.CategoryNo.so_hair_o
		})[base.SNo]);
		sscHairType.CreateList(lst);
	}

	private void CalculateUI()
	{
		ssMetallic.SetSliderValue(base.hair.parts[base.SNo].metallic);
		ssSmoothness.SetSliderValue(base.hair.parts[base.SNo].smoothness);
		sldGuidSpeed.value = hairCtrlSetting.controllerSpeed;
		sldGuidScale.value = hairCtrlSetting.controllerScale;
	}

	public override void UpdateCustomUI()
	{
		if (backSNo != base.SNo)
		{
			UpdateHairList();
			backSNo = base.SNo;
		}
		base.UpdateCustomUI();
		CalculateUI();
		tglGuidDraw.SetIsOnWithoutCallback(hairCtrlSetting.drawController);
		tglGuidType[hairCtrlSetting.controllerType].SetIsOnWithoutCallback(isOn: true);
		tglGuidType[hairCtrlSetting.controllerType & 1].SetIsOnWithoutCallback(isOn: false);
		sscHairType.SetToggleID(base.hair.parts[base.SNo].id);
		csBaseColor.SetColor(base.hair.parts[base.SNo].baseColor);
		csTopColor.SetColor(base.hair.parts[base.SNo].topColor);
		csUnderColor.SetColor(base.hair.parts[base.SNo].underColor);
		csSpecular.SetColor(base.hair.parts[base.SNo].specular);
		InitializeHairMesh();
		SetDrawSettingByHair();
	}

	public void ChangePatternImage()
	{
		ListInfoBase listInfo = base.chaCtrl.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.st_hairmeshptn, base.hair.parts[base.SNo].meshType);
		Texture2D texture2D = CommonLib.LoadAsset<Texture2D>(listInfo.GetInfo(ChaListDefine.KeyType.ThumbAB), listInfo.GetInfo(ChaListDefine.KeyType.ThumbTex));
		if ((bool)texture2D)
		{
			imgPattern.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
	}

	public void UpdateDrawControllerState()
	{
		int controllerType = hairCtrlSetting.controllerType;
		bool drawController = hairCtrlSetting.drawController;
		float controllerSpeed = hairCtrlSetting.controllerSpeed;
		float controllerScale = hairCtrlSetting.controllerScale;
		tglGuidDraw.SetIsOnWithoutCallback(drawController);
		tglGuidType[controllerType].SetIsOnWithoutCallback(isOn: true);
		sldGuidSpeed.value = controllerSpeed;
		sldGuidScale.value = controllerScale;
	}

	public void SetDrawSettingByHair()
	{
		if (base.chaCtrl.cmpHair == null)
		{
			return;
		}
		lstHairBundleSet.Clear();
		for (int num = trfContent.childCount - 1; num >= 0; num--)
		{
			Transform child = trfContent.GetChild(num);
			if (!(child.name == "CtrlSetting") && !(child.name == "control"))
			{
				child.SetParent(null);
				child.name = "delete_reserve";
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
		if (null != base.customBase.objHairControllerTop)
		{
			for (int num2 = base.customBase.objHairControllerTop.transform.childCount - 1; num2 >= 0; num2--)
			{
				Transform child2 = base.customBase.objHairControllerTop.transform.GetChild(num2);
				child2.SetParent(null);
				child2.name = "delete_reserve";
				UnityEngine.Object.Destroy(child2.gameObject);
			}
		}
		base.customBase.customCtrl.camCtrl.ClearListCollider();
		if (null == base.chaCtrl.cmpHair[base.SNo])
		{
			ShowOrHideTab(false, 1, 2, 3, 4);
			return;
		}
		ShowOrHideTab(true, 1);
		bool[] array = new bool[3]
		{
			base.chaCtrl.cmpHair[base.SNo].useAcsColor01,
			base.chaCtrl.cmpHair[base.SNo].useAcsColor02,
			base.chaCtrl.cmpHair[base.SNo].useAcsColor03
		};
		bool show = array[0] | array[1] | array[2];
		ShowOrHideTab(show, 2);
		base.customBase.drawTopHairColor = base.chaCtrl.cmpHair[base.SNo].useTopColor;
		base.customBase.drawUnderHairColor = base.chaCtrl.cmpHair[base.SNo].useUnderColor;
		btnSameSkinColor.transform.parent.gameObject.SetActiveIfDifferent(base.chaCtrl.cmpHair[base.SNo].useSameSkinColorButton);
		for (int i = 0; i < csAcsColor.Length; i++)
		{
			csAcsColor[i].gameObject.SetActiveIfDifferent(array[i]);
			csAcsColor[i].SetColor(base.hair.parts[base.SNo].acsColorInfo[i].color);
		}
		int num3 = 1;
		for (int j = 0; j < base.hair.parts[base.SNo].dictBundle.Count; j++)
		{
			if (!(null == base.chaCtrl.cmpHair[base.SNo].boneInfo[j].trfCorrect))
			{
				GameObject obj = UnityEngine.Object.Instantiate(tmpBundleObj);
				obj.transform.SetParent(trfContent, worldPositionStays: false);
				CustomHairBundleSet component = obj.GetComponent<CustomHairBundleSet>();
				component.CreateGuid(base.customBase.objHairControllerTop, base.chaCtrl.cmpHair[base.SNo].boneInfo[j]);
				component.Initialize(base.SNo, j, num3);
				lstHairBundleSet.Add(component);
				obj.SetActiveIfDifferent(active: true);
				num3++;
			}
		}
		show = lstHairBundleSet.Count != 0;
		ShowOrHideTab(show, 3);
		show = base.chaCtrl.cmpHair[base.SNo].useMesh;
		ShowOrHideTab(show, 4);
	}

	public void UpdateAllBundleUI(int excludeIdx = -1)
	{
		allReset = true;
		int count = lstHairBundleSet.Count;
		for (int i = 0; i < count; i++)
		{
			if (i != excludeIdx)
			{
				lstHairBundleSet[i].UpdateCustomUI();
			}
		}
		allReset = false;
	}

	public void UpdateGuidType()
	{
		if (lstHairBundleSet == null)
		{
			return;
		}
		int count = lstHairBundleSet.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(null == lstHairBundleSet[i].cmpGuid))
			{
				lstHairBundleSet[i].cmpGuid.SetMode(hairCtrlSetting.controllerType);
			}
		}
	}

	public void UpdateGuidSpeed()
	{
		if (lstHairBundleSet == null)
		{
			return;
		}
		int count = lstHairBundleSet.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(null == lstHairBundleSet[i].cmpGuid))
			{
				lstHairBundleSet[i].cmpGuid.speedMove = hairCtrlSetting.controllerSpeed;
			}
		}
	}

	public void UpdateGuidScale()
	{
		if (lstHairBundleSet == null)
		{
			return;
		}
		int count = lstHairBundleSet.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(null == lstHairBundleSet[i].cmpGuid))
			{
				lstHairBundleSet[i].cmpGuid.scaleAxis = hairCtrlSetting.controllerScale;
				lstHairBundleSet[i].cmpGuid.UpdateScale();
			}
		}
	}

	public bool IsDrag()
	{
		if (lstHairBundleSet == null)
		{
			return false;
		}
		int count = lstHairBundleSet.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(null == lstHairBundleSet[i].cmpGuid) && lstHairBundleSet[i].isDrag)
			{
				return true;
			}
		}
		return false;
	}

	public void ShortcutChangeGuidType(int type)
	{
		if (!IsDrag())
		{
			tglGuidType[type].isOn = true;
		}
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssMetallic.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].metallic));
		ssSmoothness.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].smoothness));
	}

	public void InitializeHairMesh()
	{
		if (lstDisposable != null && lstDisposable.Count != 0)
		{
			int count = lstDisposable.Count;
			for (int i = 0; i < count; i++)
			{
				lstDisposable[i].Dispose();
			}
		}
		IDisposable disposable = null;
		disposable = btnPatternWin.OnClickAsObservable().Subscribe(delegate
		{
			base.customBase.customCtrl.showPattern = true;
			hairMeshPtnSel.ChangeLink(1, base.SNo);
			hairMeshPtnSel.onSelect = delegate
			{
				ChangePatternImage();
				if ((bool)objPatternSet)
				{
					objPatternSet.SetActiveIfDifferent(base.hair.parts[base.SNo].meshType != 0);
				}
			};
		});
		lstDisposable.Add(disposable);
		csPatternColor.actUpdateColor = delegate(Color color)
		{
			base.hair.parts[base.SNo].meshColor = color;
			base.chaCtrl.ChangeSettingHairMeshColor(base.SNo);
		};
		ssPatternW.onChange = delegate(float value)
		{
			base.hair.parts[base.SNo].meshLayout = new Vector4(value, base.hair.parts[base.SNo].meshLayout.y, base.hair.parts[base.SNo].meshLayout.z, base.hair.parts[base.SNo].meshLayout.w);
			base.chaCtrl.ChangeSettingHairMeshLayout(base.SNo);
		};
		ssPatternW.onSetDefaultValue = () => 1f;
		ssPatternH.onChange = delegate(float value)
		{
			base.hair.parts[base.SNo].meshLayout = new Vector4(base.hair.parts[base.SNo].meshLayout.x, value, base.hair.parts[base.SNo].meshLayout.z, base.hair.parts[base.SNo].meshLayout.w);
			base.chaCtrl.ChangeSettingHairMeshLayout(base.SNo);
		};
		ssPatternH.onSetDefaultValue = () => 1f;
		ssPatternX.onChange = delegate(float value)
		{
			base.hair.parts[base.SNo].meshLayout = new Vector4(base.hair.parts[base.SNo].meshLayout.x, base.hair.parts[base.SNo].meshLayout.y, value, base.hair.parts[base.SNo].meshLayout.w);
			base.chaCtrl.ChangeSettingHairMeshLayout(base.SNo);
		};
		ssPatternX.onSetDefaultValue = () => 0f;
		ssPatternY.onChange = delegate(float value)
		{
			base.hair.parts[base.SNo].meshLayout = new Vector4(base.hair.parts[base.SNo].meshLayout.x, base.hair.parts[base.SNo].meshLayout.y, base.hair.parts[base.SNo].meshLayout.z, value);
			base.chaCtrl.ChangeSettingHairMeshLayout(base.SNo);
		};
		ssPatternY.onSetDefaultValue = () => 0f;
		ChangePatternImage();
		if ((bool)objPatternSet)
		{
			objPatternSet.SetActiveIfDifferent(base.hair.parts[base.SNo].meshType != 0);
		}
		base.hair.parts[base.SNo].meshColor = new Color(base.hair.parts[base.SNo].meshColor.r, base.hair.parts[base.SNo].meshColor.g, base.hair.parts[base.SNo].meshColor.b, 1f);
		csPatternColor.SetColor(base.hair.parts[base.SNo].meshColor);
		ssPatternW.SetSliderValue(base.hair.parts[base.SNo].meshLayout.x);
		ssPatternH.SetSliderValue(base.hair.parts[base.SNo].meshLayout.y);
		ssPatternX.SetSliderValue(base.hair.parts[base.SNo].meshLayout.z);
		ssPatternY.SetSliderValue(base.hair.parts[base.SNo].meshLayout.w);
		ssPatternW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].meshLayout.x));
		ssPatternH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].meshLayout.y));
		ssPatternX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].meshLayout.z));
		ssPatternY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.hair.parts[base.SNo].meshLayout.w));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsHair += UpdateCustomUI;
		UpdateHairList();
		sscHairType.SetToggleID(base.hair.parts[base.SNo].id);
		sscHairType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.hair.parts[base.SNo].id != info.id)
			{
				base.chaCtrl.ChangeHair(base.SNo, info.id);
				base.chaCtrl.SetHairAcsDefaultColorParameterOnly(base.SNo);
				base.chaCtrl.ChangeSettingHairAcsColor(base.SNo);
				SetDrawSettingByHair();
			}
		};
		csBaseColor.actUpdateColor = delegate(Color color)
		{
			if (autoSetting)
			{
				base.chaCtrl.CreateHairColor(color, out var topColor, out var underColor, out var specular);
				for (int i = 0; i < base.hair.parts.Length; i++)
				{
					if (sameSetting || i == base.SNo)
					{
						base.hair.parts[i].baseColor = color;
						base.hair.parts[i].topColor = topColor;
						base.hair.parts[i].underColor = underColor;
						base.hair.parts[i].specular = specular;
						base.chaCtrl.ChangeSettingHairColor(i, _main: true, autoSetting, autoSetting);
						base.chaCtrl.ChangeSettingHairSpecular(i);
						csTopColor.SetColor(base.hair.parts[base.SNo].topColor);
						csUnderColor.SetColor(base.hair.parts[base.SNo].underColor);
						csSpecular.SetColor(base.hair.parts[base.SNo].specular);
					}
				}
			}
			else
			{
				for (int j = 0; j < base.hair.parts.Length; j++)
				{
					if (sameSetting || j == base.SNo)
					{
						base.hair.parts[j].baseColor = color;
						base.chaCtrl.ChangeSettingHairColor(j, _main: true, autoSetting, autoSetting);
					}
				}
			}
		};
		csTopColor.actUpdateColor = delegate(Color color)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].topColor = color;
					base.chaCtrl.ChangeSettingHairColor(i, _main: false, _top: true, _under: false);
				}
			}
		};
		csUnderColor.actUpdateColor = delegate(Color color)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].underColor = color;
					base.chaCtrl.ChangeSettingHairColor(i, _main: false, _top: false, _under: true);
				}
			}
		};
		csSpecular.actUpdateColor = delegate(Color color)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].specular = color;
					base.chaCtrl.ChangeSettingHairSpecular(i);
				}
			}
		};
		ssMetallic.onChange = delegate(float value)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].metallic = value;
					base.chaCtrl.ChangeSettingHairMetallic(i);
				}
			}
		};
		ssMetallic.onSetDefaultValue = () => base.defChaCtrl.custom.hair.parts[base.SNo].metallic;
		ssSmoothness.onChange = delegate(float value)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].smoothness = value;
					base.chaCtrl.ChangeSettingHairSmoothness(i);
				}
			}
		};
		ssSmoothness.onSetDefaultValue = () => base.defChaCtrl.custom.hair.parts[base.SNo].smoothness;
		hcPreset.onClick = delegate(CustomHairColorPreset.HairColorInfo preset)
		{
			for (int i = 0; i < base.hair.parts.Length; i++)
			{
				if (sameSetting || i == base.SNo)
				{
					base.hair.parts[i].baseColor = preset.baseColor;
					base.hair.parts[i].topColor = preset.topColor;
					base.hair.parts[i].underColor = preset.underColor;
					base.hair.parts[i].specular = preset.specular;
					base.hair.parts[i].metallic = preset.metallic;
					base.hair.parts[i].smoothness = preset.smoothness;
					base.chaCtrl.ChangeSettingHairColor(i, _main: true, _top: true, _under: true);
					base.chaCtrl.ChangeSettingHairSpecular(i);
					base.chaCtrl.ChangeSettingHairMetallic(i);
					base.chaCtrl.ChangeSettingHairSmoothness(i);
					csBaseColor.SetColor(base.hair.parts[i].baseColor);
					csTopColor.SetColor(base.hair.parts[i].topColor);
					csUnderColor.SetColor(base.hair.parts[i].underColor);
					csSpecular.SetColor(base.hair.parts[i].specular);
					ssMetallic.SetSliderValue(base.hair.parts[i].metallic);
					ssSmoothness.SetSliderValue(base.hair.parts[i].smoothness);
				}
			}
		};
		btnSameSkinColor.OnClickAsObservable().Subscribe(delegate
		{
			Color.RGBToHSV(base.body.skinColor, out var H, out var S, out var V);
			base.hair.parts[base.SNo].underColor = Color.HSVToRGB(H, Mathf.Max(0f, S - 0.06f), Mathf.Max(0f, V - 0.06f));
			base.hair.parts[base.SNo].smoothness = Mathf.Max(0f, base.body.skinGlossPower - 0.3f);
			base.chaCtrl.ChangeSettingHairColor(base.SNo, _main: false, _top: false, _under: true);
			base.chaCtrl.ChangeSettingHairSmoothness(base.SNo);
			csUnderColor.SetColor(base.hair.parts[base.SNo].underColor);
			ssSmoothness.SetSliderValue(base.hair.parts[base.SNo].smoothness);
		});
		if (csAcsColor != null && csAcsColor.Any())
		{
			csAcsColor.ToList().ForEach(delegate(CustomColorSet item)
			{
				item.actUpdateColor = delegate(Color color)
				{
					base.hair.parts[base.SNo].acsColorInfo[0].color = color;
					base.chaCtrl.ChangeSettingHairAcsColor(base.SNo);
				};
			});
		}
		if ((bool)btnCorrectAllReset)
		{
			btnCorrectAllReset.OnClickAsObservable().Subscribe(delegate
			{
				base.chaCtrl.SetDefaultHairCorrectPosRateAll(base.SNo);
				base.chaCtrl.SetDefaultHairCorrectRotRateAll(base.SNo);
				UpdateAllBundleUI();
			});
		}
		tglGuidDraw.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			hairCtrlSetting.drawController = isOn;
		});
		if (tglGuidType.Any())
		{
			(from item in tglGuidType.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				(from isOn in item.val.onValueChanged.AsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					hairCtrlSetting.controllerType = item.idx;
					UpdateGuidType();
				});
			});
		}
		sldGuidSpeed.OnValueChangedAsObservable().Subscribe(delegate(float val)
		{
			hairCtrlSetting.controllerSpeed = val;
			UpdateGuidSpeed();
		});
		sldGuidSpeed.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (base.customBase.sliderControlWheel)
			{
				sldGuidSpeed.value = Mathf.Clamp(sldGuidSpeed.value + scl.scrollDelta.y * -0.01f, 0.1f, 1f);
			}
		});
		sldGuidScale.OnValueChangedAsObservable().Subscribe(delegate(float val)
		{
			hairCtrlSetting.controllerScale = val;
			UpdateGuidScale();
		});
		sldGuidScale.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (base.customBase.sliderControlWheel)
			{
				sldGuidScale.value = Mathf.Clamp(sldGuidScale.value + scl.scrollDelta.y * -0.01f, 0.3f, 3f);
			}
		});
		UpdateDrawControllerState();
		StartCoroutine(SetInputText());
		backSNo = base.SNo;
	}
}
