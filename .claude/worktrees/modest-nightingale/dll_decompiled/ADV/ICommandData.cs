using System.Collections.Generic;

namespace ADV;

public interface ICommandData
{
	IEnumerable<CommandData> CreateCommandData(string head);
}
