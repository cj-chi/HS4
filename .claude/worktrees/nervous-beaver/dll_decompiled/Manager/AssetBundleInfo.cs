namespace Manager;

public struct AssetBundleInfo
{
	public string name;

	public string assetbundle;

	public string asset;

	public string manifest;

	public AssetBundleInfo(string _name, string _assetbundle, string _asset, string _manifest = "")
	{
		name = _name;
		assetbundle = _assetbundle;
		asset = _asset;
		manifest = _manifest;
	}
}
