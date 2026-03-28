using System.Collections.Generic;
using System.Linq;

namespace ADV;

public class CharaParams : Params
{
	private List<CommandData> cachedList;

	private List<CommandData> _addList;

	public string HEADER { get; } = "";

	public string HEADER_PARAM => HEADER + "_";

	public override List<CommandData> list => cachedList ?? (cachedList = base.list.Concat(addList).ToList());

	public List<CommandData> addList
	{
		get
		{
			if (_addList != null)
			{
				return _addList;
			}
			_addList = new List<CommandData>(((ICommandData)base.data).CreateCommandData(HEADER_PARAM));
			CreateCommandData_Actor(_addList);
			return _addList;
		}
	}

	public IActor actor { get; private set; }

	public void Bind(IActor actor)
	{
		this.actor = actor;
	}

	public void CreateCommandData_Actor(List<CommandData> list)
	{
		if (actor != null)
		{
			list.Add(new CommandData(CommandData.Command.String, HEADER_PARAM + "[CharaName]", () => actor.chaFile.parameter.fullname));
			list.Add(new CommandData(CommandData.Command.Replace, HEADER, () => actor.chaFile.parameter.fullname));
		}
	}

	public CharaParams(ICommandData commandData, string HEADER)
		: base(commandData)
	{
		this.HEADER = HEADER;
		Initialize(HEADER_PARAM);
	}

	public override void Reset(string header = null)
	{
		_addList = null;
		cachedList = null;
	}
}
