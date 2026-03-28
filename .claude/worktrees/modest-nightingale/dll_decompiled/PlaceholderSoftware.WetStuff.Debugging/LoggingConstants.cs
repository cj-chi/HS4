namespace PlaceholderSoftware.WetStuff.Debugging;

internal class LoggingConstants
{
	internal const string AssetName = "WetStuff";

	internal const string BaseUrl = "https://placeholder-software.co.uk/wetstuff";

	internal const string CommunityUrl = "https://placeholder-software.co.uk/wetstuff/community";

	internal const string IssueTrackerUrl = "https://placeholder-software.co.uk/wetstuff/issues";

	[NotNull]
	internal static WetSurfaceDecalsException Ex(string message)
	{
		return new WetSurfaceDecalsException(message);
	}
}
