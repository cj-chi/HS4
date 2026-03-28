using System.Collections.Generic;
using AIChara;
using Illusion.Anime;

public static class ControllerExtensions
{
	public static Controller Get(this IReadOnlyDictionary<int, Controller> table, ChaControl chaCtrl)
	{
		if (chaCtrl == null)
		{
			return null;
		}
		Controller.Table.TryGetValue(chaCtrl.GetInstanceID(), out var value);
		return value;
	}
}
