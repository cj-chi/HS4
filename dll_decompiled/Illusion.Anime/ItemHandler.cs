using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Anime.Information;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using UnityEngine;

namespace Illusion.Anime;

public class ItemHandler : IDisposable
{
	public class ItemRenderEvent : Controller.AnimatorStateEvent
	{
		private IReadOnlyList<(int index, Renderer[] renderers)> _itemRenderers { get; }

		public ItemRenderEvent(List<(int index, Renderer[] renderers)> itemRenderers, Loader.AnimeEventInfo info)
			: base(info)
		{
			_itemRenderers = itemRenderers;
		}

		protected override void Change()
		{
			SetEnableItemRenderers(_itemRenderers.SafeGet(base.eventID / 2).renderers, base.eventID % 2 == 0);
		}

		private void SetEnableItemRenderers(Renderer[] renderers, bool enable)
		{
			if (renderers == null)
			{
				return;
			}
			foreach (Renderer item in renderers.Where((Renderer x) => x != null && x.enabled != enable))
			{
				item.enabled = enable;
			}
		}
	}

	private class ItemAnimInfo
	{
		public Animator animator { get; }

		public bool isSync { get; } = true;

		public ItemAnimInfo(Animator animator, bool isSync)
		{
			this.animator = animator;
			this.isSync = isSync;
		}
	}

	private class ItemScaleInfo
	{
		private Transform target { get; }

		private Loader.ItemScaleInfo info { get; }

		public ItemScaleInfo(Transform target, Loader.ItemScaleInfo info)
		{
			this.target = target;
			this.info = info;
		}

		public void Calc(float t)
		{
			if (info.mode == 0)
			{
				float num = Evaluate(t);
				target.localScale = new Vector3(num, num, num);
			}
		}

		private float Evaluate(float t)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return Mathf.Lerp(info.S, info.M, Mathf.InverseLerp(0f, 0.5f, t));
			}
			return Mathf.Lerp(info.M, info.L, Mathf.InverseLerp(0.5f, 1f, t));
		}
	}

	private Action<Animator> _playAnimationEvent { get; set; }

	private Transform _root { get; }

	private Dictionary<int, List<Loader.AnimeEventInfo>> _itemEventKeyTable { get; set; }

	private List<ItemRenderEvent> _stateItemEventList { get; } = new List<ItemRenderEvent>();

	private List<(int index, GameObject gameObject)> _items { get; } = new List<(int, GameObject)>();

	private List<(int index, Renderer[] renderers)> _itemRenderers { get; } = new List<(int, Renderer[])>();

	private Dictionary<int, ItemAnimInfo> _itemAnimatorTable { get; } = new Dictionary<int, ItemAnimInfo>();

	private List<ItemScaleInfo> _scaleCtrlInfos { get; } = new List<ItemScaleInfo>();

	private List<(Transform, Transform)> _itemChildObjects { get; } = new List<(Transform, Transform)>();

	public event Action<Animator> PlayAnimationEvent
	{
		add
		{
			_playAnimationEvent = (Action<Animator>)Delegate.Combine(_playAnimationEvent, value);
		}
		remove
		{
			_playAnimationEvent = (Action<Animator>)Delegate.Remove(_playAnimationEvent, value);
		}
	}

	public ItemHandler(Transform root)
	{
		_root = root;
	}

	public void Dispose()
	{
		ClearItems();
	}

	public void LoadEvents(int poseID, IReadOnlyCollection<ItemInfo> itemInfoCollection)
	{
		ClearItems();
		LoadEventKeyTable(poseID);
		LoadEventItems(itemInfoCollection);
	}

	public void Change(int stateNameHash)
	{
		_stateItemEventList.Clear();
		List<Loader.AnimeEventInfo> stateEvents = Controller.GetStateEvents(stateNameHash, _itemEventKeyTable);
		if (!stateEvents.IsNullOrEmpty())
		{
			_stateItemEventList.AddRange(stateEvents.Select((Loader.AnimeEventInfo info) => new ItemRenderEvent(_itemRenderers, info)));
		}
	}

	public void Update(float normalizedTime, bool isLoop, float scale)
	{
		foreach (ItemRenderEvent stateItemEvent in _stateItemEventList)
		{
			stateItemEvent.Update(normalizedTime, isLoop);
		}
		foreach (ItemScaleInfo scaleCtrlInfo in _scaleCtrlInfos)
		{
			scaleCtrlInfo.Calc(scale);
		}
	}

	public void CrossFadeItemAnimation(string stateName, float fadeTime, int layer)
	{
		CrossFadeItemAnimation(Animator.StringToHash(stateName), fadeTime, layer);
	}

	public void CrossFadeItemAnimation(int stateNameHash, float fadeTime, int layer)
	{
		foreach (ItemAnimInfo item in _itemAnimatorTable.Values.Where((ItemAnimInfo x) => x.isSync))
		{
			Animator animator = item.animator;
			_playAnimationEvent?.Invoke(animator);
			Controller.CrossFadeAnimation(animator, stateNameHash, fadeTime, layer, 0f);
		}
	}

	public void PlayItemAnimation(string stateName)
	{
		PlayItemAnimation(Animator.StringToHash(stateName));
	}

	public void PlayItemAnimation(int stateNameHash)
	{
		foreach (ItemAnimInfo item in _itemAnimatorTable.Values.Where((ItemAnimInfo x) => x.isSync))
		{
			Animator animator = item.animator;
			_playAnimationEvent?.Invoke(animator);
			Controller.PlayAnimation(animator, stateNameHash, 0, 0f);
		}
	}

	public void EnableItems()
	{
		foreach (var item in _items)
		{
			if (item.gameObject != null)
			{
				item.gameObject.SetActive(value: true);
			}
		}
	}

	public void DisableItems()
	{
		foreach (var item in _items)
		{
			if (item.gameObject != null)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public void ClearItems()
	{
		_itemRenderers.Clear();
		_itemAnimatorTable.Clear();
		_scaleCtrlInfos.Clear();
		foreach (var itemChildObject in _itemChildObjects)
		{
			itemChildObject.Item1.SetParent(itemChildObject.Item2);
		}
		_itemChildObjects.Clear();
		foreach (var item in _items)
		{
			if (item.gameObject != null)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		_items.Clear();
	}

	private void LoadEventKeyTable(int poseID)
	{
		Dictionary<int, List<Loader.AnimeEventInfo>> value = null;
		Loader.EventItemKeyTable?.TryGetValue(poseID, out value);
		_itemEventKeyTable = value;
	}

	private void LoadEventItems(IReadOnlyCollection<ItemInfo> itemInfoCollection)
	{
		foreach (ItemInfo item in itemInfoCollection.Where((ItemInfo x) => !x.fromEquipedItem))
		{
			Loader.ActionItemInfo value = null;
			IReadOnlyDictionary<int, Loader.ActionItemInfo> eventItemList = Loader.EventItemList;
			if (eventItemList != null && eventItemList.TryGetValue(item.itemID, out value))
			{
				LoadEventItem(item, value);
			}
		}
	}

	private void LoadEventItem(ItemInfo info, Loader.ActionItemInfo actItem)
	{
		LoadEventItem(info.itemID, info, actItem);
	}

	private void LoadEventItem(int itemID, ItemInfo info, Loader.ActionItemInfo actItem)
	{
		LoadEventItem(itemID, info.parentName, info.isSync, actItem, info.parentInfos);
	}

	private void LoadEventItem(int itemID, string parentName, bool isSync, Loader.ActionItemInfo actItem, List<ItemParentInfo> _parentInfos)
	{
		GameObject asset = actItem.data.GetAsset<GameObject>();
		if (asset == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(asset, _root.FindLoop(parentName), worldPositionStays: false);
		gameObject.name = asset.name;
		gameObject.SetActiveIfDifferent(active: true);
		_items.Add((itemID, gameObject));
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		_itemRenderers.Add((itemID, componentsInChildren));
		if (actItem.exists)
		{
			Animator component = gameObject.GetComponent<Animator>();
			component.runtimeAnimatorController = actItem.animeData.GetAsset<RuntimeAnimatorController>();
			_itemAnimatorTable[gameObject.GetInstanceID()] = new ItemAnimInfo(component, isSync);
		}
		Transform transform = gameObject.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		Loader.ItemScaleInfo value = null;
		IReadOnlyDictionary<int, Loader.ItemScaleInfo> eventItemScaleTable = Loader.EventItemScaleTable;
		if (eventItemScaleTable != null && eventItemScaleTable.TryGetValue(itemID, out value))
		{
			_scaleCtrlInfos.Add(new ItemScaleInfo(transform, value));
		}
		if (_parentInfos == null)
		{
			return;
		}
		foreach (ItemParentInfo _parentInfo in _parentInfos)
		{
			Transform transform2 = _root.FindLoop(_parentInfo.parent);
			Transform transform3 = gameObject.transform.FindLoop(_parentInfo.child);
			if ((bool)transform2 && (bool)transform3)
			{
				Transform parent = transform3.parent;
				transform3.SetParent(transform2, worldPositionStays: false);
				transform3.localPosition = Vector3.zero;
				transform3.localRotation = Quaternion.identity;
				transform3.localScale = Vector3.one;
				_itemChildObjects.Add((transform3, parent));
			}
		}
	}
}
