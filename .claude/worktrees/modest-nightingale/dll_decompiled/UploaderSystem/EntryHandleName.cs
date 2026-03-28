using System;
using System.Collections;
using Illusion.Game;
using Manager;
using MyLocalize;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace UploaderSystem;

public class EntryHandleName : MonoBehaviour
{
	public string backSceneName = "Title";

	[SerializeField]
	private Canvas cvsChangeScene;

	[SerializeField]
	private InputField inpHandleName;

	[SerializeField]
	private Button btnYes;

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private EntryHNCultureControl cultureCtrl;

	private string handleName = "";

	private bool notIllusion = true;

	public Action onEnd;

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		if (null != cultureCtrl)
		{
			cultureCtrl.ChangeLocalize(MyLocalizeDefine.LocalizeKeyType.EntryHandleName, Singleton<GameSystem>.Instance.languageInt);
		}
		handleName = Singleton<GameSystem>.Instance.HandleName;
		inpHandleName.text = handleName;
		inpHandleName.ActivateInputField();
		inpHandleName.OnValueChangedAsObservable().Skip(1).Subscribe(delegate
		{
			handleName = "";
		});
		inpHandleName.OnEndEditAsObservable().Subscribe(delegate(string buf)
		{
			if (buf == "イリュージョン公式")
			{
				notIllusion = false;
			}
			else
			{
				notIllusion = true;
			}
			handleName = buf;
		});
		if ((bool)btnYes)
		{
			Text text = btnYes.GetComponentInChildren<Text>(includeInactive: true);
			btnYes.UpdateAsObservable().Subscribe(delegate
			{
				bool flag = !handleName.IsNullOrEmpty() && notIllusion;
				if (!Input.GetMouseButton(0) && btnYes.interactable != flag)
				{
					btnYes.interactable = flag;
				}
				if (null != text)
				{
					text.color = new Color(text.color.r, text.color.g, text.color.b, flag ? 1f : 0.5f);
				}
			});
			btnYes.OnClickAsObservable().Subscribe(delegate
			{
				Singleton<GameSystem>.Instance.SaveHandleName(handleName);
				Utils.Sound.Play(SystemSE.ok_s);
				cvsChangeScene.gameObject.SetActive(value: true);
				if ("Uploader" == backSceneName || "Downloader" == backSceneName)
				{
					Scene.Unload();
				}
				else
				{
					Scene.Data data = new Scene.Data();
					data.levelName = "NetworkCheckScene";
					data.fadeType = FadeCanvas.Fade.In;
					data.Async(isOn: true);
					Scene.LoadReserve(data, isLoadingImageDraw: true);
				}
			});
		}
		if ((bool)btnNo)
		{
			btnNo.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				if ("Uploader" == backSceneName || "Downloader" == backSceneName)
				{
					Scene.Unload();
				}
				else
				{
					Scene.LoadReserve(new Scene.Data
					{
						levelName = "Title",
						fadeType = FadeCanvas.Fade.In
					}, isLoadingImageDraw: true);
				}
			});
		}
		base.enabled = true;
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
	}

	private void OnDestroy()
	{
		onEnd?.Invoke();
	}
}
