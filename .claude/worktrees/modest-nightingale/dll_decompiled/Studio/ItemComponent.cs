using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.CustomAttributes;
using Illusion.Extensions;
using UnityEngine;

namespace Studio;

[AddComponentMenu("Studio/ItemComponent")]
public class ItemComponent : MonoBehaviour
{
	[Serializable]
	public class MaterialInfo
	{
		public bool isColor1;

		public bool isPattern1;

		public bool isColor2;

		public bool isPattern2;

		public bool isColor3;

		public bool isPattern3;

		public bool isEmission;

		public bool isAlpha;

		public bool isGlass;

		public bool CheckColor(int _idx)
		{
			return _idx switch
			{
				0 => isColor1, 
				1 => isColor2, 
				2 => isColor3, 
				_ => false, 
			};
		}

		public bool CheckPattern(int _idx)
		{
			return _idx switch
			{
				0 => isPattern1, 
				1 => isPattern2, 
				2 => isPattern3, 
				_ => false, 
			};
		}
	}

	[Serializable]
	public class RendererInfo
	{
		public Renderer renderer;

		public MaterialInfo[] materials;

		public bool IsNormal => materials.Any((MaterialInfo m) => m.isColor1 || m.isColor2 || m.isColor3 || m.isEmission);

		public bool IsAlpha => materials.Any((MaterialInfo m) => m.isAlpha);

		public bool IsGlass => materials.Any((MaterialInfo m) => m.isGlass);

		public bool IsColor => materials.Any((MaterialInfo m) => m.isColor1 || m.isColor2 || m.isColor3);

		public bool IsColor1 => materials.Any((MaterialInfo m) => m.isColor1);

		public bool IsColor2 => materials.Any((MaterialInfo m) => m.isColor2);

		public bool IsColor3 => materials.Any((MaterialInfo m) => m.isColor3);

		public bool IsPattern => materials.Any((MaterialInfo m) => m.isPattern1 || m.isPattern2 || m.isPattern3);

		public bool IsPattern1 => materials.Any((MaterialInfo m) => m.isPattern1);

		public bool IsPattern2 => materials.Any((MaterialInfo m) => m.isPattern2);

		public bool IsPattern3 => materials.Any((MaterialInfo m) => m.isPattern3);

		public bool IsEmission => materials.Any((MaterialInfo m) => m.isEmission);
	}

	[Serializable]
	public class Info
	{
		[Header("色替え")]
		public Color defColor = Color.white;

		[Header("メタル")]
		public bool useMetallic;

		public float defMetallic;

		public float defGlossiness;

		[Header("柄")]
		public Color defColorPattern = Color.white;

		public bool defClamp = true;

		public Vector4 defUV = Vector4.zero;

		public float defRot;

		public float ut
		{
			get
			{
				return defUV.z;
			}
			set
			{
				defUV.z = value;
			}
		}

		public float vt
		{
			get
			{
				return defUV.w;
			}
			set
			{
				defUV.w = value;
			}
		}

		public float us
		{
			get
			{
				return defUV.x;
			}
			set
			{
				defUV.x = value;
			}
		}

		public float vs
		{
			get
			{
				return defUV.y;
			}
			set
			{
				defUV.y = value;
			}
		}
	}

	[Serializable]
	public class OptionInfo
	{
		public GameObject[] objectsOn;

		public GameObject[] objectsOff;

		public bool Visible
		{
			set
			{
				if (value)
				{
					SetVisible(objectsOff, _value: false);
					SetVisible(objectsOn, _value: true);
				}
				else
				{
					SetVisible(objectsOn, _value: false);
					SetVisible(objectsOff, _value: true);
				}
			}
		}

		private void SetVisible(GameObject[] _objects, bool _value)
		{
			foreach (GameObject item in _objects.Where((GameObject v) => v != null))
			{
				item.SetActiveIfDifferent(_value);
			}
		}
	}

	[Serializable]
	public class AnimeInfo
	{
		public string name = "";

		public string state = "";

		public bool Check
		{
			get
			{
				if (!name.IsNullOrEmpty())
				{
					return !state.IsNullOrEmpty();
				}
				return false;
			}
		}
	}

	[Header("レンダラー管理")]
	public RendererInfo[] rendererInfos;

	[Space]
	[Header("構成情報")]
	public Info[] info;

	public float alpha = 1f;

	[Header("ガラス関係")]
	public Color defGlass = Color.white;

	[Header("エミッション関係")]
	[ColorUsage(false, true)]
	public Color defEmissionColor = Color.clear;

	public float defEmissionStrength;

	public float defLightCancel;

	[Header("子の接続先")]
	public Transform childRoot;

	[Header("アニメ関係")]
	[Tooltip("アニメがあるか")]
	public bool isAnime;

	public AnimeInfo[] animeInfos;

	[Header("拡縮判定")]
	public bool isScale = true;

	[Header("オプション")]
	public OptionInfo[] optionInfos;

	[Header("海面関係")]
	public GameObject objSeaParent;

	public Renderer[] renderersSea;

	[Space]
	[Button("SetColor", "初期色を設定", new object[] { })]
	public int setcolor;

	public bool check
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty())
			{
				return rendererInfos.Any((RendererInfo _ri) => _ri.IsNormal || _ri.IsAlpha);
			}
			return false;
		}
	}

	public bool checkAlpha
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty())
			{
				return rendererInfos.Any((RendererInfo _ri) => _ri.IsAlpha);
			}
			return false;
		}
	}

	public bool checkGlass
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty())
			{
				return rendererInfos.Any((RendererInfo _ri) => _ri.IsGlass);
			}
			return false;
		}
	}

	public bool checkEmissionColor => HasEmissionColor();

	public bool checkEmissionStrength => HasEmissionStrength();

	public bool CheckEmission
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty())
			{
				return rendererInfos.Any((RendererInfo _ri) => _ri.IsEmission);
			}
			return false;
		}
	}

	public bool checkLightCancel => rendererInfos.Any((RendererInfo _ri) => (_ri.IsNormal || _ri.IsAlpha) && _ri.renderer.materials.Any((Material _m) => _m.HasProperty(ItemShader._LightCancel)));

	public bool CheckOption => !((IReadOnlyCollection<OptionInfo>)(object)optionInfos).IsNullOrEmpty();

	public bool CheckAnimePattern
	{
		get
		{
			if (!((IReadOnlyCollection<AnimeInfo>)(object)animeInfos).IsNullOrEmpty())
			{
				return animeInfos.Any((AnimeInfo _info) => _info.Check);
			}
			return false;
		}
	}

	public Color[] defColorMain => info.Select((Info i) => i.defColor).ToArray();

	public Color[] defColorPattern => info.Select((Info i) => i.defColorPattern).ToArray();

	public bool[] useColor
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty() && rendererInfos.Any((RendererInfo _ri) => _ri.IsColor))
			{
				return new bool[3]
				{
					rendererInfos.Any((RendererInfo _ri) => _ri.IsColor1),
					rendererInfos.Any((RendererInfo _ri) => _ri.IsColor2),
					rendererInfos.Any((RendererInfo _ri) => _ri.IsColor3)
				};
			}
			return Enumerable.Repeat(element: false, 3).ToArray();
		}
	}

	public bool[] useMetallic => info.Select((Info i) => i.useMetallic).ToArray();

	public bool[] usePattern
	{
		get
		{
			if (!((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty() && rendererInfos.Any((RendererInfo _ri) => _ri.IsPattern))
			{
				return new bool[3]
				{
					rendererInfos.Any((RendererInfo _ri) => _ri.IsPattern1),
					rendererInfos.Any((RendererInfo _ri) => _ri.IsPattern2),
					rendererInfos.Any((RendererInfo _ri) => _ri.IsPattern3)
				};
			}
			return Enumerable.Repeat(element: false, 3).ToArray();
		}
	}

	public Color DefEmissionColor => new Color(defEmissionColor.r, defEmissionColor.g, defEmissionColor.b);

	public Info this[int _idx] => info.SafeGet(_idx);

	public void SetupRendererInfo()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		if (((IReadOnlyCollection<Renderer>)(object)componentsInChildren).IsNullOrEmpty())
		{
			return;
		}
		rendererInfos = componentsInChildren.Select((Renderer _r) => new RendererInfo
		{
			renderer = _r
		}).ToArray();
		RendererInfo[] array = rendererInfos;
		foreach (RendererInfo rendererInfo in array)
		{
			Material[] sharedMaterials = rendererInfo.renderer.sharedMaterials;
			rendererInfo.materials = new MaterialInfo[sharedMaterials.Length];
			for (int num2 = 0; num2 < sharedMaterials.Length; num2++)
			{
				rendererInfo.materials[num2] = new MaterialInfo();
			}
		}
	}

	public void UpdateColor(OIItemInfo _info)
	{
		RendererInfo[] array = rendererInfos;
		foreach (RendererInfo rendererInfo in array)
		{
			if (rendererInfo.IsNormal)
			{
				for (int j = 0; j < 3; j++)
				{
					ColorInfo colorInfo = _info.colors[j];
					Material[] materials = rendererInfo.renderer.materials;
					for (int k = 0; k < materials.Length; k++)
					{
						MaterialInfo materialInfo = rendererInfo.materials.SafeGet(k);
						if (materialInfo == null)
						{
							continue;
						}
						switch (j)
						{
						case 0:
							if (materialInfo.isColor1)
							{
								materials[k].SetColor(ItemShader._Color, colorInfo.mainColor);
								if (info[j].useMetallic)
								{
									materials[k].SetFloat(ItemShader._Metallic, colorInfo.metallic);
									materials[k].SetFloat(ItemShader._Glossiness, colorInfo.glossiness);
								}
								if (materialInfo.isPattern1)
								{
									materials[k].SetColor(ItemShader._Color1_2, colorInfo.pattern.color);
									materials[k].SetVector(ItemShader._patternuv1, colorInfo.pattern.uv);
									materials[k].SetFloat(ItemShader._patternuv1Rotator, colorInfo.pattern.rot);
									materials[k].SetFloat(ItemShader._patternclamp1, colorInfo.pattern.clamp ? 1f : 0f);
								}
							}
							break;
						case 1:
							if (materialInfo.isColor2)
							{
								materials[k].SetColor(ItemShader._Color2, colorInfo.mainColor);
								if (info[j].useMetallic)
								{
									materials[k].SetFloat(ItemShader._Metallic2, colorInfo.metallic);
									materials[k].SetFloat(ItemShader._Glossiness2, colorInfo.glossiness);
								}
								if (materialInfo.isPattern2)
								{
									materials[k].SetColor(ItemShader._Color2_2, colorInfo.pattern.color);
									materials[k].SetVector(ItemShader._patternuv2, colorInfo.pattern.uv);
									materials[k].SetFloat(ItemShader._patternuv2Rotator, colorInfo.pattern.rot);
									materials[k].SetFloat(ItemShader._patternclamp2, colorInfo.pattern.clamp ? 1f : 0f);
								}
							}
							break;
						case 2:
							if (materialInfo.isColor3)
							{
								materials[k].SetColor(ItemShader._Color3, colorInfo.mainColor);
								if (info[j].useMetallic)
								{
									materials[k].SetFloat(ItemShader._Metallic3, colorInfo.metallic);
									materials[k].SetFloat(ItemShader._Glossiness3, colorInfo.glossiness);
								}
								if (materialInfo.isPattern3)
								{
									materials[k].SetColor(ItemShader._Color3_2, colorInfo.pattern.color);
									materials[k].SetVector(ItemShader._patternuv3, colorInfo.pattern.uv);
									materials[k].SetFloat(ItemShader._patternuv3Rotator, colorInfo.pattern.rot);
									materials[k].SetFloat(ItemShader._patternclamp3, colorInfo.pattern.clamp ? 1f : 0f);
								}
							}
							break;
						}
					}
				}
			}
			if (rendererInfo.IsAlpha)
			{
				Material[] materials2 = rendererInfo.renderer.materials;
				for (int l = 0; l < materials2.Length; l++)
				{
					MaterialInfo materialInfo2 = rendererInfo.materials.SafeGet(l);
					if (materialInfo2 != null && materialInfo2.isAlpha)
					{
						materials2[l].SetFloat(ItemShader._alpha, _info.alpha);
					}
				}
			}
			if (rendererInfo.IsNormal || rendererInfo.IsAlpha)
			{
				Material[] materials3 = rendererInfo.renderer.materials;
				for (int m = 0; m < materials3.Length; m++)
				{
					MaterialInfo materialInfo3 = rendererInfo.materials.SafeGet(m);
					if (materialInfo3 != null && materialInfo3.isEmission)
					{
						if (materials3[m].HasProperty(ItemShader._EmissionColor))
						{
							materials3[m].SetColor(ItemShader._EmissionColor, _info.emissionColor);
						}
						if (materials3[m].HasProperty(ItemShader._EmissionStrength))
						{
							materials3[m].SetFloat(ItemShader._EmissionStrength, _info.emissionPower);
						}
						if (materials3[m].HasProperty(ItemShader._LightCancel))
						{
							materials3[m].SetFloat(ItemShader._LightCancel, _info.lightCancel);
						}
					}
				}
			}
			if (!rendererInfo.IsGlass)
			{
				continue;
			}
			Material[] materials4 = rendererInfo.renderer.materials;
			for (int n = 0; n < materials4.Length; n++)
			{
				MaterialInfo materialInfo4 = rendererInfo.materials.SafeGet(n);
				if (materialInfo4 != null && materialInfo4.isGlass)
				{
					ColorInfo colorInfo2 = _info.colors[3];
					if (materials4[n].HasProperty(ItemShader._Color4))
					{
						materials4[n].SetColor(ItemShader._Color4, colorInfo2.mainColor);
					}
					else if (materials4[n].HasProperty(ItemShader._Color))
					{
						materials4[n].SetColor(ItemShader._Color, colorInfo2.mainColor);
					}
					materials4[n].SetColor(ItemShader._Metallic4, colorInfo2.mainColor);
					materials4[n].SetColor(ItemShader._Glossiness4, colorInfo2.mainColor);
				}
			}
		}
	}

	public void SetPatternTex(int _idx, Texture2D _texture)
	{
		int[] array = new int[3]
		{
			ItemShader._PatternMask1,
			ItemShader._PatternMask2,
			ItemShader._PatternMask3
		};
		foreach (RendererInfo item in rendererInfos.Where((RendererInfo v) => v.IsNormal))
		{
			Material[] materials = item.renderer.materials;
			for (int num = 0; num < materials.Length; num++)
			{
				MaterialInfo materialInfo = item.materials.SafeGet(num);
				if (materialInfo != null && materialInfo.CheckPattern(_idx))
				{
					materials[num].SetTexture(array[_idx], _texture);
				}
			}
		}
	}

	public void SetOptionVisible(bool _value)
	{
		if (!((IReadOnlyCollection<OptionInfo>)(object)optionInfos).IsNullOrEmpty())
		{
			OptionInfo[] array = optionInfos;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Visible = _value;
			}
		}
	}

	public void SetOptionVisible(int _idx, bool _value)
	{
		optionInfos.SafeProc(_idx, delegate(OptionInfo _info)
		{
			_info.Visible = _value;
		});
	}

	public void SetColor()
	{
		bool[] array = Enumerable.Repeat(element: false, 7).ToArray();
		foreach (RendererInfo item in from r in rendererInfos
			where r.renderer != null && !((IReadOnlyCollection<MaterialInfo>)(object)r.materials).IsNullOrEmpty()
			where r.materials.Any((MaterialInfo _m) => _m.isColor1)
			select r)
		{
			if (!array.Take(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item2 in from v in item.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor1
					select v)
				{
					Material material = item.renderer.sharedMaterials.SafeGet(item2.Item2);
					if (!(material == null))
					{
						if (!array[0] && material.HasProperty("_Color"))
						{
							info[0].defColor = material.GetColor("_Color");
							array[0] = true;
						}
						if (!array[1] && material.HasProperty("_Metallic"))
						{
							info[0].defMetallic = material.GetFloat("_Metallic");
							array[1] = true;
						}
						if (!array[2] && material.HasProperty("_Glossiness"))
						{
							info[0].defGlossiness = material.GetFloat("_Glossiness");
							array[2] = true;
						}
						if (array.Take(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (!array.Skip(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item3 in from v in item.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor1 && v.Item1.isPattern1
					select v)
				{
					Material material2 = item.renderer.sharedMaterials.SafeGet(item3.Item2);
					if (!(material2 == null))
					{
						if (!array[3] && material2.HasProperty("_Color1_2"))
						{
							info[0].defColorPattern = material2.GetColor("_Color1_2");
							array[3] = true;
						}
						if (!array[4] && material2.HasProperty("_patternuv1"))
						{
							info[0].defUV = material2.GetVector("_patternuv1");
							array[4] = true;
						}
						if (!array[5] && material2.HasProperty("_patternuv1Rotator"))
						{
							info[0].defRot = material2.GetFloat("_patternuv1Rotator");
							array[5] = true;
						}
						if (!array[6] && material2.HasProperty("_patternclamp1"))
						{
							info[0].defClamp = material2.GetFloat("_patternclamp1") != 0f;
							array[6] = true;
						}
						if (array.Skip(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (array.All((bool _b) => _b))
			{
				break;
			}
		}
		bool[] array2 = Enumerable.Repeat(element: false, 7).ToArray();
		foreach (RendererInfo item4 in from r in rendererInfos
			where r.renderer != null && !((IReadOnlyCollection<MaterialInfo>)(object)r.materials).IsNullOrEmpty()
			where r.materials.Any((MaterialInfo _m) => _m.isColor2)
			select r)
		{
			if (!array2.Take(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item5 in from v in item4.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor2
					select v)
				{
					Material material3 = item4.renderer.sharedMaterials.SafeGet(item5.Item2);
					if (!(material3 == null))
					{
						if (!array2[0] && material3.HasProperty("_Color2"))
						{
							info[1].defColor = material3.GetColor("_Color2");
							array2[0] = true;
						}
						if (!array2[1] && material3.HasProperty("_Metallic2"))
						{
							info[1].defMetallic = material3.GetFloat("_Metallic2");
							array2[1] = true;
						}
						if (!array2[2] && material3.HasProperty("_Glossiness2"))
						{
							info[1].defGlossiness = material3.GetFloat("_Glossiness2");
							array2[2] = true;
						}
						if (array2.Take(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (!array2.Skip(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item6 in from v in item4.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor2 && v.Item1.isPattern2
					select v)
				{
					Material material4 = item4.renderer.sharedMaterials.SafeGet(item6.Item2);
					if (!(material4 == null))
					{
						if (!array2[3] && material4.HasProperty("_Color2_2"))
						{
							info[1].defColorPattern = material4.GetColor("_Color2_2");
							array2[3] = true;
						}
						if (!array2[4] && material4.HasProperty("_patternuv2"))
						{
							info[1].defUV = material4.GetVector("_patternuv2");
							array2[4] = true;
						}
						if (!array2[5] && material4.HasProperty("_patternuv2Rotator"))
						{
							info[1].defRot = material4.GetFloat("_patternuv2Rotator");
							array2[5] = true;
						}
						if (!array2[6] && material4.HasProperty("_patternclamp2"))
						{
							info[1].defClamp = material4.GetFloat("_patternclamp2") != 0f;
							array2[6] = true;
						}
						if (array2.Skip(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (array2.All((bool _b) => _b))
			{
				break;
			}
		}
		bool[] array3 = Enumerable.Repeat(element: false, 7).ToArray();
		foreach (RendererInfo item7 in from r in rendererInfos
			where r.renderer != null && !((IReadOnlyCollection<MaterialInfo>)(object)r.materials).IsNullOrEmpty()
			where r.materials.Any((MaterialInfo _m) => _m.isColor3)
			select r)
		{
			if (!array3.Take(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item8 in from v in item7.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor3
					select v)
				{
					Material material5 = item7.renderer.sharedMaterials.SafeGet(item8.Item2);
					if (!(material5 == null))
					{
						if (!array3[0] && material5.HasProperty("_Color3"))
						{
							info[2].defColor = material5.GetColor("_Color3");
							array3[0] = true;
						}
						if (!array3[1] && material5.HasProperty("_Metallic3"))
						{
							info[2].defMetallic = material5.GetFloat("_Metallic3");
							array3[1] = true;
						}
						if (!array3[2] && material5.HasProperty("_Glossiness3"))
						{
							info[2].defGlossiness = material5.GetFloat("_Glossiness3");
							array3[2] = true;
						}
						if (array3.Take(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (!array3.Skip(3).All((bool _b) => _b))
			{
				foreach (Tuple<MaterialInfo, int> item9 in from v in item7.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
					where v.Item1.isColor3 && v.Item1.isPattern3
					select v)
				{
					Material material6 = item7.renderer.sharedMaterials.SafeGet(item9.Item2);
					if (!(material6 == null))
					{
						if (!array3[3] && material6.HasProperty("_Color3_2"))
						{
							info[2].defColorPattern = material6.GetColor("_Color3_2");
							array3[3] = true;
						}
						if (!array3[4] && material6.HasProperty("_patternuv3"))
						{
							info[2].defUV = material6.GetVector("_patternuv3");
							array3[4] = true;
						}
						if (!array3[5] && material6.HasProperty("_patternuv3Rotator"))
						{
							info[2].defRot = material6.GetFloat("_patternuv3Rotator");
							array3[5] = true;
						}
						if (!array3[6] && material6.HasProperty("_patternclamp3"))
						{
							info[2].defClamp = material6.GetFloat("_patternclamp3") != 0f;
							array3[6] = true;
						}
						if (array3.Skip(3).All((bool _b) => _b))
						{
							break;
						}
					}
				}
			}
			if (array3.All((bool _b) => _b))
			{
				break;
			}
		}
		RendererInfo rendererInfo = rendererInfos.Where((RendererInfo r) => r.renderer != null && !((IReadOnlyCollection<MaterialInfo>)(object)r.materials).IsNullOrEmpty()).FirstOrDefault((RendererInfo _i) => _i.materials.Any((MaterialInfo _m) => _m.isAlpha));
		if (rendererInfo != null)
		{
			Material[] sharedMaterials = rendererInfo.renderer.sharedMaterials;
			for (int num = 0; num < sharedMaterials.Length; num++)
			{
				MaterialInfo materialInfo = rendererInfo.materials.SafeGet(num);
				if (materialInfo != null && materialInfo.isAlpha && null != sharedMaterials[num] && sharedMaterials[num].HasProperty("_alpha"))
				{
					alpha = sharedMaterials[num].GetFloat("_alpha");
				}
			}
		}
		SetGlass();
		SetEmission();
	}

	public void SetGlass()
	{
		if (((IReadOnlyCollection<RendererInfo>)(object)rendererInfos).IsNullOrEmpty())
		{
			return;
		}
		RendererInfo rendererInfo = rendererInfos.Where((RendererInfo r) => r.renderer != null && !((IReadOnlyCollection<MaterialInfo>)(object)r.materials).IsNullOrEmpty()).FirstOrDefault((RendererInfo _i) => _i.materials.Any((MaterialInfo _m) => _m.isGlass));
		if (rendererInfo == null)
		{
			return;
		}
		Material[] sharedMaterials = rendererInfo.renderer.sharedMaterials;
		for (int num = 0; num < sharedMaterials.Length; num++)
		{
			MaterialInfo materialInfo = rendererInfo.materials.SafeGet(num);
			if (materialInfo != null && materialInfo.isGlass && null != sharedMaterials[num])
			{
				if (sharedMaterials[num].HasProperty("_Color4"))
				{
					defGlass = sharedMaterials[num].GetColor("_Color4");
				}
				else if (sharedMaterials[num].HasProperty("_Color"))
				{
					defGlass = sharedMaterials[num].GetColor("_Color");
				}
			}
		}
	}

	public void SetEmission()
	{
		bool[] array = new bool[3];
		foreach (RendererInfo item in rendererInfos.Where((RendererInfo v) => v.materials.Any((MaterialInfo m) => m.isEmission)))
		{
			foreach (Tuple<MaterialInfo, int> item2 in from v in item.materials.Select((MaterialInfo _m, int index) => new Tuple<MaterialInfo, int>(_m, index))
				where v.Item1.isEmission
				select v)
			{
				Material material = item.renderer.sharedMaterials[item2.Item2];
				if (!(material == null))
				{
					if (!array[0] && material.HasProperty("_EmissionColor"))
					{
						defEmissionColor = material.GetColor("_EmissionColor");
						array[0] = true;
					}
					if (!array[1] && material.HasProperty("_EmissionStrength"))
					{
						defEmissionStrength = material.GetFloat("_EmissionStrength");
						array[1] = true;
					}
					if (!array[2] && material.HasProperty("_LightCancel"))
					{
						defLightCancel = material.GetFloat("_LightCancel");
						array[2] = true;
					}
					if (array.All((bool _b) => _b))
					{
						break;
					}
				}
			}
			if (array.All((bool _b) => _b))
			{
				break;
			}
		}
	}

	public void SetSeaRenderer()
	{
		if (!(objSeaParent == null))
		{
			renderersSea = objSeaParent.GetComponentsInChildren<Renderer>();
		}
	}

	public void SetupSea()
	{
		if (((IReadOnlyCollection<Renderer>)(object)renderersSea).IsNullOrEmpty())
		{
			return;
		}
		foreach (Renderer item in renderersSea.Where((Renderer v) => v != null))
		{
			Material material = item.material;
			material.DisableKeyword("USINGWATERVOLUME");
			item.material = material;
		}
	}

	private bool HasProperty(Renderer[] _renderer, int _nameID)
	{
		return _renderer.Any((Renderer r) => r.materials.Any((Material m) => m.HasProperty(_nameID)));
	}

	private bool HasEmissionColor()
	{
		foreach (RendererInfo item in rendererInfos.Where((RendererInfo v) => v.materials.Any((MaterialInfo m) => m.isEmission)))
		{
			Material[] materials = item.renderer.materials;
			for (int num = 0; num < materials.Length; num++)
			{
				MaterialInfo materialInfo = item.materials.SafeGet(num);
				if (materialInfo != null && materialInfo.isEmission && materials[num].HasProperty(ItemShader._EmissionColor))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasEmissionStrength()
	{
		foreach (RendererInfo item in rendererInfos.Where((RendererInfo v) => v.materials.Any((MaterialInfo m) => m.isEmission)))
		{
			Material[] materials = item.renderer.materials;
			for (int num = 0; num < materials.Length; num++)
			{
				MaterialInfo materialInfo = item.materials.SafeGet(num);
				if (materialInfo != null && materialInfo.isEmission && materials[num].HasProperty(ItemShader._EmissionStrength))
				{
					return true;
				}
			}
		}
		return false;
	}
}
