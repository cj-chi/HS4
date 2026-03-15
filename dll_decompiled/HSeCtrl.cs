using System.Collections.Generic;
using System.Text;
using AIChara;
using AIProject;
using Illusion.Game;
using Illusion.Misc;
using IllusionUtility.GetUtility;
using Manager;
using UniRx;
using UnityEngine;

public class HSeCtrl
{
	private struct KeyInfo
	{
		public GameObject objParent;

		public string objParentName;

		public float key;

		public bool isLoop;

		public int loopSwitch;

		public string assetPath;

		public List<string> nameSE;

		public bool isShorts;

		public List<string> nameShortsSE;

		public int nFemale;

		public bool bChangeVol;
	}

	private class Info
	{
		public delegate bool IsCheck<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);

		public string nameAnimation;

		public List<KeyInfo> key = new List<KeyInfo>();

		public List<KeyInfo> IsKey(float _old, float _now)
		{
			IsCheck<float, float, float>[] array = new IsCheck<float, float, float>[2] { IsKeyLoop, IsKeyNormal };
			List<KeyInfo> list = new List<KeyInfo>();
			int num = ((!(_old > _now)) ? 1 : 0);
			for (int i = 0; i < key.Count; i++)
			{
				if (array[num](_old, _now, key[i].key))
				{
					list.Add(key[i]);
				}
			}
			return list;
		}

		private bool IsKeyLoop(float _old, float _now, float _key)
		{
			if (!(_old < _key))
			{
				return _now > _key;
			}
			return true;
		}

		private bool IsKeyNormal(float _old, float _now, float _key)
		{
			if (_old <= _key)
			{
				return _key < _now;
			}
			return false;
		}
	}

	private List<Info> lstInfo = new List<Info>();

	private Dictionary<string, Dictionary<string, Transform>> dicLoop = new Dictionary<string, Dictionary<string, Transform>>();

	private HSceneFlagCtrl hFlag;

	private int oldnameHash;

	private float oldNormalizeTime;

	private string abName = "";

	private string assetName = "";

	private ExcelData excelData;

	private List<string> row = new List<string>();

	private KeyInfo infoKey;

	private Info info;

	private string nameAnim;

	private bool[] dicloopContainKey = new bool[2];

	private StringBuilder procNameSE;

	public HSeCtrl(HSceneFlagCtrl hFlag)
	{
		this.hFlag = hFlag;
	}

	public bool Load(string _strAssetFolder, string _file, GameObject[] _objBoneMale, GameObject[] _objBoneFemale)
	{
		lstInfo.Clear();
		dicLoop.Clear();
		oldnameHash = 0;
		oldNormalizeTime = 0f;
		excelData = null;
		Manager.Sound.Stop(Manager.Sound.Type.GameSE3D);
		if (_file == "")
		{
			return false;
		}
		GameObject[] array = new GameObject[5]
		{
			_objBoneMale[0],
			_objBoneFemale[0],
			_objBoneFemale[1],
			_objBoneMale[1],
			BaseMap.mapRoot
		};
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(_strAssetFolder);
		assetBundleNameListFromPath.Sort();
		assetName = _file;
		int num = -1;
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]) && GlobalMethod.AssetFileExist(assetBundleNameListFromPath[i], assetName))
			{
				num = i;
			}
		}
		if (num >= 0)
		{
			abName = assetBundleNameListFromPath[num];
			excelData = CommonLib.LoadAsset<ExcelData>(abName, assetName);
			Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(abName);
			if (excelData == null)
			{
				return true;
			}
			int num2 = 1;
			while (num2 < excelData.MaxCell)
			{
				row = excelData.list[num2++].list;
				int index = 0;
				nameAnim = row.GetElement(index++);
				int num3 = int.Parse(row.GetElement(index++));
				infoKey.objParentName = row.GetElement(index++);
				array[num3].SafeProc(delegate(GameObject o)
				{
					if (o != null)
					{
						Transform transform = o.transform.FindLoop(infoKey.objParentName);
						infoKey.objParent = ((transform == null) ? null : transform.gameObject);
					}
				});
				infoKey.key = float.Parse(row.GetElement(index++));
				infoKey.isLoop = row.GetElement(index++) == "1";
				infoKey.loopSwitch = ((row.GetElement(index++) == "1") ? 1 : 0);
				infoKey.bChangeVol = row.GetElement(index++) == "1";
				infoKey.assetPath = row.GetElement(index++);
				string element = row.GetElement(index++);
				if (element != "")
				{
					string[] array2 = element.Split('/');
					infoKey.nameSE = new List<string>();
					for (int num4 = 0; num4 < array2.Length; num4++)
					{
						infoKey.nameSE.Add(array2[num4]);
					}
				}
				infoKey.isShorts = row.GetElement(index++) == "1";
				element = row.GetElement(index++);
				if (element != "")
				{
					string[] array2 = element.Split('/');
					infoKey.nameShortsSE = new List<string>();
					for (int num5 = 0; num5 < array2.Length; num5++)
					{
						infoKey.nameShortsSE.Add(array2[num5]);
					}
				}
				infoKey.nFemale = 0;
				if (row.Count > 11)
				{
					infoKey.nFemale = ((!(row.GetElement(index) == "")) ? GlobalMethod.GetIntTryParse(row.GetElement(index++)) : 0);
				}
				info = lstInfo.Find((Info info) => info.nameAnimation == nameAnim);
				if (info == null)
				{
					info = new Info();
					info.nameAnimation = nameAnim;
					info.key.Add(infoKey);
					lstInfo.Add(info);
				}
				else
				{
					info.key.Add(infoKey);
				}
			}
		}
		return true;
	}

	public bool Proc(AnimatorStateInfo _ai, ChaControl[] _females)
	{
		if (_females == null)
		{
			return false;
		}
		if (oldnameHash != _ai.shortNameHash)
		{
			oldNormalizeTime = 0f;
		}
		float now = _ai.normalizedTime % 1f;
		procNameSE = StringBuilderPool.Get();
		string key = "";
		string key2 = "";
		for (int i = 0; i < lstInfo.Count; i++)
		{
			if (!_ai.IsName(lstInfo[i].nameAnimation))
			{
				continue;
			}
			List<KeyInfo> list = lstInfo[i].IsKey(oldNormalizeTime, now);
			for (int j = 0; j < list.Count; j++)
			{
				procNameSE.Clear();
				procNameSE.Append(GetSEName(list[j].nameSE));
				if (list[j].isLoop)
				{
					if (list[j].loopSwitch == 0)
					{
						bool flag = false;
						for (int k = 0; k < list[j].nameSE.Count; k++)
						{
							int index = k;
							if (dicLoop.ContainsKey(list[j].nameSE[index]))
							{
								key = list[j].nameSE[index];
								if (dicLoop[key].ContainsKey(list[j].objParentName))
								{
									flag = true;
									key2 = list[j].objParentName;
									break;
								}
							}
						}
						if (flag)
						{
							Manager.Sound.Stop(dicLoop[key][key2]);
							dicLoop[key].Remove(key2);
						}
						continue;
					}
					dicloopContainKey[0] = dicLoop.ContainsKey(procNameSE.ToString());
					dicloopContainKey[1] = dicloopContainKey[0] && dicLoop[procNameSE.ToString()].ContainsKey(list[j].objParentName);
					if (dicloopContainKey[0] && dicloopContainKey[1])
					{
						continue;
					}
					AudioSource audioSource = Utils.Sound.Play(new Utils.Sound.Setting(Manager.Sound.Type.GameSE3D)
					{
						bundle = list[j].assetPath,
						asset = procNameSE.ToString()
					});
					if (!(audioSource == null))
					{
						audioSource.loop = true;
						audioSource.rolloffMode = AudioRolloffMode.Linear;
						GameObject parent = list[j].objParent;
						Transform trans = audioSource.transform;
						Observable.EveryLateUpdate().Subscribe(delegate
						{
							Vector3 position = (parent ? parent.transform.position : Vector3.zero);
							Quaternion rotation = (parent ? parent.transform.rotation : Quaternion.identity);
							trans.SetPositionAndRotation(position, rotation);
						}).AddTo(trans);
						if (!dicloopContainKey[0])
						{
							dicLoop.Add(procNameSE.ToString(), new Dictionary<string, Transform>());
						}
						if (!dicloopContainKey[1])
						{
							dicLoop[procNameSE.ToString()].Add(list[j].objParentName, null);
						}
						dicLoop[procNameSE.ToString()][list[j].objParentName] = trans;
					}
					continue;
				}
				Utils.Sound.Setting setting = new Utils.Sound.Setting(Manager.Sound.Type.GameSE3D)
				{
					bundle = list[j].assetPath,
					asset = procNameSE.ToString()
				};
				if (list[j].isShorts && list[j].nameShortsSE.Count > 0 && _females[list[j].nFemale].IsKokanHide())
				{
					setting.asset = GetSEName(list[j].nameShortsSE);
				}
				AudioSource audioSource2 = Utils.Sound.Play(setting);
				if (audioSource2 != null)
				{
					audioSource2.rolloffMode = AudioRolloffMode.Linear;
					GameObject parent2 = list[j].objParent;
					Transform trans2 = audioSource2.transform;
					Observable.EveryLateUpdate().Subscribe(delegate
					{
						Vector3 position = (parent2 ? parent2.transform.position : Vector3.zero);
						Quaternion rotation = (parent2 ? parent2.transform.rotation : Quaternion.identity);
						trans2.SetPositionAndRotation(position, rotation);
					}).AddTo(trans2);
				}
			}
			break;
		}
		oldNormalizeTime = now;
		oldnameHash = _ai.shortNameHash;
		StringBuilderPool.Release(procNameSE);
		return true;
	}

	public void InitOldMember(int _init = -1)
	{
		if (-1 == _init || _init == 0)
		{
			oldNormalizeTime = 0f;
		}
		if (-1 == _init || 1 == _init)
		{
			oldnameHash = 0;
		}
	}

	private string GetSEName(List<string> list)
	{
		if (list.Count < 1)
		{
			return "";
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}
}
