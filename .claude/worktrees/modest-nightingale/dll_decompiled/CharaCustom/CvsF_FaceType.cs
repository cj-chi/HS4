using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Manager;
using UnityEngine;

namespace CharaCustom;

public class CvsF_FaceType : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscFaceType;

	[SerializeField]
	private CustomPushScrollController pscFacePreset;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscSkinType;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomSliderSet ssDetailPower;

	[SerializeField]
	private CustomSelectScrollController sscDetailType;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssDetailPower.SetSliderValue(base.face.detailPower);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		UpdateSkinList();
		sscFaceType.SetToggleID(base.face.headId);
		sscSkinType.SetToggleID(base.face.skinId);
		sscDetailType.SetToggleID(base.face.detailId);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssDetailPower.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.detailPower));
	}

	public void UpdateSkinList()
	{
		List<CustomSelectInfo> source = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mt_skin_f : ChaListDefine.CategoryNo.ft_skin_f, ChaListDefine.KeyType.HeadID);
		source = source.Where((CustomSelectInfo x) => x.limitIndex == base.face.headId).ToList();
		sscSkinType.CreateList(source);
	}

	public List<CustomPushInfo> CreateFacePresetList(ChaListDefine.CategoryNo cateNo)
	{
		Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(cateNo);
		int[] array = categoryInfo.Keys.ToArray();
		List<CustomPushInfo> list = new List<CustomPushInfo>();
		for (int i = 0; i < categoryInfo.Count; i++)
		{
			list.Add(new CustomPushInfo
			{
				category = categoryInfo[array[i]].Category,
				id = categoryInfo[array[i]].Id,
				name = categoryInfo[array[i]].Name,
				assetBundle = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.ThumbAB),
				assetName = categoryInfo[array[i]].GetInfo(ChaListDefine.KeyType.Preset)
			});
		}
		return list;
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFaceType += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_head : ChaListDefine.CategoryNo.fo_head);
		sscFaceType.CreateList(lst);
		sscFaceType.SetToggleID(base.face.headId);
		sscFaceType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.headId != info.id)
			{
				base.chaCtrl.ChangeHead(info.id);
				UpdateSkinList();
				sscSkinType.SetToggleID(base.face.skinId);
			}
		};
		List<CustomPushInfo> lst2 = CreateFacePresetList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mo_head : ChaListDefine.CategoryNo.fo_head);
		pscFacePreset.CreateList(lst2);
		pscFacePreset.onPush = delegate(CustomPushInfo info)
		{
			if (info != null)
			{
				base.face.headId = info.id;
				base.chaCtrl.chaFile.LoadFacePreset();
				Singleton<Character>.Instance.customLoadGCClear = false;
				base.chaCtrl.Reload(noChangeClothes: true, noChangeHead: false, noChangeHair: true, noChangeBody: true);
				Singleton<Character>.Instance.customLoadGCClear = true;
				base.customBase.updateCvsFaceType = true;
				base.customBase.updateCvsFaceShapeWhole = true;
				base.customBase.updateCvsFaceShapeChin = true;
				base.customBase.updateCvsFaceShapeCheek = true;
				base.customBase.updateCvsFaceShapeEyebrow = true;
				base.customBase.updateCvsFaceShapeEyes = true;
				base.customBase.updateCvsFaceShapeNose = true;
				base.customBase.updateCvsFaceShapeMouth = true;
				base.customBase.updateCvsFaceShapeEar = true;
				base.customBase.updateCvsMole = true;
				base.customBase.updateCvsEyeLR = true;
				base.customBase.updateCvsEyeEtc = true;
				base.customBase.updateCvsEyeHL = true;
				base.customBase.updateCvsEyebrow = true;
				base.customBase.updateCvsEyelashes = true;
				base.customBase.updateCvsEyeshadow = true;
				base.customBase.updateCvsCheek = true;
				base.customBase.updateCvsLip = true;
				base.customBase.updateCvsFacePaint = true;
				base.customBase.SetUpdateToggleSetting();
			}
		};
		UpdateSkinList();
		sscSkinType.SetToggleID(base.face.skinId);
		sscSkinType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.skinId != info.id)
			{
				base.face.skinId = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: true, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: false);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		lst = CvsBase.CreateSelectList((base.chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.mt_detail_f : ChaListDefine.CategoryNo.ft_detail_f);
		sscDetailType.CreateList(lst);
		sscDetailType.SetToggleID(base.face.detailId);
		sscDetailType.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.detailId != info.id)
			{
				base.face.detailId = info.id;
				base.chaCtrl.ChangeFaceDetailKind();
			}
		};
		ssDetailPower.onChange = delegate(float value)
		{
			base.face.detailPower = value;
			base.chaCtrl.ChangeFaceDetailPower();
		};
		ssDetailPower.onSetDefaultValue = () => base.defChaCtrl.custom.face.detailPower;
		StartCoroutine(SetInputText());
	}
}
