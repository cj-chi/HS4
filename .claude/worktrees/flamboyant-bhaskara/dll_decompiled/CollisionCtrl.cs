using System;
using System.Collections.Generic;
using System.Text;
using AIChara;
using AIProject;
using Illusion.CustomAttributes;
using Manager;
using Obi;
using UnityEngine;

public class CollisionCtrl : MonoBehaviour
{
	[Serializable]
	public struct CollisionInfo
	{
		public string nameAnimation;

		public List<bool> lstIsActive;
	}

	public List<CollisionInfo> lstInfo = new List<CollisionInfo>();

	public List<GameObject> lstObj = new List<GameObject>();

	public Dictionary<int, ObiCollider> lstObjObiCol = new Dictionary<int, ObiCollider>();

	public ChaControl chaFemale;

	[DisabledGroup("表示")]
	public bool isActive = true;

	private ExcelData excelData;

	private StringBuilder sbAbName = new StringBuilder();

	private List<string> row = new List<string>();

	private void Update()
	{
		if ((bool)chaFemale)
		{
			Proc(chaFemale.getAnimatorStateInfo(0));
		}
	}

	public bool Init(ChaControl _female, GameObject _objHitHead, GameObject _objHitBody)
	{
		Release();
		chaFemale = _female;
		List<GameObject> headCollisionComponent = GetHeadCollisionComponent(_objHitHead);
		if (headCollisionComponent != null)
		{
			lstObj.AddRange(headCollisionComponent);
		}
		else
		{
			lstObj.Add(null);
		}
		if ((bool)_objHitBody)
		{
			HitCollision componentInChildren = _objHitBody.GetComponentInChildren<HitCollision>();
			if ((bool)componentInChildren)
			{
				lstObj.AddRange(componentInChildren.lstObj);
			}
		}
		lstObjObiCol = new Dictionary<int, ObiCollider>();
		for (int i = 0; i < lstObj.Count; i++)
		{
			if (!(lstObj[i] == null))
			{
				lstObjObiCol.Add(i, lstObj[i].GetComponent<ObiCollider>());
			}
		}
		return true;
	}

	private List<GameObject> GetHeadCollisionComponent(GameObject _objHitHead)
	{
		if (_objHitHead == null)
		{
			return null;
		}
		HitCollision componentInChildren = _objHitHead.GetComponentInChildren<HitCollision>();
		if (componentInChildren == null)
		{
			return null;
		}
		if (componentInChildren.lstObj.Count == 0)
		{
			return null;
		}
		return componentInChildren.lstObj;
	}

	public void Release()
	{
		lstObj.Clear();
		lstInfo = new List<CollisionInfo>();
		isActive = true;
		lstObjObiCol.Clear();
	}

	public void LoadExcel(string _file)
	{
		if (_file == "")
		{
			return;
		}
		lstInfo = new List<CollisionInfo>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetCollisionListFolder);
		assetBundleNameListFromPath.Sort();
		CollisionInfo item = default(CollisionInfo);
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				continue;
			}
			sbAbName.Clear();
			sbAbName.Append(assetBundleNameListFromPath[i]);
			if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), _file))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), _file);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(sbAbName.ToString());
			if (excelData == null)
			{
				continue;
			}
			int num = 1;
			List<CollisionInfo> list = new List<CollisionInfo>();
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				item.nameAnimation = row.GetElement(num2++);
				item.lstIsActive = new List<bool>();
				while (num2 < row.Count)
				{
					item.lstIsActive.Add(row.GetElement(num2++) == "1");
				}
				list.Add(item);
			}
			lstInfo = list;
		}
		excelData = null;
		row = null;
	}

	private bool Proc(AnimatorStateInfo _ai)
	{
		for (int i = 0; i < lstInfo.Count; i++)
		{
			if (!_ai.IsName(lstInfo[i].nameAnimation.ToString()))
			{
				continue;
			}
			Visible(_visible: true);
			for (int j = 0; j < lstObj.Count; j++)
			{
				if (lstObj[j] == null || lstObj[j].activeSelf == lstInfo[i].lstIsActive[j])
				{
					continue;
				}
				if (lstInfo[i].lstIsActive[j])
				{
					ObiCollider obiCollider = lstObjObiCol[j];
					if (obiCollider != null)
					{
						if (lstInfo[i].lstIsActive[j])
						{
							obiCollider.Phase = 0;
						}
						else
						{
							obiCollider.Phase = 1;
						}
					}
				}
				else
				{
					SetObiColliderPhase(lstObj[j], lstObjObiCol[j]);
				}
				lstObj[j].SetActive(lstInfo[i].lstIsActive[j]);
			}
			return true;
		}
		Visible(_visible: false);
		return false;
	}

	private void SetObiColliderPhase(GameObject obj, ObiCollider col)
	{
		if (!(obj == null) && col != null)
		{
			col.Phase = 1;
		}
	}

	private void Visible(bool _visible)
	{
		if (isActive == _visible)
		{
			return;
		}
		for (int i = 0; i < lstObj.Count; i++)
		{
			if (!lstObj[i])
			{
				continue;
			}
			if (_visible)
			{
				ObiCollider obiCollider = lstObjObiCol[i];
				if (obiCollider != null)
				{
					if (_visible)
					{
						obiCollider.Phase = 0;
					}
					else
					{
						obiCollider.Phase = 1;
					}
				}
			}
			else
			{
				SetObiColliderPhase(lstObj[i], lstObjObiCol[i]);
			}
			lstObj[i].SetActive(_visible);
		}
		isActive = _visible;
	}
}
