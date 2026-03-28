using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Anime;
using Illusion.Anime.Information;
using UnityEngine;

namespace ADV.Commands.Base;

public class Motion : CommandBase
{
	public class Data : TextScenario.IMotion, TextScenario.IChara
	{
		public int no { get; }

		public int ID { get; }

		public Data(string[] args, ref int cnt)
		{
			try
			{
				no = int.Parse(args[cnt++]);
				string text = args[cnt++];
				if (int.TryParse(text, out var result))
				{
					ID = result;
					return;
				}
				int? num = null;
				int num2 = Animator.StringToHash(text);
				foreach (KeyValuePair<int, PlayState> item in Loader.AnimePlayStateTable)
				{
					if (num2 == item.Value.MainStateInfo.InStateInfo.stateInfos[0].nameHash)
					{
						num = item.Key;
						break;
					}
				}
				ID = num ?? (-1);
			}
			catch (Exception)
			{
			}
		}

		public void Play(TextScenario scenario)
		{
			GetChara(scenario).MotionPlay(this, isCrossFade: false);
		}

		public CharaData GetChara(TextScenario scenario)
		{
			return scenario.commandController.GetChara(no);
		}
	}

	public override string[] ArgsLabel => new string[2] { "No", "State" };

	public override string[] ArgsDefault => new string[2]
	{
		int.MaxValue.ToString(),
		null
	};

	public static IReadOnlyCollection<Data> Convert(ref string[] args, TextScenario scenario, int argsLen)
	{
		List<Data> list = new List<Data>();
		int cnt = 0;
		if (args.Length > 1)
		{
			while (!args.IsNullOrEmpty(cnt))
			{
				list.Add(new Data(args, ref cnt));
			}
		}
		return list;
	}

	public override void Do()
	{
		base.Do();
		base.scenario.currentCharaData.CreateMotionList();
		List<TextScenario.IMotion[]> motionList = base.scenario.currentCharaData.motionList;
		TextScenario.IMotion[] item = Convert(ref args, base.scenario, ArgsLabel.Length).ToArray();
		motionList.Add(item);
	}
}
