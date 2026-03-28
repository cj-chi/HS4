using UnityEngine;

namespace AIChara;

public class ChangeColor
{
	public static void SetColor(string propertyName, Color color, int idx, params Renderer[] arrRend)
	{
		SetColor(Shader.PropertyToID(propertyName), color, idx, arrRend);
	}

	public static void SetColor(int propertyID, Color color, int idx, params Renderer[] arrRend)
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
					material.SetColor(propertyID, color);
				}
			}
		}
	}

	public static void SetColor(string propertyName, Color color, params Material[] mat)
	{
		SetColor(Shader.PropertyToID(propertyName), color, mat);
	}

	public static void SetColor(int propertyID, Color color, params Material[] mat)
	{
		if (mat != null && mat.Length != 0)
		{
			for (int i = 0; i < mat.Length; i++)
			{
				mat[i].SetColor(propertyID, color);
			}
		}
	}
}
