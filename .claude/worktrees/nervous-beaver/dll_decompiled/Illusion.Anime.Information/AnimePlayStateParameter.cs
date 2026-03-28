using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illusion.Anime.Information;

public class AnimePlayStateParameter : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int poseID = -1;

		public int Layer;

		public int DirectionType;

		public float EndBlendRate;

		public PlayState.PlayStateInfo MainStateInfo = new PlayState.PlayStateInfo();

		public List<PlayState.PlayStateInfo> SubStateInfos = new List<PlayState.PlayStateInfo>();

		public PlayState.ActionInfo actionInfo = new PlayState.ActionInfo();

		public PlayState.Info MaskStateInfo = new PlayState.Info();

		public List<ItemInfo> itemInfoList = new List<ItemInfo>();

		public bool UseNeckLook;
	}

	public List<Param> param = new List<Param>();
}
