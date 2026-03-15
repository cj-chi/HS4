using System.Collections.Generic;

namespace ADV;

public interface IPack
{
	IParams[] param { get; }

	IReadOnlyCollection<CommandData> commandList { get; }

	bool isParent { get; }

	List<Program.Transfer> Create();

	void Receive(TextScenario scenario);
}
