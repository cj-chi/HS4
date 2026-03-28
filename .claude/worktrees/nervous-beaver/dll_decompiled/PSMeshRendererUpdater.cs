using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PSMeshRendererUpdater : MonoBehaviour
{
	public GameObject MeshObject;

	public Color Color = Color.black;

	private const string materialName = "MeshEffect";

	private List<Material[]> rendererMaterials = new List<Material[]>();

	private List<Material[]> skinnedMaterials = new List<Material[]>();

	public bool IsActive = true;

	public float FadeTime = 1.5f;

	private bool currentActiveStatus;

	private bool needUpdateAlpha;

	private Color oldColor = Color.black;

	private float currentAlphaTime;

	private void Update()
	{
		if (Application.isPlaying)
		{
			CheckFading();
		}
		if (Color != oldColor)
		{
			oldColor = Color;
			UpdateColor(Color);
		}
	}

	public void CheckFading()
	{
		if (currentActiveStatus != IsActive)
		{
			currentActiveStatus = IsActive;
			needUpdateAlpha = true;
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				if (currentActiveStatus)
				{
					particleSystem.Clear();
					particleSystem.Play();
				}
				else
				{
					particleSystem.Stop();
				}
			}
			ME_TrailRendererNoise[] componentsInChildren2 = GetComponentsInChildren<ME_TrailRendererNoise>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].IsActive = currentActiveStatus;
			}
		}
		if (needUpdateAlpha)
		{
			if (currentActiveStatus)
			{
				currentAlphaTime += Time.deltaTime;
			}
			else
			{
				currentAlphaTime -= Time.deltaTime;
			}
			if (currentAlphaTime < 0f || currentAlphaTime > FadeTime)
			{
				needUpdateAlpha = false;
			}
			SetAlpha(Mathf.Clamp01(currentAlphaTime / FadeTime));
		}
	}

	public void SetAlpha(float alpha)
	{
		if (MeshObject == null)
		{
			return;
		}
		Light componentInChildren = MeshObject.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.intensity = alpha;
		}
		MeshRenderer componentInChildren2 = MeshObject.GetComponentInChildren<MeshRenderer>();
		Material[] materials;
		if (componentInChildren2 != null)
		{
			materials = componentInChildren2.materials;
			foreach (Material material in materials)
			{
				if (material.name.Contains("MeshEffect"))
				{
					UpdateAlphaByPropertyName(material, "_TintColor", alpha);
					UpdateAlphaByPropertyName(material, "_MainColor", alpha);
				}
			}
		}
		SkinnedMeshRenderer componentInChildren3 = MeshObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (!(componentInChildren3 != null))
		{
			return;
		}
		materials = componentInChildren3.materials;
		foreach (Material material2 in materials)
		{
			if (material2.name.Contains("MeshEffect"))
			{
				UpdateAlphaByPropertyName(material2, "_TintColor", alpha);
				UpdateAlphaByPropertyName(material2, "_MainColor", alpha);
			}
		}
	}

	private void UpdateAlphaByPropertyName(Material mat, string name, float alpha)
	{
		if (mat.HasProperty(name))
		{
			Color color = mat.GetColor(name);
			color.a = alpha;
			mat.SetColor(name, color);
		}
	}

	public void UpdateColor(Color color)
	{
		if (!(MeshObject == null))
		{
			ME_ColorHelper.HSBColor hSBColor = ME_ColorHelper.ColorToHSV(color);
			ME_ColorHelper.ChangeObjectColorByHUE(MeshObject, hSBColor.H);
		}
	}

	public void UpdateColor(float HUE)
	{
		if (!(MeshObject == null))
		{
			ME_ColorHelper.ChangeObjectColorByHUE(MeshObject, HUE);
		}
	}

	public void UpdateMeshEffect()
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = default(Quaternion);
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
		if (!(MeshObject == null))
		{
			UpdatePSMesh(MeshObject);
			AddMaterialToMesh(MeshObject);
		}
	}

	private void CheckScaleIncludedParticles()
	{
	}

	public void UpdateMeshEffect(GameObject go)
	{
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
		if (!(go == null))
		{
			MeshObject = go;
			UpdatePSMesh(MeshObject);
			AddMaterialToMesh(MeshObject);
		}
	}

	private void UpdatePSMesh(GameObject go)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		MeshRenderer componentInChildren = go.GetComponentInChildren<MeshRenderer>();
		SkinnedMeshRenderer componentInChildren2 = go.GetComponentInChildren<SkinnedMeshRenderer>();
		Light[] componentsInChildren2 = GetComponentsInChildren<Light>();
		float num = 1f;
		float num2 = 1f;
		if (componentInChildren != null)
		{
			num = componentInChildren.bounds.size.magnitude;
		}
		if (componentInChildren2 != null)
		{
			num = componentInChildren2.bounds.size.magnitude;
		}
		num2 = go.transform.lossyScale.magnitude;
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem obj in array)
		{
			obj.transform.gameObject.SetActive(value: false);
			ParticleSystem.ShapeModule shape = obj.shape;
			if (shape.enabled)
			{
				if (componentInChildren != null)
				{
					shape.shapeType = ParticleSystemShapeType.MeshRenderer;
					shape.meshRenderer = componentInChildren;
				}
				if (componentInChildren2 != null)
				{
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape.skinnedMeshRenderer = componentInChildren2;
				}
			}
			ParticleSystem.MainModule main = obj.main;
			main.startSizeMultiplier *= num / num2;
			obj.transform.gameObject.SetActive(value: true);
		}
		if (componentInChildren != null)
		{
			Light[] array2 = componentsInChildren2;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].transform.position = componentInChildren.bounds.center;
			}
		}
		if (componentInChildren2 != null)
		{
			Light[] array2 = componentsInChildren2;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].transform.position = componentInChildren2.bounds.center;
			}
		}
	}

	private void AddMaterialToMesh(GameObject go)
	{
		ME_MeshMaterialEffect componentInChildren = GetComponentInChildren<ME_MeshMaterialEffect>();
		if (!(componentInChildren == null))
		{
			MeshRenderer componentInChildren2 = go.GetComponentInChildren<MeshRenderer>();
			SkinnedMeshRenderer componentInChildren3 = go.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren2 != null)
			{
				rendererMaterials.Add(componentInChildren2.sharedMaterials);
				componentInChildren2.sharedMaterials = AddToSharedMaterial(componentInChildren2.sharedMaterials, componentInChildren);
			}
			if (componentInChildren3 != null)
			{
				skinnedMaterials.Add(componentInChildren3.sharedMaterials);
				componentInChildren3.sharedMaterials = AddToSharedMaterial(componentInChildren3.sharedMaterials, componentInChildren);
			}
		}
	}

	private Material[] AddToSharedMaterial(Material[] sharedMaterials, ME_MeshMaterialEffect meshMatEffect)
	{
		if (meshMatEffect.IsFirstMaterial)
		{
			return new Material[1] { meshMatEffect.Material };
		}
		List<Material> list = sharedMaterials.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].name.Contains("MeshEffect"))
			{
				list.RemoveAt(i);
			}
		}
		list.Add(meshMatEffect.Material);
		return list.ToArray();
	}

	private void OnDestroy()
	{
		if (MeshObject == null)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = MeshObject.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] componentsInChildren2 = MeshObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (rendererMaterials.Count == componentsInChildren.Length)
			{
				componentsInChildren[i].sharedMaterials = rendererMaterials[i];
			}
			List<Material> list = componentsInChildren[i].sharedMaterials.ToList();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].name.Contains("MeshEffect"))
				{
					list.RemoveAt(j);
				}
			}
			componentsInChildren[i].sharedMaterials = list.ToArray();
		}
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (skinnedMaterials.Count == componentsInChildren2.Length)
			{
				componentsInChildren2[k].sharedMaterials = skinnedMaterials[k];
			}
			List<Material> list2 = componentsInChildren2[k].sharedMaterials.ToList();
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l].name.Contains("MeshEffect"))
				{
					list2.RemoveAt(l);
				}
			}
			componentsInChildren2[k].sharedMaterials = list2.ToArray();
		}
		rendererMaterials.Clear();
		skinnedMaterials.Clear();
	}
}
