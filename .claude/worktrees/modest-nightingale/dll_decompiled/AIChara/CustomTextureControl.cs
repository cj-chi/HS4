using Manager;
using UnityEngine;

namespace AIChara;

public class CustomTextureControl
{
	public CustomTextureCreate[] createCustomTex;

	public Material matDraw { get; private set; }

	public CustomTextureControl(int num, string drawMatManifest, string drawMatABName, string drawMatName, Transform trfParent = null)
	{
		createCustomTex = new CustomTextureCreate[num];
		for (int i = 0; i < num; i++)
		{
			createCustomTex[i] = new CustomTextureCreate(trfParent);
		}
		matDraw = CommonLib.LoadAsset<Material>(drawMatABName, drawMatName, clone: true, drawMatManifest);
		Singleton<Character>.Instance.AddLoadAssetBundle(drawMatABName, drawMatManifest);
	}

	public bool Initialize(int index, string createMatManifest, string createMatABName, string createMatName, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32)
	{
		if (createCustomTex == null)
		{
			return false;
		}
		if (index >= createCustomTex.Length || createCustomTex[index] == null)
		{
			return false;
		}
		if (!createCustomTex[index].Initialize(createMatManifest, createMatABName, createMatName, width, height, format))
		{
			return false;
		}
		return true;
	}

	public void Release()
	{
		CustomTextureCreate[] array = createCustomTex;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Release();
		}
		Object.Destroy(matDraw);
		matDraw = null;
	}

	public bool SetNewCreateTexture(int index, int propertyId)
	{
		if (createCustomTex == null)
		{
			return false;
		}
		if (index >= createCustomTex.Length || createCustomTex[index] == null)
		{
			return false;
		}
		createCustomTex[index].RebuildTextureAndSetMaterial();
		Texture createTexture = createCustomTex[index].GetCreateTexture();
		if (null != matDraw)
		{
			matDraw.SetTexture(propertyId, createTexture);
		}
		return true;
	}
}
