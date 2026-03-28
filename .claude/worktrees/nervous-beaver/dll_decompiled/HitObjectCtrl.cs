using System.Collections;
using System.Collections.Generic;
using System.Text;
using AIChara;
using AIProject;
using Manager;
using Obi;
using UnityEngine;

public class HitObjectCtrl
{
	public struct CollisionInfo
	{
		public string nameAnimation;

		public List<bool> lstIsActive;
	}

	public bool isInit;

	private bool isActive = true;

	public int id = -1;

	private int sex = -1;

	private ChaControl chaControl;

	private List<string> atariName = new List<string>();

	private List<GameObject> lstObject = new List<GameObject>();

	private Dictionary<int, ObiCollider[]> lstObjectCols = new Dictionary<int, ObiCollider[]>();

	private List<CollisionInfo> lstInfo = new List<CollisionInfo>();

	private AnimatorStateInfo ai;

	private Dictionary<int, Dictionary<string, GameObject>> tmpDic = new Dictionary<int, Dictionary<string, GameObject>>();

	private Dictionary<string, GameObject> tmpLst = new Dictionary<string, GameObject>();

	private string pathAsset;

	private StringBuilder sbAbName = new StringBuilder();

	private ExcelData excelData;

	private List<string> row = new List<string>();

	private static List<string> lstHitObject;

	private Transform[] getChild;

	public IEnumerator HitObjInit(int Sex, GameObject _objBody, ChaControl _custom)
	{
		if (_objBody == null)
		{
			yield break;
		}
		isInit = false;
		isActive = true;
		yield return new WaitUntil(() => Singleton<HSceneManager>.IsInstance());
		if (!HSceneManager.HResourceTables.lstHitObject.ContainsKey(Sex))
		{
			yield break;
		}
		if (lstHitObject == null)
		{
			lstHitObject = new List<string>();
		}
		lstHitObject = HSceneManager.HResourceTables.lstHitObject[Sex];
		int count = lstHitObject.Count;
		GameObject value = null;
		for (int num = 0; num < count; num += 3)
		{
			GameObject objParent = GetObjParent(_objBody.transform, lstHitObject[num]);
			if (HSceneManager.HResourceTables.DicHitObject.TryGetValue(Sex, out tmpDic) && tmpDic.TryGetValue(id, out tmpLst) && tmpLst.TryGetValue(lstHitObject[num + 2], out value))
			{
				EliminateScale[] componentsInChildren = value.GetComponentsInChildren<EliminateScale>(includeInactive: true);
				for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
				{
					componentsInChildren[num2].custom = _custom;
				}
				if (objParent != null && value != null)
				{
					value.transform.SetParent(objParent.transform, worldPositionStays: false);
					value.transform.localPosition = Vector3.zero;
					value.transform.localRotation = Quaternion.identity;
				}
				lstObject.Add(value);
			}
		}
		isInit = true;
		lstObjectCols = new Dictionary<int, ObiCollider[]>();
		for (int num3 = 0; num3 < lstObject.Count; num3++)
		{
			lstObjectCols.Add(num3, lstObject[num3].GetComponentsInChildren<ObiCollider>(includeInactive: true));
		}
		lstHitObject = null;
		sex = Sex;
		chaControl = _custom;
	}

	private GameObject GetObjParent(Transform objTop, string name)
	{
		getChild = objTop.GetComponentsInChildren<Transform>();
		for (int i = 0; i < getChild.Length; i++)
		{
			if (!(getChild[i].name != name))
			{
				return getChild[i].gameObject;
			}
		}
		return null;
	}

	public void HitObjLoadExcel(string _file)
	{
		lstInfo = new List<CollisionInfo>();
		atariName = new List<string>();
		CollisionInfo item = default(CollisionInfo);
		if (_file == "")
		{
			return;
		}
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHitObjListFolder);
		assetBundleNameListFromPath.Sort();
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				continue;
			}
			sbAbName.Clear();
			sbAbName.Append(assetBundleNameListFromPath[i]);
			excelData = null;
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
			int num = 0;
			int num2 = 0;
			row = excelData.list[num++].list;
			for (num2 = 1; num2 < row.Count; num2++)
			{
				int index = num2;
				if (sex != 0 || chaControl.sex != 1 || !chaControl.fileParam.futanari || !(row.GetElement(index) == Singleton<HSceneFlagCtrl>.Instance.atariTamaName))
				{
					atariName.Add(row.GetElement(index));
				}
			}
			while (num < excelData.MaxCell)
			{
				num2 = 0;
				row = excelData.list[num++].list;
				item.nameAnimation = row.GetElement(num2++);
				item.lstIsActive = new List<bool>();
				for (int j = 1; j < row.Count; j++)
				{
					item.lstIsActive.Add(row.GetElement(num2++) == "1");
				}
				lstInfo.Add(item);
			}
		}
		excelData = null;
		row = null;
	}

	public bool setActiveObject(bool val)
	{
		for (int i = 0; i < lstObject.Count; i++)
		{
			if (!(lstObject[i] == null) && lstObject[i].activeSelf != val)
			{
				lstObject[i].SetActive(val);
			}
		}
		isActive = val;
		return true;
	}

	public void PreEndPloc()
	{
		for (int i = 0; i < lstObject.Count; i++)
		{
			if (!(lstObject[i] == null) && !lstObject[i].activeSelf)
			{
				lstObject[i].SetActive(value: true);
			}
		}
	}

	public bool EndPloc()
	{
		for (int i = 0; i < lstObject.Count; i++)
		{
			if (!(lstObject[i] == null))
			{
				Object.Destroy(lstObject[i]);
				lstObject[i] = null;
			}
		}
		isInit = false;
		isActive = true;
		return true;
	}

	public bool Proc(Animator _anim)
	{
		if (_anim == null || _anim.runtimeAnimatorController == null)
		{
			Visible(_visible: false);
			return false;
		}
		ai = _anim.GetCurrentAnimatorStateInfo(0);
		for (int i = 0; i < lstInfo.Count; i++)
		{
			if (lstInfo[i].nameAnimation.IsNullOrEmpty() || !ai.IsName(lstInfo[i].nameAnimation))
			{
				continue;
			}
			isActive = true;
			for (int j = 0; j < lstObject.Count; j++)
			{
				for (int k = 0; k < atariName.Count; k++)
				{
					if (lstObject[j].name != atariName[k] || lstObject[j].activeSelf == lstInfo[i].lstIsActive[k])
					{
						continue;
					}
					if (lstInfo[i].lstIsActive[k])
					{
						ObiCollider[] array = lstObjectCols[j];
						if (array != null && array.Length != 0)
						{
							ObiCollider[] array2 = array;
							foreach (ObiCollider obiCollider in array2)
							{
								if (obiCollider != null)
								{
									if (lstInfo[i].lstIsActive[k])
									{
										obiCollider.Phase = 0;
									}
									else
									{
										obiCollider.Phase = 1;
									}
								}
							}
						}
					}
					else
					{
						SetObiColliderPhase(lstObject[j], j);
					}
					lstObject[j].SetActive(lstInfo[i].lstIsActive[k]);
				}
			}
			return true;
		}
		Visible(_visible: false);
		return false;
	}

	private bool Visible(bool _visible)
	{
		if (isActive == _visible)
		{
			return false;
		}
		for (int i = 0; i < lstObject.Count; i++)
		{
			if (!lstObject[i])
			{
				continue;
			}
			if (_visible)
			{
				ObiCollider[] array = lstObjectCols[i];
				if (array != null && array.Length != 0)
				{
					ObiCollider[] array2 = array;
					foreach (ObiCollider obiCollider in array2)
					{
						if (obiCollider != null)
						{
							if (lstObject[i].activeSelf)
							{
								obiCollider.Phase = 0;
							}
							else
							{
								obiCollider.Phase = 1;
							}
						}
					}
				}
			}
			else
			{
				SetObiColliderPhase(lstObject[i], i);
			}
			lstObject[i].SetActive(_visible);
		}
		isActive = _visible;
		return false;
	}

	private void SetObiColliderPhase(GameObject obj, int i)
	{
		if (obj == null)
		{
			return;
		}
		ObiCollider[] array = lstObjectCols[i];
		if (array == null || array.Length == 0)
		{
			return;
		}
		ObiCollider[] array2 = array;
		foreach (ObiCollider obiCollider in array2)
		{
			if (obiCollider != null)
			{
				obiCollider.Phase = 1;
			}
		}
	}
}
