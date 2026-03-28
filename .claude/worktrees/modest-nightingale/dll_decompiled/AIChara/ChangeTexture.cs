using UnityEngine;

namespace AIChara;

public class ChangeTexture
{
	public static void SetTexture(string propertyName, Texture tex, int idx, params Renderer[] arrRend)
	{
		SetTexture(Shader.PropertyToID(propertyName), tex, idx, arrRend);
	}

	public static void SetTexture(int propertyID, Texture tex, int idx, params Renderer[] arrRend)
	{
		if (arrRend == null || arrRend.Length == 0)
		{
			return;
		}
		foreach (Renderer renderer in arrRend)
		{
			if (idx < renderer.materials.Length)
			{
				Material material = renderer.materials[idx];
				if (null != material)
				{
					material.SetTexture(propertyID, tex);
				}
			}
		}
	}

	public static void SetTexture(string propertyName, Texture tex, params Material[] mat)
	{
		SetTexture(Shader.PropertyToID(propertyName), tex, mat);
	}

	public static void SetTexture(int propertyID, Texture tex, params Material[] mat)
	{
		if (mat != null && mat.Length != 0)
		{
			for (int i = 0; i < mat.Length; i++)
			{
				mat[i].SetTexture(propertyID, tex);
			}
		}
	}
}
