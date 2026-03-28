using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIChara;
using FBSAssist;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

public class GlobalMethod
{
	public class FloatBlend
	{
		private bool blend;

		private float min;

		private float max;

		private TimeProgressCtrl tpc = new TimeProgressCtrl();

		public bool Start(float _min, float _max, float _timeBlend = 0.15f)
		{
			tpc.SetProgressTime(_timeBlend);
			tpc.Start();
			min = _min;
			max = _max;
			blend = true;
			return true;
		}

		public bool Proc(ref float _ans)
		{
			if (!blend)
			{
				return false;
			}
			float num = tpc.Calculate();
			_ans = Mathf.Lerp(min, max, num);
			if (num >= 1f)
			{
				blend = false;
			}
			return true;
		}
	}

	private static ExcelData excelData;

	private static List<string> cell = new List<string>();

	private static List<string> row = new List<string>();

	private static List<ExcelData.Param> excelParams = new List<ExcelData.Param>();

	private static List<string> lstABName = new List<string>();

	private static StringBuilder strNo = new StringBuilder();

	public static void setCameraMoveFlag(CameraControl_Ver2 _ctrl, bool _bPlay)
	{
		if (!(_ctrl == null))
		{
			_ctrl.NoCtrlCondition = () => !_bPlay;
		}
	}

	public static bool IsCameraMoveFlag(CameraControl_Ver2 _ctrl)
	{
		if (_ctrl == null)
		{
			return false;
		}
		BaseCameraControl_Ver2.NoCtrlFunc noCtrlCondition = _ctrl.NoCtrlCondition;
		bool flag = true;
		if (noCtrlCondition != null)
		{
			flag = noCtrlCondition();
		}
		return !flag;
	}

	public static bool IsCameraActionFlag(CameraControl_Ver2 _ctrl)
	{
		if (_ctrl == null)
		{
			return false;
		}
		return _ctrl.isControlNow;
	}

	public static void setCameraBase(CameraControl_Ver2 _ctrl, Transform _transTarget)
	{
		if (!(_ctrl == null))
		{
			_ctrl.transBase.localPosition = _transTarget.localPosition;
			_ctrl.transBase.localRotation = _transTarget.localRotation;
			_ctrl.transBase.SetPositionAndRotation(_transTarget.position, _transTarget.rotation);
		}
	}

	public static void setCameraBase(CameraControl_Ver2 _ctrl, Vector3 _pos, Vector3 _rot)
	{
		if (!(_ctrl == null))
		{
			_ctrl.transBase.SetPositionAndRotation(_pos, Quaternion.Euler(_rot));
		}
	}

	public static void CameraKeyCtrl(CameraControl_Ver2 _ctrl, ChaControl[] _Females)
	{
		if (_Females == null || _ctrl == null)
		{
			return;
		}
		if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Transform transform = _Females[0].objBodyBone.transform.FindLoop("cf_J_Head");
				if (!(transform == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform.position);
				}
			}
			else if (Input.GetKeyDown(KeyCode.W))
			{
				Transform transform2 = _Females[0].objBodyBone.transform.FindLoop("cf_J_Mune00");
				if (!(transform2 == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform2.position);
				}
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				Transform transform3 = _Females[0].objBodyBone.transform.FindLoop("cf_J_Kokan");
				if (!(transform3 == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform3.position);
				}
			}
		}
		else
		{
			if (!(_Females[1] != null) || !_Females[1].objBodyBone)
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Transform transform4 = _Females[1].objBodyBone.transform.FindLoop("cf_J_Head");
				if (!(transform4 == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform4.position);
				}
			}
			else if (Input.GetKeyDown(KeyCode.W))
			{
				Transform transform5 = _Females[1].objBodyBone.transform.FindLoop("cf_J_Mune00");
				if (!(transform5 == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform5.position);
				}
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				Transform transform6 = _Females[1].objBodyBone.transform.FindLoop("cf_J_Kokan");
				if (!(transform6 == null))
				{
					_ctrl.TargetPos = _ctrl.transBase.InverseTransformPoint(transform6.position);
				}
			}
		}
	}

	public static void saveCamera(CameraControl_Ver2 _ctrl, string _strAssetPath, string _strfile)
	{
		if (!(_ctrl == null))
		{
			_ctrl.CameraDataSave(_strAssetPath, _strfile);
		}
	}

	public static void loadCamera(CameraControl_Ver2 _ctrl, string _assetbundleFolder, string _strfile, bool _isDirect = false)
	{
		if (!(_ctrl == null))
		{
			_ctrl.CameraDataLoad(_assetbundleFolder, _strfile, _isDirect);
		}
	}

	public static void loadResetCamera(CameraControl_Ver2 _ctrl, string _assetbundleFolder, string _strfile, bool _isDirect = false)
	{
		if (!(_ctrl == null))
		{
			_ctrl.CameraResetDataLoad(_assetbundleFolder, _strfile, _isDirect);
		}
	}

	public static void DebugLog(string _str, int _state = 0)
	{
	}

	public static void SetAllClothState(ChaControl _female, bool _isUpper, int _state, bool _isForce = false)
	{
		if (_female == null)
		{
			return;
		}
		if (_state < 0)
		{
			_state = 0;
		}
		List<int> list = new List<int>();
		if (_isUpper)
		{
			list.Add(0);
			list.Add(2);
		}
		else
		{
			list.Add(1);
			list.Add(3);
			list.Add(5);
		}
		foreach (int item in list)
		{
			if (_female.IsClothesStateKind(item) && (_female.fileStatus.clothesState[item] < _state || _isForce))
			{
				_female.SetClothesState(item, (byte)_state);
			}
		}
	}

	public static int ValLoop(int valNow, int valMax)
	{
		if (valMax <= 1)
		{
			return 0;
		}
		return (valNow % valMax + valMax) % valMax;
	}

	public static int ValLoopEX(int valNow, int valMin, int valMax)
	{
		return ValLoop(valNow - valMin, valMax - valMin) + valMin;
	}

	public static void GetListString(string text, out string[][] data)
	{
		string[] array = text.Split('\n');
		int num = array.Length;
		if (num != 0 && array[num - 1].Trim() == "")
		{
			num--;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			string[] array2 = array[i].Split('\t');
			num2 = Mathf.Max(num2, array2.Length);
		}
		data = new string[num][];
		for (int j = 0; j < num; j++)
		{
			data[j] = new string[num2];
			string[] array3 = array[j].Split('\t');
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k] = array3[k].Replace("\r", "").Replace("\n", "");
				if (k >= num2)
				{
					break;
				}
				data[j][k] = array3[k];
			}
		}
	}

	public static int GetIntTryParse(string _text, int _init = 0)
	{
		int result = 0;
		if (int.TryParse(_text, out result))
		{
			return result;
		}
		return _init;
	}

	public static bool RangeOn<T>(T valNow, T valMin, T valMax) where T : IComparable
	{
		object obj = valMax;
		if (valNow.CompareTo(obj) <= 0)
		{
			object obj2 = valMin;
			return valNow.CompareTo(obj2) >= 0;
		}
		return false;
	}

	public static bool RangeOff<T>(T valNow, T valMin, T valMax) where T : IComparable
	{
		object obj = valMax;
		if (valNow.CompareTo(obj) < 0)
		{
			object obj2 = valMin;
			return valNow.CompareTo(obj2) > 0;
		}
		return false;
	}

	public static string LoadAllListText(string _assetbundleFolder, string _strLoadFile, List<string> _OmitFolderName = null, bool _subdirCheck = false, bool _isDrawLog = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		lstABName.Clear();
		lstABName = GetAssetBundleNameListFromPath(_assetbundleFolder, _subdirCheck);
		lstABName.Sort();
		for (int i = 0; i < lstABName.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(lstABName[i]))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lstABName[i]);
			fileNameWithoutExtension = YS_Assist.GetStringRight(fileNameWithoutExtension, 2);
			if (_OmitFolderName != null && _OmitFolderName.Contains(fileNameWithoutExtension))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstABName[i], _WithExtension: false);
			bool flag = false;
			for (int j = 0; j < allAssetName.Length; j++)
			{
				if (allAssetName[j].Compare(_strLoadFile, ignoreCase: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_isDrawLog)
				{
					DebugLog("[" + lstABName[i] + "][" + _strLoadFile + "]は見つかりません", 1);
				}
			}
			else
			{
				TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(lstABName[i], _strLoadFile);
				AssetBundleManager.UnloadAssetBundle(lstABName[i], isUnloadForceRefCount: true);
				if (!(textAsset == null))
				{
					stringBuilder.Append(textAsset.text);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string LoadAllListText(List<string> _lstAssetBundleNames, string _strLoadFile, bool _isDrawLog = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _lstAssetBundleNames.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(_lstAssetBundleNames[i]))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(_lstAssetBundleNames[i], _WithExtension: false);
			bool flag = false;
			for (int j = 0; j < allAssetName.Length; j++)
			{
				if (allAssetName[j].Compare(_strLoadFile, ignoreCase: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_isDrawLog)
				{
					DebugLog("[" + _lstAssetBundleNames[i] + "][" + _strLoadFile + "]は見つかりません", 1);
				}
			}
			else
			{
				TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(_lstAssetBundleNames[i], _strLoadFile);
				AssetBundleManager.UnloadAssetBundle(_lstAssetBundleNames[i], isUnloadForceRefCount: true);
				if (!(textAsset == null))
				{
					stringBuilder.Append(textAsset.text);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static List<string> LoadAllListTextFromList(string _assetbundleFolder, string _strLoadFile, ref List<string> lst, List<string> _OmitFolderName = null, bool _subdirCheck = false)
	{
		lstABName.Clear();
		lstABName = GetAssetBundleNameListFromPath(_assetbundleFolder, _subdirCheck);
		lstABName.Sort();
		for (int i = 0; i < lstABName.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(lstABName[i]))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lstABName[i]);
			fileNameWithoutExtension = YS_Assist.GetStringRight(fileNameWithoutExtension, 2);
			if ((_OmitFolderName == null || !_OmitFolderName.Contains(fileNameWithoutExtension)) && AssetFileExist(lstABName[i], _strLoadFile))
			{
				TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(lstABName[i], _strLoadFile);
				AssetBundleManager.UnloadAssetBundle(lstABName[i], isUnloadForceRefCount: true);
				if (!(textAsset == null))
				{
					lst.Add(textAsset.text);
				}
			}
		}
		return lst;
	}

	public static List<string> LoadAllListTextFromList(List<string> _assetbundles, string _strLoadFile, ref List<string> lst, List<string> _OmitFolderName = null)
	{
		for (int i = 0; i < _assetbundles.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(_assetbundles[i]))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_assetbundles[i]);
			fileNameWithoutExtension = YS_Assist.GetStringRight(fileNameWithoutExtension, 2);
			if ((_OmitFolderName == null || !_OmitFolderName.Contains(fileNameWithoutExtension)) && AssetFileExist(_assetbundles[i], _strLoadFile))
			{
				TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(_assetbundles[i], _strLoadFile);
				AssetBundleManager.UnloadAssetBundle(_assetbundles[i], isUnloadForceRefCount: true);
				if (!(textAsset == null))
				{
					lst.Add(textAsset.text);
				}
			}
		}
		return lst;
	}

	public static List<ExcelData.Param> LoadExcelData(string _strAssetPath, string _strFileName, int sCell, int sRow, int eCell, int eRow, bool _isWarning = true)
	{
		if (!AssetFileExist(_strAssetPath, _strFileName))
		{
			if (_isWarning)
			{
				DebugLog("excel : [" + _strAssetPath + "][" + _strFileName + "]は見つかりません", 1);
			}
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(_strAssetPath, _strFileName, typeof(ExcelData));
		AssetBundleManager.UnloadAssetBundle(_strAssetPath, isUnloadForceRefCount: true);
		if (assetBundleLoadAssetOperation.IsEmpty())
		{
			if (_isWarning)
			{
				DebugLog("excel : [" + _strFileName + "]は[" + _strAssetPath + "]の中に入っていません", 1);
			}
			return null;
		}
		excelData = assetBundleLoadAssetOperation.GetAsset<ExcelData>();
		cell.Clear();
		foreach (ExcelData.Param item in excelData.list)
		{
			cell.Add(item.list[sCell]);
		}
		row.Clear();
		foreach (string item2 in excelData.list[sRow].list)
		{
			row.Add(item2);
		}
		List<string> list = cell;
		List<string> list2 = row;
		ExcelData.Specify specify = new ExcelData.Specify(eCell, eRow);
		ExcelData.Specify specify2 = new ExcelData.Specify(list.Count, list2.Count);
		excelParams.Clear();
		if ((uint)specify.cell > specify2.cell || (uint)specify.row > specify2.row)
		{
			return null;
		}
		if (specify.cell < excelData.list.Count)
		{
			for (int i = specify.cell; i < excelData.list.Count && i <= specify2.cell; i++)
			{
				ExcelData.Param param = new ExcelData.Param();
				if (specify.row < excelData.list[i].list.Count)
				{
					param.list = new List<string>();
					for (int j = specify.row; j < excelData.list[i].list.Count && j <= specify2.row; j++)
					{
						param.list.Add(excelData.list[i].list[j]);
					}
				}
				excelParams.Add(param);
			}
		}
		return excelParams;
	}

	public static List<ExcelData.Param> LoadExcelDataAlFindlFile(string _strAssetPath, string _strFileName, int sCell, int sRow, int eCell, int eRow, List<string> _OmitFolderName = null, bool _isWarning = true, bool _subdirCheck = false, bool _isDrawLog = false)
	{
		lstABName.Clear();
		lstABName = GetAssetBundleNameListFromPath(_strAssetPath, _subdirCheck);
		lstABName.Sort();
		for (int i = 0; i < lstABName.Count; i++)
		{
			strNo.Clear();
			strNo.Append(Path.GetFileNameWithoutExtension(lstABName[i]));
			strNo.Replace(strNo.ToString(), YS_Assist.GetStringRight(strNo.ToString(), 2));
			if (_OmitFolderName != null && _OmitFolderName.Contains(strNo.ToString()))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstABName[i], _WithExtension: false);
			bool flag = false;
			for (int j = 0; j < allAssetName.Length; j++)
			{
				if (allAssetName[j].Compare(_strFileName, ignoreCase: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_isDrawLog)
				{
					DebugLog("[" + lstABName[i] + "][" + _strFileName + "]は見つかりません", 1);
				}
			}
			else
			{
				List<ExcelData.Param> list = LoadExcelData(lstABName[i], _strFileName, sCell, sRow, eCell, eRow, _isWarning);
				if (list != null)
				{
					return list;
				}
			}
		}
		return null;
	}

	public static T LoadAllFolderInOneFile<T>(string _findFolder, string _strLoadFile, List<string> _OmitFolderName = null, bool _subdirCheck = false) where T : UnityEngine.Object
	{
		lstABName.Clear();
		lstABName = GetAssetBundleNameListFromPath(_findFolder, _subdirCheck);
		lstABName.Sort();
		lstABName.Reverse();
		for (int i = 0; i < lstABName.Count; i++)
		{
			if (GameSystem.IsPathAdd50(lstABName[i]))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lstABName[i]);
				fileNameWithoutExtension = YS_Assist.GetStringRight(fileNameWithoutExtension, 2);
				if ((_OmitFolderName == null || !_OmitFolderName.Contains(fileNameWithoutExtension)) && AssetFileExist(lstABName[i].ToString(), _strLoadFile))
				{
					T result = CommonLib.LoadAsset<T>(lstABName[i], _strLoadFile);
					AssetBundleManager.UnloadAssetBundle(lstABName[i], isUnloadForceRefCount: true);
					return result;
				}
			}
		}
		return null;
	}

	public static List<T> LoadAllFolder<T>(string _findFolder, string _strLoadFile, List<string> _OmitFolderName = null, bool _subdirCheck = false, bool _isDrawLog = false) where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		lstABName.Clear();
		lstABName = GetAssetBundleNameListFromPath(_findFolder, _subdirCheck);
		lstABName.Sort();
		for (int i = 0; i < lstABName.Count; i++)
		{
			if (!GameSystem.IsPathAdd50(lstABName[i]))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lstABName[i]);
			fileNameWithoutExtension = YS_Assist.GetStringRight(fileNameWithoutExtension, 2);
			if (_OmitFolderName != null && _OmitFolderName.Contains(fileNameWithoutExtension))
			{
				continue;
			}
			string[] allAssetName = AssetBundleCheck.GetAllAssetName(lstABName[i], _WithExtension: false);
			bool flag = false;
			for (int j = 0; j < allAssetName.Length; j++)
			{
				if (allAssetName[j].Compare(_strLoadFile, ignoreCase: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_isDrawLog)
				{
					DebugLog("[" + lstABName[i] + "]に[" + _strLoadFile + "]は見つかりません", 1);
				}
			}
			else
			{
				T val = CommonLib.LoadAsset<T>(lstABName[i], _strLoadFile);
				AssetBundleManager.UnloadAssetBundle(lstABName[i], isUnloadForceRefCount: true);
				if ((bool)val)
				{
					list.Add(val);
				}
			}
		}
		return list;
	}

	public static bool CheckFlagsArray(bool[] flags, int _check = 0)
	{
		if (flags.Length == 0)
		{
			return false;
		}
		bool flag = _check == 0;
		foreach (bool flag2 in flags)
		{
			if ((_check == 0) ? (!flag2) : flag2)
			{
				return !flag;
			}
		}
		return flag;
	}

	public static List<string> GetAssetBundleNameListFromPath(string path, bool subdirCheck = false)
	{
		List<string> result = new List<string>();
		if (!AssetBundleCheck.IsSimulation)
		{
			string path2 = AssetBundleManager.BaseDownloadingURL + path;
			if (subdirCheck)
			{
				List<string> list = new List<string>();
				CommonLib.GetAllFiles(path2, "*.unity3d", list);
				result = list.Select((string s) => s.Replace(AssetBundleManager.BaseDownloadingURL, "")).ToList();
			}
			else
			{
				if (!Directory.Exists(path2))
				{
					return result;
				}
				result = (from s in Directory.GetFiles(path2, "*.unity3d")
					select s.Replace(AssetBundleManager.BaseDownloadingURL, "")).ToList();
			}
		}
		return result;
	}

	public static string[] GetAllAssetName(string assetBundleName, bool _WithExtension = true, string manifestAssetBundleName = null, bool isAllCheck = false, bool _forceAssetBundleCheck = false)
	{
		if (manifestAssetBundleName == null && isAllCheck && AssetBundleManager.AllLoadedAssetBundleNames.Contains(assetBundleName))
		{
			foreach (KeyValuePair<string, AssetBundleManager.BundlePack> item in AssetBundleManager.ManifestBundlePack)
			{
				if (!item.Value.LoadedAssetBundles.TryGetValue(assetBundleName, out var value))
				{
					continue;
				}
				if (_WithExtension)
				{
					return (from file in value.Bundle.GetAllAssetNames()
						select Path.GetFileName(file)).ToArray();
				}
				return (from file in value.Bundle.GetAllAssetNames()
					select Path.GetFileNameWithoutExtension(file)).ToArray();
			}
		}
		LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, manifestAssetBundleName);
		AssetBundle assetBundle = ((loadedAssetBundle == null) ? AssetBundle.LoadFromFile(AssetBundleManager.BaseDownloadingURL + assetBundleName) : loadedAssetBundle.Bundle);
		string[] array = null;
		array = ((!_WithExtension) ? (from file in assetBundle.GetAllAssetNames()
			select Path.GetFileNameWithoutExtension(file)).ToArray() : (from file in assetBundle.GetAllAssetNames()
			select Path.GetFileName(file)).ToArray());
		if (loadedAssetBundle == null)
		{
			assetBundle.Unload(unloadAllLoadedObjects: true);
		}
		return array;
	}

	public static bool StartsWith(string a, string b)
	{
		int length = a.Length;
		int length2 = b.Length;
		int num = 0;
		int num2 = 0;
		while (num < length && num2 < length2 && a[num] == b[num2])
		{
			num++;
			num2++;
		}
		if (num2 != length2 || length < length2)
		{
			if (num == length)
			{
				return length2 >= length;
			}
			return false;
		}
		return true;
	}

	public static bool AssetFileExist(string path, string targetName, string manifest = "")
	{
		bool result = false;
		if (path.IsNullOrEmpty())
		{
			return result;
		}
		if (AssetBundleCheck.IsSimulation)
		{
			return AssetBundleCheck.IsFile(path, targetName);
		}
		string[] allAssetName = AssetBundleCheck.GetAllAssetName(path, _WithExtension: false, manifest);
		for (int i = 0; i < allAssetName.Length; i++)
		{
			if (allAssetName[i].Compare(targetName, ignoreCase: true))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static float StringToFloat(string _text)
	{
		float result = 0f;
		if (!float.TryParse(_text, out result))
		{
			return 0f;
		}
		return result;
	}
}
