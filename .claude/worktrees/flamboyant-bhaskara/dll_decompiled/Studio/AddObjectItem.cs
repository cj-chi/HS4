using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using Studio.Sound;
using UnityEngine;
using UnityEngine.Networking;

namespace Studio;

public static class AddObjectItem
{
	public static OCIItem Add(int _group, int _category, int _no)
	{
		int newIndex = Studio.GetNewIndex();
		Singleton<UndoRedoManager>.Instance.Do(new AddObjectCommand.AddItemCommand(_group, _category, _no, newIndex, Studio.optionSystem.initialPosition));
		return Studio.GetCtrlInfo(newIndex) as OCIItem;
	}

	public static OCIItem Load(OIItemInfo _info, ObjectCtrlInfo _parent, TreeNodeObject _parentNode)
	{
		ChangeAmount source = _info.changeAmount.Clone();
		OCIItem oCIItem = Load(_info, _parent, _parentNode, _addInfo: false, -1);
		_info.changeAmount.Copy(source);
		AddObjectAssist.LoadChild(_info.child, oCIItem, null);
		return oCIItem;
	}

	public static OCIItem Load(OIItemInfo _info, ObjectCtrlInfo _parent, TreeNodeObject _parentNode, bool _addInfo, int _initialPosition)
	{
		OCIItem oCIItem = new OCIItem();
		Info.ItemLoadInfo loadInfo = GetLoadInfo(_info.group, _info.category, _info.no);
		if (loadInfo == null)
		{
			loadInfo = GetLoadInfo(0, 0, 399);
		}
		oCIItem.objectInfo = _info;
		GameObject gameObject = CommonLib.LoadAsset<GameObject>(loadInfo.bundlePath, loadInfo.fileName, clone: true, loadInfo.manifest);
		if (gameObject == null)
		{
			Studio.DeleteIndex(_info.dicKey);
			return null;
		}
		gameObject.transform.SetParent(Scene.commonSpace.transform);
		oCIItem.objectItem = gameObject;
		oCIItem.itemComponent = gameObject.GetComponent<ItemComponent>();
		oCIItem.arrayRender = (from v in gameObject.GetComponentsInChildren<Renderer>()
			where v.enabled
			select v).ToArray();
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		if (!((IReadOnlyCollection<ParticleSystem>)(object)componentsInChildren).IsNullOrEmpty())
		{
			oCIItem.arrayParticle = componentsInChildren.Where((ParticleSystem v) => v.isPlaying).ToArray();
		}
		MeshCollider component = gameObject.GetComponent<MeshCollider>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		oCIItem.dynamicBones = gameObject.GetComponentsInChildren<DynamicBone>();
		GuideObject guideObject = Singleton<GuideObjectManager>.Instance.Add(gameObject.transform, _info.dicKey);
		guideObject.isActive = false;
		guideObject.scaleSelect = 0.1f;
		guideObject.scaleRot = 0.05f;
		guideObject.isActiveFunc = (GuideObject.IsActiveFunc)Delegate.Combine(guideObject.isActiveFunc, new GuideObject.IsActiveFunc(oCIItem.OnSelect));
		guideObject.enableScale = !(oCIItem.itemComponent != null) || oCIItem.itemComponent.isScale;
		guideObject.SetVisibleCenter(_value: true);
		oCIItem.guideObject = guideObject;
		if (oCIItem.itemComponent != null && oCIItem.itemComponent.childRoot != null)
		{
			oCIItem.childRoot = oCIItem.itemComponent.childRoot;
		}
		if (oCIItem.childRoot == null)
		{
			oCIItem.childRoot = gameObject.transform;
		}
		oCIItem.animator = gameObject.GetComponentInChildren<Animator>();
		if ((bool)oCIItem.animator)
		{
			oCIItem.animator.enabled = oCIItem.itemComponent != null && oCIItem.itemComponent.isAnime;
		}
		if (oCIItem.itemComponent != null)
		{
			oCIItem.itemComponent.SetGlass();
			oCIItem.itemComponent.SetEmission();
			if (_addInfo && oCIItem.itemComponent.check)
			{
				Color[] defColorMain = oCIItem.itemComponent.defColorMain;
				for (int num = 0; num < 3; num++)
				{
					_info.colors[num].mainColor = defColorMain[num];
				}
				defColorMain = oCIItem.itemComponent.defColorPattern;
				for (int num2 = 0; num2 < 3; num2++)
				{
					_info.colors[num2].pattern.color = defColorMain[num2];
					_info.colors[num2].metallic = oCIItem.itemComponent.info[num2].defMetallic;
					_info.colors[num2].glossiness = oCIItem.itemComponent.info[num2].defGlossiness;
					_info.colors[num2].pattern.clamp = oCIItem.itemComponent.info[num2].defClamp;
					_info.colors[num2].pattern.uv = oCIItem.itemComponent.info[num2].defUV;
					_info.colors[num2].pattern.rot = oCIItem.itemComponent.info[num2].defRot;
				}
				_info.colors[3].mainColor = oCIItem.itemComponent.defGlass;
				_info.emissionColor = oCIItem.itemComponent.DefEmissionColor;
				_info.emissionPower = oCIItem.itemComponent.defEmissionStrength;
				_info.lightCancel = oCIItem.itemComponent.defLightCancel;
			}
			oCIItem.itemComponent.SetupSea();
		}
		oCIItem.particleComponent = gameObject.GetComponent<ParticleComponent>();
		if (oCIItem.particleComponent != null && _addInfo)
		{
			_info.colors[0].mainColor = oCIItem.particleComponent.defColor01;
		}
		oCIItem.iconComponent = gameObject.GetComponent<IconComponent>();
		if (oCIItem.iconComponent != null)
		{
			oCIItem.iconComponent.Layer = LayerMask.NameToLayer("Studio/Camera");
		}
		oCIItem.VisibleIcon = Singleton<Studio>.Instance.workInfo.visibleGimmick;
		oCIItem.panelComponent = gameObject.GetComponent<PanelComponent>();
		if (_addInfo && oCIItem.panelComponent != null)
		{
			_info.colors[0].mainColor = oCIItem.panelComponent.defColor;
			_info.colors[0].pattern.uv = oCIItem.panelComponent.defUV;
			_info.colors[0].pattern.clamp = oCIItem.panelComponent.defClamp;
			_info.colors[0].pattern.rot = oCIItem.panelComponent.defRot;
		}
		oCIItem.seComponent = gameObject.GetComponent<SEComponent>();
		if (_addInfo && oCIItem.itemComponent != null && !((IReadOnlyCollection<ItemComponent.OptionInfo>)(object)oCIItem.itemComponent.optionInfos).IsNullOrEmpty())
		{
			_info.option = Enumerable.Repeat(element: true, oCIItem.itemComponent.optionInfos.Length).ToList();
		}
		NetworkProximityChecker componentInChildren = gameObject.GetComponentInChildren<NetworkProximityChecker>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren);
		}
		NetworkIdentity componentInChildren2 = gameObject.GetComponentInChildren<NetworkIdentity>();
		if (componentInChildren2 != null)
		{
			UnityEngine.Object.Destroy(componentInChildren2);
		}
		if (_addInfo)
		{
			Studio.AddInfo(_info, oCIItem);
		}
		else
		{
			Studio.AddObjectCtrlInfo(oCIItem);
		}
		TreeNodeObject parent = ((_parentNode != null) ? _parentNode : _parent?.treeNodeObject);
		TreeNodeObject treeNodeObject = Studio.AddNode(loadInfo.name, parent);
		treeNodeObject.treeState = _info.treeState;
		treeNodeObject.onVisible = (TreeNodeObject.OnVisibleFunc)Delegate.Combine(treeNodeObject.onVisible, new TreeNodeObject.OnVisibleFunc(oCIItem.OnVisible));
		treeNodeObject.enableVisible = true;
		treeNodeObject.visible = _info.visible;
		guideObject.guideSelect.treeNodeObject = treeNodeObject;
		oCIItem.treeNodeObject = treeNodeObject;
		if (!loadInfo.bones.IsNullOrEmpty())
		{
			oCIItem.itemFKCtrl = gameObject.AddComponent<ItemFKCtrl>();
			oCIItem.itemFKCtrl.InitBone(oCIItem, loadInfo, _addInfo);
		}
		else
		{
			oCIItem.itemFKCtrl = null;
		}
		if (_initialPosition == 1)
		{
			_info.changeAmount.pos = Singleton<Studio>.Instance.cameraCtrl.targetPos;
		}
		_info.changeAmount.OnChange();
		Studio.AddCtrlInfo(oCIItem);
		_parent?.OnLoadAttach((_parentNode != null) ? _parentNode : _parent.treeNodeObject, oCIItem);
		if ((bool)oCIItem.animator)
		{
			if (_info.animePattern != 0)
			{
				oCIItem.SetAnimePattern(_info.animePattern);
			}
			oCIItem.animator.speed = _info.animeSpeed;
			if (_info.animeNormalizedTime != 0f && oCIItem.animator.layerCount != 0)
			{
				oCIItem.animator.Update(1f);
				AnimatorStateInfo currentAnimatorStateInfo = oCIItem.animator.GetCurrentAnimatorStateInfo(0);
				oCIItem.animator.Play(currentAnimatorStateInfo.shortNameHash, 0, _info.animeNormalizedTime);
			}
		}
		oCIItem.SetupPatternTex();
		oCIItem.SetMainTex();
		oCIItem.UpdateColor();
		oCIItem.ActiveFK(oCIItem.itemInfo.enableFK);
		oCIItem.UpdateFKColor();
		oCIItem.ActiveDynamicBone(oCIItem.itemInfo.enableDynamicBone);
		oCIItem.UpdateOption();
		oCIItem.particleComponent?.PlayOnLoad();
		return oCIItem;
	}

	private static Info.ItemLoadInfo GetLoadInfo(int _group, int _category, int _no)
	{
		Dictionary<int, Dictionary<int, Info.ItemLoadInfo>> value = null;
		if (!Singleton<Info>.Instance.dicItemLoadInfo.TryGetValue(_group, out value))
		{
			return null;
		}
		Dictionary<int, Info.ItemLoadInfo> value2 = null;
		if (!value.TryGetValue(_category, out value2))
		{
			return null;
		}
		Info.ItemLoadInfo value3 = null;
		if (!value2.TryGetValue(_no, out value3))
		{
			return null;
		}
		return value3;
	}
}
