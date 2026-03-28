using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.CustomAttributes;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

public class HMobCtrl : MonoBehaviour
{
	[Serializable]
	public class MobObjectInfo
	{
		[Label("オブジェクト")]
		public GameObject obj;

		[Label("アニメーション名")]
		public string nameAnimation = "";

		[Label("配置種類")]
		public int kind;
	}

	[Serializable]
	public class PlacementInfo
	{
		[Label("配置した？")]
		public bool isUse;

		[Label("配置種類")]
		public int kind;

		[Label("配置Null名")]
		public string nameNull;

		[Label("性別配置")]
		public int kindSex = -1;

		[Label("アニメーション名")]
		public string nameAnimation = "";
	}

	[Serializable]
	public class MobInfo
	{
		[Label("マップ番号")]
		public int idMap = -1;

		[Label("経路アセットパス")]
		public string pathAssetRoot;

		[Label("経路ファイル名")]
		public string fileRoot;

		[Label("男モブマニフェスト名")]
		public string manifestMale;

		[Label("男モブアセットパス")]
		public string pathAssetMaleMob;

		[Label("男モブファイル名")]
		public string fileMaleMob;

		[Label("男モブ種類数")]
		public int numMaleKind = 2;

		[Label("女モブマニフェスト名")]
		public string manifestFemale;

		[Label("女モブアセットパス")]
		public string pathAssetFemaleMob;

		[Label("女モブファイル名")]
		public string fileFemaleMob;

		[Label("女モブ種類数")]
		public int numFemaleKind = 2;

		[Label("男人数")]
		public int numMale;

		[Label("女人数")]
		public int numFemale;

		[Label("移動する人数")]
		public int numMove;

		[Label("座らせる人数")]
		public int numSit;

		[Label("立たせる人数")]
		public int numStand;

		public List<PlacementInfo> lstPlacement = new List<PlacementInfo>();
	}

	[Header("Debug表示")]
	[SerializeField]
	private MobInfo infoMob = new MobInfo();

	[SerializeField]
	private GameObject objRoot;

	[SerializeField]
	private List<MobObjectInfo> lstObjMale = new List<MobObjectInfo>();

	[SerializeField]
	private List<MobObjectInfo> lstObjFemale = new List<MobObjectInfo>();

	private Material materialMale;

	private Material materialFemale;

	private List<PlacementInfo> lstWalkPlacementInfo = new List<PlacementInfo>();

	private List<PlacementInfo> lstSitPlacementInfo = new List<PlacementInfo>();

	private List<PlacementInfo> lstStandPlacementInfo = new List<PlacementInfo>();

	public bool Load(string _pathAssetFolder, int _mapID, GameObject _objMap)
	{
		string text = GlobalMethod.LoadAllListText(_pathAssetFolder, "h_mob");
		if (text == "")
		{
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		int length = data.GetLength(0);
		int length2 = data.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			int num = 0;
			int num2 = int.Parse(data[i][num++]);
			if (num2 != _mapID)
			{
				continue;
			}
			infoMob.idMap = num2;
			infoMob.pathAssetRoot = data[i][num++];
			infoMob.fileRoot = data[i][num++];
			infoMob.pathAssetMaleMob = data[i][num++];
			infoMob.fileMaleMob = data[i][num++];
			infoMob.pathAssetFemaleMob = data[i][num++];
			infoMob.fileFemaleMob = data[i][num++];
			infoMob.numMale = GlobalMethod.GetIntTryParse(data[i][num++]);
			infoMob.numFemale = GlobalMethod.GetIntTryParse(data[i][num++]);
			infoMob.numMove = GlobalMethod.GetIntTryParse(data[i][num++]);
			infoMob.numSit = GlobalMethod.GetIntTryParse(data[i][num++]);
			infoMob.numStand = GlobalMethod.GetIntTryParse(data[i][num++]);
			for (int j = num; j < length2; j += 2)
			{
				string text2 = data[i][j];
				if (text2.IsNullOrEmpty())
				{
					break;
				}
				PlacementInfo placementInfo = new PlacementInfo();
				placementInfo.kind = GlobalMethod.GetIntTryParse(text2);
				placementInfo.nameNull = data[i][j + 1];
				if (placementInfo.kind == 0)
				{
					lstWalkPlacementInfo.Add(placementInfo);
				}
				else if (placementInfo.kind == 1)
				{
					lstSitPlacementInfo.Add(placementInfo);
				}
				else if (placementInfo.kind == 2)
				{
					lstStandPlacementInfo.Add(placementInfo);
				}
			}
			break;
		}
		if (infoMob.idMap == -1)
		{
			GlobalMethod.DebugLog("マップ番号がなかった", 1);
			return false;
		}
		infoMob.lstPlacement.AddRange(lstWalkPlacementInfo);
		infoMob.lstPlacement.AddRange(lstSitPlacementInfo.OrderBy((PlacementInfo inf) => Guid.NewGuid()).ToList());
		infoMob.lstPlacement.AddRange(lstStandPlacementInfo.OrderBy((PlacementInfo inf) => Guid.NewGuid()).ToList());
		LoadObject(_objMap);
		return true;
	}

	public bool LoadExtensionMap(string _pathAssetFolder, int _mapID, GameObject _objMap)
	{
		string text = GlobalMethod.LoadAllListText(_pathAssetFolder, "h_mob_01");
		if (text == "")
		{
			return false;
		}
		infoMob = new MobInfo();
		GlobalMethod.GetListString(text, out var data);
		int length = data.GetLength(0);
		int length2 = data.GetLength(1);
		for (int i = 0; i < length2; i++)
		{
			int num = 0;
			int num2 = int.Parse(data[num++][i]);
			if (num2 != _mapID)
			{
				continue;
			}
			infoMob.idMap = num2;
			infoMob.pathAssetRoot = data[num++][i];
			infoMob.fileRoot = data[num++][i];
			infoMob.manifestMale = data[num++][i];
			infoMob.pathAssetMaleMob = data[num++][i];
			infoMob.fileMaleMob = data[num++][i];
			infoMob.numMaleKind = GlobalMethod.GetIntTryParse(data[num++][i], 2);
			infoMob.manifestFemale = data[num++][i];
			infoMob.pathAssetFemaleMob = data[num++][i];
			infoMob.fileFemaleMob = data[num++][i];
			infoMob.numFemaleKind = GlobalMethod.GetIntTryParse(data[num++][i], 2);
			infoMob.numMale = GlobalMethod.GetIntTryParse(data[num++][i]);
			infoMob.numFemale = GlobalMethod.GetIntTryParse(data[num++][i]);
			infoMob.numMove = GlobalMethod.GetIntTryParse(data[num++][i]);
			infoMob.numSit = GlobalMethod.GetIntTryParse(data[num++][i]);
			infoMob.numStand = GlobalMethod.GetIntTryParse(data[num++][i]);
			for (int j = num; j < length; j += 4)
			{
				string text2 = data[j][i];
				if (text2.IsNullOrEmpty())
				{
					break;
				}
				PlacementInfo placementInfo = new PlacementInfo();
				placementInfo.kind = GlobalMethod.GetIntTryParse(text2);
				placementInfo.nameNull = data[j + 1][i];
				placementInfo.kindSex = GlobalMethod.GetIntTryParse(data[j + 2][i], -1);
				placementInfo.nameAnimation = data[j + 3][i];
				if (placementInfo.kind == 0)
				{
					lstWalkPlacementInfo.Add(placementInfo);
				}
				else if (placementInfo.kind == 1)
				{
					lstSitPlacementInfo.Add(placementInfo);
				}
				else if (placementInfo.kind == 2)
				{
					lstStandPlacementInfo.Add(placementInfo);
				}
			}
			break;
		}
		if (infoMob.idMap == -1)
		{
			GlobalMethod.DebugLog("マップ番号がなかった", 1);
			return false;
		}
		infoMob.lstPlacement.AddRange(lstWalkPlacementInfo);
		infoMob.lstPlacement.AddRange(lstSitPlacementInfo.OrderBy((PlacementInfo inf) => Guid.NewGuid()).ToList());
		infoMob.lstPlacement.AddRange(lstStandPlacementInfo.OrderBy((PlacementInfo inf) => Guid.NewGuid()).ToList());
		LoadObject(_objMap);
		return true;
	}

	private bool LoadObject(GameObject _objMap)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < infoMob.numMale; i++)
		{
			list.Add(0);
		}
		for (int j = 0; j < infoMob.numFemale; j++)
		{
			list.Add(1);
		}
		if (list.Count == 0)
		{
			GlobalMethod.DebugLog("男と女のトータル人数が0人", 1);
			return false;
		}
		if (list.Count != infoMob.numMove + infoMob.numSit + infoMob.numStand)
		{
			GlobalMethod.DebugLog("男と女の人数と配置しようとしている人数が合わない 男と女の人数: " + list.Count + " 配置人数: " + (infoMob.numMove + infoMob.numSit + infoMob.numStand), 1);
			return false;
		}
		list = list.OrderBy((int inf) => Guid.NewGuid()).ToList();
		int _index = 0;
		SetMobKind(0, infoMob.numMove, list, ref _index);
		SetMobKind(1, infoMob.numSit, list, ref _index);
		SetMobKind(2, infoMob.numStand, list, ref _index);
		objRoot = CommonLib.LoadAsset<GameObject>(infoMob.pathAssetRoot, infoMob.fileRoot, clone: true);
		Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(infoMob.pathAssetRoot);
		for (int num = 0; num < lstObjMale.Count; num++)
		{
			MobObjectInfo mobObjectInfo = lstObjMale[num];
			string text = infoMob.fileMaleMob + (UnityEngine.Random.Range(0, infoMob.numMaleKind) + 1).ToString("00");
			mobObjectInfo.obj = CommonLib.LoadAsset<GameObject>(infoMob.pathAssetMaleMob, text, clone: true, infoMob.manifestMale);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(infoMob.pathAssetMaleMob);
			if (mobObjectInfo.obj == null)
			{
				GlobalMethod.DebugLog("Mobの取得に失敗 : " + text, 1);
				continue;
			}
			SkinnedMeshRenderer[] componentsInChildren = mobObjectInfo.obj.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			if (componentsInChildren.Length != 0 && materialMale == null)
			{
				materialMale = componentsInChildren[0].sharedMaterial;
			}
			SetPlacementMob(mobObjectInfo, _objMap, 0);
		}
		for (int num2 = 0; num2 < lstObjFemale.Count; num2++)
		{
			MobObjectInfo mobObjectInfo2 = lstObjFemale[num2];
			string text2 = infoMob.fileFemaleMob + (UnityEngine.Random.Range(0, infoMob.numFemaleKind) + 1).ToString("00");
			mobObjectInfo2.obj = CommonLib.LoadAsset<GameObject>(infoMob.pathAssetFemaleMob, text2, clone: true, infoMob.manifestFemale);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(infoMob.pathAssetFemaleMob);
			if (mobObjectInfo2.obj == null)
			{
				GlobalMethod.DebugLog("Mobの取得に失敗 : " + text2, 1);
				continue;
			}
			SkinnedMeshRenderer[] componentsInChildren2 = mobObjectInfo2.obj.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			if (componentsInChildren2.Length != 0 && materialFemale == null)
			{
				materialFemale = componentsInChildren2[0].sharedMaterial;
			}
			SetPlacementMob(mobObjectInfo2, _objMap, 1);
		}
		return true;
	}

	private bool SetMobKind(int _kind, int _loop, List<int> _list, ref int _index)
	{
		for (int i = 0; i < _loop; i++)
		{
			MobObjectInfo mobObjectInfo = new MobObjectInfo();
			mobObjectInfo.kind = _kind;
			if (_list[_index] == 0)
			{
				lstObjMale.Add(mobObjectInfo);
			}
			else if (_list[_index] == 1)
			{
				lstObjFemale.Add(mobObjectInfo);
			}
			_index++;
		}
		return true;
	}

	private bool SetPlacementMob(MobObjectInfo _info, GameObject _objMap, int _sex)
	{
		for (int i = 0; i < infoMob.lstPlacement.Count; i++)
		{
			PlacementInfo placementInfo = infoMob.lstPlacement[i];
			if (placementInfo.kind != _info.kind || placementInfo.isUse)
			{
				continue;
			}
			if (placementInfo.kind == 0)
			{
				if (placementInfo.kindSex != -1 && placementInfo.kindSex != _sex)
				{
					continue;
				}
				if (objRoot == null)
				{
					GlobalMethod.DebugLog("経路からオブジェクト取りたいけど経路がない", 1);
					continue;
				}
				GameObject gameObject = objRoot.transform.FindLoop(placementInfo.nameNull).gameObject;
				if (gameObject == null)
				{
					GlobalMethod.DebugLog("経路オブジェクトに[" + placementInfo.nameNull + "]なんてものは入ってない", 1);
					continue;
				}
				_info.obj.transform.parent = gameObject.transform;
				_info.obj.transform.localPosition = Vector3.zero;
				_info.obj.transform.localRotation = Quaternion.identity;
				_info.obj.transform.localScale = Vector3.one;
				_info.nameAnimation = ((placementInfo.nameAnimation == "") ? "aruki" : placementInfo.nameAnimation);
				SetAnimation(_info.obj, _info.nameAnimation);
			}
			else
			{
				if (placementInfo.kindSex != -1 && placementInfo.kindSex != _sex)
				{
					continue;
				}
				if (_objMap == null)
				{
					GlobalMethod.DebugLog("マップからオブジェクト取りたいけどマップがない", 1);
					continue;
				}
				GameObject gameObject2 = _objMap.transform.FindLoop(placementInfo.nameNull).gameObject;
				if (gameObject2 == null)
				{
					GlobalMethod.DebugLog("マップオブジェクトに[" + placementInfo.nameNull + "]なんてものは入ってない", 1);
					continue;
				}
				_info.obj.transform.position = gameObject2.transform.position;
				_info.obj.transform.rotation = gameObject2.transform.rotation;
				_info.nameAnimation = ((!(placementInfo.nameAnimation == "")) ? placementInfo.nameAnimation : ((_info.kind == 1) ? "suwari" : ((UnityEngine.Random.Range(0, 100) % 2 == 0) ? "tachi_00" : "tachi_01")));
				SetAnimation(_info.obj, _info.nameAnimation);
			}
			placementInfo.isUse = true;
			break;
		}
		return true;
	}

	private bool SetAnimation(GameObject _obj, string _anim)
	{
		if (_obj == null)
		{
			return false;
		}
		Animator componentInChildren = _obj.GetComponentInChildren<Animator>(includeInactive: true);
		if (componentInChildren == null)
		{
			return false;
		}
		componentInChildren.Play(_anim);
		return true;
	}

	public void SetConfigValue(Color _color, bool _visible, int _sex)
	{
	}
}
