using System;
using Illusion.Extensions;
using Manager;
using SceneAssist;
using UIAnimatorCore;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LobbyMapSelectInfoComponent : MonoBehaviour
{
	[Serializable]
	public class RowInfo
	{
		public LobbyMapSelectInfoScrollController.ScrollData scrollInfo;

		public MapInfo.Param info;

		public Button btn;

		public RawImage riMap;

		public Text text;

		public GameObject objSelect;

		public GameObject objLock;

		public UIAnimator uiAnimator;

		public PointerEnterExitAction pointerAction;

		public SingleAssignmentDisposable disposableUpdate;

		public bool isMoveMap;
	}

	[SerializeField]
	private RowInfo[] rows;

	public void SetData(int _index, bool _isFreeH, LobbyMapSelectInfoScrollController.ScrollData _info, Action _onClickAction)
	{
		bool flag = _info != null && _info.info != null;
		rows[_index].btn.gameObject.SetActiveIfDifferent(flag);
		rows[_index].btn.onClick.RemoveAllListeners();
		if (!flag)
		{
			return;
		}
		MapInfo.Param info = _info.info;
		if ((bool)rows[_index].text)
		{
			rows[_index].text.text = _info.info.MapNames[Singleton<GameSystem>.Instance.languageInt];
		}
		if ((bool)rows[_index].riMap)
		{
			rows[_index].riMap.texture = CommonLib.LoadAsset<Texture2D>(info.ThumbnailBundle_S, info.ThumbnailAsset_S, clone: false, info.ThumbnailManifest_S);
			AssetBundleManager.UnloadAssetBundle(info.ThumbnailBundle_S, isUnloadForceRefCount: true);
		}
		rows[_index].btn.interactable = _info.isEnable;
		if ((bool)rows[_index].objLock)
		{
			rows[_index].objLock.SetActiveIfDifferent(_info.isLock);
		}
		if ((bool)rows[_index].objSelect)
		{
			rows[_index].objSelect.SetActiveIfDifferent(active: false);
		}
		int sel = _index;
		if ((bool)rows[_index].pointerAction)
		{
			rows[_index].pointerAction.listActionEnter.Clear();
			rows[_index].pointerAction.listActionEnter.Add(delegate
			{
				if (rows[sel].btn.interactable)
				{
					if ((bool)rows[_index].objSelect)
					{
						rows[sel].objSelect.SetActiveIfDifferent(active: true);
					}
					rows[_index].isMoveMap = true;
					if ((bool)rows[_index].uiAnimator)
					{
						rows[_index].uiAnimator.Paused = false;
					}
				}
			});
			rows[_index].pointerAction.listActionExit.Clear();
			rows[_index].pointerAction.listActionExit.Add(delegate
			{
				if ((bool)rows[_index].objSelect)
				{
					rows[sel].objSelect.SetActiveIfDifferent(active: false);
				}
				rows[_index].isMoveMap = false;
				if ((bool)rows[_index].uiAnimator)
				{
					rows[_index].uiAnimator.ResetToStart();
					rows[_index].uiAnimator.Paused = true;
				}
			});
		}
		rows[_index].info = info;
		rows[_index].scrollInfo = _info;
		if (rows[_index].disposableUpdate != null)
		{
			rows[_index].disposableUpdate.Dispose();
			rows[_index].disposableUpdate = null;
		}
		rows[_index].disposableUpdate = new SingleAssignmentDisposable();
		rows[_index].disposableUpdate.Disposable = (from _ in this.UpdateAsObservable()
			where rows[_index].isMoveMap
			select _).Subscribe(delegate
		{
			if ((bool)rows[_index].uiAnimator && !rows[_index].uiAnimator.IsPlaying)
			{
				rows[_index].uiAnimator.PlayAnimation(AnimSetupType.Outro);
			}
		}).AddTo(this);
	}

	public MapInfo.Param GetListInfo(int _index)
	{
		return rows[_index].info;
	}

	public void SetListInfo(int _index, MapInfo.Param _info)
	{
		rows[_index].info = _info;
	}

	public RowInfo GetRow(int _index)
	{
		return rows[_index];
	}

	public void Disable(bool disable)
	{
	}

	public void Disvisible(bool disvisible)
	{
	}
}
