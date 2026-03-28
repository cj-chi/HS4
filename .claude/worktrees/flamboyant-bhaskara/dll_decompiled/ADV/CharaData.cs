using System.Collections.Generic;
using ADV.Commands.Base;
using AIChara;
using Actor;
using Illusion.Anime;
using Manager;
using UnityEngine;

namespace ADV;

public class CharaData
{
	public class Backup
	{
		public Transform transform { get; }

		public Transform parent { get; }

		public Vector3 position { get; }

		public Quaternion rotation { get; }

		public Backup(Transform transform, TextScenario scenario)
		{
			this.transform = transform;
			parent = transform.parent;
			position = transform.localPosition;
			rotation = transform.localRotation;
			transform.SetParent(scenario.commandController.Character, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}

		public void Repair(bool _isBackUp = true)
		{
			if (!(transform == null))
			{
				if (!parent || ((bool)parent && parent.gameObject.activeSelf))
				{
					transform.SetParent(parent, !_isBackUp);
				}
				if (_isBackUp)
				{
					transform.localPosition = position;
					transform.localRotation = rotation;
				}
			}
		}
	}

	public class CharaItem
	{
		public GameObject item { get; private set; }

		public CharaItem()
		{
		}

		public CharaItem(GameObject item)
		{
			this.item = item;
		}

		public void Delete()
		{
			if (item != null)
			{
				Object.Destroy(item);
				item = null;
			}
		}

		public void LoadObject(string bundle, string asset, Transform root, bool worldPositionStays = false, string manifest = null)
		{
			Delete();
			GameObject asset2 = AssetBundleManager.LoadAsset(bundle, asset, typeof(GameObject), manifest).GetAsset<GameObject>();
			item = Object.Instantiate(asset2);
			AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false, manifest);
			item.name = asset2.name;
			item.transform.SetParent(root, worldPositionStays);
		}

		public void LoadAnimator(string bundle, string asset, string state)
		{
			Animator orAddComponent = item.GetOrAddComponent<Animator>();
			orAddComponent.runtimeAnimatorController = AssetBundleManager.LoadAsset(bundle, asset, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
			AssetBundleManager.UnloadAssetBundle(bundle, isUnloadForceRefCount: false);
			orAddComponent.Play(state);
		}
	}

	private ChaControl _chaCtrl;

	private Controller _animeController;

	public Dictionary<int, CharaItem> itemTable { get; } = new Dictionary<int, CharaItem>();

	public bool isADVCreateChara { get; }

	private GameObject dataBaseRoot { get; set; }

	public Transform root
	{
		get
		{
			if (!(chaCtrl == null))
			{
				return chaCtrl.transform;
			}
			return null;
		}
	}

	public Heroine heroine => data.param as Heroine;

	public int voiceNo => data.voiceNo;

	public float voicePitch => data.voicePitch;

	public Transform voiceTrans
	{
		get
		{
			if (chaCtrl == null || !chaCtrl.loadEnd)
			{
				return null;
			}
			GameObject referenceInfo = chaCtrl.GetReferenceInfo(ChaReference.RefObjKey.HeadParent);
			if (referenceInfo == null)
			{
				return null;
			}
			return referenceInfo.transform;
		}
	}

	public TextScenario.ParamData data { get; private set; }

	public ChaControl chaCtrl => this.GetCacheObject(ref _chaCtrl, () => data.chaCtrl);

	public Transform transform
	{
		get
		{
			if (!(data.transform == null))
			{
				return data.transform;
			}
			return root;
		}
	}

	public Controller animeController => this.GetCache(ref _animeController, () => Controller.Table.Get(chaCtrl));

	private TextScenario scenario { get; set; }

	public Backup backup { get; set; }

	public CharaData(TextScenario.ParamData data, TextScenario scenario, bool isParent)
	{
		this.data = data;
		this.scenario = scenario;
		_chaCtrl = data.chaCtrl;
		if (_chaCtrl == null)
		{
			isADVCreateChara = true;
		}
		else if (isParent)
		{
			backup = new Backup(_chaCtrl.transform, scenario);
		}
	}

	public void MotionDefault()
	{
		animeController?.ResetDefaultAnimatorController();
	}

	public void MotionPlay(ADV.Commands.Base.Motion.Data motion, bool isCrossFade)
	{
		if (isCrossFade)
		{
			scenario.CrossFadeStart();
		}
		animeController?.PlayID(motion.ID);
	}

	public void Release(bool _isBackUp = true)
	{
		foreach (KeyValuePair<int, CharaItem> item in itemTable)
		{
			item.Value.Delete();
		}
		if (isADVCreateChara && Singleton<Character>.IsInstance() && chaCtrl != null)
		{
			Singleton<Character>.Instance.DeleteChara(chaCtrl);
		}
		if (backup != null)
		{
			backup.Repair(_isBackUp);
		}
	}
}
