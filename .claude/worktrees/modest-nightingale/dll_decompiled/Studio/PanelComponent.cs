using UnityEngine;

namespace Studio;

public class PanelComponent : MonoBehaviour
{
	public Renderer[] renderer;

	public Color defColor = Color.white;

	public Vector4 defUV = Vector4.one;

	public float defRot;

	public bool defClamp = true;

	public void SetMainTex(Texture2D _texture)
	{
		Renderer[] array = renderer;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material.SetTexture(ItemShader._MainTex, _texture);
		}
	}

	public void UpdateColor(OIItemInfo _info)
	{
		Renderer[] array = renderer;
		foreach (Renderer obj in array)
		{
			obj.material.SetColor(ItemShader._Color, _info.colors[0].mainColor);
			obj.material.SetVector(ItemShader._patternuv1, _info.colors[0].pattern.uv);
			obj.material.SetFloat(ItemShader._patternuv1Rotator, _info.colors[0].pattern.rot);
			obj.material.SetFloat(ItemShader._patternclamp1, _info.colors[0].pattern.clamp ? 1f : 0f);
		}
	}

	public void Setup()
	{
		Renderer renderer = this.renderer.SafeGet(0);
		if (!(renderer == null))
		{
			Material sharedMaterial = renderer.sharedMaterial;
			defColor = sharedMaterial.GetColor("_Color");
			defUV = sharedMaterial.GetVector("_patternuv1");
			defRot = sharedMaterial.GetFloat("_patternuv1Rotator");
			defClamp = !Mathf.Approximately(sharedMaterial.GetFloat("_patternclamp1"), 0f);
		}
	}
}
