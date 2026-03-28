using System;
using System.Collections;
using System.Collections.Generic;
using AIChara;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomClothesPatternSelect : MonoBehaviour
{
	[SerializeField]
	private CustomSelectScrollController sscPattern;

	[SerializeField]
	private Button btnClose;

	private CanvasGroup canvasGroup;

	public Action<int, int> onSelect;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl => customBase.chaCtrl;

	private ChaFileClothes nowClothes => chaCtrl.nowCoordinate.clothes;

	private ChaFileClothes orgClothes => chaCtrl.chaFile.coordinate.clothes;

	private ChaFileHair hair => chaCtrl.fileHair;

	public int type { get; set; } = -1;

	public int parts { get; set; } = -1;

	public int idx { get; set; } = -1;

	public void ChangeLink(int _type, int _parts, int _idx = 0)
	{
		type = _type;
		parts = _parts;
		idx = _idx;
		if (-1 == type || -1 == parts || -1 == idx)
		{
			return;
		}
		ReCreateList(type);
		if (type == 0)
		{
			sscPattern.SetToggleID(nowClothes.parts[parts].colorInfo[idx].pattern);
		}
		else
		{
			sscPattern.SetToggleID(hair.parts[parts].meshType);
		}
		sscPattern.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null)
			{
				if (type == 0)
				{
					if (nowClothes.parts[parts].colorInfo[idx].pattern != info.id)
					{
						nowClothes.parts[parts].colorInfo[idx].pattern = info.id;
						orgClothes.parts[parts].colorInfo[idx].pattern = info.id;
						chaCtrl.ChangeCustomClothes(parts, updateColor: false, idx == 0, 1 == idx, 2 == idx);
						onSelect?.Invoke(parts, idx);
					}
				}
				else if (hair.parts[parts].meshType != info.id)
				{
					hair.parts[parts].meshType = info.id;
					chaCtrl.ChangeSettingHairMeshType(parts);
					onSelect?.Invoke(parts, idx);
				}
			}
		};
	}

	public void ReCreateList(int type)
	{
		List<CustomSelectInfo> list = null;
		list = ((type != 0) ? CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_hairmeshptn) : CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_pattern));
		sscPattern.CreateList(list);
	}

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				customBase.customCtrl.showPattern = false;
			});
		}
	}
}
