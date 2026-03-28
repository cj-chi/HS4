using System.Collections;
using System.IO;
using Illusion.Extensions;
using Manager;
using MyLocalize;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UploaderSystem;

public class NetworkCheckScene : MonoBehaviour
{
	public Text txtInfomation;

	public GameObject objClick;

	public GameObject objMsg;

	private CoroutineAssist caCheck;

	private int nextType = -1;

	private bool changeScene;

	private float startTime;

	private bool maintenance;

	private bool isActiveUploader;

	[SerializeField]
	private NetworkCheckCultureControl cultureCtrl;

	private bool isHS2
	{
		get
		{
			if (Singleton<GameSystem>.Instance.networkType != 0)
			{
				return false;
			}
			return true;
		}
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		if (null != cultureCtrl)
		{
			cultureCtrl.ChangeLocalize(MyLocalizeDefine.LocalizeKeyType.NetworkCheck, Singleton<GameSystem>.Instance.languageInt);
		}
		if (File.Exists(UserData.Path + "maintenance.dat"))
		{
			maintenance = true;
		}
		caCheck = new CoroutineAssist(this, CheckNetworkStatus);
		caCheck.Start(_enableTimeout: true, 10f);
		startTime = Time.realtimeSinceStartup;
		base.enabled = true;
		Scene.sceneFadeCanvas.StartFade(FadeCanvas.Fade.Out);
	}

	public IEnumerator CheckNetworkStatus()
	{
		string text = CreateURL.LoadURL(isHS2 ? "hs2_check_url.dat" : "ais_check_url.dat");
		if (text.IsNullOrEmpty())
		{
			if ((bool)txtInfomation)
			{
				txtInfomation.text = NetworkDefine.msgServerAccessInfoField[Singleton<GameSystem>.Instance.languageInt];
			}
			caCheck.EndStatus();
			if ((bool)objClick)
			{
				objClick.SetActiveIfDifferent(active: true);
			}
			if ((bool)objMsg)
			{
				objMsg.SetActiveIfDifferent(active: false);
			}
			nextType = 2;
			yield break;
		}
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", (!(Singleton<GameSystem>.Instance.networkSceneName == "Uploader")) ? 1 : 0);
		wWWForm.AddField("version", Singleton<GameSystem>.Instance.GameVersion.ToString());
		UnityWebRequest request = UnityWebRequest.Post(text, wWWForm);
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			if ((bool)txtInfomation)
			{
				txtInfomation.text = NetworkDefine.msgServerAccessField[Singleton<GameSystem>.Instance.languageInt];
			}
			caCheck.EndStatus();
			if ((bool)objClick)
			{
				objClick.SetActiveIfDifferent(active: true);
			}
			if ((bool)objMsg)
			{
				objMsg.SetActiveIfDifferent(active: false);
			}
			nextType = 2;
			yield break;
		}
		string[] array = request.downloadHandler.text.Split("\t"[0]);
		if ("0" == array[0])
		{
			nextType = 1;
		}
		else if ("1" == array[0])
		{
			nextType = 0;
			txtInfomation.text = array[1];
			if ((bool)objClick)
			{
				objClick.SetActiveIfDifferent(active: true);
			}
			if ((bool)objMsg)
			{
				objMsg.SetActiveIfDifferent(active: false);
			}
		}
		else if (maintenance && array[1].Contains("メンテナンス"))
		{
			nextType = 1;
		}
		else
		{
			nextType = 2;
			txtInfomation.text = array[1];
			if ((bool)objClick)
			{
				objClick.SetActiveIfDifferent(active: true);
			}
			if ((bool)objMsg)
			{
				objMsg.SetActiveIfDifferent(active: false);
			}
		}
		caCheck.EndStatus();
		yield return null;
	}

	private void Update()
	{
		if (-1 == nextType)
		{
			int num = Mathf.FloorToInt(Time.realtimeSinceStartup - startTime);
			string text = NetworkDefine.msgServerCheck[Singleton<GameSystem>.Instance.languageInt];
			for (int i = 0; i < num; i++)
			{
				text += "．";
			}
			if ((bool)txtInfomation)
			{
				txtInfomation.text = text;
			}
		}
		if (caCheck != null && CoroutineAssist.Status.Run == caCheck.status && caCheck.TimeOutCheck())
		{
			caCheck.End();
			if ((bool)txtInfomation)
			{
				txtInfomation.text = NetworkDefine.msgServerAccessField[Singleton<GameSystem>.Instance.languageInt];
			}
			caCheck.EndStatus();
			if ((bool)objClick)
			{
				objClick.SetActiveIfDifferent(active: true);
			}
			if ((bool)objMsg)
			{
				objMsg.SetActiveIfDifferent(active: false);
			}
			nextType = 2;
		}
		if (changeScene)
		{
			return;
		}
		if (nextType == 0)
		{
			if (Input.anyKeyDown)
			{
				nextType = 1;
			}
		}
		else if (1 == nextType)
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = Singleton<GameSystem>.Instance.networkSceneName,
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
			changeScene = true;
		}
		else if (2 == nextType && Input.anyKeyDown)
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "Title",
				fadeType = FadeCanvas.Fade.In
			}, isLoadingImageDraw: true);
			changeScene = true;
		}
	}
}
