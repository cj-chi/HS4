using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manager;
using Studio.Sound;
using UnityEngine;

namespace Studio;

public class OCIItem : ObjectCtrlInfo
{
	public GameObject objectItem;

	public Transform childRoot;

	public Animator animator;

	public ItemComponent itemComponent;

	public ParticleComponent particleComponent;

	private Texture2D[] texturePattern = new Texture2D[3];

	public IconComponent iconComponent;

	public PanelComponent panelComponent;

	private Texture2D textureMain;

	public SEComponent seComponent;

	public ItemFKCtrl itemFKCtrl;

	public List<OCIChar.BoneInfo> listBones;

	public DynamicBone[] dynamicBones;

	private bool m_Visible = true;

	public Renderer[] arrayRender;

	public ParticleSystem[] arrayParticle;

	public OIItemInfo itemInfo => objectInfo as OIItemInfo;

	public bool isAnime
	{
		get
		{
			if (!(animator != null))
			{
				return false;
			}
			return animator.enabled;
		}
	}

	public bool isChangeColor
	{
		get
		{
			bool flag = false;
			if (itemComponent != null)
			{
				flag |= itemComponent.check | itemComponent.checkGlass;
			}
			if (particleComponent != null)
			{
				flag |= particleComponent.check;
			}
			return flag;
		}
	}

	public bool[] useColor
	{
		get
		{
			bool[] result = Enumerable.Repeat(element: false, 3).ToArray();
			if (itemComponent != null)
			{
				bool[] list = itemComponent.useColor;
				int i = 0;
				while (i < 3)
				{
					list.SafeProc(i, delegate(bool _b)
					{
						result[i] = _b;
					});
					int num = i + 1;
					i = num;
				}
			}
			if (particleComponent != null)
			{
				result[0] |= particleComponent.UseColor1;
			}
			return result;
		}
	}

	public bool useColor4
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.checkGlass;
		}
	}

	public Color[] defColor
	{
		get
		{
			Color[] result = Enumerable.Repeat(Color.white, 3).ToArray();
			if (itemComponent != null && !((IReadOnlyCollection<ItemComponent.Info>)(object)itemComponent.info).IsNullOrEmpty())
			{
				int i = 0;
				while (i < 3)
				{
					itemComponent.info.SafeProc(i, delegate(ItemComponent.Info _i)
					{
						result[i] = _i.defColor;
					});
					int num = i + 1;
					i = num;
				}
			}
			if (particleComponent != null && particleComponent.UseColor1)
			{
				result[0] = particleComponent.defColor01;
			}
			return result;
		}
	}

	public bool[] useMetallic
	{
		get
		{
			if (!(itemComponent == null))
			{
				return itemComponent.useMetallic;
			}
			return new bool[3];
		}
	}

	public bool[] usePattern
	{
		get
		{
			if (!(itemComponent == null))
			{
				return itemComponent.usePattern;
			}
			return new bool[3];
		}
	}

	public bool CheckAlpha
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.checkAlpha;
		}
	}

	public bool CheckEmission
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.CheckEmission;
		}
	}

	public bool CheckEmissionColor
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.checkEmissionColor;
		}
	}

	public bool CheckEmissionPower
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.checkEmissionStrength;
		}
	}

	public bool CheckLightCancel
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.checkLightCancel;
		}
	}

	public bool IsParticle => particleComponent != null;

	public bool VisibleIcon
	{
		set
		{
			iconComponent.SafeProc(delegate(IconComponent _ic)
			{
				_ic.Active = value;
			});
		}
	}

	public bool checkPanel => panelComponent != null;

	public bool isFK => !listBones.IsNullOrEmpty();

	public bool isDynamicBone
	{
		get
		{
			if (!(isFK & itemInfo.enableFK))
			{
				return !((IReadOnlyCollection<DynamicBone>)(object)dynamicBones).IsNullOrEmpty();
			}
			return false;
		}
	}

	public bool CheckOption
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.CheckOption;
		}
	}

	public bool CheckAnimePattern
	{
		get
		{
			if (!(itemComponent != null))
			{
				return false;
			}
			return itemComponent.CheckAnimePattern;
		}
	}

	public bool CheckAnim => isChangeColor | checkPanel | isFK | isDynamicBone | CheckOption | CheckAnimePattern;

	public bool visible
	{
		get
		{
			return m_Visible;
		}
		set
		{
			m_Visible = value;
			for (int i = 0; i < arrayRender.Length; i++)
			{
				arrayRender[i].enabled = value;
			}
			if (!((IReadOnlyCollection<ParticleSystem>)(object)arrayParticle).IsNullOrEmpty())
			{
				for (int j = 0; j < arrayParticle.Length; j++)
				{
					if (value)
					{
						arrayParticle[j].Play();
					}
					else
					{
						arrayParticle[j].Pause();
					}
				}
			}
			if (seComponent != null)
			{
				seComponent.enabled = value;
			}
		}
	}

	public bool IsParticleArray
	{
		get
		{
			if (((IReadOnlyCollection<ParticleSystem>)(object)arrayParticle).IsNullOrEmpty())
			{
				if (!(particleComponent != null))
				{
					return false;
				}
				return particleComponent.IsPlay;
			}
			return true;
		}
	}

	public override float animeSpeed
	{
		get
		{
			return itemInfo.animeSpeed;
		}
		set
		{
			if (Utility.SetStruct(ref itemInfo.animeSpeed, value) && (bool)animator)
			{
				animator.speed = itemInfo.animeSpeed;
			}
		}
	}

	public override void OnDelete()
	{
		if (!listBones.IsNullOrEmpty())
		{
			for (int i = 0; i < listBones.Count; i++)
			{
				Singleton<GuideObjectManager>.Instance.Delete(listBones[i].guideObject);
			}
			listBones.Clear();
		}
		Singleton<GuideObjectManager>.Instance.Delete(guideObject);
		Object.Destroy(objectItem);
		if (parentInfo != null)
		{
			parentInfo.OnDetachChild(this);
		}
		Studio.DeleteInfo(objectInfo);
	}

	public override void OnAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		if (_child.parentInfo == null)
		{
			Studio.DeleteInfo(_child.objectInfo, _delKey: false);
		}
		else
		{
			_child.parentInfo.OnDetachChild(_child);
		}
		if (!itemInfo.child.Contains(_child.objectInfo))
		{
			itemInfo.child.Add(_child.objectInfo);
		}
		bool flag = false;
		if (_child is OCIItem)
		{
			flag = (_child as OCIItem).IsParticleArray;
		}
		if (!flag)
		{
			_child.guideObject.transformTarget.SetParent(childRoot);
		}
		_child.guideObject.parent = childRoot;
		_child.guideObject.mode = GuideObject.Mode.World;
		_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
		if (!flag)
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.transformTarget.localPosition;
			_child.objectInfo.changeAmount.rot = _child.guideObject.transformTarget.localEulerAngles;
		}
		else if (_child.guideObject.nonconnect)
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.guideObject.transformTarget.position);
			Quaternion quaternion = _child.guideObject.transformTarget.rotation * Quaternion.Inverse(_child.guideObject.parent.rotation);
			_child.objectInfo.changeAmount.rot = quaternion.eulerAngles;
		}
		else
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.objectInfo.changeAmount.pos);
			Quaternion quaternion2 = Quaternion.Euler(_child.objectInfo.changeAmount.rot) * Quaternion.Inverse(_child.guideObject.parent.rotation);
			_child.objectInfo.changeAmount.rot = quaternion2.eulerAngles;
		}
		_child.guideObject.nonconnect = flag;
		_child.guideObject.calcScale = !flag;
		_child.parentInfo = this;
	}

	public override void OnLoadAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		if (_child.parentInfo == null)
		{
			Studio.DeleteInfo(_child.objectInfo, _delKey: false);
		}
		else
		{
			_child.parentInfo.OnDetachChild(_child);
		}
		if (!itemInfo.child.Contains(_child.objectInfo))
		{
			itemInfo.child.Add(_child.objectInfo);
		}
		bool flag = false;
		if (_child is OCIItem)
		{
			flag = (_child as OCIItem).IsParticleArray;
		}
		if (!flag)
		{
			_child.guideObject.transformTarget.SetParent(childRoot, worldPositionStays: false);
		}
		_child.guideObject.parent = childRoot;
		_child.guideObject.nonconnect = flag;
		_child.guideObject.calcScale = !flag;
		_child.guideObject.mode = GuideObject.Mode.World;
		_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
		_child.objectInfo.changeAmount.OnChange();
		_child.parentInfo = this;
	}

	public override void OnDetach()
	{
		parentInfo.OnDetachChild(this);
		guideObject.parent = null;
		Studio.AddInfo(objectInfo, this);
		objectItem.transform.SetParent(Scene.commonSpace.transform);
		objectInfo.changeAmount.pos = objectItem.transform.localPosition;
		objectInfo.changeAmount.rot = objectItem.transform.localEulerAngles;
		guideObject.mode = GuideObject.Mode.Local;
		guideObject.moveCalc = GuideMove.MoveCalc.TYPE1;
		treeNodeObject.ResetVisible();
	}

	public override void OnSelect(bool _select)
	{
		int layer = LayerMask.NameToLayer(_select ? "Studio/Col" : "Studio/Select");
		if (!listBones.IsNullOrEmpty())
		{
			for (int i = 0; i < listBones.Count; i++)
			{
				listBones[i].layer = layer;
			}
		}
	}

	public override void OnDetachChild(ObjectCtrlInfo _child)
	{
		itemInfo.child.Remove(_child.objectInfo);
		_child.parentInfo = null;
	}

	public override void OnSavePreprocessing()
	{
		base.OnSavePreprocessing();
		if (isAnime && animator.layerCount != 0)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			itemInfo.animeNormalizedTime = currentAnimatorStateInfo.normalizedTime;
		}
	}

	public override void OnVisible(bool _visible)
	{
		visible = _visible;
	}

	public void SetColor(Color _color, int _idx)
	{
		if (MathfEx.RangeEqualOn(0, _idx, 3))
		{
			itemInfo.colors[_idx].mainColor = _color;
		}
		UpdateColor();
	}

	public void SetMetallic(int _idx, float _value)
	{
		if (MathfEx.RangeEqualOn(0, _idx, 3))
		{
			itemInfo.colors[_idx].metallic = _value;
		}
		UpdateColor();
	}

	public void SetGlossiness(int _idx, float _value)
	{
		if (MathfEx.RangeEqualOn(0, _idx, 3))
		{
			itemInfo.colors[_idx].glossiness = _value;
		}
		UpdateColor();
	}

	public void SetupPatternTex()
	{
		for (int i = 0; i < 3; i++)
		{
			PatternInfo pattern = itemInfo.colors[i].pattern;
			if (!pattern.filePath.IsNullOrEmpty())
			{
				string fileName = Path.GetFileName(pattern.filePath);
				SetPatternTex(i, UserData.Path + "pattern/" + fileName);
			}
			else
			{
				SetPatternTex(i, pattern.key);
			}
		}
	}

	public string SetPatternTex(int _idx, int _key)
	{
		if (_key <= 0)
		{
			itemInfo.colors[_idx].pattern.key = _key;
			itemInfo.colors[_idx].pattern.filePath = "";
			if ((bool)itemComponent)
			{
				itemComponent.SetPatternTex(_idx, null);
			}
			ReleasePatternTex(_idx);
			return "なし";
		}
		PatternSelectInfo patternSelectInfo = Singleton<Studio>.Instance.patternSelectListCtrl.lstSelectInfo.Find((PatternSelectInfo p) => p.index == _key);
		string result = "なし";
		if (patternSelectInfo != null)
		{
			if (patternSelectInfo.assetBundle.IsNullOrEmpty())
			{
				string path = UserData.Path + "pattern/" + patternSelectInfo.assetName;
				if (!File.Exists(path))
				{
					return "なし";
				}
				texturePattern[_idx] = PngAssist.LoadTexture(path);
				itemInfo.colors[_idx].pattern.key = -1;
				itemInfo.colors[_idx].pattern.filePath = patternSelectInfo.assetName;
				result = patternSelectInfo.assetName;
			}
			else
			{
				string assetBundleName = patternSelectInfo.assetBundle.Replace("thumb/", "");
				string assetName = patternSelectInfo.assetName.Replace("thumb_", "");
				texturePattern[_idx] = CommonLib.LoadAsset<Texture2D>(assetBundleName, assetName);
				itemInfo.colors[_idx].pattern.key = _key;
				itemInfo.colors[_idx].pattern.filePath = "";
				result = patternSelectInfo.name;
			}
		}
		itemComponent.SetPatternTex(_idx, texturePattern[_idx]);
		UnityEngine.Resources.UnloadUnusedAssets();
		return result;
	}

	public void SetPatternTex(int _idx, string _path)
	{
		if (_path.IsNullOrEmpty())
		{
			itemInfo.colors[_idx].pattern.key = 0;
			itemInfo.colors[_idx].pattern.filePath = "";
			itemComponent.SetPatternTex(_idx, null);
			ReleasePatternTex(_idx);
			return;
		}
		itemInfo.colors[_idx].pattern.key = -1;
		itemInfo.colors[_idx].pattern.filePath = _path;
		if (File.Exists(_path))
		{
			texturePattern[_idx] = PngAssist.LoadTexture(_path);
		}
		itemComponent.SetPatternTex(_idx, texturePattern[_idx]);
		UnityEngine.Resources.UnloadUnusedAssets();
	}

	private void ReleasePatternTex(int _idx)
	{
		texturePattern[_idx] = null;
	}

	public void SetPatternColor(int _idx, Color _color)
	{
		itemInfo.colors[_idx].pattern.color = _color;
		UpdateColor();
	}

	public void SetPatternClamp(int _idx, bool _flag)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.clamp, _flag))
		{
			UpdateColor();
		}
	}

	public void SetPatternUT(int _idx, float _value)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.uv.z, _value))
		{
			UpdateColor();
		}
	}

	public void SetPatternVT(int _idx, float _value)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.uv.w, _value))
		{
			UpdateColor();
		}
	}

	public void SetPatternUS(int _idx, float _value)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.uv.x, _value))
		{
			UpdateColor();
		}
	}

	public void SetPatternVS(int _idx, float _value)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.uv.y, _value))
		{
			UpdateColor();
		}
	}

	public void SetPatternRot(int _idx, float _value)
	{
		if (Utility.SetStruct(ref itemInfo.colors[_idx].pattern.rot, _value))
		{
			UpdateColor();
		}
	}

	public void SetAlpha(float _value)
	{
		if (Utility.SetStruct(ref itemInfo.alpha, _value))
		{
			UpdateColor();
		}
	}

	public void SetEmissionColor(Color _color)
	{
		itemInfo.emissionColor = _color;
		UpdateColor();
	}

	public void SetEmissionPower(float _value)
	{
		itemInfo.emissionPower = _value;
		UpdateColor();
	}

	public void SetLightCancel(float _value)
	{
		itemInfo.lightCancel = _value;
		UpdateColor();
	}

	public void UpdateColor()
	{
		if (itemComponent != null && (itemComponent.check | itemComponent.checkGlass))
		{
			itemComponent.UpdateColor(itemInfo);
		}
		if (particleComponent != null && particleComponent.check)
		{
			particleComponent.UpdateColor(itemInfo);
		}
		if (panelComponent != null)
		{
			panelComponent.UpdateColor(itemInfo);
		}
	}

	public void SetMainTex()
	{
		SetMainTex(itemInfo.panel.filePath);
	}

	public void SetMainTex(string _file)
	{
		if (panelComponent == null)
		{
			return;
		}
		if (_file.IsNullOrEmpty())
		{
			itemInfo.panel.filePath = "";
			panelComponent.SetMainTex(null);
			textureMain = null;
			return;
		}
		itemInfo.panel.filePath = _file;
		string path = Singleton<Studio>.Instance.ApplicationPath + _file;
		if (File.Exists(path))
		{
			textureMain = PngAssist.LoadTexture(path);
			panelComponent.SetMainTex(textureMain);
			UnityEngine.Resources.UnloadUnusedAssets();
		}
	}

	public void ActiveFK(bool _active)
	{
		if (itemFKCtrl == null)
		{
			return;
		}
		itemFKCtrl.enabled = _active;
		itemInfo.enableFK = _active;
		bool enabled = !_active && itemInfo.enableDynamicBone;
		DynamicBone[] array = dynamicBones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = enabled;
		}
		foreach (OCIChar.BoneInfo listBone in listBones)
		{
			listBone.active = _active;
		}
	}

	public void UpdateFKColor()
	{
		if (listBones.IsNullOrEmpty())
		{
			return;
		}
		foreach (OCIChar.BoneInfo listBone in listBones)
		{
			listBone.color = Studio.optionSystem.colorFKItem;
		}
	}

	public void ActiveDynamicBone(bool _active)
	{
		itemInfo.enableDynamicBone = _active;
		if (!((IReadOnlyCollection<DynamicBone>)(object)dynamicBones).IsNullOrEmpty() && !(isFK & itemInfo.enableFK))
		{
			DynamicBone[] array = dynamicBones;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = _active;
			}
		}
	}

	public void SetOptionVisible(bool _visible)
	{
		int count = itemInfo.option.Count;
		for (int i = 0; i < count; i++)
		{
			itemInfo.option[i] = _visible;
		}
		itemComponent?.SetOptionVisible(_visible);
	}

	public void SetOptionVisible(int _idx, bool _visible)
	{
		if (MathfEx.RangeEqualOn(0, _idx, itemInfo.option.Count - 1))
		{
			itemInfo.option[_idx] = _visible;
		}
		itemComponent?.SetOptionVisible(_idx, _visible);
	}

	public void UpdateOption()
	{
		int count = itemInfo.option.Count;
		for (int i = 0; i < count; i++)
		{
			itemComponent?.SetOptionVisible(i, itemInfo.option[i]);
		}
	}

	public void SetAnimePattern(int _idx)
	{
		if (isAnime)
		{
			itemInfo.animePattern = _idx;
			ItemComponent.AnimeInfo animeInfo = itemComponent?.animeInfos.SafeGet(_idx);
			if (animeInfo != null)
			{
				animator.Play(animeInfo.state);
			}
		}
	}

	public void RestartAnime()
	{
		if (isAnime && animator.layerCount != 0)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			animator.Play(currentAnimatorStateInfo.shortNameHash, 0, 0f);
		}
	}
}
