using System.Diagnostics;

public static class Debug_CheckTheTime
{
	public static long time = 0L;

	public static Stopwatch sw = new Stopwatch();

	[Conditional("UNITY_EDITOR")]
	public static void StartTime()
	{
		sw.Start();
	}

	[Conditional("UNITY_EDITOR")]
	public static void StopTime()
	{
		sw.Stop();
		time += sw.ElapsedMilliseconds;
	}

	[Conditional("UNITY_EDITOR")]
	public static void ResetTime()
	{
		sw.Reset();
		time = 0L;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawTime()
	{
	}
}
