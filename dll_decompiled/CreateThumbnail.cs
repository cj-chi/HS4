using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Manager;
using UnityEngine;

public class CreateThumbnail : MonoBehaviour
{
	public class FacePaintLayout
	{
		public int index = -1;

		public float x;

		public float y;

		public float s;
	}

	public class MoleLayout
	{
		public int index = -1;

		public float x;

		public float y;

		public float s;
	}

	public Dictionary<int, FacePaintLayout> dictFacePaintLayout;

	public Dictionary<int, MoleLayout> dictMoleLayout;

	public CameraControl_Ver2 camCtrl;

	public Camera camMain;

	public Camera camBack;

	public Camera camFront;

	public GameObject objImgBack;

	public GameObject objImgFront;

	public ChaControl chaCtrl;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		ReloadChara(1);
		yield return null;
		yield return null;
		string assetBundleName = ChaABDefine.CustomAnimAssetBundle(chaCtrl.sex);
		string assetName = ChaABDefine.CustomAnimAsset(chaCtrl.sex);
		chaCtrl.LoadAnimation(assetBundleName, assetName);
		chaCtrl.AnimPlay("mannequin");
		ChaListControl chaListCtrl = Singleton<Character>.Instance.chaListCtrl;
		Dictionary<int, ListInfoBase> categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.facepaint_layout);
		dictFacePaintLayout = categoryInfo.Select((KeyValuePair<int, ListInfoBase> dict) => new FacePaintLayout
		{
			index = dict.Value.Id,
			x = dict.Value.GetInfoFloat(ChaListDefine.KeyType.PosX),
			y = dict.Value.GetInfoFloat(ChaListDefine.KeyType.PosY),
			s = dict.Value.GetInfoFloat(ChaListDefine.KeyType.Scale)
		}).ToDictionary((FacePaintLayout v) => v.index, (FacePaintLayout v) => v);
		categoryInfo = chaListCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.mole_layout);
		dictMoleLayout = categoryInfo.Select((KeyValuePair<int, ListInfoBase> dict) => new MoleLayout
		{
			index = dict.Value.Id,
			x = dict.Value.GetInfoFloat(ChaListDefine.KeyType.PosX),
			y = dict.Value.GetInfoFloat(ChaListDefine.KeyType.PosY),
			s = dict.Value.GetInfoFloat(ChaListDefine.KeyType.Scale)
		}).ToDictionary((MoleLayout v) => v.index, (MoleLayout v) => v);
	}

	public void ReloadChara(int sex)
	{
		if (chaCtrl != null)
		{
			Singleton<Character>.Instance.DeleteChara(chaCtrl);
		}
		chaCtrl = Singleton<Character>.Instance.CreateChara((byte)sex, base.gameObject, 0);
		int num = Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length;
		for (int i = 0; i < num; i++)
		{
			chaCtrl.nowCoordinate.clothes.parts[i].id = 0;
		}
		chaCtrl.releaseCustomInputTexture = false;
		chaCtrl.Load();
		chaCtrl.hideMoz = true;
		chaCtrl.loadWithDefaultColorAndPtn = true;
		chaCtrl.ChangeEyesOpenMax(1f);
		chaCtrl.ChangeEyesBlinkFlag(blink: false);
	}

	private void Update()
	{
		if (QualitySettings.shadowDistance != 80f)
		{
			QualitySettings.shadowDistance = 80f;
		}
	}
}
