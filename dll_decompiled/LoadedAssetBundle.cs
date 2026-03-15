using UnityEngine;

public class LoadedAssetBundle
{
	public uint ReferencedCount;

	public AssetBundle Bundle { get; }

	public LoadedAssetBundle(AssetBundle assetBundle)
	{
		Bundle = assetBundle;
		ReferencedCount = 1u;
	}
}
