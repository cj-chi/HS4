using UnityEngine;

namespace AIChara;

public class CustomTextureCreate
{
	private RenderTextureFormat rtFormat;

	private RenderTexture createTex;

	private Material matCreate;

	private Texture texMain;

	public Transform trfParent;

	public int baseW { get; private set; }

	public int baseH { get; private set; }

	public CustomTextureCreate(Transform _trfParent = null)
	{
		trfParent = _trfParent;
	}

	public bool Initialize(string createMatManifest, string createMatABName, string createMatName, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32)
	{
		baseW = width;
		baseH = height;
		rtFormat = format;
		matCreate = CommonLib.LoadAsset<Material>(createMatABName, createMatName, clone: true);
		if (null == matCreate)
		{
			return false;
		}
		texMain = matCreate.GetTexture(ChaShader.MainTex);
		createTex = new RenderTexture(baseW, baseH, 0, rtFormat);
		createTex.useMipMap = true;
		return true;
	}

	public void Release()
	{
		Object.Destroy(createTex);
		createTex = null;
		Object.Destroy(matCreate);
		matCreate = null;
	}

	public void ReleaseCreateMaterial()
	{
		Object.Destroy(matCreate);
		matCreate = null;
	}

	public void SetMainTexture(Texture tex)
	{
		if (!(null == matCreate))
		{
			texMain = tex;
		}
	}

	public void SetTexture(string propertyName, Texture tex)
	{
		if (null != matCreate)
		{
			matCreate.SetTexture(propertyName, tex);
		}
	}

	public void SetTexture(int propertyID, Texture tex)
	{
		if (null != matCreate)
		{
			matCreate.SetTexture(propertyID, tex);
		}
	}

	public void SetColor(string propertyName, Color color)
	{
		if (null != matCreate)
		{
			matCreate.SetColor(propertyName, color);
		}
	}

	public Color GetColor(string propertyName)
	{
		if (!(null != matCreate))
		{
			return Color.white;
		}
		return matCreate.GetColor(propertyName);
	}

	public void SetColor(int propertyID, Color color)
	{
		if (null != matCreate)
		{
			matCreate.SetColor(propertyID, color);
		}
	}

	public Color GetColor(int propertyID)
	{
		if (!(null != matCreate))
		{
			return Color.white;
		}
		return matCreate.GetColor(propertyID);
	}

	public void SetOffsetAndTilingDirect(string propertyName, float tx, float ty, float ox, float oy)
	{
		if (!(null == matCreate))
		{
			matCreate.SetTextureOffset(propertyName, new Vector2(ox, oy));
			matCreate.SetTextureScale(propertyName, new Vector2(tx, ty));
		}
	}

	public void SetOffsetAndTilingDirect(int propertyID, float tx, float ty, float ox, float oy)
	{
		if (!(null == matCreate))
		{
			matCreate.SetTextureOffset(propertyID, new Vector2(ox, oy));
			matCreate.SetTextureScale(propertyID, new Vector2(tx, ty));
		}
	}

	public void SetOffsetAndTiling(string propertyName, int addW, int addH, float addPx, float addPy)
	{
		if (!(null == matCreate))
		{
			float num = (float)baseW / (float)addW;
			float num2 = (float)baseH / (float)addH;
			float ox = (0f - addPx / (float)baseW) * num;
			float oy = (0f - ((float)baseH - addPy - (float)addH) / (float)baseH) * num2;
			SetOffsetAndTilingDirect(propertyName, num, num2, ox, oy);
		}
	}

	public void SetOffsetAndTiling(int propertyID, int addW, int addH, float addPx, float addPy)
	{
		if (!(null == matCreate))
		{
			float num = (float)baseW / (float)addW;
			float num2 = (float)baseH / (float)addH;
			float ox = (0f - addPx / (float)baseW) * num;
			float oy = (0f - ((float)baseH - addPy - (float)addH) / (float)baseH) * num2;
			SetOffsetAndTilingDirect(propertyID, num, num2, ox, oy);
		}
	}

	public void SetFloat(string propertyName, float value)
	{
		if (null != matCreate)
		{
			matCreate.SetFloat(propertyName, value);
		}
	}

	public float GetFloat(string propertyName)
	{
		if (!(null != matCreate))
		{
			return 0f;
		}
		return matCreate.GetFloat(propertyName);
	}

	public void SetFloat(int propertyID, float value)
	{
		if (null != matCreate)
		{
			matCreate.SetFloat(propertyID, value);
		}
	}

	public float GetFloat(int propertyID)
	{
		if (!(null != matCreate))
		{
			return 0f;
		}
		return matCreate.GetFloat(propertyID);
	}

	public void SetVector4(string propertyName, Vector4 value)
	{
		if (null != matCreate)
		{
			matCreate.SetVector(propertyName, value);
		}
	}

	public Vector4 GetVector4(string propertyName)
	{
		if (!(null != matCreate))
		{
			return Vector4.zero;
		}
		return matCreate.GetVector(propertyName);
	}

	public void SetVector4(int propertyID, Vector4 value)
	{
		if (null != matCreate)
		{
			matCreate.SetVector(propertyID, value);
		}
	}

	public Vector4 GetVector4(int propertyID)
	{
		if (!(null != matCreate))
		{
			return Vector4.zero;
		}
		return matCreate.GetVector(propertyID);
	}

	public Texture RebuildTextureAndSetMaterial()
	{
		if (null == matCreate)
		{
			return null;
		}
		bool sRGBWrite = GL.sRGBWrite;
		GL.sRGBWrite = true;
		Graphics.SetRenderTarget(createTex);
		GL.Clear(clearDepth: false, clearColor: true, Color.clear);
		Graphics.SetRenderTarget(null);
		Graphics.Blit(texMain, createTex, matCreate, 0);
		GL.sRGBWrite = sRGBWrite;
		return createTex;
	}

	public Texture GetCreateTexture()
	{
		return createTex;
	}
}
