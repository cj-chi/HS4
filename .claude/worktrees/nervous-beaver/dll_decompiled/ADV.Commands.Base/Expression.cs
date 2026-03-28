using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Manager;

namespace ADV.Commands.Base;

public class Expression : CommandBase
{
	public class Data : Manager.Game.Expression, TextScenario.IExpression, TextScenario.IChara
	{
		public int no { get; private set; }

		public Data(string[] args)
			: base(args)
		{
		}

		public Data(string[] args, ref int cnt)
			: base(args, ref cnt)
		{
		}

		public Data(int no, Manager.Game.Expression src)
		{
			this.no = no;
			src.Copy(this);
		}

		public override void Initialize(string[] args, ref int cnt, bool isThrow = false)
		{
			try
			{
				no = int.Parse(args[cnt++]);
				cnt++;
				base.Initialize(args, ref cnt, isThrow: true);
			}
			catch (Exception)
			{
				if (isThrow)
				{
					throw new Exception(string.Join(",", args));
				}
			}
		}

		public void Play(TextScenario scenario)
		{
			Change(GetChara(scenario).chaCtrl);
		}

		public CharaData GetChara(TextScenario scenario)
		{
			return scenario.commandController.GetChara(no);
		}
	}

	public override string[] ArgsLabel => new string[13]
	{
		"No", "ExpName", "眉", "目", "口", "眉開き", "目開き", "口開き", "視線", "頬赤",
		"ハイライト", "涙", "瞬き"
	};

	public override string[] ArgsDefault => new string[2]
	{
		int.MaxValue.ToString(),
		null
	};

	public static IReadOnlyCollection<Data> Convert(ref string[] args, TextScenario scenario)
	{
		List<Data> list = new List<Data>();
		if (args.Length > 1)
		{
			int cnt = 0;
			while (!args.IsNullOrEmpty(cnt))
			{
				string expName = null;
				if (args.SafeProc(cnt + 1, delegate(string s)
				{
					expName = s;
				}))
				{
					int no = int.Parse(args[cnt]);
					IParams obj = scenario.commandController.GetChara(no)?.data?.param;
					Manager.Game.Expression expression = Singleton<Manager.Game>.Instance.GetExpression((obj as Actor.CharaData)?.voiceNo ?? 0, expName);
					if (expression != null)
					{
						list.Add(new Data(no, expression));
						cnt += 2;
						continue;
					}
				}
				Data data = new Data(args, ref cnt);
				data.IsChangeSkip = true;
				list.Add(data);
			}
		}
		return list;
	}

	public override void Do()
	{
		base.Do();
		base.scenario.currentCharaData.CreateExpressionList();
		List<TextScenario.IExpression[]> expressionList = base.scenario.currentCharaData.expressionList;
		TextScenario.IExpression[] item = Convert(ref args, base.scenario).ToArray();
		expressionList.Add(item);
	}
}
