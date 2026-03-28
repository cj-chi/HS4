using System;
using Illusion.Extensions;
using Illusion.Game;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CvsC_ClothesInput : MonoBehaviour
{
	[SerializeField]
	private InputField inpName;

	[SerializeField]
	private GameObject objNameDummy;

	[SerializeField]
	private Text textNameDummy;

	[SerializeField]
	private Button btnEntry;

	[SerializeField]
	private Button btnBack;

	public Action<string> actEntry;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	public void SetupInputCoordinateNameWindow(string name)
	{
		customBase.customCtrl.showInputCoordinate = true;
		inpName.text = name;
		if (null != textNameDummy)
		{
			textNameDummy.text = name;
		}
		inpName.ActivateInputField();
	}

	private void Start()
	{
		customBase.lstInputField.Add(inpName);
		if (!(null != inpName))
		{
			return;
		}
		inpName.OnEndEditAsObservable().Subscribe(delegate
		{
			if (null != textNameDummy)
			{
				textNameDummy.text = inpName.text;
			}
		});
		if (null != objNameDummy)
		{
			objNameDummy.UpdateAsObservable().Subscribe(delegate
			{
				bool isFocused = inpName.isFocused;
				if (objNameDummy.activeSelf == isFocused)
				{
					objNameDummy.SetActiveIfDifferent(!isFocused);
				}
			});
		}
		if (null != btnBack)
		{
			btnBack.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				customBase.customCtrl.showInputCoordinate = false;
			});
		}
		if (null != btnEntry)
		{
			btnEntry.UpdateAsObservable().Subscribe(delegate
			{
				bool interactable = !inpName.text.IsNullOrEmpty();
				btnEntry.interactable = interactable;
			});
			btnEntry.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.save);
				customBase.customCtrl.showInputCoordinate = false;
				actEntry?.Invoke(inpName.text);
			});
		}
	}
}
