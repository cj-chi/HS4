using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illusion.Extensions;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class MPItemCtrl : MonoBehaviour
{
	[Serializable]
	private class ColorCombination
	{
		public GameObject objRoot;

		public Image imageColor;

		public Button buttonColor;

		public Button buttonColorDefault;

		public bool interactable
		{
			set
			{
				buttonColor.interactable = value;
				if ((bool)buttonColorDefault)
				{
					buttonColorDefault.interactable = value;
				}
			}
		}

		public Color color
		{
			get
			{
				return imageColor.color;
			}
			set
			{
				imageColor.color = value;
			}
		}

		public bool active
		{
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class InputCombination
	{
		public GameObject objRoot;

		public TMP_InputField input;

		public Slider slider;

		public Button buttonDefault;

		public bool interactable
		{
			set
			{
				input.interactable = value;
				slider.interactable = value;
				if ((bool)buttonDefault)
				{
					buttonDefault.interactable = value;
				}
			}
		}

		public string text
		{
			get
			{
				return input.text;
			}
			set
			{
				input.text = value;
				slider.value = Utility.StringToFloat(value);
			}
		}

		public float value
		{
			get
			{
				return slider.value;
			}
			set
			{
				slider.value = value;
				input.text = value.ToString("0.0");
			}
		}

		public bool active
		{
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class ColorInfo
	{
		public GameObject objRoot;

		public ColorCombination _colorMain = new ColorCombination();

		[Header("メタリック関係")]
		public GameObject objMetallic;

		public InputCombination inputMetallic = new InputCombination();

		public InputCombination inputGlossiness = new InputCombination();

		[Header("柄関係")]
		public GameObject objPattern;

		public Button _buttonPattern;

		public TextMeshProUGUI _textPattern;

		public ColorCombination _colorPattern = new ColorCombination();

		public Toggle _toggleClamp;

		public InputCombination[] _input;

		public Color colorMain
		{
			set
			{
				_colorMain.imageColor.color = value;
			}
		}

		public string textPattern
		{
			set
			{
				_textPattern.text = value;
			}
		}

		public Color colorPattern
		{
			set
			{
				_colorPattern.imageColor.color = value;
			}
		}

		public bool isOn
		{
			set
			{
				_toggleClamp.isOn = value;
			}
		}

		public InputCombination this[int _idx] => _input.SafeGet(_idx);

		public bool enable
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				if (objRoot.activeSelf != value)
				{
					objRoot.SetActive(value);
				}
			}
		}

		public bool EnableMetallic
		{
			set
			{
				objMetallic.SetActiveIfDifferent(value);
			}
		}

		public bool EnablePattern
		{
			set
			{
				objPattern.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class ColorInputCombination
	{
		public GameObject objRoot;

		public ColorCombination color = new ColorCombination();

		public InputCombination input = new InputCombination();

		public bool active
		{
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class EmissionInfo : ColorInputCombination
	{
	}

	[Serializable]
	private class LineInfo : ColorInputCombination
	{
	}

	[Serializable]
	private class KinematicInfo
	{
		public GameObject objRoot;

		public Toggle toggleFK;

		public Toggle toggleDynamicBone;

		public bool Active
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class PanelInfo
	{
		public GameObject objRoot;

		public Button _buttonTex;

		public TextMeshProUGUI _textTex;

		public ColorCombination _color;

		public Toggle _toggleClamp;

		public InputCombination[] _input;

		public string textTex
		{
			set
			{
				_textTex.text = value;
			}
		}

		public Color color
		{
			set
			{
				_color.imageColor.color = value;
			}
		}

		public bool isOn
		{
			set
			{
				_toggleClamp.isOn = value;
			}
		}

		public InputCombination this[int _idx] => _input.SafeGet(_idx);

		public bool enable
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class PanelList
	{
		[SerializeField]
		private GameObject objRoot;

		[SerializeField]
		private GameObject objectNode;

		[SerializeField]
		private Transform transformRoot;

		public Action<string> actUpdatePath;

		private List<string> listPath = new List<string>();

		private Dictionary<int, StudioNode> dicNode = new Dictionary<int, StudioNode>();

		private int select = -1;

		public bool active
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}

		public void Init()
		{
			for (int i = 0; i < transformRoot.childCount; i++)
			{
				UnityEngine.Object.Destroy(transformRoot.GetChild(i).gameObject);
			}
			transformRoot.DetachChildren();
			listPath = (from _s in Directory.GetFiles(DefaultData.Create(BackgroundList.dirName), "*.png")
				select DefaultData.RootName + "/" + BackgroundList.dirName + "/" + Path.GetFileName(_s)).ToList();
			IEnumerable<string> collection = from _s in Directory.GetFiles(UserData.Create(BackgroundList.dirName), "*.png")
				select UserData.RootName + "/" + BackgroundList.dirName + "/" + Path.GetFileName(_s);
			listPath.AddRange(collection);
			CreateNode(-1, "なし");
			int count = listPath.Count;
			for (int num = 0; num < count; num++)
			{
				CreateNode(num, Path.GetFileNameWithoutExtension(listPath[num]));
			}
		}

		public void Setup(string _file, Action<string> _actUpdate)
		{
			SetSelect(select, _flag: false);
			select = listPath.FindIndex((string s) => s == _file);
			SetSelect(select, _flag: true);
			actUpdatePath = _actUpdate;
			active = true;
		}

		private void OnClickSelect(int _idx)
		{
			SetSelect(select, _flag: false);
			select = _idx;
			SetSelect(select, _flag: true);
			if (actUpdatePath != null)
			{
				actUpdatePath((select != -1) ? listPath[_idx] : "");
			}
			active = false;
		}

		private void SetSelect(int _idx, bool _flag)
		{
			StudioNode value = null;
			if (dicNode.TryGetValue(_idx, out value))
			{
				value.select = _flag;
			}
		}

		private void CreateNode(int _idx, string _text)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(objectNode);
			gameObject.transform.SetParent(transformRoot, worldPositionStays: false);
			StudioNode component = gameObject.GetComponent<StudioNode>();
			component.active = true;
			component.addOnClick = delegate
			{
				OnClickSelect(_idx);
			};
			component.text = _text;
			dicNode.Add(_idx, component);
		}
	}

	[Serializable]
	private class OptionInfo
	{
		public GameObject objRoot;

		public Toggle toggleAll;

		public bool Active
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}
	}

	[Serializable]
	private class AnimeInfo
	{
		public GameObject objRoot;

		public TMP_Dropdown dropdownAnime;

		public bool Active
		{
			get
			{
				return objRoot.activeSelf;
			}
			set
			{
				objRoot.SetActiveIfDifferent(value);
			}
		}

		public void InitDropdown(OCIItem _ocii)
		{
			dropdownAnime.ClearOptions();
			if (_ocii != null && !(_ocii.itemComponent == null))
			{
				ItemComponent.AnimeInfo[] animeInfos = _ocii.itemComponent.animeInfos;
				List<TMP_Dropdown.OptionData> options = (((IReadOnlyCollection<ItemComponent.AnimeInfo>)(object)animeInfos).IsNullOrEmpty() ? new List<TMP_Dropdown.OptionData>() : animeInfos.Select((ItemComponent.AnimeInfo c) => new TMP_Dropdown.OptionData(c.name)).ToList());
				dropdownAnime.options = options;
				dropdownAnime.value = _ocii.itemInfo.animePattern;
			}
		}
	}

	[SerializeField]
	private ColorInfo[] colorInfo;

	[SerializeField]
	private PanelInfo panelInfo;

	[SerializeField]
	private PanelList panelList;

	[SerializeField]
	private InputCombination inputAlpha = new InputCombination();

	[SerializeField]
	private EmissionInfo emissionInfo;

	[SerializeField]
	private InputCombination inputLightCancel;

	[SerializeField]
	[Header("キネマティクス関係")]
	private KinematicInfo kinematicInfo = new KinematicInfo();

	[SerializeField]
	[Header("オプション関係")]
	private OptionInfo optionInfo = new OptionInfo();

	[SerializeField]
	[Header("パターン関係")]
	private CanvasGroup cgPattern;

	[SerializeField]
	[Header("アニメ関係")]
	private AnimeInfo animeInfo = new AnimeInfo();

	[SerializeField]
	private AnimeControl animeControl;

	[SerializeField]
	private MPCharCtrl mpCharCtrl;

	private OCIItem m_OCIItem;

	private bool m_Active;

	private bool isUpdateInfo;

	private bool isColorFunc;

	public OCIItem ociItem
	{
		get
		{
			return m_OCIItem;
		}
		set
		{
			m_OCIItem = value;
			if (m_OCIItem != null)
			{
				UpdateInfo();
			}
		}
	}

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_Active = value;
			if (m_Active)
			{
				base.gameObject.SetActive(m_OCIItem != null && m_OCIItem.CheckAnim);
				animeControl.active = m_OCIItem != null && m_OCIItem.isAnime;
				return;
			}
			if (!mpCharCtrl.active)
			{
				animeControl.active = false;
			}
			base.gameObject.SetActive(value: false);
			if (isColorFunc)
			{
				Singleton<Studio>.Instance.colorPalette.Close();
			}
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			panelList.active = false;
			cgPattern.Enable(enable: false);
		}
	}

	public bool Deselect(OCIItem _ociItem)
	{
		if (m_OCIItem != _ociItem)
		{
			return false;
		}
		ociItem = null;
		active = false;
		return true;
	}

	public void UpdateInfo()
	{
		if (m_OCIItem == null)
		{
			return;
		}
		isUpdateInfo = true;
		bool[] useColor = m_OCIItem.useColor;
		bool[] useMetallic = m_OCIItem.useMetallic;
		bool[] usePattern = m_OCIItem.usePattern;
		for (int i = 0; i < 3; i++)
		{
			this.colorInfo[i].enable = useColor[i];
			if (useColor[i])
			{
				global::Studio.ColorInfo colorInfo = m_OCIItem.itemInfo.colors[i];
				this.colorInfo[i].colorMain = colorInfo.mainColor;
				this.colorInfo[i].EnableMetallic = useMetallic[i];
				if (useMetallic[i])
				{
					this.colorInfo[i].inputMetallic.value = colorInfo.metallic;
					this.colorInfo[i].inputGlossiness.value = colorInfo.glossiness;
				}
				this.colorInfo[i].EnablePattern = usePattern[i];
				if (usePattern[i])
				{
					this.colorInfo[i].textPattern = colorInfo.pattern.name;
					this.colorInfo[i].colorPattern = colorInfo.pattern.color;
					this.colorInfo[i].isOn = !colorInfo.pattern.clamp;
					this.colorInfo[i][0].value = colorInfo.pattern.ut;
					this.colorInfo[i][1].value = colorInfo.pattern.vt;
					this.colorInfo[i][2].value = colorInfo.pattern.us;
					this.colorInfo[i][3].value = colorInfo.pattern.vs;
					this.colorInfo[i][4].value = colorInfo.pattern.rot;
				}
			}
		}
		this.colorInfo[3].enable = m_OCIItem.useColor4;
		if (this.colorInfo[3].enable)
		{
			this.colorInfo[3].colorMain = m_OCIItem.itemInfo.colors[3].mainColor;
			this.colorInfo[3].EnableMetallic = false;
			this.colorInfo[3].EnablePattern = false;
		}
		panelInfo.enable = m_OCIItem.checkPanel;
		panelList.active = false;
		SetPanelTexName(m_OCIItem.itemInfo.panel.filePath);
		panelInfo.color = m_OCIItem.itemInfo.colors[0].mainColor;
		panelInfo.isOn = !m_OCIItem.itemInfo.colors[0].pattern.clamp;
		panelInfo[0].value = m_OCIItem.itemInfo.colors[0].pattern.ut;
		panelInfo[1].value = m_OCIItem.itemInfo.colors[0].pattern.vt;
		panelInfo[2].value = m_OCIItem.itemInfo.colors[0].pattern.us;
		panelInfo[3].value = m_OCIItem.itemInfo.colors[0].pattern.vs;
		panelInfo[4].value = m_OCIItem.itemInfo.colors[0].pattern.rot;
		inputAlpha.value = m_OCIItem.itemInfo.alpha;
		inputAlpha.active = m_OCIItem.CheckAlpha;
		emissionInfo.active = m_OCIItem.CheckEmission;
		if (m_OCIItem.CheckEmissionColor)
		{
			emissionInfo.color.interactable = true;
			emissionInfo.color.color = m_OCIItem.itemInfo.EmissionColor;
		}
		else
		{
			emissionInfo.color.interactable = false;
			emissionInfo.color.color = Color.white;
		}
		if (m_OCIItem.CheckEmissionPower)
		{
			emissionInfo.input.interactable = true;
			emissionInfo.input.value = m_OCIItem.itemInfo.emissionPower;
		}
		else
		{
			emissionInfo.input.interactable = false;
			emissionInfo.input.value = 0f;
		}
		inputLightCancel.active = m_OCIItem.CheckLightCancel;
		inputLightCancel.value = m_OCIItem.itemInfo.lightCancel;
		kinematicInfo.Active = m_OCIItem.isFK || m_OCIItem.isDynamicBone;
		kinematicInfo.toggleFK.interactable = m_OCIItem.isFK;
		kinematicInfo.toggleFK.isOn = m_OCIItem.itemInfo.enableFK;
		kinematicInfo.toggleDynamicBone.interactable = m_OCIItem.isDynamicBone;
		kinematicInfo.toggleDynamicBone.isOn = m_OCIItem.itemInfo.enableDynamicBone;
		optionInfo.Active = m_OCIItem.CheckOption;
		optionInfo.toggleAll.isOn = m_OCIItem.itemInfo.option.SafeGet(0);
		animeInfo.Active = m_OCIItem.CheckAnimePattern;
		animeInfo.InitDropdown(m_OCIItem);
		animeControl.objectCtrlInfo = m_OCIItem;
		cgPattern.Enable(enable: false);
		isUpdateInfo = false;
	}

	private void OnClickColorMain(int _idx)
	{
		string[] array = new string[3] { "アイテム カラー１", "アイテム カラー２", "アイテム カラー３" };
		if (Singleton<Studio>.Instance.colorPalette.Check(array[_idx]))
		{
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			return;
		}
		IEnumerable<OCIItem> array2 = from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem;
		Singleton<Studio>.Instance.colorPalette.Setup(array[_idx], m_OCIItem.itemInfo.colors[_idx].mainColor, delegate(Color _c)
		{
			foreach (OCIItem item in array2)
			{
				item.SetColor(item.IsParticle ? _c : new Color(_c.r, _c.g, _c.b, 1f), _idx);
			}
			colorInfo[_idx].colorMain = _c;
		}, m_OCIItem.IsParticle);
		isColorFunc = true;
	}

	private void OnClickColor4()
	{
		if (Singleton<Studio>.Instance.colorPalette.Check("アイテム カラー４"))
		{
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			return;
		}
		IEnumerable<OCIItem> array = from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem;
		Singleton<Studio>.Instance.colorPalette.Setup("アイテム カラー４", m_OCIItem.itemInfo.colors[3].mainColor, delegate(Color _c)
		{
			foreach (OCIItem item in array)
			{
				item.SetColor(_c, 3);
			}
			colorInfo[3].colorMain = _c;
		}, _useAlpha: true);
		isColorFunc = true;
	}

	private void OnClickColorMainDef(int _idx)
	{
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			if (item.isChangeColor && item.useColor.SafeGet(_idx))
			{
				item.SetColor(item.defColor[_idx], _idx);
			}
		}
		m_OCIItem.defColor.SafeProc(_idx, delegate(Color _c)
		{
			colorInfo[_idx].colorMain = _c;
		});
		isColorFunc = false;
		Singleton<Studio>.Instance.colorPalette.visible = false;
	}

	private void OnClickColor4Def()
	{
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetColor(item.itemComponent.defGlass, 3);
		}
		colorInfo[3].colorMain = m_OCIItem.itemComponent.defGlass;
		isColorFunc = false;
		Singleton<Studio>.Instance.colorPalette.visible = false;
	}

	private void OnValueChangeMetallic(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetMetallic(_idx, _value);
		}
		colorInfo[_idx].inputMetallic.value = _value;
	}

	private void OnEndEditMetallic(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetMetallic(_idx, value);
		}
		colorInfo[_idx].inputMetallic.value = value;
	}

	private void OnClickMetallicDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetMetallic(_idx, info.defMetallic);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx].inputMetallic.value = info.defMetallic;
		});
	}

	private void OnValueChangeGlossiness(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetGlossiness(_idx, _value);
		}
		colorInfo[_idx].inputGlossiness.value = _value;
	}

	private void OnEndEditGlossiness(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetGlossiness(_idx, value);
		}
		colorInfo[_idx].inputGlossiness.value = value;
	}

	private void OnClickGlossinessDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetGlossiness(_idx, info.defGlossiness);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx].inputGlossiness.value = info.defGlossiness;
		});
	}

	private void OnClickColorPattern(int _idx)
	{
		string[] array = new string[3] { "柄の色１", "柄の色２", "柄の色３" };
		if (Singleton<Studio>.Instance.colorPalette.Check(array[_idx]))
		{
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			return;
		}
		IEnumerable<OCIItem> array2 = from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem;
		Singleton<Studio>.Instance.colorPalette.Setup(array[_idx], m_OCIItem.itemInfo.colors[_idx].pattern.color, delegate(Color _c)
		{
			foreach (OCIItem item in array2)
			{
				item.SetPatternColor(_idx, _c);
			}
			colorInfo[_idx].colorPattern = _c;
		}, _useAlpha: true);
		isColorFunc = true;
	}

	private void OnClickColorPatternDef(int _idx)
	{
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternColor(_idx, item.itemComponent.defColorPattern[_idx]);
		}
		colorInfo[_idx].colorPattern = m_OCIItem.itemComponent.defColorPattern[_idx];
		isColorFunc = false;
		Singleton<Studio>.Instance.colorPalette.visible = false;
	}

	private void OnToggleColor(int _idx, bool _flag)
	{
		if (m_OCIItem == null)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternClamp(_idx, !_flag);
		}
	}

	private void OnValueChangeUT(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternUT(_idx, _value);
		}
		colorInfo[_idx][0].value = _value;
	}

	private void OnEndEditUT(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternUT(_idx, value);
		}
		colorInfo[_idx][0].value = value;
	}

	private void OnClickUTDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetPatternUT(_idx, info.ut);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx][0].value = info.ut;
		});
	}

	private void OnValueChangeVT(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternVT(_idx, _value);
		}
		colorInfo[_idx][1].value = _value;
	}

	private void OnEndEditVT(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternVT(_idx, value);
		}
		colorInfo[_idx][1].value = value;
	}

	private void OnClickVTDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetPatternVT(_idx, info.vt);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx][1].value = info.vt;
		});
	}

	private void OnValueChangeUS(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternUS(_idx, _value);
		}
		colorInfo[_idx][2].value = _value;
	}

	private void OnEndEditUS(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0.01f, 20f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternUS(_idx, value);
		}
		colorInfo[_idx][2].value = value;
	}

	private void OnClickUSDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetPatternUS(_idx, info.us);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx][2].value = info.us;
		});
	}

	private void OnValueChangeVS(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternVS(_idx, _value);
		}
		colorInfo[_idx][3].value = _value;
	}

	private void OnEndEditVS(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0.01f, 20f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternVS(_idx, value);
		}
		colorInfo[_idx][3].value = value;
	}

	private void OnClickVSDef(int _idx)
	{
		foreach (OCIItem v in from objectCtrlInfo in Studio.GetSelectObjectCtrl()
			where objectCtrlInfo.kind == 1
			select objectCtrlInfo as OCIItem)
		{
			v.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
			{
				v.SetPatternVS(_idx, info.vs);
			});
		}
		m_OCIItem.itemComponent[_idx].SafeProc(delegate(ItemComponent.Info info)
		{
			colorInfo[_idx][3].value = info.vs;
		});
	}

	private void OnValueChangeRot(int _idx, float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternRot(_idx, _value);
		}
		colorInfo[_idx][4].value = _value;
	}

	private void OnEndEditRot(int _idx, string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetPatternRot(_idx, value);
		}
		colorInfo[_idx][4].value = value;
	}

	private void OnClickPanel()
	{
		if (panelList.active)
		{
			panelList.active = false;
			return;
		}
		panelList.Setup(m_OCIItem.itemInfo.panel.filePath, SetMainTex);
		isColorFunc = true;
	}

	private void SetMainTex(string _file)
	{
		SetPanelTexName(_file);
		m_OCIItem.SetMainTex(_file);
	}

	private void SetPanelTexName(string _str)
	{
		panelInfo.textTex = (_str.IsNullOrEmpty() ? "なし" : Path.GetFileNameWithoutExtension(_str));
	}

	private void OnClickColorPanel()
	{
		if (Singleton<Studio>.Instance.colorPalette.Check("画像板"))
		{
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			return;
		}
		Singleton<Studio>.Instance.colorPalette.Setup("画像板", m_OCIItem.itemInfo.colors[0].mainColor, delegate(Color _c)
		{
			Color color = new Color(_c.r, _c.g, _c.b, 1f);
			m_OCIItem.SetColor(color, 0);
			panelInfo.color = color;
		}, _useAlpha: false);
		isColorFunc = true;
	}

	private void OnToggleColor(bool _flag)
	{
		if (m_OCIItem != null)
		{
			m_OCIItem.SetPatternClamp(0, !_flag);
		}
	}

	private void OnValueChangeUT(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetPatternUT(0, _value);
			panelInfo[0].value = _value;
		}
	}

	private void OnEndEditUT(string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		m_OCIItem.SetPatternUT(0, value);
		panelInfo[0].value = value;
	}

	private void OnClickUTDef()
	{
		panelInfo[0].value = 0f;
	}

	private void OnValueChangeVT(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetPatternVT(0, _value);
			panelInfo[1].value = _value;
		}
	}

	private void OnEndEditVT(string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		m_OCIItem.SetPatternVT(0, value);
		panelInfo[1].value = value;
	}

	private void OnClickVTDef()
	{
		panelInfo[1].value = 0f;
	}

	private void OnValueChangeUS(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetPatternUS(0, _value);
			panelInfo[2].value = _value;
		}
	}

	private void OnEndEditUS(string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0.01f, 20f);
		m_OCIItem.SetPatternUS(0, value);
		panelInfo[2].value = value;
	}

	private void OnClickUSDef()
	{
		panelInfo[2].value = 1f;
	}

	private void OnValueChangeVS(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetPatternVS(0, _value);
			panelInfo[3].value = _value;
		}
	}

	private void OnEndEditVS(string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), 0.01f, 20f);
		m_OCIItem.SetPatternVS(0, value);
		panelInfo[3].value = value;
	}

	private void OnClickVSDef()
	{
		panelInfo[3].value = 1f;
	}

	private void OnValueChangeRot(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetPatternRot(0, _value);
			panelInfo[4].value = _value;
		}
	}

	private void OnEndEditRot(string _text)
	{
		float value = Mathf.Clamp(Utility.StringToFloat(_text), -1f, 1f);
		m_OCIItem.SetPatternRot(0, value);
		panelInfo[4].value = value;
	}

	private void OnValueChangeAlpha(float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetAlpha(_value);
		}
		inputAlpha.value = _value;
	}

	private void OnEndEditAlpha(string _text)
	{
		float num = Mathf.Clamp(Utility.StringToFloat(_text), 0f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetAlpha(num);
		}
		inputAlpha.value = num;
	}

	private void OnClickEmissionColor()
	{
		if (Singleton<Studio>.Instance.colorPalette.Check("発光色"))
		{
			isColorFunc = false;
			Singleton<Studio>.Instance.colorPalette.visible = false;
			return;
		}
		IEnumerable<OCIItem> array = from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem;
		Singleton<Studio>.Instance.colorPalette.Setup("発光色", m_OCIItem.itemInfo.EmissionColor, delegate(Color _c)
		{
			foreach (OCIItem item in array)
			{
				item.itemInfo.EmissionColor = _c;
				item.SetEmissionColor(item.itemInfo.emissionColor);
			}
			emissionInfo.color.color = _c;
		}, _useAlpha: false);
		isColorFunc = true;
	}

	private void OnClickEmissionColorDef()
	{
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetEmissionColor(item.itemComponent.DefEmissionColor);
		}
		emissionInfo.color.color = m_OCIItem.itemComponent.DefEmissionColor;
		isColorFunc = false;
		Singleton<Studio>.Instance.colorPalette.visible = false;
	}

	private void OnValueChangeEmissionPower(float _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetEmissionPower(_value);
		}
		emissionInfo.input.value = _value;
	}

	private void OnEndEditEmissionPower(string _text)
	{
		float num = Mathf.Clamp(Utility.StringToFloat(_text), 0f, 1f);
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetEmissionPower(num);
		}
		emissionInfo.input.value = num;
	}

	private void OnClickEmissionPowerDef()
	{
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetEmissionPower(item.itemComponent.defEmissionStrength);
		}
		emissionInfo.input.value = m_OCIItem.itemComponent.defEmissionStrength;
	}

	private void OnValueChangeLightCancel(float _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetLightCancel(_value);
			inputLightCancel.value = _value;
		}
	}

	private void OnEndEditLightCancel(string _text)
	{
		float num = Mathf.Clamp(Utility.StringToFloat(_text), 0f, 1f);
		m_OCIItem.SetLightCancel(num);
		inputLightCancel.value = num;
	}

	private void OnClickLightCancelDef()
	{
		m_OCIItem.SetLightCancel(m_OCIItem.itemComponent.defLightCancel);
		inputLightCancel.value = m_OCIItem.itemComponent.defLightCancel;
	}

	private void OnValueChangedFK(bool _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.ActiveFK(_value);
			kinematicInfo.toggleDynamicBone.interactable = m_OCIItem.isDynamicBone;
		}
	}

	private void OnValueChangedDynamicBone(bool _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.ActiveDynamicBone(_value);
		}
	}

	private void OnValueChangedOption(bool _value)
	{
		if (isUpdateInfo)
		{
			return;
		}
		foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
			where v.kind == 1
			select v as OCIItem)
		{
			item.SetOptionVisible(_value);
		}
	}

	private void OnClickPattern(int _idx)
	{
		if (cgPattern.alpha != 0f)
		{
			cgPattern.Enable(enable: false);
			return;
		}
		Singleton<Studio>.Instance.patternSelectListCtrl.onChangeItemFunc = delegate(int _index)
		{
			string text = "";
			foreach (OCIItem item in from v in Studio.GetSelectObjectCtrl()
				where v.kind == 1
				select v as OCIItem)
			{
				string text2 = item.SetPatternTex(_idx, _index);
				if (text.IsNullOrEmpty())
				{
					text = text2;
				}
			}
			colorInfo[_idx].textPattern = Path.GetFileNameWithoutExtension(text);
			cgPattern.Enable(enable: false);
		};
		cgPattern.Enable(enable: true);
	}

	private void OnValueChangedAnimePattern(int _value)
	{
		if (!isUpdateInfo)
		{
			m_OCIItem.SetAnimePattern(_value);
		}
	}

	private string ConvertString(float _t)
	{
		return ((int)Mathf.Lerp(0f, 100f, _t)).ToString();
	}

	private void Awake()
	{
		panelList.Init();
		for (int i = 0; i < 3; i++)
		{
			int no = i;
			colorInfo[i]._colorMain.buttonColor.OnClickAsObservable().Subscribe(delegate
			{
				OnClickColorMain(no);
			});
			colorInfo[i]._colorMain.buttonColorDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickColorMainDef(no);
			});
			colorInfo[i].inputMetallic.slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeMetallic(no, f);
			});
			colorInfo[i].inputMetallic.input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditMetallic(no, s);
			});
			colorInfo[i].inputMetallic.buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickMetallicDef(no);
			});
			colorInfo[i].inputGlossiness.slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeGlossiness(no, f);
			});
			colorInfo[i].inputGlossiness.input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditGlossiness(no, s);
			});
			colorInfo[i].inputGlossiness.buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickGlossinessDef(no);
			});
			colorInfo[i]._buttonPattern.OnClickAsObservable().Subscribe(delegate
			{
				OnClickPattern(no);
			});
			colorInfo[i]._colorPattern.buttonColor.OnClickAsObservable().Subscribe(delegate
			{
				OnClickColorPattern(no);
			});
			colorInfo[i]._colorPattern.buttonColorDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickColorPatternDef(no);
			});
			colorInfo[i]._toggleClamp.OnValueChangedAsObservable().Subscribe(delegate(bool f)
			{
				OnToggleColor(no, f);
			});
			colorInfo[i][0].slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeUT(no, f);
			});
			colorInfo[i][0].input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditUT(no, s);
			});
			colorInfo[i][0].buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickUTDef(no);
			});
			colorInfo[i][1].slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeVT(no, f);
			});
			colorInfo[i][1].input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditVT(no, s);
			});
			colorInfo[i][1].buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickVTDef(no);
			});
			colorInfo[i][2].slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeUS(no, f);
			});
			colorInfo[i][2].input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditUS(no, s);
			});
			colorInfo[i][2].buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickUSDef(no);
			});
			colorInfo[i][3].slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeVS(no, f);
			});
			colorInfo[i][3].input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditVS(no, s);
			});
			colorInfo[i][3].buttonDefault.OnClickAsObservable().Subscribe(delegate
			{
				OnClickVSDef(no);
			});
			colorInfo[i][4].slider.onValueChanged.AddListener(delegate(float f)
			{
				OnValueChangeRot(no, f);
			});
			colorInfo[i][4].input.onEndEdit.AddListener(delegate(string s)
			{
				OnEndEditRot(no, s);
			});
		}
		colorInfo[3]._colorMain.buttonColor.OnClickAsObservable().Subscribe(delegate
		{
			OnClickColor4();
		});
		colorInfo[3]._colorMain.buttonColorDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickColor4Def();
		});
		panelInfo._buttonTex.OnClickAsObservable().Subscribe(delegate
		{
			OnClickPanel();
		});
		panelInfo._color.buttonColor.OnClickAsObservable().Subscribe(delegate
		{
			OnClickColorPanel();
		});
		panelInfo._toggleClamp.OnValueChangedAsObservable().Subscribe(delegate(bool f)
		{
			OnToggleColor(f);
		});
		panelInfo[0].slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeUT(f);
		});
		panelInfo[0].input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditUT(s);
		});
		panelInfo[0].buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickUTDef();
		});
		panelInfo[1].slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeVT(f);
		});
		panelInfo[1].input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditVT(s);
		});
		panelInfo[1].buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickVTDef();
		});
		panelInfo[2].slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeUS(f);
		});
		panelInfo[2].input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditUS(s);
		});
		panelInfo[2].buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickUSDef();
		});
		panelInfo[3].slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeVS(f);
		});
		panelInfo[3].input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditVS(s);
		});
		panelInfo[3].buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickVSDef();
		});
		panelInfo[4].slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeRot(f);
		});
		panelInfo[4].input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditRot(s);
		});
		inputAlpha.slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeAlpha(f);
		});
		inputAlpha.input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditAlpha(s);
		});
		emissionInfo.color.buttonColor.OnClickAsObservable().Subscribe(delegate
		{
			OnClickEmissionColor();
		});
		emissionInfo.color.buttonColorDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickEmissionColorDef();
		});
		emissionInfo.input.slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeEmissionPower(f);
		});
		emissionInfo.input.input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditEmissionPower(s);
		});
		emissionInfo.input.buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickEmissionPowerDef();
		});
		inputLightCancel.slider.onValueChanged.AddListener(delegate(float f)
		{
			OnValueChangeLightCancel(f);
		});
		inputLightCancel.input.onEndEdit.AddListener(delegate(string s)
		{
			OnEndEditLightCancel(s);
		});
		inputLightCancel.buttonDefault.OnClickAsObservable().Subscribe(delegate
		{
			OnClickLightCancelDef();
		});
		kinematicInfo.toggleFK.onValueChanged.AddListener(OnValueChangedFK);
		kinematicInfo.toggleDynamicBone.onValueChanged.AddListener(OnValueChangedDynamicBone);
		optionInfo.toggleAll.onValueChanged.AddListener(OnValueChangedOption);
		animeInfo.dropdownAnime.onValueChanged.AddListener(OnValueChangedAnimePattern);
		isUpdateInfo = false;
		m_Active = false;
		base.gameObject.SetActive(value: false);
	}
}
