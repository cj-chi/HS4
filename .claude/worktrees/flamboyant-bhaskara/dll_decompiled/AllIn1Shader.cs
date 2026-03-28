using System.IO;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("AllIn1SpriteShader/AddAllIn1Shader")]
public class AllIn1Shader : MonoBehaviour
{
	private enum AfterSetAction
	{
		Clear,
		CopyMaterial,
		Reset
	}

	private Material currMaterial;

	private Material prevMaterial;

	private bool matAssigned;

	private bool destroyed;

	public void MakeNewMaterial()
	{
		SetMaterial(AfterSetAction.Clear);
	}

	public void MakeCopy()
	{
		SetMaterial(AfterSetAction.CopyMaterial);
	}

	private void ResetAllProperties()
	{
		SetMaterial(AfterSetAction.Reset);
	}

	private void SetMaterial(AfterSetAction action)
	{
		Shader shader = Resources.Load("AllIn1SpriteShader", typeof(Shader)) as Shader;
		if (!Application.isPlaying && Application.isEditor && shader != null)
		{
			bool flag = false;
			if (GetComponent<SpriteRenderer>() != null)
			{
				flag = true;
				prevMaterial = new Material(GetComponent<Renderer>().sharedMaterial);
				currMaterial = new Material(shader);
				GetComponent<Renderer>().sharedMaterial = currMaterial;
				GetComponent<Renderer>().sharedMaterial.hideFlags = HideFlags.None;
				matAssigned = true;
				DoAfterSetAction(action);
			}
			else
			{
				Image component = GetComponent<Image>();
				if (component != null)
				{
					flag = true;
					prevMaterial = new Material(component.material);
					currMaterial = new Material(shader);
					component.material = currMaterial;
					component.material.hideFlags = HideFlags.None;
					matAssigned = true;
					DoAfterSetAction(action);
				}
			}
			if (!flag)
			{
				MissingRenderer();
			}
			else
			{
				SetSceneDirty();
			}
		}
		else
		{
			_ = shader == null;
		}
	}

	private void DoAfterSetAction(AfterSetAction action)
	{
		switch (action)
		{
		case AfterSetAction.Clear:
			ClearAllKeywords();
			break;
		case AfterSetAction.CopyMaterial:
			currMaterial.CopyPropertiesFromMaterial(prevMaterial);
			break;
		}
	}

	public void TryCreateNew()
	{
		bool flag = false;
		if (GetComponent<SpriteRenderer>() != null)
		{
			flag = true;
			Renderer component = GetComponent<Renderer>();
			if (component != null && component.sharedMaterial != null && component.sharedMaterial.name.Contains("AllIn1"))
			{
				ResetAllProperties();
				ClearAllKeywords();
			}
			else
			{
				CleanMaterial();
				MakeNewMaterial();
			}
		}
		else
		{
			Image component2 = GetComponent<Image>();
			if (component2 != null)
			{
				flag = true;
				if (component2.material.name.Contains("AllIn1"))
				{
					ResetAllProperties();
					ClearAllKeywords();
				}
				else
				{
					MakeNewMaterial();
				}
			}
		}
		if (!flag)
		{
			MissingRenderer();
		}
	}

	public void ClearAllKeywords()
	{
		SetKeyword("RECTSIZE_ON");
		SetKeyword("OFFSETUV_ON");
		SetKeyword("CLIPPING_ON");
		SetKeyword("POLARUV_ON");
		SetKeyword("TWISTUV_ON");
		SetKeyword("ROTATEUV_ON");
		SetKeyword("FISHEYE_ON");
		SetKeyword("PINCH_ON");
		SetKeyword("SHAKEUV_ON");
		SetKeyword("WAVEUV_ON");
		SetKeyword("ROUNDWAVEUV_ON");
		SetKeyword("DOODLE_ON");
		SetKeyword("ZOOMUV_ON");
		SetKeyword("FADE_ON");
		SetKeyword("TEXTURESCROLL_ON");
		SetKeyword("GLOW_ON");
		SetKeyword("OUTBASE_ON");
		SetKeyword("OUTTEX_ON");
		SetKeyword("OUTDIST_ON");
		SetKeyword("DISTORT_ON");
		SetKeyword("WIND_ON");
		SetKeyword("GRADIENT_ON");
		SetKeyword("COLORSWAP_ON");
		SetKeyword("HSV_ON");
		SetKeyword("HITEFFECT_ON");
		SetKeyword("PIXELATE_ON");
		SetKeyword("NEGATIVE_ON");
		SetKeyword("COLORRAMP_ON");
		SetKeyword("GREYSCALE_ON");
		SetKeyword("POSTERIZE_ON");
		SetKeyword("BLUR_ON");
		SetKeyword("MOTIONBLUR_ON");
		SetKeyword("GHOST_ON");
		SetKeyword("INNEROUTLINE_ON");
		SetKeyword("HOLOGRAM_ON");
		SetKeyword("CHROMABERR_ON");
		SetKeyword("GLITCH_ON");
		SetKeyword("FLICKER_ON");
		SetKeyword("SHADOW_ON");
		SetKeyword("ALPHACUTOFF_ON");
		SetKeyword("CHANGECOLOR_ON");
		SetSceneDirty();
	}

	private void SetKeyword(string keyword, bool state = false)
	{
		if (destroyed)
		{
			return;
		}
		if (currMaterial == null)
		{
			FindCurrMaterial();
			if (currMaterial == null)
			{
				MissingRenderer();
				return;
			}
		}
		if (!state)
		{
			currMaterial.DisableKeyword(keyword);
		}
		else
		{
			currMaterial.EnableKeyword(keyword);
		}
	}

	private void FindCurrMaterial()
	{
		if (GetComponent<SpriteRenderer>() != null)
		{
			currMaterial = GetComponent<Renderer>().sharedMaterial;
			matAssigned = true;
			return;
		}
		Image component = GetComponent<Image>();
		if (component != null)
		{
			currMaterial = component.material;
			matAssigned = true;
		}
	}

	public void CleanMaterial()
	{
		if (GetComponent<SpriteRenderer>() != null)
		{
			GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
			matAssigned = false;
		}
		else
		{
			Image component = GetComponent<Image>();
			if (component != null)
			{
				component.material = new Material(Shader.Find("Sprites/Default"));
				matAssigned = false;
			}
		}
		SetSceneDirty();
	}

	public void SaveMaterial()
	{
	}

	private void SaveMaterialWithOtherName(string path, int i = 1)
	{
		int num = i;
		string text = string.Concat(path + num, ".mat");
		if (File.Exists(text))
		{
			num++;
			SaveMaterialWithOtherName(path, num);
		}
		else
		{
			DoSaving(text);
		}
	}

	private void DoSaving(string fileName)
	{
	}

	private void SetSceneDirty()
	{
	}

	private void MissingRenderer()
	{
	}

	public void ToggleSetAtlasUvs(bool activate)
	{
		SetAtlasUvs setAtlasUvs = GetComponent<SetAtlasUvs>();
		if (activate)
		{
			if (setAtlasUvs == null)
			{
				setAtlasUvs = base.gameObject.AddComponent<SetAtlasUvs>();
			}
			setAtlasUvs.GetAndSetUVs();
			SetKeyword("ATLAS_ON", state: true);
		}
		else
		{
			if (setAtlasUvs != null)
			{
				setAtlasUvs.ResetAtlasUvs();
				Object.DestroyImmediate(setAtlasUvs);
			}
			SetKeyword("ATLAS_ON");
		}
	}
}
