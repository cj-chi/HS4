using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AIChara;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UploaderSystem;

namespace ModChecker;

public class ModChecker : MonoBehaviour
{
	[Serializable]
	public class TypeInfoText
	{
		public Text[] textChara;
	}

	public class CheckDataInfo
	{
		public string dataID = "";

		public string userID = "";

		public string filename = "";

		public List<string> lstCheck = new List<string>();
	}

	[SerializeField]
	private CheckLog log;

	[SerializeField]
	private Processing processing;

	[SerializeField]
	private TypeInfoText typeInfoText;

	[SerializeField]
	private Button btnCheck;

	[SerializeField]
	private Button btnOutput;

	[SerializeField]
	private Button btnCancel;

	[SerializeField]
	private CanvasGroup cvsgMenu;

	private Dictionary<int, NetworkInfo.UserInfo> dictUserInfo = new Dictionary<int, NetworkInfo.UserInfo>();

	private FolderAssist faFile = new FolderAssist();

	private int vSyncCountBack;

	private bool cancel;

	private readonly string[] key = new string[1] { "*.png" };

	private readonly string checkDirName = "check/chara";

	private readonly string checkModDirName = "check/mod";

	private List<CheckDataInfo> lstModInfo = new List<CheckDataInfo>();

	private string EncodeFromBase64(string buf)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(buf));
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		vSyncCountBack = QualitySettings.vSyncCount;
		QualitySettings.vSyncCount = 0;
		log.StartLog();
		processing.update = false;
		if ((bool)btnCheck)
		{
			btnCheck.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				cvsgMenu.Enable(enable: false);
				log.AddLog("Modチェックを開始します");
				Observable.FromCoroutine((IObserver<bool> res) => CheckMod(res)).Subscribe(delegate
				{
					log.AddLog("Modチェックが完了しました");
				}, delegate
				{
					cvsgMenu.Enable(enable: true);
				}, delegate
				{
					cvsgMenu.Enable(enable: true);
				});
			});
		}
		if ((bool)btnOutput)
		{
			btnOutput.OnClickAsObservable().Subscribe(delegate
			{
				EventSystem.current.SetSelectedGameObject(null);
				cvsgMenu.Enable(enable: false);
				log.AddLog("Mod情報の作成を開始します");
				Observable.FromCoroutine((IObserver<bool> res) => OutputModInfo(res)).Subscribe(delegate
				{
					log.AddLog("Mod情報の作成が完了しました");
				}, delegate
				{
					cvsgMenu.Enable(enable: true);
				}, delegate
				{
					cvsgMenu.Enable(enable: true);
				});
			});
		}
		if ((bool)btnCancel)
		{
			btnCancel.OnClickAsObservable().Subscribe(delegate
			{
				cancel = true;
			});
		}
		log.AddLog("チェックを開始出来ます。");
	}

	private IEnumerator GetFiles(IObserver<bool> observer)
	{
		string folder = UserData.Create(checkDirName);
		UserData.Create(checkModDirName);
		faFile.CreateFolderInfoEx(folder, key);
		faFile.SortName();
		observer.OnNext(value: true);
		observer.OnCompleted();
		yield break;
	}

	private IEnumerator CheckMod(IObserver<bool> observer)
	{
		processing.update = true;
		log.AddLog("キャラ情報を取得しています。");
		yield return Observable.FromCoroutine((IObserver<bool> res) => GetFiles(res)).ToYieldInstruction(throwOnError: false);
		log.AddLog("キャラファイルをチェック中");
		yield return Observable.FromCoroutine(CheckChara).ToYieldInstruction(throwOnError: false);
		processing.update = false;
		observer.OnNext(value: true);
		observer.OnCompleted();
	}

	private IEnumerator CheckChara()
	{
		string modDir = UserData.Create(checkModDirName);
		int filefig = faFile.GetFileCount();
		log.AddLog("\u3000\u3000総ファイル数：{0}", filefig);
		int idxFemale = log.AddLog("\u3000\u3000女ファイル数：0");
		int idxMale = log.AddLog("\u3000\u3000男ファイル数：0");
		int idxUnknown = log.AddLog("\u3000不明ファイル数：0");
		int idxMod = log.AddLog("ＭＯＤファイル数：0");
		int femaleFig = 0;
		int maleFig = 0;
		int unknownFig = 0;
		int modFig = 0;
		Stopwatch sw = new Stopwatch();
		sw.Start();
		int i = 0;
		while (i < filefig)
		{
			if (cancel)
			{
				log.AddLog("処理を中断しました。");
				cancel = false;
				break;
			}
			ChaFileControl chaFileControl = new ChaFileControl();
			chaFileControl.skipRangeCheck = true;
			bool num = chaFileControl.LoadCharaFile(faFile.lstFile[i].FullPath);
			chaFileControl.skipRangeCheck = false;
			if (num)
			{
				if (chaFileControl.parameter.sex == 0)
				{
					maleFig++;
				}
				else
				{
					femaleFig++;
				}
				List<string> list = new List<string>();
				if (ChaFileControl.CheckDataRange(chaFileControl, chk_custom: true, chk_clothes: true, chk_parameter: true, list))
				{
					string fileName = Path.GetFileName(faFile.lstFile[i].FullPath);
					string text = modDir + fileName;
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.Move(faFile.lstFile[i].FullPath, text);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(faFile.lstFile[i].FileName);
					lstModInfo.Add(new CheckDataInfo
					{
						dataID = chaFileControl.dataID,
						userID = chaFileControl.userID,
						filename = fileNameWithoutExtension,
						lstCheck = list
					});
					modFig++;
				}
			}
			else
			{
				unknownFig++;
			}
			log.UpdateLog(idxFemale, "\u3000\u3000女ファイル数：{0}", femaleFig);
			log.UpdateLog(idxMale, "\u3000\u3000男ファイル数：{0}", maleFig);
			log.UpdateLog(idxUnknown, "\u3000不明ファイル数：{0}", unknownFig);
			log.UpdateLog(idxMod, "ＭＯＤファイル数：{0}", modFig);
			yield return null;
			int num2 = i + 1;
			i = num2;
		}
		typeInfoText.textChara[0].text = femaleFig.ToString();
		typeInfoText.textChara[1].text = maleFig.ToString();
		typeInfoText.textChara[2].text = modFig.ToString();
		typeInfoText.textChara[3].text = unknownFig.ToString();
		sw.Stop();
		UnityEngine.Debug.Log("処理時間" + sw.Elapsed);
	}

	private IEnumerator OutputModInfo(IObserver<bool> observer)
	{
		log.AddLog("キャラMod情報を作成中");
		yield return Observable.FromCoroutine((CancellationToken _) => CreateModInfo()).ToYieldInstruction(throwOnError: false);
		observer.OnNext(value: true);
		observer.OnCompleted();
	}

	private IEnumerator CreateModInfo()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int count = lstModInfo.Count;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ユーザーID, データID, ファイル名, Mod情報\n");
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append(lstModInfo[i].userID).Append("\t");
			stringBuilder.Append(lstModInfo[i].dataID).Append("\t");
			stringBuilder.Append(lstModInfo[i].filename).Append("\t");
			if (lstModInfo[i].lstCheck != null)
			{
				int count2 = lstModInfo[i].lstCheck.Count;
				for (int j = 0; j < count2; j++)
				{
					stringBuilder.Append(lstModInfo[i].lstCheck[j]).Append("\t");
				}
			}
			stringBuilder.Append("\n");
		}
		File.WriteAllText(UserData.Create(checkModDirName) + "modinfo.txt", stringBuilder.ToString());
		stopwatch.Stop();
		UnityEngine.Debug.Log("処理時間" + stopwatch.Elapsed);
		yield break;
	}

	private void OnDestroy()
	{
		log.EndLog();
		QualitySettings.vSyncCount = vSyncCountBack;
	}
}
