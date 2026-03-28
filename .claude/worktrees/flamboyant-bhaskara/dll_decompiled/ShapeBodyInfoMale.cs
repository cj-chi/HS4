using UnityEngine;

public class ShapeBodyInfoMale : ShapeBodyInfoFemale
{
	public override void InitShapeInfo(string manifest, string assetBundleAnmKey, string assetBundleCategory, string anmKeyInfoPath, string cateInfoPath, Transform trfObj)
	{
		base.InitShapeInfo(manifest, assetBundleAnmKey, assetBundleCategory, anmKeyInfoPath, cateInfoPath, trfObj);
	}

	public override void ForceUpdate()
	{
		base.ForceUpdate();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void UpdateAlways()
	{
		base.UpdateAlways();
	}
}
