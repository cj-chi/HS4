using UnityEngine;

namespace KK_Lib;

public class WaitTime
{
	private const float intervalTime = 0.03f;

	private float nextFrameTime;

	public bool isOver => Time.realtimeSinceStartup >= nextFrameTime;

	public WaitTime()
	{
		Next();
	}

	public void Next()
	{
		nextFrameTime = Time.realtimeSinceStartup + 0.03f;
	}
}
