using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion;
using Manager;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class MPCharCtrl : MonoBehaviour
{
	[Serializable]
	private class CommonInfo
	{
		public GameObject objRoot;

		public virtual bool active
		{
			set
			{
				if (objRoot.activeSelf != value)
				{
					objRoot.SetActive(value);
				}
			}
		}

		public bool isUpdateInfo { get; set; }

		public OCIChar ociChar { get; set; }

		public virtual void Init()
		{
			isUpdateInfo = false;
		}

		public virtual void UpdateInfo(OCIChar _char)
		{
			ociChar = _char;
		}
	}

	[Serializable]
	private class RootButtonInfo
	{
		public Button button;

		public GameObject root;

		public bool active
		{
			get
			{
				return root.activeSelf;
			}
			set
			{
				if (root.activeSelf != value)
				{
					root.SetActive(value);
					button.image.color = (root.activeSelf ? Color.green : Color.white);
				}
			}
		}
	}

	[Serializable]
	public class StateCommonInfo
	{
		public Button buttonOpen;

		public GameObject objOpen;

		private bool m_Open = true;

		public Sprite[] spriteVisible { get; set; }

		public bool isOpen
		{
			get
			{
				return m_Open;
			}
			set
			{
				if (Utility.SetStruct(ref m_Open, value))
				{
					Change();
				}
			}
		}

		public bool active
		{
			set
			{
				GameObject gameObject = buttonOpen.transform.parent.gameObject;
				if (gameObject.activeSelf != value)
				{
					gameObject.SetActive(value);
					bool flag = value & m_Open;
					if (objOpen.activeSelf != flag)
					{
						objOpen.SetActive(flag);
					}
				}
			}
		}

		public virtual void Init(Sprite[] _spriteVisible)
		{
			spriteVisible = _spriteVisible;
			buttonOpen.onClick.AddListener(OnClick);
			m_Open = true;
		}

		public virtual void UpdateInfo(OCIChar _char)
		{
		}

		private void OnClick()
		{
			isOpen = !isOpen;
		}

		private void Change()
		{
			if (objOpen.activeSelf != m_Open)
			{
				objOpen.SetActive(m_Open);
			}
			buttonOpen.image.sprite = spriteVisible[m_Open ? 1 : 0];
		}
	}

	[Serializable]
	public class StateButtonInfo
	{
		public GameObject root;

		public Button[] buttons;

		public bool interactable
		{
			set
			{
				for (int i = 0; i < buttons.Length; i++)
				{
					buttons[i].interactable = value;
				}
			}
		}

		public int select
		{
			set
			{
				int num = Mathf.Clamp(value, 0, buttons.Length - 1);
				for (int i = 0; i < buttons.Length; i++)
				{
					buttons[i].image.color = ((buttons[i].interactable && i == num) ? Color.green : Color.white);
				}
			}
		}

		public bool active
		{
			set
			{
				if ((bool)root && root.activeSelf != value)
				{
					root.SetActive(value);
				}
			}
		}

		public void Interactable(int _state, bool _flag)
		{
			buttons[_state].interactable = _flag;
		}

		public void Interactable(params int[] _state)
		{
			if (((IReadOnlyCollection<int>)(object)_state).IsNullOrEmpty())
			{
				interactable = false;
				return;
			}
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].interactable = _state.Contains(i);
			}
		}
	}

	[Serializable]
	public class StateSliderInfo
	{
		public GameObject root;

		public Slider slider;

		public bool active
		{
			set
			{
				if ((bool)root && root.activeSelf != value)
				{
					root.SetActive(value);
				}
			}
		}
	}

	[Serializable]
	public class StateToggleInfo
	{
		public GameObject root;

		public Toggle toggle;

		public bool active
		{
			set
			{
				if ((bool)root && root.activeSelf != value)
				{
					root.SetActive(value);
				}
			}
		}
	}

	[Serializable]
	public class ClothingDetailsInfo : StateCommonInfo
	{
		public delegate void OnClickFunc(int _id, byte _state);

		public StateButtonInfo top = new StateButtonInfo();

		public StateButtonInfo buttom = new StateButtonInfo();

		public StateButtonInfo bra = new StateButtonInfo();

		public StateButtonInfo shorts = new StateButtonInfo();

		public StateButtonInfo pantyhose = new StateButtonInfo();

		public StateButtonInfo gloves = new StateButtonInfo();

		public StateButtonInfo socks = new StateButtonInfo();

		public StateButtonInfo cloth = new StateButtonInfo();

		public StateButtonInfo shoes = new StateButtonInfo();

		private OCIChar ociChar;

		private StateButtonInfo[] infoArray => new StateButtonInfo[8] { top, buttom, bra, shorts, gloves, pantyhose, socks, shoes };

		public void Init(Sprite[] _spriteVisible, OnClickFunc _func)
		{
			base.Init(_spriteVisible);
			StateButtonInfo[] array = infoArray;
			for (int i = 0; i < array.Length; i++)
			{
				int id = i;
				SetFunc(array[i], _func, id);
			}
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			ociChar = _char;
			StateButtonInfo[] array = infoArray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].active = true;
				if (i == 3)
				{
					Dictionary<byte, string> clothesStateKind = _char.charInfo.GetClothesStateKind(i);
					array[i].Interactable(((IEnumerable<byte>)clothesStateKind?.Keys).Select((Func<byte, int>)((byte v) => v)).ToArray());
				}
				else
				{
					array[i].interactable = _char.charInfo.IsClothesStateKind(i);
				}
				array[i].select = _char.charFileStatus.clothesState[i];
			}
		}

		private void SetFunc(StateButtonInfo _info, OnClickFunc _func, int _id)
		{
			for (int i = 0; i < _info.buttons.Length; i++)
			{
				byte state = (byte)i;
				_info.buttons[i].onClick.AddListener(delegate
				{
					_func(_id, state);
				});
				_info.buttons[i].onClick.AddListener(UpdateState);
			}
		}

		private void UpdateState()
		{
			if (ociChar != null)
			{
				StateButtonInfo[] array = infoArray;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].select = ociChar.charFileStatus.clothesState[i];
				}
			}
		}
	}

	[Serializable]
	public class AccessoriesInfo : StateCommonInfo
	{
		public delegate void OnClickFunc(int _id, bool _flag);

		public StateButtonInfo[] slots = new StateButtonInfo[20];

		public void Init(Sprite[] _spriteVisible, OnClickFunc _func)
		{
			base.Init(_spriteVisible);
			for (int i = 0; i < slots.Length; i++)
			{
				int id = i;
				for (int j = 0; j < 2; j++)
				{
					bool flag = j == 0;
					slots[i].buttons[j].onClick.AddListener(delegate
					{
						_func(id, flag);
					});
					int state = j;
					slots[i].buttons[j].onClick.AddListener(delegate
					{
						slots[id].select = state;
					});
				}
			}
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].interactable = _char.charInfo.objAccessory[i] != null;
			}
			for (int j = 0; j < slots.Length; j++)
			{
				slots[j].select = ((!_char.charFileStatus.showAccessory[j]) ? 1 : 0);
			}
		}
	}

	[Serializable]
	public class LiquidInfo : StateCommonInfo
	{
		public delegate void OnClickFunc(ChaFileDefine.SiruParts _parts, byte _state);

		public StateButtonInfo face = new StateButtonInfo();

		public StateButtonInfo breast = new StateButtonInfo();

		public StateButtonInfo back = new StateButtonInfo();

		public StateButtonInfo belly = new StateButtonInfo();

		public StateButtonInfo hip = new StateButtonInfo();

		public void Init(Sprite[] _spriteVisible, OnClickFunc _func)
		{
			base.Init(_spriteVisible);
			SetFunc(face, _func, ChaFileDefine.SiruParts.SiruKao);
			SetFunc(breast, _func, ChaFileDefine.SiruParts.SiruFrontTop);
			SetFunc(back, _func, ChaFileDefine.SiruParts.SiruBackTop);
			SetFunc(belly, _func, ChaFileDefine.SiruParts.SiruFrontBot);
			SetFunc(hip, _func, ChaFileDefine.SiruParts.SiruBackBot);
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			if (_char.oiCharInfo.sex == 1)
			{
				base.active = true;
				face.select = _char.GetSiruFlags(ChaFileDefine.SiruParts.SiruKao);
				breast.select = _char.GetSiruFlags(ChaFileDefine.SiruParts.SiruFrontTop);
				back.select = _char.GetSiruFlags(ChaFileDefine.SiruParts.SiruBackTop);
				belly.select = _char.GetSiruFlags(ChaFileDefine.SiruParts.SiruFrontBot);
				hip.select = _char.GetSiruFlags(ChaFileDefine.SiruParts.SiruBackBot);
			}
			else
			{
				base.active = false;
			}
		}

		private void SetFunc(StateButtonInfo _info, OnClickFunc _func, ChaFileDefine.SiruParts _parts)
		{
			for (int i = 0; i < _info.buttons.Length; i++)
			{
				byte state = (byte)i;
				_info.buttons[i].onClick.AddListener(delegate
				{
					_func(_parts, state);
				});
				_info.buttons[i].onClick.AddListener(delegate
				{
					_info.select = state;
				});
			}
		}
	}

	[Serializable]
	public class OtherInfo : StateCommonInfo
	{
		public delegate void OnClickTears(byte _state);

		public StateSliderInfo tears = new StateSliderInfo();

		public StateSliderInfo cheek = new StateSliderInfo();

		public StateSliderInfo nipple = new StateSliderInfo();

		public StateSliderInfo skin = new StateSliderInfo();

		public StateSliderInfo wet = new StateSliderInfo();

		public StateToggleInfo single = new StateToggleInfo();

		public StateButtonInfo color = new StateButtonInfo();

		public StateToggleInfo son = new StateToggleInfo();

		public StateSliderInfo sonLen = new StateSliderInfo();

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			bool flag = _char.oiCharInfo.sex == 1;
			nipple.active = flag;
			skin.active = true;
			wet.active = true;
			single.active = true;
			color.active = true;
			son.active = true;
			sonLen.active = true;
			tears.slider.value = _char.GetTears();
			cheek.slider.value = _char.GetHohoAkaRate();
			son.toggle.isOn = _char.oiCharInfo.visibleSon;
			sonLen.slider.value = _char.GetSonLength();
			if (flag)
			{
				nipple.slider.value = _char.oiCharInfo.nipple;
			}
			skin.slider.value = _char.oiCharInfo.SkinTuyaRate;
			wet.slider.value = _char.oiCharInfo.WetRate;
			single.toggle.isOn = _char.GetVisibleSimple();
			SetSimpleColor(_char.oiCharInfo.simpleColor);
		}

		public void SetSimpleColor(Color _color)
		{
			color.buttons[0].image.color = _color;
		}
	}

	[Serializable]
	private class StateInfo : CommonInfo
	{
		public Sprite[] spriteVisible;

		public Button[] buttonCosState;

		public ClothingDetailsInfo clothingDetailsInfo = new ClothingDetailsInfo();

		public AccessoriesInfo accessoriesInfo = new AccessoriesInfo();

		public LiquidInfo liquidInfo = new LiquidInfo();

		public OtherInfo otherInfo = new OtherInfo();

		public override void Init()
		{
			base.Init();
			buttonCosState[0].onClick.AddListener(delegate
			{
				OnClickCosState(0);
			});
			buttonCosState[1].onClick.AddListener(delegate
			{
				OnClickCosState(1);
			});
			buttonCosState[2].onClick.AddListener(delegate
			{
				OnClickCosState(2);
			});
			clothingDetailsInfo.Init(spriteVisible, OnClickClothingDetails);
			accessoriesInfo.Init(spriteVisible, OnClickAccessories);
			liquidInfo.Init(spriteVisible, OnClickLiquid);
			otherInfo.Init(spriteVisible);
			otherInfo.tears.slider.onValueChanged.AddListener(OnValueChangedTears);
			otherInfo.cheek.slider.onValueChanged.AddListener(OnValueChangedCheek);
			otherInfo.nipple.slider.onValueChanged.AddListener(OnValueChangedNipple);
			otherInfo.skin.slider.onValueChanged.AddListener(OnValueChangedSkin);
			otherInfo.wet.slider.onValueChanged.AddListener(OnValueChangedWet);
			otherInfo.single.toggle.onValueChanged.AddListener(OnValueChangedSimple);
			otherInfo.color.buttons[0].onClick.AddListener(OnClickSimpleColor);
			otherInfo.son.toggle.onValueChanged.AddListener(OnValueChangedSon);
			otherInfo.sonLen.slider.onValueChanged.AddListener(OnValueChangedSonLength);
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			clothingDetailsInfo.UpdateInfo(_char);
			accessoriesInfo.UpdateInfo(_char);
			liquidInfo.UpdateInfo(_char);
			otherInfo.UpdateInfo(_char);
			base.isUpdateInfo = false;
		}

		private void OnClickCosState(int _value)
		{
			base.ociChar.SetClothesStateAll(_value);
			clothingDetailsInfo.UpdateInfo(base.ociChar);
		}

		private void OnClickClothingDetails(int _id, byte _state)
		{
			base.ociChar.SetClothesState(_id, _state);
		}

		private void OnClickAccessories(int _id, bool _flag)
		{
			base.ociChar.ShowAccessory(_id, _flag);
		}

		private void OnClickLiquid(ChaFileDefine.SiruParts _parts, byte _state)
		{
			base.ociChar.SetSiruFlags(_parts, _state);
		}

		private void OnValueChangedTears(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetTears(_value);
			}
		}

		private void OnValueChangedCheek(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetHohoAkaRate(_value);
			}
		}

		private void OnValueChangedNipple(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetNipStand(_value);
			}
		}

		private void OnValueChangedSkin(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetTuyaRate(_value);
			}
		}

		private void OnValueChangedWet(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetWetRate(_value);
			}
		}

		private void OnValueChangedSimple(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetVisibleSimple(_value);
			}
		}

		private void OnClickSimpleColor()
		{
			Singleton<Studio>.Instance.colorPalette.Setup("単色", base.ociChar.oiCharInfo.simpleColor, OnValueChangeSimpleColor, _useAlpha: true);
			Singleton<Studio>.Instance.colorPalette.visible = true;
		}

		private void OnValueChangeSimpleColor(Color _color)
		{
			base.ociChar.SetSimpleColor(_color);
			otherInfo.SetSimpleColor(_color);
		}

		private void OnValueChangedSon(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetVisibleSon(_value);
			}
		}

		private void OnValueChangedSonLength(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.SetSonLength(_value);
			}
		}
	}

	[Serializable]
	private class FKInfo : CommonInfo
	{
		public Toggle toggleFunction;

		public Toggle toggleHair;

		public Toggle toggleNeck;

		public Toggle toggleBreast;

		public Toggle toggleBody;

		public Toggle toggleRightHand;

		public Toggle toggleLeftHand;

		public Toggle toggleSkirt;

		public Slider sliderSize;

		public Button buttonAnime;

		public Button buttonReflectIK;

		public Button[] buttonAnimeSingle;

		public Button[] buttonInitSingle;

		[Space]
		public Toggle toggleVisible;

		private Toggle[] array;

		public override void Init()
		{
			base.Init();
			toggleFunction.onValueChanged.AddListener(OnChangeValueFunction);
			toggleHair.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.Hair, b);
			});
			toggleNeck.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.Neck, b);
			});
			toggleBreast.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.Breast, b);
			});
			toggleBody.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.Body, b);
			});
			toggleRightHand.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.RightHand, b);
			});
			toggleLeftHand.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.LeftHand, b);
			});
			toggleSkirt.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(OIBoneInfo.BoneGroup.Skirt, b);
			});
			sliderSize.onValueChanged.AddListener(OnValueChangedSize);
			buttonInitSingle[0].onClick.AddListener(delegate
			{
				OnClickInitSingle(OIBoneInfo.BoneGroup.Hair);
			});
			buttonInitSingle[1].onClick.AddListener(delegate
			{
				OnClickInitSingle(OIBoneInfo.BoneGroup.Skirt);
			});
			toggleVisible.gameObject.SetActive(value: false);
			array = new Toggle[7] { toggleHair, toggleNeck, toggleBreast, toggleBody, toggleRightHand, toggleLeftHand, toggleSkirt };
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			toggleFunction.isOn = _char.oiCharInfo.enableFK;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].isOn = _char.oiCharInfo.activeFK[i];
			}
			buttonReflectIK.interactable = _char.oiCharInfo.enableFK;
			toggleHair.interactable = _char.oiCharInfo.sex != 0 || _char.IsFKGroup(OIBoneInfo.BoneGroup.Hair);
			toggleBreast.interactable = _char.oiCharInfo.sex != 0 || _char.IsFKGroup(OIBoneInfo.BoneGroup.Breast);
			toggleSkirt.interactable = _char.oiCharInfo.sex != 0 || _char.IsFKGroup(OIBoneInfo.BoneGroup.Skirt);
			base.isUpdateInfo = false;
		}

		private void OnChangeValueFunction(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ActiveKinematicMode(OICharInfo.KinematicMode.FK, _value, _force: false);
				buttonReflectIK.interactable = _value;
			}
		}

		private void OnChangeValueIndividual(OIBoneInfo.BoneGroup _group, bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ActiveFK(_group, _value);
			}
		}

		private void OnValueChangedSize(float _value)
		{
			if (!base.isUpdateInfo)
			{
				int count = base.ociChar.listBones.Count;
				for (int i = 0; i < count; i++)
				{
					base.ociChar.listBones[i].scaleRate = _value;
				}
			}
		}

		private void OnClickInitSingle(OIBoneInfo.BoneGroup _group)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.InitFKBone(_group);
			}
		}
	}

	[Serializable]
	private class IKInfo : CommonInfo
	{
		public Toggle toggleFunction;

		public Toggle toggleAll;

		public Toggle toggleBody;

		public Toggle toggleRightHand;

		public Toggle toggleLeftHand;

		public Toggle toggleRightLeg;

		public Toggle toggleLeftLeg;

		public Slider sliderSize;

		public Button buttonAnime;

		public Button buttonReflectFK;

		public Button[] buttonAnimeSingle;

		[Space]
		public Toggle toggleVisible;

		private Toggle[] array;

		public override void Init()
		{
			base.Init();
			toggleFunction.onValueChanged.AddListener(OnChangeValueFunction);
			toggleAll.onValueChanged.AddListener(OnValueChangedAll);
			toggleBody.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(0, b);
			});
			toggleRightLeg.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(1, b);
			});
			toggleLeftLeg.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(2, b);
			});
			toggleRightHand.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(3, b);
			});
			toggleLeftHand.onValueChanged.AddListener(delegate(bool b)
			{
				OnChangeValueIndividual(4, b);
			});
			sliderSize.onValueChanged.AddListener(OnValueChangedSize);
			toggleVisible.gameObject.SetActive(value: false);
			array = new Toggle[5] { toggleBody, toggleRightLeg, toggleLeftLeg, toggleRightHand, toggleLeftHand };
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			toggleFunction.isOn = base.ociChar.oiCharInfo.enableIK;
			bool flag = false;
			for (int i = 0; i < 5; i++)
			{
				array[i].isOn = base.ociChar.oiCharInfo.activeIK[i];
				flag |= base.ociChar.oiCharInfo.activeIK[i];
			}
			toggleAll.isOn = flag;
			buttonReflectFK.interactable = base.ociChar.oiCharInfo.enableIK;
			toggleAll.interactable = base.ociChar.oiCharInfo.enableIK;
			for (int j = 0; j < 5; j++)
			{
				array[j].interactable = base.ociChar.oiCharInfo.enableIK;
			}
			base.isUpdateInfo = false;
		}

		private void OnChangeValueFunction(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ActiveKinematicMode(OICharInfo.KinematicMode.IK, _value, _force: false);
				buttonReflectFK.interactable = _value;
				toggleAll.interactable = _value;
				for (int i = 0; i < 5; i++)
				{
					array[i].interactable = _value;
				}
			}
		}

		private void OnValueChangedAll(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				for (int i = 0; i < 5; i++)
				{
					array[i].isOn = _value;
				}
			}
		}

		private void OnChangeValueIndividual(int _no, bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ActiveIK((OIBoneInfo.BoneGroup)(1 << _no), _value);
				base.isUpdateInfo = true;
				bool flag = false;
				for (int i = 0; i < 5; i++)
				{
					flag |= base.ociChar.oiCharInfo.activeIK[i];
				}
				toggleAll.isOn = flag;
				base.isUpdateInfo = false;
			}
		}

		private void OnValueChangedSize(float _value)
		{
			if (!base.isUpdateInfo)
			{
				int count = base.ociChar.listIKTarget.Count;
				for (int i = 0; i < count; i++)
				{
					base.ociChar.listIKTarget[i].scaleRate = _value;
				}
			}
		}
	}

	[Serializable]
	private class LookAtInfo : CommonInfo
	{
		public Button[] buttonMode;

		public Slider sliderSize;

		public override void Init()
		{
			base.Init();
			for (int i = 0; i < buttonMode.Length; i++)
			{
				int no = i;
				buttonMode[i].onClick.AddListener(delegate
				{
					OnClick(no);
				});
			}
			sliderSize.onValueChanged.AddListener(OnVauleChangeSize);
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			int eyesLookPtn = base.ociChar.charFileStatus.eyesLookPtn;
			for (int i = 0; i < buttonMode.Length; i++)
			{
				buttonMode[i].image.color = ((i == eyesLookPtn) ? Color.green : Color.white);
			}
			sliderSize.value = _char.lookAtInfo.guideObject.scaleRate;
			sliderSize.interactable = _char.charFileStatus.eyesLookPtn == 4;
			base.isUpdateInfo = false;
		}

		private void OnClick(int _no)
		{
			int eyesLookPtn = base.ociChar.charFileStatus.eyesLookPtn;
			base.ociChar.ChangeLookEyesPtn(_no);
			sliderSize.interactable = _no == 4;
			buttonMode[eyesLookPtn].image.color = Color.white;
			buttonMode[_no].image.color = Color.green;
		}

		private void OnVauleChangeSize(float _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.lookAtInfo.guideObject.scaleRate = _value;
			}
		}
	}

	[Serializable]
	private class NeckInfo : CommonInfo
	{
		public Button[] buttonMode;

		private int[] patterns = new int[4] { 0, 1, 3, 4 };

		public override void Init()
		{
			base.Init();
			for (int i = 0; i < buttonMode.Length; i++)
			{
				int no = i;
				buttonMode[i].onClick.AddListener(delegate
				{
					OnClick(no);
				});
			}
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			int no = base.ociChar.charFileStatus.neckLookPtn;
			no = Array.FindIndex(patterns, (int v) => v == no);
			for (int num = 0; num < buttonMode.Length; num++)
			{
				buttonMode[num].image.color = ((num == no) ? Color.green : Color.white);
				buttonMode[num].interactable = !_char.oiCharInfo.enableFK || !_char.oiCharInfo.activeFK[1];
			}
			base.isUpdateInfo = false;
		}

		private void OnClick(int _idx)
		{
			int old = base.ociChar.charFileStatus.neckLookPtn;
			old = Array.FindIndex(patterns, (int v) => v == old);
			base.ociChar.ChangeLookNeckPtn(patterns[_idx]);
			buttonMode[old].image.color = Color.white;
			buttonMode[_idx].image.color = Color.green;
		}
	}

	[Serializable]
	public class PatternInfo
	{
		public delegate void OnClickFunc(int _no);

		public Button[] buttons = new Button[2];

		public TextMeshProUGUI textPtn;

		private int m_Ptn = -1;

		public OnClickFunc onClickFunc;

		public int ptn
		{
			get
			{
				return m_Ptn;
			}
			set
			{
				if (Utility.SetStruct(ref m_Ptn, value))
				{
					textPtn.text = $"{m_Ptn:0}";
				}
			}
		}

		public int num { get; set; }

		public void Init()
		{
			buttons[0].onClick.AddListener(delegate
			{
				OnClick(-1);
			});
			buttons[1].onClick.AddListener(delegate
			{
				OnClick(1);
			});
		}

		private void OnClick(int _add)
		{
			int num = m_Ptn + _add;
			ptn = ((num < 0) ? (this.num - 1) : (num % this.num));
			if (onClickFunc != null)
			{
				onClickFunc(m_Ptn);
			}
		}
	}

	[Serializable]
	private class EtcInfo : CommonInfo
	{
		public PatternInfo piEyebrows = new PatternInfo();

		public PatternInfo piEyes = new PatternInfo();

		public Slider sliderEyesOpen;

		public Toggle toggleBlink;

		public PatternInfo piMouth = new PatternInfo();

		public Slider sliderMouthOpen;

		public Toggle toggleLipSync;

		private int[] eyebrowsKeys;

		private int[] eyesKeys;

		public override void Init()
		{
			base.Init();
			piEyebrows.Init();
			PatternInfo patternInfo = piEyebrows;
			patternInfo.onClickFunc = (PatternInfo.OnClickFunc)Delegate.Combine(patternInfo.onClickFunc, new PatternInfo.OnClickFunc(ChangeEyebrowsPtn));
			piEyes.Init();
			PatternInfo patternInfo2 = piEyes;
			patternInfo2.onClickFunc = (PatternInfo.OnClickFunc)Delegate.Combine(patternInfo2.onClickFunc, new PatternInfo.OnClickFunc(ChangeEyesPtn));
			sliderEyesOpen.onValueChanged.AddListener(OnValueChangedEyesOpen);
			toggleBlink.onValueChanged.AddListener(OnValueChangedEyesBlink);
			piMouth.Init();
			PatternInfo patternInfo3 = piMouth;
			patternInfo3.onClickFunc = (PatternInfo.OnClickFunc)Delegate.Combine(patternInfo3.onClickFunc, new PatternInfo.OnClickFunc(ChangeMouthPtn));
			sliderMouthOpen.onValueChanged.AddListener(OnValueChangedMouthOpen);
			toggleLipSync.onValueChanged.AddListener(OnValueChangedLipSync);
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			_ = _char.charInfo.eyebrowCtrl;
			Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo((_char.sex == 0) ? ChaListDefine.CategoryNo.custom_eyebrow_m : ChaListDefine.CategoryNo.custom_eyebrow_f);
			eyebrowsKeys = categoryInfo.Keys.ToArray();
			piEyebrows.num = categoryInfo.Count;
			piEyebrows.ptn = Mathf.Clamp(Array.FindIndex(eyebrowsKeys, (int _i) => _i == _char.charInfo.GetEyebrowPtn()), 0, eyebrowsKeys.Length - 1);
			_ = _char.charInfo.eyesCtrl;
			Dictionary<int, ListInfoBase> categoryInfo2 = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo((_char.sex == 0) ? ChaListDefine.CategoryNo.custom_eye_m : ChaListDefine.CategoryNo.custom_eye_f);
			eyesKeys = categoryInfo2.Keys.ToArray();
			piEyes.num = categoryInfo2.Count;
			piEyes.ptn = Mathf.Clamp(Array.FindIndex(eyesKeys, (int _i) => _i == _char.charInfo.GetEyesPtn()), 0, eyesKeys.Length - 1);
			sliderEyesOpen.value = _char.charFileStatus.eyesOpenMax;
			toggleBlink.isOn = _char.charFileStatus.eyesBlink;
			FBSCtrlMouth mouthCtrl = _char.charInfo.mouthCtrl;
			piMouth.num = mouthCtrl.GetMaxPtn();
			piMouth.ptn = _char.charInfo.GetMouthPtn();
			sliderMouthOpen.value = mouthCtrl.FixedRate;
			toggleLipSync.isOn = _char.oiCharInfo.lipSync;
			base.isUpdateInfo = false;
		}

		private void ChangeEyebrowsPtn(int _no)
		{
			base.ociChar.charInfo.ChangeEyebrowPtn(eyebrowsKeys[Mathf.Clamp(_no, 0, eyebrowsKeys.Length - 1)]);
		}

		private void ChangeEyesPtn(int _no)
		{
			base.ociChar.charInfo.ChangeEyesPtn(eyesKeys[Mathf.Clamp(_no, 0, eyesKeys.Length - 1)]);
		}

		private void OnValueChangedEyesOpen(float _value)
		{
			base.ociChar.ChangeEyesOpen(_value);
		}

		private void OnValueChangedEyesBlink(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ChangeBlink(_value);
			}
		}

		private void ChangeMouthPtn(int _no)
		{
			base.ociChar.ChangeMouthPtn(_no);
		}

		private void OnValueChangedMouthOpen(float _value)
		{
			base.ociChar.ChangeMouthOpen(_value);
		}

		private void OnValueChangedLipSync(bool _value)
		{
			if (!base.isUpdateInfo)
			{
				base.ociChar.ChangeLipSync(_value);
			}
		}
	}

	[Serializable]
	private class HandInfo : CommonInfo
	{
		public PatternInfo piRightHand = new PatternInfo();

		public PatternInfo piLeftHand = new PatternInfo();

		public override void Init()
		{
			base.Init();
			piRightHand.Init();
			PatternInfo patternInfo = piRightHand;
			patternInfo.onClickFunc = (PatternInfo.OnClickFunc)Delegate.Combine(patternInfo.onClickFunc, new PatternInfo.OnClickFunc(ChangeRightHandAnime));
			piLeftHand.Init();
			PatternInfo patternInfo2 = piLeftHand;
			patternInfo2.onClickFunc = (PatternInfo.OnClickFunc)Delegate.Combine(patternInfo2.onClickFunc, new PatternInfo.OnClickFunc(ChangeLeftHandAnime));
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			piRightHand.num = _char.HandAnimeNum + 1;
			piRightHand.ptn = _char.oiCharInfo.handPtn[1];
			piLeftHand.num = _char.HandAnimeNum + 1;
			piLeftHand.ptn = _char.oiCharInfo.handPtn[0];
			base.isUpdateInfo = false;
		}

		private void ChangeLeftHandAnime(int _no)
		{
			base.ociChar.ChangeHandAnime(0, _no);
		}

		private void ChangeRightHandAnime(int _no)
		{
			base.ociChar.ChangeHandAnime(1, _no);
		}
	}

	[Serializable]
	private class PoseInfo : CommonInfo
	{
		public PauseRegistrationList pauseRegistrationList;

		public override bool active
		{
			set
			{
				pauseRegistrationList.active = value;
			}
		}

		public override void Init()
		{
			base.Init();
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			pauseRegistrationList.ociChar = _char;
			base.isUpdateInfo = false;
		}
	}

	[Serializable]
	private class CostumeInfo : CommonInfo
	{
		public CharaFileSort fileSort = new CharaFileSort();

		public GameObject prefabNode;

		public RawImage imageThumbnail;

		public Button[] buttonSort;

		public Button buttonLoad;

		private int sex = -1;

		public override void Init()
		{
			base.Init();
			buttonSort[0].onClick.AddListener(delegate
			{
				OnClickSort(0);
			});
			buttonSort[1].onClick.AddListener(delegate
			{
				OnClickSort(1);
			});
			buttonLoad.onClick.AddListener(OnClickLoad);
			sex = -1;
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			InitList(_char.oiCharInfo.sex);
			base.isUpdateInfo = false;
		}

		private void InitList(int _sex)
		{
			if (sex == _sex)
			{
				return;
			}
			fileSort.DeleteAllNode();
			InitFileList(_sex);
			int count = fileSort.cfiList.Count;
			for (int i = 0; i < count; i++)
			{
				CharaFileInfo info = fileSort.cfiList[i];
				info.index = i;
				GameObject gameObject = UnityEngine.Object.Instantiate(prefabNode);
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
				gameObject.transform.SetParent(fileSort.root, worldPositionStays: false);
				info.node = gameObject.GetComponent<ListNode>();
				info.button = gameObject.GetComponent<Button>();
				info.node.AddActionToButton(delegate
				{
					OnSelect(info.index);
				});
				info.node.text = info.name;
				info.node.listEnterAction.Add(delegate
				{
					LoadImage(info.index);
				});
			}
			sex = _sex;
			fileSort.Sort(0, _ascend: false);
			buttonLoad.interactable = false;
			imageThumbnail.color = Color.clear;
		}

		private void InitFileList(int _sex)
		{
			List<string> files = new List<string>();
			Utils.File.GetAllFiles(UserData.Path + ((_sex == 0) ? "coordinate/male/" : "coordinate/female/"), "*.png", ref files);
			fileSort.cfiList.Clear();
			int count = files.Count;
			ChaFileCoordinate chaFileCoordinate = new ChaFileCoordinate();
			for (int i = 0; i < count; i++)
			{
				if (chaFileCoordinate.LoadFile(files[i]))
				{
					fileSort.cfiList.Add(new CharaFileInfo(files[i], chaFileCoordinate.coordinateName)
					{
						time = File.GetLastWriteTime(files[i])
					});
				}
			}
		}

		private void OnSelect(int _idx)
		{
			if (fileSort.select != _idx)
			{
				fileSort.select = _idx;
				buttonLoad.interactable = true;
			}
		}

		private void LoadImage(int _idx)
		{
			CharaFileInfo charaFileInfo = fileSort.cfiList[_idx];
			imageThumbnail.texture = PngAssist.LoadTexture(charaFileInfo.file);
			imageThumbnail.color = Color.white;
			UnityEngine.Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		private void OnClickLoad()
		{
			base.ociChar.LoadClothesFile(fileSort.selectPath);
		}

		private void OnClickSort(int _type)
		{
			fileSort.select = -1;
			buttonLoad.interactable = false;
			fileSort.Sort(_type);
		}
	}

	[Serializable]
	private class JointInfo : CommonInfo
	{
		public Toggle[] toggles;

		public override void Init()
		{
			base.Init();
			for (int i = 0; i < toggles.Length; i++)
			{
				int idx = i;
				toggles[i].onValueChanged.AddListener(delegate(bool b)
				{
					OnValueChanged(idx, b);
				});
			}
		}

		public override void UpdateInfo(OCIChar _char)
		{
			base.UpdateInfo(_char);
			base.isUpdateInfo = true;
			for (int i = 0; i < toggles.Length; i++)
			{
				toggles[i].isOn = base.ociChar.oiCharInfo.expression[i];
			}
			base.isUpdateInfo = false;
		}

		private void OnValueChanged(int _group, bool _value)
		{
			base.ociChar.EnableExpressionCategory(_group, _value);
		}
	}

	[SerializeField]
	private RootButtonInfo[] rootButtonInfo;

	[SerializeField]
	private AnimeGroupList animeGroupList;

	[SerializeField]
	private AnimeControl animeControl;

	[SerializeField]
	private VoiceControl voiceControl;

	[SerializeField]
	private StateInfo stateInfo = new StateInfo();

	[SerializeField]
	private FKInfo fkInfo = new FKInfo();

	[SerializeField]
	private IKInfo ikInfo = new IKInfo();

	[SerializeField]
	private LookAtInfo lookAtInfo = new LookAtInfo();

	[SerializeField]
	private NeckInfo neckInfo = new NeckInfo();

	[SerializeField]
	private PoseInfo poseInfo = new PoseInfo();

	[SerializeField]
	private EtcInfo etcInfo = new EtcInfo();

	[SerializeField]
	private HandInfo handInfo = new HandInfo();

	[SerializeField]
	private Button[] buttonKinematic;

	[SerializeField]
	private CostumeInfo costumeInfo = new CostumeInfo();

	[SerializeField]
	private JointInfo jointInfo = new JointInfo();

	private OCIChar m_OCIChar;

	private int kinematic = -1;

	private int select = -1;

	private SingleAssignmentDisposable disposableFK;

	private SingleAssignmentDisposable disposableIK;

	public OCIChar ociChar
	{
		get
		{
			return m_OCIChar;
		}
		set
		{
			m_OCIChar = value;
			if (m_OCIChar != null)
			{
				UpdateInfo();
			}
		}
	}

	public bool active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			if (base.gameObject.activeSelf != value)
			{
				base.gameObject.SetActive(value);
				if (!base.gameObject.activeSelf)
				{
					OnClickRoot(-1);
				}
			}
		}
	}

	public void OnClickRoot(int _idx)
	{
		select = _idx;
		for (int i = 0; i < rootButtonInfo.Length; i++)
		{
			rootButtonInfo[i].active = i == _idx;
		}
		animeControl.active = _idx == 2;
		voiceControl.active = _idx == 3;
		switch (_idx)
		{
		case 0:
			stateInfo.UpdateInfo(m_OCIChar);
			break;
		case 1:
			fkInfo.UpdateInfo(m_OCIChar);
			ikInfo.UpdateInfo(m_OCIChar);
			lookAtInfo.UpdateInfo(m_OCIChar);
			neckInfo.UpdateInfo(m_OCIChar);
			poseInfo.UpdateInfo(m_OCIChar);
			etcInfo.UpdateInfo(m_OCIChar);
			handInfo.UpdateInfo(m_OCIChar);
			break;
		case 2:
			animeGroupList.InitList((AnimeGroupList.SEX)m_OCIChar.oiCharInfo.sex);
			animeControl.objectCtrlInfo = m_OCIChar;
			break;
		case 3:
			voiceControl.ociChar = m_OCIChar;
			break;
		case 4:
			costumeInfo.UpdateInfo(m_OCIChar);
			break;
		case 5:
			jointInfo.UpdateInfo(m_OCIChar);
			break;
		}
	}

	public void OnClickKinematic(int _idx)
	{
		if (kinematic != _idx)
		{
			CommonInfo[] array = new CommonInfo[8] { fkInfo, ikInfo, lookAtInfo, neckInfo, null, etcInfo, handInfo, poseInfo };
			if (MathfEx.RangeEqualOn(0, kinematic, array.Length - 1) && array[kinematic] != null)
			{
				array[kinematic].active = false;
				buttonKinematic[kinematic].image.color = Color.white;
			}
			kinematic = _idx;
			if (array[kinematic] != null)
			{
				array[kinematic].active = true;
				array[kinematic].UpdateInfo(m_OCIChar);
				buttonKinematic[kinematic].image.color = Color.green;
			}
		}
	}

	public void LoadAnime(AnimeGroupList.SEX _sex, int _group, int _category, int _no)
	{
		m_OCIChar.LoadAnime(_group, _category, _no);
		animeControl.UpdateInfo();
	}

	public bool Deselect(OCIChar _ociChar)
	{
		if (m_OCIChar != _ociChar)
		{
			return false;
		}
		ociChar = null;
		active = false;
		return true;
	}

	private void UpdateInfo()
	{
		OnClickRoot(select);
	}

	private void SetCopyBoneFK(OIBoneInfo.BoneGroup _group)
	{
		if (disposableFK != null)
		{
			disposableFK.Dispose();
			disposableFK = null;
		}
		disposableFK = new SingleAssignmentDisposable();
		disposableFK.Disposable = this.LateUpdateAsObservable().Take(1).Subscribe((Action<Unit>)delegate
		{
			CopyBoneFK(_group);
		}, (Action)delegate
		{
			disposableFK.Dispose();
			disposableFK = null;
		});
	}

	private void SetCopyBoneIK(OIBoneInfo.BoneGroup _group)
	{
		if (disposableIK != null)
		{
			disposableIK.Dispose();
			disposableIK = null;
		}
		disposableIK = new SingleAssignmentDisposable();
		disposableIK.Disposable = this.LateUpdateAsObservable().Take(1).Subscribe((Action<Unit>)delegate
		{
			CopyBoneIK(_group);
		}, (Action)delegate
		{
			disposableIK.Dispose();
			disposableIK = null;
		});
	}

	private void CopyBoneFK(OIBoneInfo.BoneGroup _group)
	{
		if (m_OCIChar != null)
		{
			m_OCIChar.fkCtrl.CopyBone(_group);
		}
	}

	private void CopyBoneIK(OIBoneInfo.BoneGroup _group)
	{
		if (m_OCIChar != null)
		{
			m_OCIChar.ikCtrl.CopyBone(_group);
		}
	}

	private void Awake()
	{
		fkInfo.Init();
		fkInfo.buttonAnime.onClick.AddListener(delegate
		{
			SetCopyBoneFK((OIBoneInfo.BoneGroup)353);
		});
		fkInfo.buttonAnimeSingle[0].onClick.AddListener(delegate
		{
			SetCopyBoneFK(OIBoneInfo.BoneGroup.Body);
		});
		fkInfo.buttonAnimeSingle[1].onClick.AddListener(delegate
		{
			SetCopyBoneFK(OIBoneInfo.BoneGroup.Neck);
		});
		fkInfo.buttonAnimeSingle[2].onClick.AddListener(delegate
		{
			SetCopyBoneFK(OIBoneInfo.BoneGroup.LeftHand);
		});
		fkInfo.buttonAnimeSingle[3].onClick.AddListener(delegate
		{
			SetCopyBoneFK(OIBoneInfo.BoneGroup.RightHand);
		});
		fkInfo.buttonReflectIK.onClick.AddListener(delegate
		{
			CopyBoneIK((OIBoneInfo.BoneGroup)31);
		});
		ikInfo.Init();
		ikInfo.buttonAnime.onClick.AddListener(delegate
		{
			SetCopyBoneIK((OIBoneInfo.BoneGroup)31);
		});
		ikInfo.buttonAnimeSingle[0].onClick.AddListener(delegate
		{
			SetCopyBoneIK(OIBoneInfo.BoneGroup.Body);
		});
		ikInfo.buttonAnimeSingle[1].onClick.AddListener(delegate
		{
			SetCopyBoneIK(OIBoneInfo.BoneGroup.LeftArm);
		});
		ikInfo.buttonAnimeSingle[2].onClick.AddListener(delegate
		{
			SetCopyBoneIK(OIBoneInfo.BoneGroup.RightArm);
		});
		ikInfo.buttonAnimeSingle[3].onClick.AddListener(delegate
		{
			SetCopyBoneIK(OIBoneInfo.BoneGroup.LeftLeg);
		});
		ikInfo.buttonAnimeSingle[4].onClick.AddListener(delegate
		{
			SetCopyBoneIK(OIBoneInfo.BoneGroup.RightLeg);
		});
		ikInfo.buttonReflectFK.onClick.AddListener(delegate
		{
			CopyBoneFK((OIBoneInfo.BoneGroup)353);
		});
		stateInfo.Init();
		lookAtInfo.Init();
		neckInfo.Init();
		etcInfo.Init();
		handInfo.Init();
		costumeInfo.Init();
		jointInfo.Init();
		select = -1;
	}
}
