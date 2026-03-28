using System.Text;
using AIChara;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HSceneSpriteAccessoryCondition : MonoBehaviour
{
	public HSceneSpriteChaChoice hSceneSpriteChaChoice;

	public HSceneSpriteToggleCategory AccessorySlots;

	public Toggle AllChange;

	private HScene hScene;

	private ChaControl[] females;

	private ChaControl[] males;

	private bool[] allState = new bool[4] { true, true, true, true };

	private HSceneSprite hSceneSprite;

	private HSceneManager hSceneManager;

	private StringBuilder sbAcsName;

	public Color AcsOnColor = Color.white;

	public Color AcsOffColor = Color.gray;

	public PointerDownAction[] downActions;

	public void Init()
	{
		hScene = Singleton<HSceneFlagCtrl>.Instance.GetComponent<HScene>();
		hSceneSprite = Singleton<HSceneSprite>.Instance;
		hSceneManager = Singleton<HSceneManager>.Instance;
		females = hScene.GetFemales();
		males = hScene.GetMales();
		sbAcsName = new StringBuilder();
		SetAccessoryCharacter(init: true);
		hSceneSpriteChaChoice.SetAction(delegate
		{
			SetAccessoryCharacter();
		});
	}

	public void SetDownAction(UnityAction addAct)
	{
		PointerDownAction[] array = downActions;
		foreach (PointerDownAction pointerDownAction in array)
		{
			if (!(pointerDownAction == null) && !pointerDownAction.listAction.Contains(addAct))
			{
				pointerDownAction.listAction.Add(addAct);
			}
		}
	}

	public void SetAccessoryCharacter(bool init = false)
	{
		if (!init && !base.gameObject.activeSelf)
		{
			return;
		}
		Text text = null;
		bool isOn = false;
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? females[hSceneManager.numFemaleClothCustom] : males[hSceneManager.numFemaleClothCustom - 2]);
		for (int i = 0; i < AccessorySlots.GetToggleNum(); i++)
		{
			sbAcsName.Clear();
			int num = i;
			ListInfoBase listInfoBase = chaControl.infoAccessory[num];
			text = AccessorySlots.lstToggle[num].GetComponentInChildren<Text>();
			if (listInfoBase != null)
			{
				sbAcsName.Append(listInfoBase.Name);
				AccessorySlots.lstToggle[num].interactable = true;
				AccessorySlots.lstToggle[num].isOn = chaControl.fileStatus.showAccessory[num];
				if (AccessorySlots.lstToggle[num].isOn)
				{
					text.color = AcsOnColor;
					isOn = true;
				}
				else
				{
					text.color = AcsOffColor;
				}
			}
			else
			{
				sbAcsName.AppendFormat("スロット{0}", num);
				AccessorySlots.lstToggle[num].interactable = false;
				text.color = AcsOffColor;
			}
			text.text = sbAcsName.ToString();
		}
		AccessorySlots.SetActive(chaControl.objBodyBone != null && chaControl.visibleAll);
		AllChange.isOn = isOn;
		allState[hSceneManager.numFemaleClothCustom] = AllChange.isOn;
		text = AllChange.GetComponentInChildren<Text>();
		AllChange.interactable = AccessorySlots.GetAllEnable() != 0;
		Color color = text.color;
		color = ((!allState[hSceneManager.numFemaleClothCustom]) ? AcsOffColor : AcsOnColor);
		text.color = color;
	}

	public void OnClickAccessory(int _accessory)
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || hSceneSprite.isFade)
		{
			return;
		}
		for (int i = 0; i < AccessorySlots.GetToggleNum(); i++)
		{
			int num = i;
			if (num == _accessory)
			{
				Text componentInChildren = AccessorySlots.lstToggle[num].GetComponentInChildren<Text>();
				Color color = componentInChildren.color;
				color = ((!AccessorySlots.lstToggle[_accessory].isOn) ? AcsOffColor : AcsOnColor);
				componentInChildren.color = color;
				break;
			}
		}
		((hSceneManager.numFemaleClothCustom < 2) ? females[hSceneManager.numFemaleClothCustom] : males[hSceneManager.numFemaleClothCustom - 2]).SetAccessoryState(_accessory, AccessorySlots.lstToggle[_accessory].isOn);
	}

	public void OnClickAllAccessory()
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || hSceneSprite.isFade)
		{
			return;
		}
		allState[hSceneManager.numFemaleClothCustom] = AllChange.isOn;
		for (int i = 0; i < AccessorySlots.lstToggle.Count; i++)
		{
			if (!GlobalMethod.StartsWith(AccessorySlots.lstToggle[i].GetComponentInChildren<Text>().text, "スロット"))
			{
				AccessorySlots.SetCheck(AllChange.isOn, i);
			}
		}
		Text componentInChildren = AllChange.GetComponentInChildren<Text>();
		Color color = componentInChildren.color;
		color = ((!allState[hSceneManager.numFemaleClothCustom]) ? AcsOffColor : AcsOnColor);
		componentInChildren.color = color;
		((hSceneManager.numFemaleClothCustom < 2) ? females[hSceneManager.numFemaleClothCustom] : males[hSceneManager.numFemaleClothCustom - 2]).SetAccessoryStateAll(allState[hSceneManager.numFemaleClothCustom]);
	}
}
