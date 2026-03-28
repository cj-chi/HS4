using System;
using System.Collections.Generic;
using System.Linq;

namespace ADV;

public class CharaPackData : IPack
{
	public Action onComplete { get; set; }

	public Dictionary<string, ValData> Vars { get; set; }

	public ICommandData[] commandData { get; private set; }

	public IParams[] param { get; private set; }

	public IReadOnlyCollection<CommandData> commandList => _commandList;

	private List<CommandData> _commandList { get; } = new List<CommandData>();

	public virtual bool isParent { get; set; }

	public void SetCommandData(params ICommandData[] commandData)
	{
		this.commandData = commandData.Where((ICommandData p) => p != null).ToArray();
	}

	public void SetParam(params IParams[] param)
	{
		this.param = param.Where((IParams p) => p != null).ToArray();
	}

	public virtual void Init()
	{
	}

	public virtual void Release()
	{
	}

	public virtual List<Program.Transfer> Create()
	{
		Vars = null;
		_commandList.Clear();
		List<Program.Transfer> list = Program.Transfer.NewList();
		if (commandData != null)
		{
			ICommandData[] array = commandData;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AddList(_commandList, "G_");
			}
			CommandData.CreateCommand(list, _commandList);
		}
		if (param != null)
		{
			IParams[] array2 = param;
			foreach (IParams obj in array2)
			{
				obj.param.Reset();
				obj.param.CreateCommand(list);
				_commandList.AddRange(obj.param.list);
			}
		}
		return list;
	}

	public virtual void Receive(TextScenario scenario)
	{
		foreach (CommandData command in _commandList)
		{
			command.ReceiveADV(scenario);
		}
		Vars = scenario.Vars;
		onComplete?.Invoke();
	}

	public Dictionary<int, List<CommandData>> CommandListToTable()
	{
		List<string> list = new List<string>();
		return _commandList.ToLookup((CommandData x) => Check(x.key)).ToDictionary((IGrouping<int, CommandData> v) => v.Key, (IGrouping<int, CommandData> v) => v.ToList());
		int Check(string key)
		{
			int num = 0;
			foreach (string item in list)
			{
				if (item == key)
				{
					num++;
				}
			}
			list.Add(key);
			return num;
		}
	}
}
