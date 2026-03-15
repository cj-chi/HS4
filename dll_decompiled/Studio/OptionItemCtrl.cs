using System.Collections.Generic;
using System.Linq;
using Illusion;
using IllusionUtility.GetUtility;
using UnityEngine;

namespace Studio;

public class OptionItemCtrl : MonoBehaviour
{
	public class ChildInfo
	{
		public Vector3 scale = Vector3.one;

		public GameObject obj;

		public ChildInfo(Vector3 _scale, GameObject _obj)
		{
			scale = _scale;
			obj = _obj;
		}
	}

	public class ItemInfo
	{
		public GameObject gameObject;

		public Animator animator;

		public List<ChildInfo> child = new List<ChildInfo>();

		private Renderer[] renderer;

		private bool scaleOption;

		private Transform _transform;

		public float m_Height = 0.5f;

		private bool m_Active = true;

		public Vector3 scale { get; set; }

		public bool IsScale { get; private set; }

		public bool DefaultScaleOption
		{
			get
			{
				return scaleOption;
			}
			set
			{
				scaleOption = value;
				IsScale = value;
			}
		}

		public bool IsSync { get; set; }

		public bool IsAnime => animator != null;

		private Transform Transform => _transform ?? (_transform = gameObject.transform);

		public float height
		{
			get
			{
				return m_Height;
			}
			set
			{
				m_Height = value;
			}
		}

		public bool active
		{
			get
			{
				return m_Active;
			}
			set
			{
				if (m_Active != value)
				{
					m_Active = value;
					for (int i = 0; i < renderer.Length; i++)
					{
						renderer[i].enabled = value;
					}
				}
			}
		}

		public AnimatorStateInfo CurrentAnimatorStateInfo => animator.GetCurrentAnimatorStateInfo(0);

		public ItemInfo(float _height)
		{
			m_Height = _height;
		}

		public void Release()
		{
			Object.DestroyImmediate(gameObject);
			for (int i = 0; i < child.Count; i++)
			{
				Object.DestroyImmediate(child[i].obj);
			}
		}

		public void SetRender()
		{
			List<Renderer> list = new List<Renderer>();
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			if (!((IReadOnlyCollection<Renderer>)(object)componentsInChildren).IsNullOrEmpty())
			{
				list.AddRange(componentsInChildren);
			}
			for (int i = 0; i < child.Count; i++)
			{
				componentsInChildren = child[i].obj.GetComponentsInChildren<Renderer>();
				if (!((IReadOnlyCollection<Renderer>)(object)componentsInChildren).IsNullOrEmpty())
				{
					list.AddRange(componentsInChildren);
				}
			}
			renderer = list.Where((Renderer v) => v.enabled).ToArray();
		}

		public void RestartAnime()
		{
			AnimatorStateInfo currentAnimatorStateInfo = CurrentAnimatorStateInfo;
			animator.Play(currentAnimatorStateInfo.shortNameHash, 0, 0f);
		}

		public void SyncAnime(int stateNameHash, int layer, float normalizedTime)
		{
			animator.Play(stateNameHash, layer, normalizedTime);
		}

		public void CounterScale()
		{
			if (IsScale)
			{
				Vector3 localScale = Transform.localScale;
				Vector3 lossyScale = Transform.lossyScale;
				Transform.localScale = new Vector3(localScale.x / lossyScale.x * scale.x, localScale.y / lossyScale.y * scale.y, localScale.z / lossyScale.z * scale.z);
				IsScale = false;
			}
		}

		public void ReCounterScale()
		{
			DefaultScaleOption = DefaultScaleOption;
		}
	}

	private Animator m_Animator;

	private HashSet<ItemInfo> hashItem = new HashSet<ItemInfo>();

	private bool m_OutsideVisible = true;

	public Animator animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator = base.gameObject.GetComponentInChildren<Animator>();
			}
			return m_Animator;
		}
		set
		{
			m_Animator = value;
		}
	}

	public OICharInfo oiCharInfo { get; set; }

	public HashSet<ItemInfo> HashItem => hashItem;

	public bool visible
	{
		get
		{
			return oiCharInfo.animeOptionVisible;
		}
		set
		{
			oiCharInfo.animeOptionVisible = value;
			SetVisible(m_OutsideVisible & oiCharInfo.animeOptionVisible);
		}
	}

	public bool outsideVisible
	{
		get
		{
			return m_OutsideVisible;
		}
		set
		{
			m_OutsideVisible = value;
			SetVisible(m_OutsideVisible & oiCharInfo.animeOptionVisible);
		}
	}

	public float height
	{
		set
		{
			foreach (ItemInfo item in hashItem)
			{
				item.height = value;
			}
		}
	}

	public void LoadAnimeItem(Info.AnimeLoadInfo _info, string _clip, float _height, float _motion)
	{
		ReleaseAllItem();
		if (_info.option.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < _info.option.Count; i++)
		{
			Info.OptionItemInfo optionItemInfo = _info.option[i];
			GameObject gameObject = Utility.LoadAsset<GameObject>(optionItemInfo.bundlePath, optionItemInfo.fileName, optionItemInfo.manifest);
			if (gameObject == null)
			{
				continue;
			}
			ItemInfo itemInfo = new ItemInfo(_height);
			itemInfo.gameObject = gameObject;
			itemInfo.scale = gameObject.transform.localScale;
			itemInfo.animator = gameObject.GetComponentInChildren<Animator>();
			if (itemInfo.animator != null)
			{
				if (optionItemInfo.anmInfo.Check)
				{
					RuntimeAnimatorController runtimeAnimatorController = CommonLib.LoadAsset<RuntimeAnimatorController>(optionItemInfo.anmInfo.bundlePath, optionItemInfo.anmInfo.fileName);
					if (runtimeAnimatorController != null)
					{
						itemInfo.animator.runtimeAnimatorController = runtimeAnimatorController;
					}
					AssetBundleManager.UnloadAssetBundle(optionItemInfo.anmInfo.bundlePath, isUnloadForceRefCount: false);
					if (optionItemInfo.anmOveride.Check)
					{
						CommonLib.LoadAsset<RuntimeAnimatorController>(optionItemInfo.anmOveride.bundlePath, optionItemInfo.anmOveride.fileName).SafeProc(delegate(RuntimeAnimatorController _rac)
						{
							itemInfo.animator.runtimeAnimatorController = Utils.Animator.SetupAnimatorOverrideController(itemInfo.animator.runtimeAnimatorController, _rac);
						});
						AssetBundleManager.UnloadAssetBundle(optionItemInfo.anmOveride.bundlePath, isUnloadForceRefCount: false);
					}
					itemInfo.animator.Play(_clip);
				}
				itemInfo.animator.SetFloat("height", _height);
				itemInfo.IsSync = optionItemInfo.isAnimeSync;
			}
			else
			{
				itemInfo.IsSync = false;
			}
			if (((IReadOnlyCollection<Info.ParentageInfo>)(object)optionItemInfo.parentageInfo).IsNullOrEmpty())
			{
				GameObject gameObject2 = base.gameObject;
				GameObject gameObject3 = gameObject;
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = Vector3.zero;
				gameObject3.transform.localRotation = Quaternion.identity;
				if (optionItemInfo.counterScale)
				{
					itemInfo.DefaultScaleOption = true;
				}
				else
				{
					gameObject3.transform.localScale = itemInfo.scale;
				}
			}
			else
			{
				for (int num = 0; num < optionItemInfo.parentageInfo.Length; num++)
				{
					GameObject gameObject4 = base.gameObject.transform.FindLoop(optionItemInfo.parentageInfo[num].parent)?.gameObject;
					GameObject gameObject5 = gameObject;
					if (!optionItemInfo.parentageInfo[num].child.IsNullOrEmpty())
					{
						gameObject5 = gameObject5.transform.FindLoop(optionItemInfo.parentageInfo[num].child)?.gameObject;
						itemInfo.child.Add(new ChildInfo(gameObject5.transform.localScale, gameObject5));
					}
					gameObject5.transform.SetParent(gameObject4.transform);
					gameObject5.transform.localPosition = Vector3.zero;
					gameObject5.transform.localRotation = Quaternion.identity;
					if (optionItemInfo.counterScale)
					{
						itemInfo.DefaultScaleOption = true;
					}
					else
					{
						gameObject5.transform.localScale = itemInfo.scale;
					}
				}
			}
			itemInfo.SetRender();
			hashItem.Add(itemInfo);
		}
		SetVisible(visible);
	}

	public void ReleaseAllItem()
	{
		foreach (ItemInfo item in hashItem)
		{
			item?.Release();
		}
		hashItem.Clear();
	}

	public void PlayAnime()
	{
		foreach (ItemInfo item in hashItem.Where((ItemInfo v) => v.IsAnime))
		{
			item.RestartAnime();
		}
	}

	public void SetMotion(float _motion)
	{
		foreach (ItemInfo item in hashItem)
		{
			if ((bool)item.animator && item.IsSync)
			{
				item.animator.SetFloat("motion", _motion);
			}
		}
	}

	public void ChangeScale(Vector3 _value)
	{
		foreach (ItemInfo item in hashItem)
		{
			item.gameObject.transform.localScale = item.scale;
		}
	}

	public void ReCounterScale()
	{
		foreach (ItemInfo item in hashItem)
		{
			item.ReCounterScale();
		}
	}

	private void SetVisible(bool _visible)
	{
		foreach (ItemInfo item in hashItem)
		{
			item.active = _visible;
		}
	}

	private void Awake()
	{
		m_OutsideVisible = true;
	}

	private void LateUpdate()
	{
		if (animator == null || hashItem.Count == 0)
		{
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		foreach (ItemInfo item in hashItem)
		{
			if (item.IsAnime && item.IsSync)
			{
				item.SyncAnime(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
			}
			item.CounterScale();
		}
	}
}
