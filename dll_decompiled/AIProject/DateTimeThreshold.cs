using System;

namespace AIProject;

public struct DateTimeThreshold
{
	public DateTime start;

	public DateTime end;

	public DateTimeThreshold(DateTime start, DateTime end)
	{
		this.start = start;
		this.end = end;
	}

	public bool Contains(DateTime time)
	{
		if (end > start)
		{
			if (time > start)
			{
				return time < end;
			}
			return false;
		}
		if (time > start && time > end)
		{
			if (time > start)
			{
				return time < new DateTime(1, 1, 1, 24, 0, 0);
			}
			return false;
		}
		if (time > new DateTime(1, 1, 1, 0, 0, 0))
		{
			return time < end;
		}
		return false;
	}
}
