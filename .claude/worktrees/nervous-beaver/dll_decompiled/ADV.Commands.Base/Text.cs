using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Manager;

namespace ADV.Commands.Base;

public class Text : CommandBase
{
	public class Data
	{
		public string name { get; set; } = "";

		public string text { get; set; } = "";

		public string colorKey { get; private set; }

		public Data(params string[] args)
		{
			int num = 1;
			int num2 = num;
			if (Singleton<GameSystem>.IsInstance())
			{
				num2 += Singleton<GameSystem>.Instance.languageInt;
			}
			colorKey = (name = args.SafeGet(0) ?? string.Empty);
			this.text = args.SafeGet(num2) ?? args.SafeGet(num) ?? string.Empty;
		}
	}

	private class Next
	{
		private int currentNo { get; set; }

		private List<List<Action>> playList { get; } = new List<List<Action>>();

		private TextScenario scenario { get; }

		private Action onChange { get; set; }

		private bool voicePlayEnd { get; set; }

		public Next(TextScenario scenario)
		{
			this.scenario = scenario;
			TextScenario.CurrentCharaData currentCharaData = scenario.currentCharaData;
			List<TextScenario.IMotion[]> motionList = currentCharaData.motionList;
			List<TextScenario.IExpression[]> expressionList = currentCharaData.expressionList;
			int cnt = 0;
			while (IsMotion() || IsExp())
			{
				TextScenario.IMotion[] motions = ((!IsMotion()) ? null : motionList[cnt]);
				TextScenario.IExpression[] expressions = ((!IsExp()) ? null : expressionList[cnt]);
				playList.Add(new List<Action>
				{
					delegate
					{
						Play((IReadOnlyCollection<TextScenario.IExpression>)(object)expressions);
					},
					delegate
					{
						Play((IReadOnlyCollection<TextScenario.IMotion>)(object)motions);
					}
				});
				int num = cnt + 1;
				cnt = num;
			}
			if (!currentCharaData.voiceList.IsNullOrEmpty())
			{
				onChange = delegate
				{
					Play();
				};
				scenario.VoicePlay(currentCharaData.voiceList, onChange, delegate
				{
					voicePlayEnd = true;
				});
			}
			bool IsExp()
			{
				if (!expressionList.IsNullOrEmpty())
				{
					return expressionList.SafeGet(cnt) != null;
				}
				return false;
			}
			bool IsMotion()
			{
				if (!motionList.IsNullOrEmpty())
				{
					return motionList.SafeGet(cnt) != null;
				}
				return false;
			}
		}

		private bool Play()
		{
			return playList.SafeProc(currentNo++, delegate(List<Action> p)
			{
				p.ForEach(delegate(Action proc)
				{
					proc();
				});
			});
		}

		public bool Process()
		{
			if (scenario.currentCharaData.isSkip)
			{
				return true;
			}
			if (playList.Count <= currentNo && voicePlayEnd)
			{
				return true;
			}
			if (onChange == null)
			{
				bool flag = false;
				List<TextScenario.IMotion[]> motionList = scenario.currentCharaData.motionList;
				if (currentNo == 0)
				{
					flag = true;
				}
				else
				{
					bool flag2 = false;
					if (!motionList.IsNullOrEmpty() && currentNo < motionList.Count)
					{
						flag2 = !((IReadOnlyCollection<TextScenario.IMotion>)(object)motionList[currentNo]).IsNullOrEmpty();
					}
					TextScenario.IMotion[] array = (flag2 ? motionList[currentNo - 1] : null);
					flag = ((IReadOnlyCollection<TextScenario.IMotion>)(object)array).IsNullOrEmpty() || MotionEndCheck((IReadOnlyCollection<TextScenario.IMotion>)(object)array);
				}
				if (flag && !Play())
				{
					bool lastMotionEnd = true;
					bool flag3 = !motionList.IsNullOrEmpty();
					if (flag3)
					{
						flag3 = motionList.LastOrDefault().SafeProc(delegate(TextScenario.IMotion[] last)
						{
							lastMotionEnd = MotionEndCheck((IReadOnlyCollection<TextScenario.IMotion>)(object)last);
						});
					}
					if (flag3 && !lastMotionEnd)
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public void Result()
		{
			while (Play())
			{
			}
		}

		private void Play(IReadOnlyCollection<TextScenario.IMotion> motions)
		{
			if (motions.IsNullOrEmpty())
			{
				return;
			}
			scenario.CrossFadeStart();
			foreach (TextScenario.IMotion motion in motions)
			{
				motion.Play(scenario);
			}
		}

		private void Play(IReadOnlyCollection<TextScenario.IExpression> expressions)
		{
			if (expressions.IsNullOrEmpty())
			{
				return;
			}
			foreach (TextScenario.IExpression expression in expressions)
			{
				expression.Play(scenario);
			}
		}

		private bool MotionEndCheck(IReadOnlyCollection<TextScenario.IMotion> motions)
		{
			return motions.All((TextScenario.IMotion motion) => endCheck(motion.GetChara(scenario).chaCtrl));
			static bool endCheck(ChaControl chaCtrl)
			{
				return chaCtrl.getAnimatorStateInfo(0).normalizedTime >= 1f;
			}
		}
	}

	public override string[] ArgsLabel => new string[2] { "Name", "Text" };

	public override string[] ArgsDefault => null;

	private Next next { get; set; }

	public override void Convert(string fileName, ref string[] args)
	{
		int index = 1;
		string text = args.SafeGet(index);
		if (!text.IsNullOrEmpty())
		{
			args[index++] = string.Join("\n", text.Split(new string[1] { "@br" }, StringSplitOptions.None));
		}
	}

	public override void Do()
	{
		base.Do();
		Data outPut = new Data(args);
		base.scenario.fontColorKey = outPut.name;
		if (outPut.name != string.Empty)
		{
			outPut.name = base.scenario.ReplaceText(outPut.name);
		}
		if (outPut.text != string.Empty)
		{
			outPut.text = base.scenario.ReplaceText(outPut.text);
		}
		TextScenario.CurrentCharaData currentCharaData = base.scenario.currentCharaData;
		if (currentCharaData.isSkip)
		{
			base.scenario.VoicePlay(null, null, null);
		}
		currentCharaData.isSkip = false;
		base.scenario.textController.Set(outPut.name, outPut.text);
		(base.scenario as MainScenario).SafeProc(delegate(MainScenario main)
		{
			main.BackLog.Add(new BackLog.Data(base.localLine, outPut, base.scenario.currentCharaData.isKaraoke ? null : base.scenario.currentCharaData.voiceList));
		});
		next = new Next(base.scenario);
	}

	public override bool Process()
	{
		base.Process();
		return next.Process();
	}

	public override void Result(bool processEnd)
	{
		base.Result(processEnd);
		next.Result();
	}
}
