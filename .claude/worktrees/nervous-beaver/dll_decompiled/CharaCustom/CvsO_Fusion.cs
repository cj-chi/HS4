using System.Collections;
using System.Collections.Generic;
using AIChara;
using Manager;
using MessagePack;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsO_Fusion : CvsBase
{
	[SerializeField]
	private CustomCharaWindow charaLoadWinA;

	[SerializeField]
	private CustomCharaWindow charaLoadWinB;

	[SerializeField]
	private Button btnFusion;

	[SerializeField]
	private Button btnExit;

	private bool isFusion;

	public void UpdateCharasList()
	{
		List<CustomCharaFileInfo> lst = CustomCharaFileInfoAssist.CreateCharaFileInfoList(base.chaCtrl.sex == 0, 1 == base.chaCtrl.sex, useMyData: true, useDownload: true, usePreset: false, _isFindSaveData: false);
		charaLoadWinA.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: false, lst);
		charaLoadWinB.UpdateWindow(base.customBase.modeNew, base.customBase.modeSex, save: false, lst);
	}

	public int RandomIntWhich(int a, int b)
	{
		if (Random.Range(0, 2) == 0)
		{
			return a;
		}
		return b;
	}

	public Color ColorBlend(Color a, Color b, float rate)
	{
		return new Color(Mathf.Lerp(a.r, b.r, rate), Mathf.Lerp(a.g, b.g, rate), Mathf.Lerp(a.b, b.b, rate), Mathf.Lerp(a.a, b.a, rate));
	}

	public void FusionProc(string pathA, string pathB)
	{
		ChaFileControl chaFileControl = new ChaFileControl();
		chaFileControl.LoadCharaFile(pathA, base.customBase.modeSex, noLoadPng: true);
		ChaFileControl chaFileControl2 = new ChaFileControl();
		chaFileControl2.LoadCharaFile(pathB, base.customBase.modeSex, noLoadPng: true);
		float num = 0.5f;
		ChaFileFace chaFileFace = chaFileControl.custom.face;
		ChaFileFace chaFileFace2 = chaFileControl2.custom.face;
		float t = 0.5f + Random.Range(-0.5f, 0.5f);
		float num2 = 0.5f + Random.Range(-0.5f, 0.5f);
		num = 0.5f + Random.Range(-0.2f, 0.2f);
		for (int i = 0; i < base.face.shapeValueFace.Length; i++)
		{
			base.face.shapeValueFace[i] = Mathf.Lerp(chaFileFace.shapeValueFace[i], chaFileFace2.shapeValueFace[i], num);
		}
		base.face.headId = RandomIntWhich(chaFileFace.headId, chaFileFace2.headId);
		base.face.skinId = RandomIntWhich(chaFileFace.skinId, chaFileFace2.skinId);
		base.face.detailId = RandomIntWhich(chaFileFace.detailId, chaFileFace2.detailId);
		base.face.detailPower = Mathf.Lerp(chaFileFace.detailPower, chaFileFace2.detailPower, t);
		base.face.eyebrowId = RandomIntWhich(chaFileFace.eyebrowId, chaFileFace2.eyebrowId);
		base.face.eyebrowColor = ColorBlend(chaFileFace.eyebrowColor, chaFileFace2.eyebrowColor, num2);
		base.face.eyebrowLayout = ((Random.Range(0, 2) == 0) ? chaFileFace.eyebrowLayout : chaFileFace2.eyebrowLayout);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.face.eyebrowTilt = Mathf.Lerp(chaFileFace.eyebrowTilt, chaFileFace2.eyebrowTilt, num);
		bool flag = ((Random.Range(0, 2) == 0) ? chaFileFace.pupilSameSetting : chaFileFace2.pupilSameSetting);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int j = 0; j < 2; j++)
		{
			if (flag && 1 == j)
			{
				base.face.pupil[j].whiteColor = base.face.pupil[0].whiteColor;
			}
			else
			{
				base.face.pupil[j].whiteColor = ColorBlend(chaFileFace.pupil[j].whiteColor, chaFileFace2.pupil[j].whiteColor, num);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			if (flag && 1 == k)
			{
				base.face.pupil[k].pupilId = base.face.pupil[0].pupilId;
			}
			else
			{
				base.face.pupil[k].pupilId = RandomIntWhich(chaFileFace.pupil[k].pupilId, chaFileFace2.pupil[k].pupilId);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int l = 0; l < 2; l++)
		{
			if (flag && 1 == l)
			{
				base.face.pupil[l].pupilColor = base.face.pupil[0].pupilColor;
			}
			else
			{
				base.face.pupil[l].pupilColor = ColorBlend(chaFileFace.pupil[l].pupilColor, chaFileFace2.pupil[l].pupilColor, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int m = 0; m < 2; m++)
		{
			if (flag && 1 == m)
			{
				base.face.pupil[m].pupilW = base.face.pupil[0].pupilW;
			}
			else
			{
				base.face.pupil[m].pupilW = Mathf.Lerp(chaFileFace.pupil[m].pupilW, chaFileFace2.pupil[m].pupilW, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int n = 0; n < 2; n++)
		{
			if (flag && 1 == n)
			{
				base.face.pupil[n].pupilH = base.face.pupil[0].pupilH;
			}
			else
			{
				base.face.pupil[n].pupilH = Mathf.Lerp(chaFileFace.pupil[n].pupilH, chaFileFace2.pupil[n].pupilH, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int num3 = 0; num3 < 2; num3++)
		{
			if (flag && 1 == num3)
			{
				base.face.pupil[num3].pupilEmission = base.face.pupil[0].pupilEmission;
			}
			else
			{
				base.face.pupil[num3].pupilEmission = Mathf.Lerp(chaFileFace.pupil[num3].pupilEmission, chaFileFace2.pupil[num3].pupilEmission, num);
			}
		}
		for (int num4 = 0; num4 < 2; num4++)
		{
			if (flag && 1 == num4)
			{
				base.face.pupil[num4].blackId = base.face.pupil[0].blackId;
			}
			else
			{
				base.face.pupil[num4].blackId = RandomIntWhich(chaFileFace.pupil[num4].blackId, chaFileFace2.pupil[num4].blackId);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int num5 = 0; num5 < 2; num5++)
		{
			if (flag && 1 == num5)
			{
				base.face.pupil[num5].blackColor = base.face.pupil[0].blackColor;
			}
			else
			{
				base.face.pupil[num5].blackColor = ColorBlend(chaFileFace.pupil[num5].blackColor, chaFileFace2.pupil[num5].blackColor, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int num6 = 0; num6 < 2; num6++)
		{
			if (flag && 1 == num6)
			{
				base.face.pupil[num6].blackW = base.face.pupil[0].blackW;
			}
			else
			{
				base.face.pupil[num6].blackW = Mathf.Lerp(chaFileFace.pupil[num6].blackW, chaFileFace2.pupil[num6].blackW, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		for (int num7 = 0; num7 < 2; num7++)
		{
			if (flag && 1 == num7)
			{
				base.face.pupil[num7].blackH = base.face.pupil[0].blackH;
			}
			else
			{
				base.face.pupil[num7].blackH = Mathf.Lerp(chaFileFace.pupil[num7].blackH, chaFileFace2.pupil[num7].blackH, num);
			}
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.face.pupilY = Mathf.Lerp(chaFileFace.pupilY, chaFileFace2.pupilY, num);
		base.face.hlId = RandomIntWhich(chaFileFace.hlId, chaFileFace2.hlId);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.face.hlColor = ColorBlend(chaFileFace.hlColor, chaFileFace2.hlColor, num);
		base.face.hlLayout = ((Random.Range(0, 2) == 0) ? chaFileFace.hlLayout : chaFileFace2.hlLayout);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.face.hlTilt = Mathf.Lerp(chaFileFace.hlTilt, chaFileFace2.hlTilt, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.face.whiteShadowScale = Mathf.Lerp(chaFileFace.whiteShadowScale, chaFileFace2.whiteShadowScale, num);
		base.face.eyelashesId = RandomIntWhich(chaFileFace.eyelashesId, chaFileFace2.eyelashesId);
		base.face.eyelashesColor = ColorBlend(chaFileFace.eyelashesColor, chaFileFace2.eyelashesColor, num2);
		if (Random.Range(0, 2) == 0)
		{
			base.face.moleId = chaFileFace.moleId;
			base.face.moleColor = chaFileFace.moleColor;
			base.face.moleLayout = chaFileFace.moleLayout;
		}
		else
		{
			base.face.moleId = chaFileFace2.moleId;
			base.face.moleColor = chaFileFace2.moleColor;
			base.face.moleLayout = chaFileFace2.moleLayout;
		}
		if (Random.Range(0, 2) == 0)
		{
			base.face.makeup.eyeshadowId = chaFileFace.makeup.eyeshadowId;
			base.face.makeup.eyeshadowColor = chaFileFace.makeup.eyeshadowColor;
			base.face.makeup.eyeshadowGloss = chaFileFace.makeup.eyeshadowGloss;
		}
		else
		{
			base.face.makeup.eyeshadowId = chaFileFace2.makeup.eyeshadowId;
			base.face.makeup.eyeshadowColor = chaFileFace2.makeup.eyeshadowColor;
			base.face.makeup.eyeshadowGloss = chaFileFace2.makeup.eyeshadowGloss;
		}
		if (Random.Range(0, 2) == 0)
		{
			base.face.makeup.cheekId = chaFileFace.makeup.cheekId;
			base.face.makeup.cheekColor = chaFileFace.makeup.cheekColor;
			base.face.makeup.cheekGloss = chaFileFace.makeup.cheekGloss;
		}
		else
		{
			base.face.makeup.cheekId = chaFileFace2.makeup.cheekId;
			base.face.makeup.cheekColor = chaFileFace2.makeup.cheekColor;
			base.face.makeup.cheekGloss = chaFileFace2.makeup.cheekGloss;
		}
		if (Random.Range(0, 2) == 0)
		{
			base.face.makeup.lipId = chaFileFace.makeup.lipId;
			base.face.makeup.lipColor = chaFileFace.makeup.lipColor;
			base.face.makeup.lipGloss = chaFileFace.makeup.lipGloss;
		}
		else
		{
			base.face.makeup.lipId = chaFileFace2.makeup.lipId;
			base.face.makeup.lipColor = chaFileFace2.makeup.lipColor;
			base.face.makeup.lipGloss = chaFileFace2.makeup.lipGloss;
		}
		if (Random.Range(0, 2) == 0)
		{
			base.face.makeup.paintInfo[0].Copy(chaFileFace.makeup.paintInfo[0]);
		}
		else
		{
			base.face.makeup.paintInfo[0].Copy(chaFileFace2.makeup.paintInfo[0]);
		}
		if (Random.Range(0, 2) == 0)
		{
			base.face.makeup.paintInfo[1].Copy(chaFileFace.makeup.paintInfo[1]);
		}
		else
		{
			base.face.makeup.paintInfo[1].Copy(chaFileFace2.makeup.paintInfo[1]);
		}
		if (base.chaCtrl.sex == 0)
		{
			if (Random.Range(0, 2) == 0)
			{
				base.face.beardId = chaFileFace.beardId;
				base.face.beardColor = chaFileFace.beardColor;
			}
			else
			{
				base.face.beardId = chaFileFace2.beardId;
				base.face.beardColor = chaFileFace2.beardColor;
			}
		}
		ChaFileBody chaFileBody = chaFileControl.custom.body;
		ChaFileBody chaFileBody2 = chaFileControl2.custom.body;
		num = 0.5f + Random.Range(-0.2f, 0.2f);
		for (int num8 = 0; num8 < base.body.shapeValueBody.Length; num8++)
		{
			base.body.shapeValueBody[num8] = Mathf.Lerp(chaFileBody.shapeValueBody[num8], chaFileBody2.shapeValueBody[num8], num);
		}
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.bustSoftness = Mathf.Lerp(chaFileBody.bustSoftness, chaFileBody2.bustSoftness, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.bustWeight = Mathf.Lerp(chaFileBody.bustWeight, chaFileBody2.bustWeight, num);
		base.body.skinId = RandomIntWhich(chaFileBody.skinId, chaFileBody2.skinId);
		base.body.detailId = RandomIntWhich(chaFileBody.detailId, chaFileBody2.detailId);
		base.body.detailPower = Mathf.Lerp(chaFileBody.detailPower, chaFileBody2.detailPower, t);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.skinColor = ColorBlend(chaFileBody.skinColor, chaFileBody2.skinColor, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.skinGlossPower = Mathf.Lerp(chaFileBody.skinGlossPower, chaFileBody2.skinGlossPower, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.skinMetallicPower = Mathf.Lerp(chaFileBody.skinMetallicPower, chaFileBody2.skinMetallicPower, num);
		base.body.sunburnId = RandomIntWhich(chaFileBody.sunburnId, chaFileBody2.sunburnId);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.sunburnColor = ColorBlend(chaFileBody.sunburnColor, chaFileBody2.sunburnColor, num);
		if (Random.Range(0, 2) == 0)
		{
			base.body.paintInfo[0].Copy(chaFileBody.paintInfo[0]);
		}
		else
		{
			base.body.paintInfo[0].Copy(chaFileBody2.paintInfo[0]);
		}
		if (Random.Range(0, 2) == 0)
		{
			base.body.paintInfo[1].Copy(chaFileBody.paintInfo[1]);
		}
		else
		{
			base.body.paintInfo[1].Copy(chaFileBody2.paintInfo[1]);
		}
		base.body.nipId = RandomIntWhich(chaFileBody.nipId, chaFileBody2.nipId);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.nipColor = ColorBlend(chaFileBody.nipColor, chaFileBody2.nipColor, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.nipGlossPower = Mathf.Lerp(chaFileBody.nipGlossPower, chaFileBody2.nipGlossPower, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.areolaSize = Mathf.Lerp(chaFileBody.areolaSize, chaFileBody2.areolaSize, num);
		base.body.underhairId = RandomIntWhich(chaFileBody.underhairId, chaFileBody2.underhairId);
		base.body.underhairColor = ColorBlend(chaFileBody.underhairColor, chaFileBody2.underhairColor, num2);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.nailColor = ColorBlend(chaFileBody.nailColor, chaFileBody2.nailColor, num);
		num = 0.5f + Random.Range(-0.5f, 0.5f);
		base.body.nailGlossPower = Mathf.Lerp(chaFileBody.nailGlossPower, chaFileBody2.nailGlossPower, num);
		ChaFileHair chaFileHair = chaFileControl.custom.hair;
		ChaFileHair chaFileHair2 = chaFileControl2.custom.hair;
		if (Random.Range(0, 2) == 0)
		{
			byte[] bytes = MessagePackSerializer.Serialize(chaFileHair);
			base.chaCtrl.chaFile.custom.hair = MessagePackSerializer.Deserialize<ChaFileHair>(bytes);
		}
		else
		{
			byte[] bytes2 = MessagePackSerializer.Serialize(chaFileHair2);
			base.chaCtrl.chaFile.custom.hair = MessagePackSerializer.Deserialize<ChaFileHair>(bytes2);
		}
		for (int num9 = 0; num9 < base.hair.parts.Length; num9++)
		{
			base.hair.parts[num9].baseColor = ColorBlend(chaFileHair.parts[num9].baseColor, chaFileHair2.parts[num9].baseColor, num2);
			base.hair.parts[num9].topColor = ColorBlend(chaFileHair.parts[num9].topColor, chaFileHair2.parts[num9].topColor, num2);
			base.hair.parts[num9].underColor = ColorBlend(chaFileHair.parts[num9].underColor, chaFileHair2.parts[num9].underColor, num2);
			base.hair.parts[num9].specular = ColorBlend(chaFileHair.parts[num9].specular, chaFileHair2.parts[num9].specular, num2);
			base.hair.parts[num9].smoothness = Mathf.Lerp(chaFileHair.parts[num9].smoothness, chaFileHair2.parts[num9].smoothness, num2);
			base.hair.parts[num9].metallic = Mathf.Lerp(chaFileHair.parts[num9].metallic, chaFileHair2.parts[num9].metallic, num2);
		}
		if (Random.Range(0, 2) == 0)
		{
			base.chaCtrl.chaFile.CopyCoordinate(chaFileControl.coordinate);
		}
		else
		{
			base.chaCtrl.chaFile.CopyCoordinate(chaFileControl2.coordinate);
		}
		base.chaCtrl.ChangeNowCoordinate();
		Singleton<Character>.Instance.customLoadGCClear = false;
		base.chaCtrl.Reload();
		Singleton<Character>.Instance.customLoadGCClear = true;
	}

	protected override IEnumerator Start()
	{
		yield return base.Start();
		base.customBase.actUpdateCvsFusion += UpdateCustomUI;
		UpdateCharasList();
		if (null != btnFusion)
		{
			btnFusion.OnClickAsObservable().Subscribe(delegate
			{
				CustomCharaScrollController.ScrollData selectInfo = charaLoadWinA.GetSelectInfo();
				CustomCharaScrollController.ScrollData selectInfo2 = charaLoadWinB.GetSelectInfo();
				FusionProc(selectInfo.info.FileName, selectInfo2.info.FileName);
				isFusion = true;
			});
			btnFusion.UpdateAsObservable().Subscribe(delegate
			{
				CustomCharaScrollController.ScrollData selectInfo = charaLoadWinA.GetSelectInfo();
				CustomCharaScrollController.ScrollData selectInfo2 = charaLoadWinB.GetSelectInfo();
				btnFusion.interactable = selectInfo != null && selectInfo2 != null;
			});
		}
		if (!(null != btnExit))
		{
			yield break;
		}
		btnExit.OnClickAsObservable().Subscribe(delegate
		{
			base.customBase.customCtrl.showFusionCvs = false;
			base.customBase.customCtrl.showMainCvs = true;
			charaLoadWinA.SelectInfoClear();
			charaLoadWinB.SelectInfoClear();
			if (isFusion)
			{
				base.customBase.updateCustomUI = true;
				for (int i = 0; i < 20; i++)
				{
					base.customBase.ChangeAcsSlotName(i);
				}
				base.customBase.SetUpdateToggleSetting();
				base.customBase.forceUpdateAcsList = true;
			}
			isFusion = false;
		});
	}
}
