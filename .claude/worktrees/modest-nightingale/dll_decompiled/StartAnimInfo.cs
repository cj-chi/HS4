using System.Collections.Generic;

public class StartAnimInfo
{
	public class StartAnimIDs
	{
		public List<int>[] ID = new List<int>[3]
		{
			new List<int>(),
			new List<int>(),
			new List<int>()
		};
	}

	public StartAnimIDs[] AnimIDs = new StartAnimIDs[2]
	{
		new StartAnimIDs(),
		new StartAnimIDs()
	};
}
