using System.Collections.Generic;
using AIChara;
using Manager;
using SceneAssist;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HSceneSpriteClothCondition : HSceneSpriteCategory
{
	public HSceneSpriteChaChoice hSceneSpriteChaChoice;

	public List<HSceneSpriteClothBtn> objs = new List<HSceneSpriteClothBtn>();

	public Button[] AllChange;

	private HScene hScene;

	private ChaControl[] femailes;

	private ChaControl[] mailes;

	private int[] allState = new int[4];

	private HSceneSprite hSceneSprite;

	private HSceneManager hSceneManager;

	[SerializeField]
	private PointerDownAction[] downActions;

	public void Init()
	{
		hScene = Singleton<HSceneFlagCtrl>.Instance.GetComponent<HScene>();
		hSceneSprite = Singleton<HSceneSprite>.Instance;
		hSceneManager = Singleton<HSceneManager>.Instance;
		femailes = hScene.GetFemales();
		mailes = hScene.GetMales();
		SetClothCharacter(init: true);
		hSceneSpriteChaChoice.SetAction(delegate
		{
			SetClothCharacter();
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

	public void SetClothCharacter(bool init = false)
	{
		if (!base.gameObject.activeSelf && !init)
		{
			return;
		}
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
		if (!(chaControl.objBodyBone != null) || !chaControl.visibleAll)
		{
			return;
		}
		int num = -1;
		bool flag = true;
		for (int i = 0; i < 8; i++)
		{
			objs[i].SetButton(chaControl.fileStatus.clothesState[i]);
			bool flag2 = chaControl.IsClothesStateKind(i);
			SetActive(flag2, i);
			if (flag2 && flag)
			{
				if (num < 0)
				{
					num = chaControl.fileStatus.clothesState[i];
				}
				else
				{
					flag = num == chaControl.fileStatus.clothesState[i];
				}
			}
		}
		if (!flag)
		{
			allState[hSceneManager.numFemaleClothCustom] = 1;
		}
		else if (num >= 0)
		{
			allState[hSceneManager.numFemaleClothCustom] = num;
			allState[hSceneManager.numFemaleClothCustom] %= 3;
		}
		for (int j = 0; j < AllChange.Length; j++)
		{
			if (allState[hSceneManager.numFemaleClothCustom] != j)
			{
				AllChange[j].gameObject.SetActive(value: false);
			}
			else
			{
				AllChange[j].gameObject.SetActive(value: true);
			}
		}
	}

	public void OnClickCloth(int _cloth)
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || hSceneSprite.isFade)
		{
			return;
		}
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
		chaControl.SetClothesStateNext(_cloth);
		int num = -1;
		bool flag = true;
		for (int i = 0; i < 8; i++)
		{
			if (!objs[i].gameObject.activeSelf)
			{
				continue;
			}
			objs[i].SetButton(chaControl.fileStatus.clothesState[i]);
			if (flag)
			{
				if (num < 0)
				{
					num = chaControl.fileStatus.clothesState[i];
				}
				else
				{
					flag = num == chaControl.fileStatus.clothesState[i];
				}
			}
		}
		if (!flag)
		{
			allState[hSceneManager.numFemaleClothCustom] = 1;
		}
		else if (num >= 0)
		{
			allState[hSceneManager.numFemaleClothCustom] = num;
			allState[hSceneManager.numFemaleClothCustom] %= 3;
		}
		for (int j = 0; j < AllChange.Length; j++)
		{
			if (allState[hSceneManager.numFemaleClothCustom] != j)
			{
				AllChange[j].gameObject.SetActive(value: false);
			}
			else
			{
				AllChange[j].gameObject.SetActive(value: true);
			}
		}
	}

	public void OnClickAllCloth()
	{
		if (Scene.IsNowLoading || Scene.IsNowLoadingFade || hSceneSprite.isFade)
		{
			return;
		}
		allState[hSceneManager.numFemaleClothCustom]++;
		allState[hSceneManager.numFemaleClothCustom] %= 3;
		ChaControl chaControl = ((hSceneManager.numFemaleClothCustom < 2) ? femailes[hSceneManager.numFemaleClothCustom] : mailes[hSceneManager.numFemaleClothCustom - 2]);
		chaControl.SetClothesStateAll((byte)allState[hSceneManager.numFemaleClothCustom]);
		for (int i = 0; i < AllChange.Length; i++)
		{
			if (allState[hSceneManager.numFemaleClothCustom] != i)
			{
				AllChange[i].gameObject.SetActive(value: false);
			}
			else
			{
				AllChange[i].gameObject.SetActive(value: true);
			}
		}
		for (int j = 0; j < 8; j++)
		{
			objs[j].SetButton(chaControl.fileStatus.clothesState[j]);
		}
	}

	public override void SetActive(bool _active, int _array = -1)
	{
		if (_array < 0)
		{
			for (int i = 0; i < objs.Count; i++)
			{
				if (objs[i].gameObject.activeSelf != _active)
				{
					objs[i].gameObject.SetActive(_active);
				}
			}
		}
		else if (objs.Count > _array && objs[_array].gameObject.activeSelf != _active)
		{
			objs[_array].gameObject.SetActive(_active);
		}
	}
}
