using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime.Information;

public class PlayState
{
	[Serializable]
	public class PlayStateInfo
	{
		public AssetBundleData AssetBundleInfo = new AssetBundleData();

		public AnimStateInfo InStateInfo = new AnimStateInfo();

		public AnimStateInfo OutStateInfo = new AnimStateInfo();

		public float FadeOutTime = 1f;

		public bool IsLoop;

		public int LoopMin;

		public int LoopMax;
	}

	[Serializable]
	public class AnimStateInfo
	{
		public Info[] stateInfos = new Info[0];

		public float fadeTime;

		public bool EnableFade => !Mathf.Approximately(0f, fadeTime);
	}

	[Serializable]
	public class Info
	{
		private int? _nameHash;

		public string state = "";

		public int layer;

		public int nameHash
		{
			get
			{
				int? num = _nameHash;
				if (!num.HasValue)
				{
					int? num2 = (_nameHash = Animator.StringToHash(state));
					return num2.Value;
				}
				return num.GetValueOrDefault();
			}
		}
	}

	[Serializable]
	public class ActionInfo
	{
		public bool use;

		public int rate;
	}

	public int Layer { get; }

	public int DirectionType { get; }

	public bool EndEnableBlend => !Mathf.Approximately(0f, EndBlendRate);

	public float EndBlendRate { get; }

	public PlayStateInfo MainStateInfo { get; }

	public IReadOnlyList<PlayStateInfo> SubStateInfos { get; }

	public ActionInfo actionInfo { get; }

	public Info MaskStateInfo { get; }

	public IReadOnlyList<ItemInfo> itemInfoList { get; }

	public bool UseNeckLook { get; }

	public PlayState(AnimePlayStateParameter.Param param)
	{
		Layer = param.Layer;
		DirectionType = param.DirectionType;
		EndBlendRate = param.EndBlendRate;
		MainStateInfo = param.MainStateInfo;
		SubStateInfos = param.SubStateInfos;
		actionInfo = param.actionInfo;
		MaskStateInfo = param.MaskStateInfo;
		itemInfoList = param.itemInfoList;
		UseNeckLook = param.UseNeckLook;
	}
}
