using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Illusion.Game;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace UploaderSystem;

public class DownPhpControl : NetPhpControl
{
	public DownloadScene downScene;

	public DownUIControl uiCtrl;

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

	public IEnumerator AddApplauseCount(IObserver<bool> observer, DataType type, NetworkInfo.BaseIndex info)
	{
		string errorMsg = NetworkDefine.msgDownLikes[Singleton<GameSystem>.Instance.languageInt];
		_ = base.profile.userIdx;
		int idx = info.idx;
		if (Singleton<GameSystem>.Instance.IsApplause(type, isHS2, info.data_uid))
		{
			observer.OnNext(value: true);
			observer.OnCompleted();
			yield break;
		}
		string uri = CreateURL.LoadURL(isHS2 ? "hs2_uploadChara_url.dat" : "ais_uploadChara_url.dat");
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 6);
		wWWForm.AddField("index", idx);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 20;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(request.downloadHandler.text) && "S_OK" == request.downloadHandler.text)
		{
			observer.OnNext(value: true);
			observer.OnCompleted();
		}
		else
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
		}
	}

	public IEnumerator GetThumbnail(IObserver<bool> observer, DataType type)
	{
		string errorMsg = NetworkDefine.msgDownFailedGetThumbnail[Singleton<GameSystem>.Instance.languageInt];
		Exception exp = null;
		Dictionary<string, Dictionary<int, Tuple<int, byte[]>>> dictUpdateThumb = new Dictionary<string, Dictionary<int, Tuple<int, byte[]>>>();
		Dictionary<int, Tuple<int, byte[]>> dictGetThumb = new Dictionary<int, Tuple<int, byte[]>>();
		Dictionary<int, byte[]> dictThumbBytes = new Dictionary<int, byte[]>();
		Tuple<int, int>[] datas = uiCtrl.GetThumbnailIndex(type);
		if (datas.Length == 0)
		{
			string text = NetworkDefine.msgDownNotUploadDataFound[Singleton<GameSystem>.Instance.languageInt];
			base.netInfo.DrawMessage(Color.red, text);
			observer.OnError(new Exception(text));
			yield break;
		}
		if (base.cacheCtrl.enableCache)
		{
			for (int i = 0; i < datas.Length; i++)
			{
				dictThumbBytes[datas[i].Item1] = null;
				NetCacheControl.CacheHeader ch;
				string cacheHeader = base.cacheCtrl.GetCacheHeader(type, isHS2, datas[i].Item1, out ch);
				if ("" != cacheHeader)
				{
					if (datas[i].Item2 == ch.update_idx)
					{
						dictThumbBytes[datas[i].Item1] = base.cacheCtrl.LoadCache(cacheHeader, ch.pos, ch.size);
						continue;
					}
					if (!dictUpdateThumb.TryGetValue(cacheHeader, out var value))
					{
						value = (dictUpdateThumb[cacheHeader] = new Dictionary<int, Tuple<int, byte[]>>());
					}
					value[ch.idx] = new Tuple<int, byte[]>(datas[i].Item2, null);
				}
				else
				{
					dictGetThumb[datas[i].Item1] = new Tuple<int, byte[]>(datas[i].Item2, null);
				}
			}
		}
		else
		{
			for (int j = 0; j < datas.Length; j++)
			{
				dictGetThumb[datas[j].Item1] = new Tuple<int, byte[]>(datas[j].Item2, null);
				dictThumbBytes[datas[j].Item1] = null;
			}
		}
		List<int> lstIndexs;
		UnityWebRequest request;
		if (dictGetThumb.Count != 0 || dictUpdateThumb.Count != 0)
		{
			lstIndexs = new List<int>();
			StringBuilder stringBuilder = new StringBuilder(256);
			foreach (KeyValuePair<string, Dictionary<int, Tuple<int, byte[]>>> item in dictUpdateThumb)
			{
				foreach (KeyValuePair<int, Tuple<int, byte[]>> item2 in item.Value)
				{
					stringBuilder.Append(item2.Key.ToString()).Append("\t");
					lstIndexs.Add(item2.Key);
				}
			}
			foreach (KeyValuePair<int, Tuple<int, byte[]>> item3 in dictGetThumb)
			{
				stringBuilder.Append(item3.Key.ToString()).Append("\t");
				lstIndexs.Add(item3.Key);
			}
			string value2 = stringBuilder.ToString().TrimEnd('\t');
			string uri = CreateURL.LoadURL(isHS2 ? "hs2_uploadChara_url.dat" : "ais_uploadChara_url.dat");
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("mode", 1);
			wWWForm.AddField("indexs", value2);
			request = UnityWebRequest.Post(uri, wWWForm);
			request.timeout = 120;
			yield return request.SendWebRequest();
			if (request.isHttpError || request.isNetworkError)
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				exp = new Exception(request.error);
			}
			else
			{
				if (!request.isDone)
				{
					goto IL_0522;
				}
				if (request.downloadHandler.text.StartsWith("ERROR_"))
				{
					base.netInfo.DrawMessage(Color.red, errorMsg);
					exp = new Exception(request.downloadHandler.text);
				}
				else
				{
					if (!request.downloadHandler.text.StartsWith("SQLSTATE"))
					{
						goto IL_0522;
					}
					base.netInfo.DrawMessage(Color.red, errorMsg);
					exp = new Exception(request.downloadHandler.text);
				}
			}
		}
		goto IL_07bf;
		IL_07bf:
		byte[][] array = new byte[datas.Length][];
		for (int k = 0; k < datas.Length; k++)
		{
			if (dictThumbBytes.TryGetValue(datas[k].Item1, out var value3))
			{
				array[k] = value3;
			}
		}
		uiCtrl.ChangeThumbnail(array);
		if (exp == null)
		{
			observer.OnNext(value: true);
			observer.OnCompleted();
		}
		else
		{
			observer.OnError(exp);
		}
		yield break;
		IL_0522:
		if (!string.IsNullOrEmpty(request.downloadHandler.text))
		{
			string[] array2 = request.downloadHandler.text.Split('\t');
			for (int l = 0; l < lstIndexs.Count; l++)
			{
				byte[] array3 = null;
				if (!(array2[l] == ""))
				{
					array3 = Convert.FromBase64String(array2[l]);
				}
				dictThumbBytes[lstIndexs[l]] = array3;
				if (dictGetThumb.ContainsKey(lstIndexs[l]))
				{
					dictGetThumb[lstIndexs[l]] = new Tuple<int, byte[]>(dictGetThumb[lstIndexs[l]].Item1, array3);
					continue;
				}
				foreach (KeyValuePair<string, Dictionary<int, Tuple<int, byte[]>>> item4 in dictUpdateThumb)
				{
					if (item4.Value.ContainsKey(lstIndexs[l]))
					{
						item4.Value[lstIndexs[l]] = new Tuple<int, byte[]>(item4.Value[lstIndexs[l]].Item1, array3);
						break;
					}
				}
			}
			foreach (KeyValuePair<string, Dictionary<int, Tuple<int, byte[]>>> item5 in dictUpdateThumb)
			{
				Dictionary<int, Tuple<int, byte[]>> dictionary2 = base.cacheCtrl.LoadCacheFile(item5.Key);
				foreach (KeyValuePair<int, Tuple<int, byte[]>> item6 in item5.Value)
				{
					if (item6.Value.Item2 != null)
					{
						dictionary2[item6.Key] = item6.Value;
					}
				}
				base.cacheCtrl.SaveCacheFile(item5.Key, dictionary2);
			}
			base.cacheCtrl.CreateCache(type, isHS2, dictGetThumb);
			base.cacheCtrl.UpdateCacheHeaderInfo(type, isHS2);
		}
		else
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			exp = new Exception(errorMsg);
		}
		goto IL_07bf;
	}

	public IEnumerator DownloadPNG(IObserver<byte[]> observer, DataType type)
	{
		string errorMsg = NetworkDefine.msgDownFailed[Singleton<GameSystem>.Instance.languageInt];
		NetworkInfo.BaseIndex selectServerInfo = uiCtrl.GetSelectServerInfo(type);
		int idx = selectServerInfo.idx;
		int i = ((!Singleton<GameSystem>.Instance.IsDownload(type, isHS2, selectServerInfo.data_uid)) ? 1 : 0);
		string uri = CreateURL.LoadURL(isHS2 ? "hs2_uploadChara_url.dat" : "ais_uploadChara_url.dat");
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 4);
		wWWForm.AddField("index", idx);
		wWWForm.AddField("add_count", i);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 120;
		yield return request.SendWebRequest();
		if (request.isHttpError || request.isNetworkError)
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(request.error));
			yield break;
		}
		if (request.isDone)
		{
			if (request.downloadHandler.text.StartsWith("ERROR_"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
			if (request.downloadHandler.text.StartsWith("SQLSTATE"))
			{
				base.netInfo.DrawMessage(Color.red, errorMsg);
				observer.OnError(new Exception(request.downloadHandler.text));
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(request.downloadHandler.text))
		{
			byte[] data = null;
			string[] array = request.downloadHandler.text.Split('\t');
			bool num = !("0" == array[0]);
			data = Convert.FromBase64String(array[1]);
			if (num)
			{
				base.netInfo.popupMsg.StartMessage(0.2f, 2f, 0.2f, NetworkDefine.msgDownDecompressingFile[Singleton<GameSystem>.Instance.languageInt], 2);
				FileZip fileZip = new FileZip();
				ObservableYieldInstruction<byte[]> retBytes = Observable.FromCoroutine((IObserver<byte[]> res) => fileZip.FileUnzipCor(res, data)).ToYieldInstruction(throwOnError: false);
				yield return retBytes;
				if (retBytes.HasError)
				{
					string text = NetworkDefine.msgDownFailedDecompressingFile[Singleton<GameSystem>.Instance.languageInt];
					base.netInfo.DrawMessage(Color.red, text);
					observer.OnError(new Exception(text));
					yield break;
				}
				data = retBytes.Result;
			}
			observer.OnNext(data);
			observer.OnCompleted();
		}
		else
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
		}
	}

	public IEnumerator DeleteMyData(IObserver<bool> observer, DataType type)
	{
		if (!isHS2)
		{
			yield break;
		}
		string msgCheck = NetworkDefine.msgDownConfirmDelete[Singleton<GameSystem>.Instance.languageInt];
		Utils.Sound.Play(SystemSE.sel);
		ObservableYieldInstruction<bool> chk = Observable.FromCoroutine((IObserver<bool> res) => base.popupCheck.CheckAnswerCor(res, msgCheck)).ToYieldInstruction(throwOnError: false);
		yield return chk;
		if (!chk.Result)
		{
			observer.OnError(new Exception("削除しない"));
			yield break;
		}
		base.netInfo.BlockUI();
		string errorMsg = NetworkDefine.msgDownFailedDelete[Singleton<GameSystem>.Instance.languageInt];
		int idx = uiCtrl.GetSelectServerInfo(type).idx;
		string userUUID = Singleton<GameSystem>.Instance.UserUUID;
		string userPasswd = Singleton<GameSystem>.Instance.UserPasswd;
		string uri = CreateURL.LoadURL(isHS2 ? "hs2_uploadChara_url.dat" : "ais_uploadChara_url.dat");
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("mode", 5);
		wWWForm.AddField("index", idx);
		wWWForm.AddField("uuid", userUUID);
		wWWForm.AddField("passwd", userPasswd);
		UnityWebRequest request = UnityWebRequest.Post(uri, wWWForm);
		request.timeout = 120;
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
		if (!string.IsNullOrEmpty(request.downloadHandler.text) && "S_OK" == request.downloadHandler.text)
		{
			if (type == DataType.Chara)
			{
				base.netInfo.changeCharaList = true;
			}
			yield return Observable.FromCoroutine((IObserver<bool> res) => UpdateBaseInfo(res)).ToYieldInstruction(throwOnError: false);
			uiCtrl.changeSearchSetting = true;
			observer.OnNext(value: true);
			observer.OnCompleted();
			base.netInfo.UnblockUI();
		}
		else
		{
			base.netInfo.DrawMessage(Color.red, errorMsg);
			observer.OnError(new Exception(errorMsg));
			base.netInfo.UnblockUI();
		}
	}

	public IEnumerator DeleteCache(IObserver<bool> observer, DataType type)
	{
		string msgCheck = NetworkDefine.msgDownConfirmDelete[Singleton<GameSystem>.Instance.languageInt];
		Utils.Sound.Play(SystemSE.sel);
		ObservableYieldInstruction<bool> chk = Observable.FromCoroutine((IObserver<bool> res) => base.popupCheck.CheckAnswerCor(res, msgCheck)).ToYieldInstruction(throwOnError: false);
		yield return chk;
		if (!chk.Result)
		{
			observer.OnError(new Exception("削除しない"));
			yield break;
		}
		base.netInfo.BlockUI();
		base.cacheCtrl.DeleteCache(type, isHS2);
		observer.OnNext(value: true);
		observer.OnCompleted();
		base.netInfo.UnblockUI();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
