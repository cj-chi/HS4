using System;
using System.Collections;
using System.IO;
using AIChara;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace UploaderSystem;

public class UpPhpControl : NetPhpControl
{
	public UploadScene upScene;

	public UpUIControl uiCtrl;

	public IEnumerator UploadChara(IObserver<bool> observer, bool update)
	{
		int msgType = ((!update) ? 1 : 0);
		string msgCheck = NetworkDefine.msgUpConfirmation[Singleton<GameSystem>.Instance.languageInt, msgType];
		Utils.Sound.Play(SystemSE.sel);
		ObservableYieldInstruction<bool> chk = Observable.FromCoroutine((IObserver<bool> res) => base.popupCheck.CheckAnswerCor(res, msgCheck)).ToYieldInstruction(throwOnError: false);
		yield return chk;
		if (!chk.Result)
		{
			yield break;
		}
		base.netInfo.BlockUI();
		string errorMsg = NetworkDefine.msgUpFailer[Singleton<GameSystem>.Instance.languageInt, msgType];
		string uploadFile = uiCtrl.GetUploadFile(DataType.Chara);
		byte[] array = null;
		using (FileStream fileStream = new FileStream(uploadFile, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			array = binaryReader.ReadBytes((int)fileStream.Length);
		}
		if (array == null)
		{
			string text = NetworkDefine.msgUpCannotRead[Singleton<GameSystem>.Instance.languageInt, msgType];
			base.netInfo.DrawMessage(Color.red, text);
			observer.OnError(new Exception(text));
			base.netInfo.UnblockUI();
			yield break;
		}
		string comment = uiCtrl.GetComment(DataType.Chara);
		Singleton<Character>.Instance.isMod = false;
		ChaFileControl cfc = new ChaFileControl();
		cfc.LoadCharaFile(uploadFile, byte.MaxValue, noLoadPng: true);
		bool flag = false;
		if (Singleton<Character>.Instance.isMod)
		{
			flag = true;
		}
		NetworkInfo.CharaInfo charaInfo = base.netInfo.lstCharaInfo.Find((NetworkInfo.CharaInfo x) => x.data_uid == cfc.dataID);
		int value = 0;
		if (update)
		{
			if (charaInfo == null)
			{
				if (!base.netInfo.dictUploaded[0].TryGetValue(cfc.dataID, out value))
				{
					string text = NetworkDefine.msgUpCannotBeIdentified[Singleton<GameSystem>.Instance.languageInt];
					base.netInfo.DrawMessage(Color.red, text);
					observer.OnError(new Exception(text));
					base.netInfo.UnblockUI();
					yield break;
				}
			}
			else
			{
				value = charaInfo.idx;
			}
		}
		else if (charaInfo != null)
		{
			string text = NetworkDefine.msgUpAlreadyUploaded[Singleton<GameSystem>.Instance.languageInt];
			base.netInfo.DrawMessage(Color.red, text);
			observer.OnError(new Exception(text));
			base.netInfo.UnblockUI();
			yield break;
		}
		string uri = CreateURL.LoadURL("hs2_uploadChara_url.dat");
		WWWForm wWWForm = new WWWForm();
		if (update)
		{
			wWWForm.AddField("mode", 3);
		}
		else
		{
			wWWForm.AddField("mode", 2);
		}
		wWWForm.AddBinaryData("png", array);
		if (update)
		{
			wWWForm.AddField("index", value);
		}
		else
		{
			wWWForm.AddField("chara_uid", cfc.dataID);
		}
		wWWForm.AddField("upload_type", 0);
		wWWForm.AddField("uuid", Singleton<GameSystem>.Instance.UserUUID);
		wWWForm.AddField("passwd", Singleton<GameSystem>.Instance.UserPasswd);
		wWWForm.AddField("mac_id", Singleton<GameSystem>.Instance.EncryptedMacAddress);
		wWWForm.AddField("name", cfc.parameter.fullname);
		wWWForm.AddField("voicetype", (cfc.parameter.sex == 0) ? 99 : cfc.parameter2.personality);
		wWWForm.AddField("birthmonth", cfc.parameter.birthMonth);
		wWWForm.AddField("birthday", cfc.parameter.birthDay);
		wWWForm.AddField("comment", comment);
		wWWForm.AddField("sex", flag ? 99 : cfc.parameter.sex);
		wWWForm.AddField("height", cfc.custom.GetHeightKind());
		wWWForm.AddField("bust", (cfc.parameter.sex == 0) ? 99 : cfc.custom.GetBustSizeKind());
		wWWForm.AddField("hair", cfc.custom.hair.kind);
		wWWForm.AddField("trait", cfc.parameter2.trait);
		wWWForm.AddField("mind", cfc.parameter2.mind);
		wWWForm.AddField("h_attribute", cfc.parameter2.hAttribute);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 60;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			base.netInfo.UnblockUI();
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				base.netInfo.UnblockUI();
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				base.netInfo.UnblockUI();
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(request.downloadHandler.text))
		{
			bool flag2 = false;
			if (!update)
			{
				if (int.TryParse(request.downloadHandler.text, out var result))
				{
					flag2 = true;
					base.netInfo.dictUploaded[0][cfc.dataID] = result;
					uiCtrl.ChangeUploadData();
				}
			}
			else if ("S_OK" == request.downloadHandler.text)
			{
				flag2 = true;
			}
			if (flag2)
			{
				observer.OnNext(value: true);
				observer.OnCompleted();
				base.netInfo.changeCharaList = true;
				base.netInfo.DrawMessage(NetworkDefine.colorWhite, NetworkDefine.msgUpDone[Singleton<GameSystem>.Instance.languageInt, msgType]);
				base.netInfo.UnblockUI();
				yield break;
			}
		}
		base.netInfo.DrawMessage(Color.red, errorMsg);
		observer.OnError(new Exception(errorMsg));
		base.netInfo.UnblockUI();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
