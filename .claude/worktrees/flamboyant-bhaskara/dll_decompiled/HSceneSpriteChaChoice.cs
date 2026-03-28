using System.Collections.Generic;
using AIChara;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HSceneSpriteChaChoice : MonoBehaviour
{
	private ChaControl[] females;

	private ChaControl[] males;

	private HScene hScene;

	private HSceneManager hSceneManager;

	[SerializeField]
	private Dropdown dropdown;

	[SerializeField]
	private PointerAction action;

	private bool[] maleActive = new bool[2];

	private bool[] femaleActive = new bool[2];

	private HDropdownCharChoiceTemplate template;

	public ChaControl[] Females => females;

	public ChaControl[] Males => males;

	public bool[] MaleActive => maleActive;

	public bool[] FeMaleActive => femaleActive;

	public void Init()
	{
		hScene = Singleton<HSceneFlagCtrl>.Instance.GetComponent<HScene>();
		hSceneManager = Singleton<HSceneManager>.Instance;
		females = hScene.GetFemales();
		males = hScene.GetMales();
		dropdown.ClearOptions();
		List<string> list = new List<string>();
		ChaControl[] array = females;
		foreach (ChaControl chaControl in array)
		{
			if (!(chaControl == null) && chaControl.fileParam != null)
			{
				list.Add(chaControl.fileParam.fullname);
			}
		}
		array = males;
		foreach (ChaControl chaControl2 in array)
		{
			if (!(chaControl2 == null) && chaControl2.fileParam != null)
			{
				list.Add(chaControl2.fileParam.fullname);
			}
		}
		dropdown.AddOptions(list);
		dropdown.onValueChanged.RemoveAllListeners();
		dropdown.onValueChanged.AddListener(delegate(int val)
		{
			if (template != null)
			{
				hSceneManager.numFemaleClothCustom = template.Nodes[val].ChoiseNo;
			}
		});
	}

	public void SetDownAction(UnityAction addAct)
	{
		if (!(action == null) && !action.listDownAction.Contains(addAct))
		{
			action.listDownAction.Add(addAct);
		}
	}

	public void SetAction(UnityAction action)
	{
		dropdown.onValueChanged.AddListener(delegate
		{
			action();
		});
	}

	public void ChangeChaOptions(HScene.AnimationListInfo info, int ClothKind)
	{
		femaleActive[0] = !info.fileFemale.IsNullOrEmpty();
		femaleActive[1] = !info.fileFemale2.IsNullOrEmpty();
		if (ClothKind == 2)
		{
			if (info.fileMale.IsNullOrEmpty())
			{
				maleActive[0] = false;
			}
			if (info.fileMale2.IsNullOrEmpty())
			{
				maleActive[1] = false;
			}
		}
		else
		{
			maleActive[0] = false;
			maleActive[1] = false;
		}
		bool flag = ((hSceneManager.numFemaleClothCustom < 2) ? femaleActive[hSceneManager.numFemaleClothCustom] : maleActive[hSceneManager.numFemaleClothCustom - 2]);
		if (hSceneManager.numFemaleClothCustom != 0 && !flag)
		{
			hSceneManager.numFemaleClothCustom = 0;
			dropdown.value = 0;
		}
	}

	public void SetMale(bool val)
	{
		HScene.AnimationListInfo animationListInfo = (Singleton<HSceneFlagCtrl>.IsInstance() ? Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo : null);
		if (animationListInfo != null)
		{
			if (!animationListInfo.fileMale.IsNullOrEmpty())
			{
				maleActive[0] = val;
			}
			else
			{
				maleActive[0] = false;
			}
			if (!animationListInfo.fileMale2.IsNullOrEmpty())
			{
				maleActive[1] = val;
			}
			else
			{
				maleActive[1] = false;
			}
			bool flag = ((hSceneManager.numFemaleClothCustom < 2) ? femaleActive[hSceneManager.numFemaleClothCustom] : maleActive[hSceneManager.numFemaleClothCustom - 2]);
			if (hSceneManager.numFemaleClothCustom != 0 && !flag)
			{
				hSceneManager.numFemaleClothCustom = 0;
				dropdown.value = 0;
			}
		}
	}

	public void SetTemplate(HDropdownCharChoiceTemplate _template)
	{
		template = _template;
	}
}
