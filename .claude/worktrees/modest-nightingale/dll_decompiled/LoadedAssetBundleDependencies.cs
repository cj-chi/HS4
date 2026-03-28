public class LoadedAssetBundleDependencies
{
	public int ReferencedCount;

	public string Key { get; }

	public string[] BundleNames { get; }

	public LoadedAssetBundleDependencies(string key, string[] bundleNames)
	{
		Key = key;
		BundleNames = bundleNames;
		ReferencedCount = 1;
	}
}
