using System.Collections;
using AIChara;
using Illusion.Game;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class FoundFemaleWindow : SvsBase
{
	[Header("【名前】------------------------")]
	[SerializeField]
	private Text txtName;

	[Header("【性格】------------------------")]
	[SerializeField]
	private Text txtPersonal;

	[Header("【声質】----------------------")]
	[SerializeField]
	private Text txtPitch;

	[Header("【特性】----------------------")]
	[SerializeField]
	private Text txtTrait;

	[Header("【心情】----------------------")]
	[SerializeField]
	private Text txtMind;

	[Header("【H属性】----------------------")]
	[SerializeField]
	private Text txtHAttribute;

	[Header("【保存】----------------------")]
	[SerializeField]
	private Button btnSave;

	[SerializeField]
	private Text txtSave;

	private readonly string[] strHigh = new string[5] { "高い", "", "", "", "" };

	private readonly string[] strNormal = new string[5] { "普通", "", "", "", "" };

	private readonly string[] strLow = new string[5] { "低い", "", "", "", "" };

	public void UpdateUI(ChaFileControl _chaFile)
	{
		if (_chaFile == null)
		{
			Init();
			return;
		}
		txtName.text = _chaFile.parameter.fullname;
		ChaFileParameter2 chaFileParameter = _chaFile.parameter2;
		int languageInt = Singleton<GameSystem>.Instance.languageInt;
		txtPersonal.text = Voice.infoTable[chaFileParameter.personality].Get(languageInt);
		if (chaFileParameter.voiceRate > 0.66f)
		{
			txtPitch.text = strHigh[languageInt];
		}
		else if (chaFileParameter.voiceRate > 0.33f)
		{
			txtPitch.text = strNormal[languageInt];
		}
		else
		{
			txtPitch.text = strLow[languageInt];
		}
		txtTrait.text = Game.infoTraitTable[chaFileParameter.trait];
		txtMind.text = Game.infoMindTable[chaFileParameter.mind];
		txtHAttribute.text = Game.infoHAttributeTable[chaFileParameter.hAttribute];
	}

	public void Init()
	{
		txtName.text = string.Empty;
		txtPersonal.text = string.Empty;
		txtPitch.text = string.Empty;
		txtTrait.text = string.Empty;
		txtMind.text = string.Empty;
		txtHAttribute.text = string.Empty;
	}

	protected override IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		Init();
		btnSave.OnClickAsObservable().Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.ok_s);
			base.searchBase.searchCtrl.saveMode = true;
			txtSave.color = Game.defaultFontColor;
		});
		btnSave.OnPointerEnterAsObservable().Subscribe(delegate
		{
			txtSave.color = Game.selectFontColor;
		});
		btnSave.OnPointerExitAsObservable().Subscribe(delegate
		{
			txtSave.color = Game.defaultFontColor;
		});
		yield return null;
	}
}
