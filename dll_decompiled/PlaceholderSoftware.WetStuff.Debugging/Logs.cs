using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Debugging;

public static class Logs
{
	private struct LogMessage
	{
		private readonly LogLevel _level;

		private readonly string _message;

		public LogMessage(string message, LogLevel level)
		{
			_message = message;
			_level = level;
		}

		public void Log()
		{
			switch (_level)
			{
			case LogLevel.Trace:
				UnityEngine.Debug.Log(_message);
				break;
			case LogLevel.Debug:
				UnityEngine.Debug.Log(_message);
				break;
			case LogLevel.Info:
				UnityEngine.Debug.Log(_message);
				break;
			case LogLevel.Warn:
				UnityEngine.Debug.LogWarning(_message);
				break;
			case LogLevel.Error:
				UnityEngine.Debug.LogError(_message);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private static readonly List<LogMessage> LogsFromOtherThreads = new List<LogMessage>(512);

	private static Thread _main;

	[NotNull]
	public static Log Create(LogCategory category, string name)
	{
		return Create((int)category, name);
	}

	[NotNull]
	public static Log Create(int category, string name)
	{
		return new Log(category, name);
	}

	public static void SetLogLevel(LogCategory category, LogLevel level)
	{
		SetLogLevel((int)category, level);
	}

	public static void SetLogLevel(int category, LogLevel level)
	{
		DebugSettings.Instance.SetLevel(category, level);
	}

	public static LogLevel GetLogLevel(LogCategory category)
	{
		return GetLogLevel((int)category);
	}

	public static LogLevel GetLogLevel(int category)
	{
		return DebugSettings.Instance.GetLevel(category);
	}

	internal static void WriteMultithreadedLogs()
	{
		if (_main == null)
		{
			_main = Thread.CurrentThread;
		}
		lock (LogsFromOtherThreads)
		{
			for (int i = 0; i < LogsFromOtherThreads.Count; i++)
			{
				LogsFromOtherThreads[i].Log();
			}
			LogsFromOtherThreads.Clear();
		}
	}

	internal static void SendLogMessage(string message, LogLevel level)
	{
		LogMessage item = new LogMessage(message, level);
		if (_main == Thread.CurrentThread)
		{
			item.Log();
			return;
		}
		lock (LogsFromOtherThreads)
		{
			LogsFromOtherThreads.Add(item);
		}
	}
}
