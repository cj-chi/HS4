using System;
using System.Diagnostics;

namespace PlaceholderSoftware.WetStuff.Debugging;

public class Log
{
	private readonly string _basicFormat;

	private readonly int _category;

	private readonly string _debugFormat;

	private readonly string _traceFormat;

	public bool IsTrace => ShouldLog(LogLevel.Trace);

	public bool IsDebug => ShouldLog(LogLevel.Debug);

	public bool IsInfo => ShouldLog(LogLevel.Info);

	public bool IsWarn => ShouldLog(LogLevel.Warn);

	public bool IsError => ShouldLog(LogLevel.Error);

	internal Log(int category, string name)
	{
		_category = category;
		_basicFormat = string.Concat("[WetStuff:", (LogCategory)category, "] ({0:HH:mm:ss.fff}) ", name, ": {1}");
		_debugFormat = "DEBUG " + _basicFormat;
		_traceFormat = "TRACE " + _basicFormat;
	}

	[DebuggerHidden]
	private bool ShouldLog(LogLevel level)
	{
		return level >= Logs.GetLogLevel(_category);
	}

	[DebuggerHidden]
	private void WriteLog(LogLevel level, string message)
	{
		if (ShouldLog(level))
		{
			string format;
			switch (level)
			{
			case LogLevel.Trace:
				format = _traceFormat;
				break;
			case LogLevel.Debug:
				format = _debugFormat;
				break;
			case LogLevel.Info:
			case LogLevel.Warn:
			case LogLevel.Error:
				format = _basicFormat;
				break;
			default:
				throw new ArgumentOutOfRangeException("level", level, null);
			}
			Logs.SendLogMessage(string.Format(format, DateTime.UtcNow, message), level);
		}
	}

	[DebuggerHidden]
	private void WriteLogFormat(LogLevel level, string format, params object[] parameters)
	{
		if (ShouldLog(level))
		{
			WriteLog(level, string.Format(format, parameters));
		}
	}

	[DebuggerHidden]
	private void WriteLogFormat<TA>(LogLevel level, string format, [CanBeNull] TA p0)
	{
		if (ShouldLog(level))
		{
			WriteLog(level, string.Format(format, p0));
		}
	}

	[DebuggerHidden]
	private void WriteLogFormat<TA, TB>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		if (ShouldLog(level))
		{
			WriteLog(level, string.Format(format, p0, p1));
		}
	}

	[DebuggerHidden]
	private void WriteLogFormat<TA, TB, TC>(LogLevel level, string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
	{
		if (ShouldLog(level))
		{
			WriteLog(level, string.Format(format, p0, p1, p2));
		}
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Trace(string message)
	{
		WriteLog(LogLevel.Trace, message);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Trace(string format, params object[] parameters)
	{
		WriteLogFormat(LogLevel.Trace, format, parameters);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Trace<TA>(string format, [CanBeNull] TA p0)
	{
		WriteLogFormat(LogLevel.Trace, format, p0);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Trace<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		WriteLogFormat(LogLevel.Trace, format, p0, p1);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Debug(string message)
	{
		WriteLog(LogLevel.Debug, message);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Debug(string format, params object[] parameters)
	{
		WriteLogFormat(LogLevel.Debug, format, parameters);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Debug<TA>(string format, [CanBeNull] TA p0)
	{
		WriteLogFormat(LogLevel.Debug, format, p0);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public void Debug<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		WriteLogFormat(LogLevel.Debug, format, p0, p1);
	}

	[DebuggerHidden]
	public void Info(string message)
	{
		WriteLog(LogLevel.Info, message);
	}

	[DebuggerHidden]
	public void Info(string format, params object[] parameters)
	{
		WriteLogFormat(LogLevel.Info, format, parameters);
	}

	[DebuggerHidden]
	public void Info<TA>(string format, [CanBeNull] TA p0)
	{
		WriteLogFormat(LogLevel.Info, format, p0);
	}

	[DebuggerHidden]
	public void Info<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		WriteLogFormat(LogLevel.Info, format, p0, p1);
	}

	[DebuggerHidden]
	public void Warn(string message)
	{
		WriteLog(LogLevel.Warn, message);
	}

	[DebuggerHidden]
	public void Warn(string format, params object[] parameters)
	{
		WriteLogFormat(LogLevel.Warn, format, parameters);
	}

	[DebuggerHidden]
	public void Warn<TA>(string format, [CanBeNull] TA p0)
	{
		WriteLogFormat(LogLevel.Warn, format, p0);
	}

	[DebuggerHidden]
	public void Warn<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		WriteLogFormat(LogLevel.Warn, format, p0, p1);
	}

	[DebuggerHidden]
	public void Error(string message)
	{
		WriteLog(LogLevel.Error, message);
	}

	[DebuggerHidden]
	public void Error(string format, params object[] parameters)
	{
		WriteLogFormat(LogLevel.Error, format, parameters);
	}

	[DebuggerHidden]
	public void Error<TA>(string format, [CanBeNull] TA p0)
	{
		WriteLogFormat(LogLevel.Error, format, p0);
	}

	[DebuggerHidden]
	public void Error<TA, TB>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1)
	{
		WriteLogFormat(LogLevel.Error, format, p0, p1);
	}

	[DebuggerHidden]
	public void Error<TA, TB, TC>(string format, [CanBeNull] TA p0, [CanBeNull] TB p1, [CanBeNull] TC p2)
	{
		WriteLogFormat(LogLevel.Error, format, p0, p1, p2);
	}

	[DebuggerHidden]
	[NotNull]
	public Exception CreateUserErrorException(string problem, string likelyCause, string documentationLink, string guid)
	{
		return LoggingConstants.Ex(UserErrorMessage(problem, likelyCause, documentationLink, guid));
	}

	[DebuggerHidden]
	[NotNull]
	public string UserErrorMessage(string problem, string likelyCause, string documentationLink, string guid)
	{
		string arg = string.Format("Error: {0}! This is likely caused by \"{1}\", see the documentation at \"{2}\" or visit the community at \"{3}\" to get help. Error ID: {4}", problem, likelyCause, documentationLink, "https://placeholder-software.co.uk/wetstuff/community", guid);
		return string.Format(_basicFormat, arg, DateTime.UtcNow);
	}

	[DebuggerHidden]
	[NotNull]
	public string PossibleBugMessage(string problem, string guid)
	{
		return string.Format("Error: {0}! This is probably a bug in Wet Stuff, we're sorry! Please report the bug on the issue tracker \"{1}\". You could also seek help on the community at \"{2}\" to get help for a temporary workaround. Error ID: {3}", problem, "https://placeholder-software.co.uk/wetstuff/issues", "https://placeholder-software.co.uk/wetstuff/community", guid);
	}

	[DebuggerHidden]
	[NotNull]
	public Exception CreatePossibleBugException(string problem, string guid)
	{
		return LoggingConstants.Ex(PossibleBugMessage(problem, guid));
	}

	[DebuggerHidden]
	[NotNull]
	public T CreatePossibleBugException<T>([NotNull] Func<string, T> factory, string problem, string guid) where T : Exception
	{
		return factory(PossibleBugMessage(problem, guid));
	}

	[ContractAnnotation("assertion:true => false; assertion:false => true")]
	public bool AssertAndLogWarn(bool assertion, string msg)
	{
		if (!assertion)
		{
			Warn(msg);
		}
		return !assertion;
	}

	[ContractAnnotation("assertion:true => false; assertion:false => true")]
	public bool AssertAndLogWarn<TA>(bool assertion, string format, TA arg0)
	{
		if (!assertion)
		{
			Warn(format, arg0);
		}
		return !assertion;
	}

	[ContractAnnotation("assertion:true => false; assertion:false => true")]
	public bool AssertAndLogError(bool assertion, string guid, string msg)
	{
		if (!assertion)
		{
			Error(PossibleBugMessage(msg, guid));
		}
		return !assertion;
	}

	[ContractAnnotation("assertion:true => false; assertion:false => true")]
	public bool AssertAndLogError<TA>(bool assertion, string guid, string format, TA arg0)
	{
		if (!assertion)
		{
			Error(PossibleBugMessage(string.Format(format, arg0), guid));
		}
		return !assertion;
	}

	[ContractAnnotation("assertion:false => halt")]
	public void AssertAndThrowPossibleBug(bool assertion, string guid, string msg)
	{
		if (!assertion)
		{
			throw CreatePossibleBugException(msg, guid);
		}
	}

	[ContractAnnotation("assertion:false => halt")]
	public void AssertAndThrowPossibleBug<TA>(bool assertion, string guid, string format, TA arg0)
	{
		if (!assertion)
		{
			throw CreatePossibleBugException(string.Format(format, arg0), guid);
		}
	}

	[ContractAnnotation("assertion:false => halt")]
	public void AssertAndThrowPossibleBug<TA, TB>(bool assertion, string guid, string format, TA arg0, TB arg1)
	{
		if (!assertion)
		{
			throw CreatePossibleBugException(string.Format(format, arg0, arg1), guid);
		}
	}

	[ContractAnnotation("assertion:false => halt")]
	public void AssertAndThrowPossibleBug<TA, TB, TC>(bool assertion, string guid, string format, TA arg0, TB arg1, TC arg2)
	{
		if (!assertion)
		{
			throw CreatePossibleBugException(string.Format(format, arg0, arg1, arg2), guid);
		}
	}
}
