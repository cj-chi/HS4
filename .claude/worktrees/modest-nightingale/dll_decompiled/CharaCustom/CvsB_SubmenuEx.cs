using AIChara;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsB_SubmenuEx : MonoBehaviour
{
	[SerializeField]
	private Toggle tglFutanari;

	protected CustomBase customBase => Singleton<CustomBase>.Instance;

	protected ChaControl chaCtrl => customBase.chaCtrl;

	protected ChaFileParameter parameter => chaCtrl.fileParam;

	public void UpdateCustomUI()
	{
		tglFutanari.SetIsOnWithoutCallback(parameter.futanari);
	}

	private void Start()
	{
		customBase.actUpdateCvsFutanari += UpdateCustomUI;
		tglFutanari.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			parameter.futanari = isOn;
		});
	}
}
