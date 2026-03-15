using System.Linq;
using AIChara;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsH_SubmenuEx : MonoBehaviour
{
	[SerializeField]
	private Toggle[] tglShaderType;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	protected ChaFileHair hair => chaCtrl.fileHair;

	public void UpdateCustomUI()
	{
		tglShaderType[hair.shaderType].SetIsOnWithoutCallback(isOn: true);
		tglShaderType[(hair.shaderType + 1) % 2].SetIsOnWithoutCallback(isOn: false);
	}

	private void Start()
	{
		customBase.actUpdateCvsHair += UpdateCustomUI;
		if (!tglShaderType.Any())
		{
			return;
		}
		tglShaderType.Select((Toggle p, int idx) => new
		{
			toggle = p,
			index = (byte)idx
		}).ToList().ForEach(p =>
		{
			(from isOn in p.toggle.onValueChanged.AsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				hair.shaderType = p.index;
				chaCtrl.ChangeSettingHairShader();
				chaCtrl.ChangeSettingHairTypeAccessoryShaderAll();
			});
		});
	}
}
