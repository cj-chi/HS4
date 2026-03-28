using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class CvsF_Mole : CvsBase
{
	[Header("【設定01】----------------------")]
	[SerializeField]
	private CustomSelectScrollController sscMole;

	[Header("【設定02】----------------------")]
	[SerializeField]
	private CustomColorSet csMole;

	[Header("【設定03】----------------------")]
	[SerializeField]
	private CustomPushScrollController pscMoleLayout;

	[SerializeField]
	private CustomSliderSet ssMoleW;

	[SerializeField]
	private CustomSliderSet ssMoleH;

	[SerializeField]
	private CustomSliderSet ssMoleX;

	[SerializeField]
	private CustomSliderSet ssMoleY;

	private Dictionary<int, Vector4> dictMoleLayout;

	public override void ChangeMenuFunc()
	{
		base.ChangeMenuFunc();
		base.customBase.customCtrl.showColorCvs = false;
		base.customBase.customCtrl.showFileList = false;
	}

	private void CalculateUI()
	{
		ssMoleW.SetSliderValue(base.face.moleLayout.x);
		ssMoleH.SetSliderValue(base.face.moleLayout.y);
		ssMoleX.SetSliderValue(base.face.moleLayout.z);
		ssMoleY.SetSliderValue(base.face.moleLayout.w);
	}

	public override void UpdateCustomUI()
	{
		base.UpdateCustomUI();
		CalculateUI();
		sscMole.SetToggleID(base.face.moleId);
		csMole.SetColor(base.face.moleColor);
	}

	public IEnumerator SetInputText()
	{
		yield return new WaitUntil(() => null != base.chaCtrl);
		ssMoleW.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.moleLayout.x));
		ssMoleH.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.moleLayout.y));
		ssMoleX.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.moleLayout.z));
		ssMoleY.SetInputTextValue(CustomBase.ConvertTextFromRate(0, 100, base.face.moleLayout.w));
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsMole += UpdateCustomUI;
		List<CustomSelectInfo> lst = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_mole);
		sscMole.CreateList(lst);
		sscMole.SetToggleID(base.face.moleId);
		sscMole.onSelect = delegate(CustomSelectInfo info)
		{
			if (info != null && base.face.moleId != info.id)
			{
				base.face.moleId = info.id;
				base.chaCtrl.AddUpdateCMFaceTexFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: true);
				base.chaCtrl.CreateFaceTexture();
			}
		};
		csMole.actUpdateColor = delegate(Color color)
		{
			base.face.moleColor = color;
			base.chaCtrl.AddUpdateCMFaceColorFlags(inpBase: false, inpEyeshadow: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLip: false, inpMole: true);
			base.chaCtrl.CreateFaceTexture();
		};
		Dictionary<int, ListInfoBase> categoryInfo = base.lstCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.mole_layout);
		dictMoleLayout = categoryInfo.Select((KeyValuePair<int, ListInfoBase> val, int idx) => new
		{
			idx = idx,
			x = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
			y = val.Value.GetInfoFloat(ChaListDefine.KeyType.Scale),
			z = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosX),
			w = val.Value.GetInfoFloat(ChaListDefine.KeyType.PosY)
		}).ToDictionary(v => v.idx, v => new Vector4
		{
			x = v.x,
			y = v.y,
			z = v.z,
			w = v.w
		});
		List<CustomPushInfo> lst2 = CvsBase.CreatePushList(ChaListDefine.CategoryNo.mole_layout);
		pscMoleLayout.CreateList(lst2);
		pscMoleLayout.onPush = delegate(CustomPushInfo info)
		{
			if (info != null && dictMoleLayout.TryGetValue(info.id, out var value))
			{
				base.face.moleLayout = value;
				base.chaCtrl.AddUpdateCMFaceLayoutFlags(inpPaint01: false, inpPaint02: false, inpMole: true);
				base.chaCtrl.CreateFaceTexture();
				ssMoleW.SetSliderValue(base.face.moleLayout.x);
				ssMoleH.SetSliderValue(base.face.moleLayout.y);
				ssMoleX.SetSliderValue(base.face.moleLayout.z);
				ssMoleY.SetSliderValue(base.face.moleLayout.w);
			}
		};
		ssMoleW.onChange = delegate(float value)
		{
			base.face.moleLayout = new Vector4(value, base.face.moleLayout.y, base.face.moleLayout.z, base.face.moleLayout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(inpPaint01: false, inpPaint02: false, inpMole: true);
			base.chaCtrl.CreateFaceTexture();
		};
		ssMoleW.onSetDefaultValue = () => base.defChaCtrl.custom.face.moleLayout.x;
		ssMoleH.onChange = delegate(float value)
		{
			base.face.moleLayout = new Vector4(base.face.moleLayout.x, value, base.face.moleLayout.z, base.face.moleLayout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(inpPaint01: false, inpPaint02: false, inpMole: true);
			base.chaCtrl.CreateFaceTexture();
		};
		ssMoleH.onSetDefaultValue = () => base.defChaCtrl.custom.face.moleLayout.y;
		ssMoleX.onChange = delegate(float value)
		{
			base.face.moleLayout = new Vector4(base.face.moleLayout.x, base.face.moleLayout.y, value, base.face.moleLayout.w);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(inpPaint01: false, inpPaint02: false, inpMole: true);
			base.chaCtrl.CreateFaceTexture();
		};
		ssMoleX.onSetDefaultValue = () => base.defChaCtrl.custom.face.moleLayout.z;
		ssMoleY.onChange = delegate(float value)
		{
			base.face.moleLayout = new Vector4(base.face.moleLayout.x, base.face.moleLayout.y, base.face.moleLayout.z, value);
			base.chaCtrl.AddUpdateCMFaceLayoutFlags(inpPaint01: false, inpPaint02: false, inpMole: true);
			base.chaCtrl.CreateFaceTexture();
		};
		ssMoleY.onSetDefaultValue = () => base.defChaCtrl.custom.face.moleLayout.w;
		StartCoroutine(SetInputText());
	}
}
