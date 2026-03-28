using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIChara;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace UploaderSystem;

public class NetPhpControl : MonoBehaviour
{
	public Canvas cvsChangeScene;

	protected NetworkInfo netInfo => Singleton<NetworkInfo>.Instance;

	protected NetCacheControl cacheCtrl
	{
		get
		{
			if (!Singleton<NetworkInfo>.IsInstance())
			{
				return null;
			}
			return netInfo.cacheCtrl;
		}
	}

	protected LogView logview => netInfo.logview;

	protected Net_PopupMsg popupMsg => netInfo.popupMsg;

	protected Net_PopupCheck popupCheck => netInfo.popupCheck;

	public NetworkInfo.Profile profile => netInfo.profile;

	public Dictionary<int, NetworkInfo.UserInfo> dictUserInfo => netInfo.dictUserInfo;

	public List<NetworkInfo.CharaInfo> lstCharaInfo => netInfo.lstCharaInfo;

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

	private bool isUpload => "Uploader" == Singleton<GameSystem>.Instance.networkSceneName;

	protected string EncodeFromBase64(string buf)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(buf));
	}

	public IEnumerator GetBaseInfo(bool upload)
	{
		logview.StartLog();
		logview.AddLog(NetworkDefine.msgNetGetInfoFromServer[Singleton<GameSystem>.Instance.languageInt]);
		logview.AddLog(NetworkDefine.msgNetGetVersion[Singleton<GameSystem>.Instance.languageInt]);
		yield return Observable.FromCoroutine((IObserver<bool> res) => GetNewestVersion(res)).ToYieldInstruction(throwOnError: false);
		if (!isHS2)
		{
			goto IL_019a;
		}
		logview.AddLog(NetworkDefine.msgNetConfirmUser[Singleton<GameSystem>.Instance.languageInt]);
		ObservableYieldInstruction<bool> userIdx = Observable.FromCoroutine((IObserver<bool> res) => UpdateUserInfo(res)).ToYieldInstruction(throwOnError: false);
		yield return userIdx;
		if (!userIdx.HasError)
		{
			logview.AddLog(NetworkDefine.msgNetStartEntryHN[Singleton<GameSystem>.Instance.languageInt]);
			ObservableYieldInstruction<string> hn = Observable.FromCoroutine((IObserver<string> res) => UpdateHandleName(res)).ToYieldInstruction(throwOnError: false);
			yield return hn;
			if (!hn.HasError)
			{
				goto IL_019a;
			}
		}
		goto IL_02cc;
		IL_02cc:
		logview.AddLog(NetworkDefine.msgNetNotReady[Singleton<GameSystem>.Instance.languageInt]);
		logview.EndLog();
		logview.onClose = delegate
		{
			Scene.LoadReserve(new Scene.Data
			{
				levelName = "Title",
				isAdd = false,
				isFade = true
			}, isLoadingImageDraw: true);
			if (null != cvsChangeScene)
			{
				cvsChangeScene.gameObject.SetActiveIfDifferent(active: true);
			}
		};
		yield break;
		IL_019a:
		logview.AddLog(NetworkDefine.msgNetGetAllHN[Singleton<GameSystem>.Instance.languageInt]);
		ObservableYieldInstruction<bool> allusers = Observable.FromCoroutine((IObserver<bool> res) => GetAllUsers(res)).ToYieldInstruction(throwOnError: false);
		yield return allusers;
		if (!allusers.HasError)
		{
			logview.AddLog(NetworkDefine.msgNetGetAllCharaInfo[Singleton<GameSystem>.Instance.languageInt]);
			ObservableYieldInstruction<bool> allchara = Observable.FromCoroutine((IObserver<bool> res) => GetAllCharaInfo(res)).ToYieldInstruction(throwOnError: false);
			yield return allchara;
			if (!allchara.HasError)
			{
				netInfo.dictUploaded[0].Clear();
				if (!upload)
				{
					netInfo.netSelectHN.Init();
				}
				logview.AddLog(NetworkDefine.msgNetReady[Singleton<GameSystem>.Instance.languageInt]);
				logview.EndLog();
				logview.SetActiveCanvas(active: false);
				yield break;
			}
		}
		goto IL_02cc;
	}

	public IEnumerator UpdateBaseInfo(IObserver<bool> observer)
	{
		netInfo.BlockUI();
		netInfo.noUserControl = true;
		if (netInfo.changeCharaList)
		{
			netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgNetGetAllCharaInfo[Singleton<GameSystem>.Instance.languageInt]);
			ObservableYieldInstruction<bool> allchara = Observable.FromCoroutine((IObserver<bool> res) => GetAllCharaInfo(res)).ToYieldInstruction(throwOnError: false);
			yield return allchara;
			if (allchara.HasError)
			{
				observer.OnError(new Exception(NetworkDefine.msgNetFailedGetCharaInfo[Singleton<GameSystem>.Instance.languageInt]));
			}
			netInfo.changeCharaList = false;
			netInfo.dictUploaded[0].Clear();
		}
		netInfo.noUserControl = false;
		netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgNetReadyGetData[Singleton<GameSystem>.Instance.languageInt]);
		observer.OnNext(value: true);
		observer.OnCompleted();
		netInfo.UnblockUI();
	}

	public IEnumerator GetNewestVersion(IObserver<bool> observer)
	{
		string errorMsg = NetworkDefine.msgNetFailedGetVersion[Singleton<GameSystem>.Instance.languageInt];
		string uri = CreateURL.LoadURL((!isUpload && !isHS2) ? "ais_version_url.dat" : "hs2_version_url.dat");
		UnityWebRequest request = UnityWebRequest.Get(uri);
		request.timeout = 20;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!request.downloadHandler.text.IsNullOrEmpty())
		{
			netInfo.newestVersion = new System.Version(request.downloadHandler.text);
			netInfo.updateVersion = true;
			observer.OnNext(value: true);
			observer.OnCompleted();
		}
		else
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
		}
	}

	public IEnumerator UpdateUserInfo(IObserver<bool> observer)
	{
		string uri = CreateURL.LoadURL((!isUpload && !isHS2) ? "ais_system_url.dat" : "hs2_system_url.dat");
		string errorMsg = NetworkDefine.msgNetFailedConfirmUser[Singleton<GameSystem>.Instance.languageInt];
		string userUUID = Singleton<GameSystem>.Instance.UserUUID;
		string userPasswd = Singleton<GameSystem>.Instance.UserPasswd;
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 0);
		wWWForm.AddField("uuid", userUUID);
		wWWForm.AddField("passwd", userPasswd);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 20;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		bool value = true;
		if (!request.downloadHandler.text.IsNullOrEmpty())
		{
			string[] array = request.downloadHandler.text.Split('\t');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = EncodeFromBase64(array[i]);
			}
			int num = 0;
			if (array.Length >= 1)
			{
				if (!int.TryParse(array[num], out var result))
				{
					value = false;
				}
				else
				{
					profile.userIdx = result;
				}
				observer.OnNext(value);
				observer.OnCompleted();
				netInfo.updateProfile = true;
				yield break;
			}
		}
		netInfo.DrawMessage(Color.red, errorMsg);
		observer.OnError(new Exception(errorMsg));
	}

	public IEnumerator UpdateHandleName(IObserver<string> observer)
	{
		string uri = CreateURL.LoadURL((!isUpload && !isHS2) ? "ais_system_url.dat" : "hs2_system_url.dat");
		string errorMsg = NetworkDefine.msgNetFailedUpdateHN[Singleton<GameSystem>.Instance.languageInt];
		string userUUID = Singleton<GameSystem>.Instance.UserUUID;
		string userPasswd = Singleton<GameSystem>.Instance.UserPasswd;
		string handleName = Singleton<GameSystem>.Instance.HandleName;
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 1);
		wWWForm.AddField("uuid", userUUID);
		wWWForm.AddField("passwd", userPasswd);
		wWWForm.AddField("handle_name", handleName);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 20;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!request.downloadHandler.text.IsNullOrEmpty())
		{
			observer.OnNext(request.downloadHandler.text);
			observer.OnCompleted();
			netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgNetUpdatedHN[Singleton<GameSystem>.Instance.languageInt]);
		}
		else
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
		}
	}

	public IEnumerator GetAllUsers(IObserver<bool> observer)
	{
		string uri = CreateURL.LoadURL((!isUpload && !isHS2) ? "ais_system_url.dat" : "hs2_system_url.dat");
		string errorMsg = NetworkDefine.msgNetFailedGetAllHN[Singleton<GameSystem>.Instance.languageInt];
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 2);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 30;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			netInfo.DrawMessage(Color.red, request.error);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(request.downloadHandler.text))
		{
			dictUserInfo.Clear();
			string[] array = request.downloadHandler.text.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('\t');
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = EncodeFromBase64(array2[j]);
				}
				if (array2.Length >= 1 && int.TryParse(array2[0], out var result))
				{
					NetworkInfo.UserInfo userInfo = new NetworkInfo.UserInfo();
					userInfo.handleName = array2[1];
					dictUserInfo[result] = userInfo;
				}
			}
			observer.OnNext(value: true);
			observer.OnCompleted();
		}
		else
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
		}
	}

	public IEnumerator GetAllCharaInfo(IObserver<bool> observer)
	{
		string uri = CreateURL.LoadURL((!isUpload && !isHS2) ? "ais_uploadChara_url.dat" : "hs2_uploadChara_url.dat");
		string errorMsg = NetworkDefine.msgNetFailedGetCharaInfo[Singleton<GameSystem>.Instance.languageInt];
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 0);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 30;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(request.downloadHandler.text))
		{
			lstCharaInfo.Clear();
			string[] array = request.downloadHandler.text.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('\t');
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = EncodeFromBase64(array2[j]);
				}
				if (array2.Length < 0)
				{
					continue;
				}
				NetworkInfo.CharaInfo charaInfo = new NetworkInfo.CharaInfo();
				int num = 0;
				if (int.TryParse(array2[num++], out var result))
				{
					charaInfo.idx = result;
				}
				charaInfo.data_uid = array2[num++];
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.user_idx = result;
				}
				charaInfo.name = array2[num++];
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.type = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.birthmonth = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.birthday = result;
				}
				charaInfo.strBirthDay = ChaFileDefine.GetBirthdayStr(charaInfo.birthmonth, charaInfo.birthday, Singleton<GameSystem>.Instance.language);
				charaInfo.comment = array2[num++];
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.sex = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.height = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.bust = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.hair = result;
				}
				if (Singleton<GameSystem>.Instance.networkType == 0)
				{
					if (int.TryParse(array2[num++], out result))
					{
						charaInfo.trait = result;
					}
					if (int.TryParse(array2[num++], out result))
					{
						charaInfo.mind = result;
					}
					if (int.TryParse(array2[num++], out result))
					{
						charaInfo.hAttribute = result;
					}
				}
				else
				{
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
					num++;
				}
				if (!isHS2 && int.TryParse(array2[num++], out result))
				{
					charaInfo.isChangeParameter = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.dlCount = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.weekCount = result;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.applause = result;
				}
				if (DateTime.TryParse(array2[num++], out var result2))
				{
					charaInfo.updateTime = result2;
				}
				if (int.TryParse(array2[num++], out result))
				{
					charaInfo.update_idx = result;
				}
				if (DateTime.TryParse(array2[num], out result2))
				{
					charaInfo.createTime = result2;
				}
				lstCharaInfo.Add(charaInfo);
			}
			if (lstCharaInfo.Count != 0)
			{
				int count = lstCharaInfo.Count;
				netInfo.lstCharaInfo = lstCharaInfo.OrderByDescending((NetworkInfo.CharaInfo item) => item.dlCount).ToList();
				for (int num2 = 0; num2 < count; num2++)
				{
					lstCharaInfo[num2].rankingT = num2;
				}
				netInfo.lstCharaInfo = lstCharaInfo.OrderByDescending((NetworkInfo.CharaInfo item) => item.weekCount).ToList();
				for (int num3 = 0; num3 < count; num3++)
				{
					lstCharaInfo[num3].rankingW = num3;
				}
			}
		}
		observer.OnNext(value: true);
		observer.OnCompleted();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
