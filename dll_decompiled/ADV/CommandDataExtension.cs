using System.Collections.Generic;

namespace ADV;

internal static class CommandDataExtension
{
	public static bool AddList(this ICommandData self, List<CommandData> list, string head = null)
	{
		if (self == null || list == null)
		{
			return false;
		}
		list.AddRange(self.CreateCommandData(head));
		return true;
	}
}
