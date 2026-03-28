using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetAtlasUvs : MonoBehaviour
{
	[SerializeField]
	private bool updateEveryFrame;

	private Renderer render;

	private SpriteRenderer spriteRender;

	private Image uiImage;

	private bool isUI;

	private void Start()
	{
		if (GetRendererReferencesIfNeeded())
		{
			GetAndSetUVs();
		}
		else
		{
			base.enabled = false;
		}
		if (!updateEveryFrame && Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (updateEveryFrame)
		{
			GetAndSetUVs();
		}
	}

	public void GetAndSetUVs()
	{
		if (GetRendererReferencesIfNeeded())
		{
			if (!isUI)
			{
				Rect rect = spriteRender.sprite.rect;
				rect.x /= spriteRender.sprite.texture.width;
				rect.width /= spriteRender.sprite.texture.width;
				rect.y /= spriteRender.sprite.texture.height;
				rect.height /= spriteRender.sprite.texture.height;
				render.sharedMaterial.SetFloat("_MinXUV", rect.xMin);
				render.sharedMaterial.SetFloat("_MaxXUV", rect.xMax);
				render.sharedMaterial.SetFloat("_MinYUV", rect.yMin);
				render.sharedMaterial.SetFloat("_MaxYUV", rect.yMax);
			}
			else
			{
				Rect rect2 = uiImage.sprite.rect;
				rect2.x /= uiImage.sprite.texture.width;
				rect2.width /= uiImage.sprite.texture.width;
				rect2.y /= uiImage.sprite.texture.height;
				rect2.height /= uiImage.sprite.texture.height;
				uiImage.material.SetFloat("_MinXUV", rect2.xMin);
				uiImage.material.SetFloat("_MaxXUV", rect2.xMax);
				uiImage.material.SetFloat("_MinYUV", rect2.yMin);
				uiImage.material.SetFloat("_MaxYUV", rect2.yMax);
			}
		}
	}

	public void ResetAtlasUvs()
	{
		if (GetRendererReferencesIfNeeded())
		{
			if (!isUI)
			{
				render.sharedMaterial.SetFloat("_MinXUV", 0f);
				render.sharedMaterial.SetFloat("_MaxXUV", 1f);
				render.sharedMaterial.SetFloat("_MinYUV", 0f);
				render.sharedMaterial.SetFloat("_MaxYUV", 1f);
			}
			else
			{
				uiImage.material.SetFloat("_MinXUV", 0f);
				uiImage.material.SetFloat("_MaxXUV", 1f);
				uiImage.material.SetFloat("_MinYUV", 0f);
				uiImage.material.SetFloat("_MaxYUV", 1f);
			}
		}
	}

	public void UpdateEveryFrame(bool everyFrame)
	{
		updateEveryFrame = everyFrame;
	}

	private bool GetRendererReferencesIfNeeded()
	{
		if (spriteRender == null)
		{
			spriteRender = GetComponent<SpriteRenderer>();
		}
		if (spriteRender != null)
		{
			if (render == null)
			{
				render = GetComponent<Renderer>();
			}
			isUI = false;
		}
		else
		{
			if (uiImage == null)
			{
				uiImage = GetComponent<Image>();
				if (!(uiImage != null))
				{
					Object.DestroyImmediate(this);
					return false;
				}
			}
			if (render == null)
			{
				render = GetComponent<Renderer>();
			}
			isUI = true;
		}
		if (spriteRender == null && uiImage == null)
		{
			Object.DestroyImmediate(this);
			return false;
		}
		return true;
	}
}
