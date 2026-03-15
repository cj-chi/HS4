using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Config;
using IllusionUtility.GetUtility;
using Manager;
using UnityEngine;

public class CameraControl_Ver2 : BaseCameraControl_Ver2
{
	public class VisibleObject
	{
		public string nameCollider;

		public float delay;

		public bool isVisible = true;

		public List<GameObject> listObj = new List<GameObject>();
	}

	private bool isConfigVanish = true;

	private Renderer targetRender;

	private List<VisibleObject> lstMapVanish = new List<VisibleObject>();

	private List<Collider> listCollider = new List<Collider>();

	public bool isOutsideTargetTex { get; set; }

	public bool isCursorLock { get; set; }

	public bool isConfigTargetTex { get; set; }

	public bool ConfigVanish
	{
		get
		{
			return isConfigVanish;
		}
		set
		{
			if (isConfigVanish != value)
			{
				isConfigVanish = value;
				visibleForceVanish(_visible: true);
			}
		}
	}

	public Transform targetTex { get; private set; }

	protected override IEnumerator Start()
	{
		base.enabled = false;
		yield return base.Start();
		targetTex = base.transform.Find("CameraTarget");
		if ((bool)targetTex)
		{
			targetTex.localScale = Vector3.one * 0.1f;
			targetRender = targetTex.GetComponent<Renderer>();
		}
		isOutsideTargetTex = true;
		isConfigTargetTex = true;
		isConfigVanish = true;
		isCursorLock = true;
		viewCollider = base.gameObject.AddComponent<CapsuleCollider>();
		viewCollider.radius = 0.05f;
		viewCollider.isTrigger = true;
		viewCollider.direction = 2;
		Rigidbody orAddComponent = base.gameObject.GetOrAddComponent<Rigidbody>();
		orAddComponent.useGravity = false;
		orAddComponent.isKinematic = true;
		isInit = true;
		listCollider.Clear();
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		base.enabled = true;
	}

	protected new void LateUpdate()
	{
		if (Scene.Overlaps.Any((Scene.IOverlap x) => x is ConfigWindow))
		{
			return;
		}
		if (!Scene.IsNowLoading && !Scene.IsNowLoadingFade)
		{
			base.LateUpdate();
		}
		if ((bool)targetTex)
		{
			if (transBase != null)
			{
				targetTex.position = transBase.TransformPoint(CamDat.Pos);
			}
			else
			{
				targetTex.position = CamDat.Pos;
			}
			Vector3 position = base.transform.position;
			position.y = targetTex.position.y;
			targetTex.transform.LookAt(position);
			targetTex.Rotate(new Vector3(90f, 0f, 0f));
			if (null != targetRender)
			{
				targetRender.enabled = base.isControlNow & isOutsideTargetTex & isConfigTargetTex;
			}
			if (Singleton<GameCursor>.IsInstance() && isCursorLock)
			{
				Singleton<GameCursor>.Instance.SetCursorLock(base.isControlNow & isOutsideTargetTex);
			}
		}
		VanishProc();
	}

	private void OnDisable()
	{
		visibleForceVanish(_visible: true);
	}

	public void ClearListCollider()
	{
		listCollider.Clear();
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (listCollider.Find((Collider x) => x != null && other.name == x.name) == null)
		{
			listCollider.Add(other);
		}
	}

	protected void OnTriggerStay(Collider other)
	{
		if (listCollider.Find((Collider x) => x != null && other.name == x.name) == null)
		{
			listCollider.Add(other);
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		listCollider.Clear();
	}

	public void autoCamera(float _fSpeed)
	{
		CamDat.Rot.y = (CamDat.Rot.y + _fSpeed * Time.deltaTime) % 360f;
	}

	public void CameraDataSave(string _strCreateAssetPath, string _strFile)
	{
		using StreamWriter streamWriter = new StreamWriter(new FileData().Create(_strCreateAssetPath) + _strFile + ".txt", append: false, Encoding.GetEncoding("UTF-8"));
		streamWriter.Write(CamDat.Pos.x);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Pos.y);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Pos.z);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Dir.x);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Dir.y);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Dir.z);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Rot.x);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Rot.y);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Rot.z);
		streamWriter.Write('\n');
		streamWriter.Write(CamDat.Fov);
		streamWriter.Write('\n');
	}

	public bool CameraDataLoad(string _assetbundleFolder, string _strFile, bool _isDirect = false)
	{
		string text = "";
		if (!_isDirect)
		{
			text = GlobalMethod.LoadAllListText(_assetbundleFolder, _strFile);
		}
		else
		{
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(_assetbundleFolder, _strFile);
			AssetBundleManager.UnloadAssetBundle(_assetbundleFolder, isUnloadForceRefCount: true);
			if ((bool)textAsset)
			{
				text = textAsset.text;
			}
		}
		if (text == "")
		{
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		CamDat.Pos.x = float.Parse(data[0][0]);
		CamDat.Pos.y = float.Parse(data[1][0]);
		CamDat.Pos.z = float.Parse(data[2][0]);
		CamDat.Dir.x = float.Parse(data[3][0]);
		CamDat.Dir.y = float.Parse(data[4][0]);
		CamDat.Dir.z = float.Parse(data[5][0]);
		CamDat.Rot.x = float.Parse(data[6][0]);
		CamDat.Rot.y = float.Parse(data[7][0]);
		CamDat.Rot.z = float.Parse(data[8][0]);
		CamDat.Fov = float.Parse(data[9][0]);
		if (base.thisCamera != null)
		{
			base.thisCamera.fieldOfView = CamDat.Fov;
		}
		CamReset.Copy(CamDat, Quaternion.identity);
		CameraUpdate();
		if (!isInit)
		{
			isInit = true;
		}
		return true;
	}

	public bool CameraResetDataLoad(string _assetbundleFolder, string _strFile, bool _isDirect = false)
	{
		string text = "";
		if (!_isDirect)
		{
			text = GlobalMethod.LoadAllListText(_assetbundleFolder, _strFile);
		}
		else
		{
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(_assetbundleFolder, _strFile);
			AssetBundleManager.UnloadAssetBundle(_assetbundleFolder, isUnloadForceRefCount: true);
			if ((bool)textAsset)
			{
				text = textAsset.text;
			}
		}
		if (text == "")
		{
			GlobalMethod.DebugLog("cameraファイル読み込めません", 1);
			return false;
		}
		GlobalMethod.GetListString(text, out var data);
		CameraData copy = new CameraData
		{
			Pos = 
			{
				x = float.Parse(data[0][0]),
				y = float.Parse(data[1][0]),
				z = float.Parse(data[2][0])
			},
			Dir = 
			{
				x = float.Parse(data[3][0]),
				y = float.Parse(data[4][0]),
				z = float.Parse(data[5][0])
			},
			Rot = 
			{
				x = float.Parse(data[6][0]),
				y = float.Parse(data[7][0]),
				z = float.Parse(data[8][0])
			},
			Fov = float.Parse(data[9][0])
		};
		CamReset.Copy(copy, Quaternion.identity);
		return true;
	}

	public bool loadVanish(string _assetbundleFolder, string _strMap, GameObject _objMap)
	{
		lstMapVanish.Clear();
		if (_objMap == null)
		{
			return false;
		}
		GlobalMethod.GetListString(GlobalMethod.LoadAllListText(_assetbundleFolder, _strMap), out var data);
		int length = data.GetLength(0);
		int length2 = data.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			VisibleObject visibleObject = new VisibleObject();
			visibleObject.nameCollider = data[i][0];
			for (int j = 1; j < length2; j++)
			{
				string text = data[i][j];
				if (text == "")
				{
					break;
				}
				Transform transform = _objMap.transform.FindLoop(text);
				if (!(transform == null))
				{
					visibleObject.listObj.Add(transform.gameObject);
				}
			}
			lstMapVanish.Add(visibleObject);
		}
		return true;
	}

	public bool loadVanishExcelData(string _assetbundleFolder, int _mapID, GameObject _objMap)
	{
		lstMapVanish.Clear();
		listCollider.Clear();
		if (_objMap == null)
		{
			return false;
		}
		List<ExcelData> list = new List<ExcelData>();
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(_assetbundleFolder);
		assetBundleNameListFromPath.Sort();
		ExcelData excelData = null;
		for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
		{
			if (GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
			{
				excelData = CommonLib.LoadAsset<ExcelData>(assetBundleNameListFromPath[i], "map_col_name");
				if (!(excelData == null))
				{
					list.Add(excelData);
				}
			}
		}
		string text = null;
		for (int j = 0; j < list.Count; j++)
		{
			int num = 0;
			while (num < list[j].MaxCell)
			{
				List<string> list2 = list[j].list[num++].list;
				if (int.TryParse(list2[1], out var result) && result == _mapID)
				{
					text = list2[0];
				}
			}
		}
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		for (int k = 0; k < assetBundleNameListFromPath.Count; k++)
		{
			if (!GlobalMethod.AssetFileExist(assetBundleNameListFromPath[k], text))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(assetBundleNameListFromPath[k], text);
			if (excelData == null)
			{
				continue;
			}
			int num2 = 2;
			while (num2 < excelData.MaxCell)
			{
				List<string> list3 = excelData.list[num2++].list;
				VisibleObject visibleObject = new VisibleObject();
				visibleObject.nameCollider = list3[0];
				for (int l = 1; l < list3.Count; l++)
				{
					string text2 = list3[l];
					if (text2 == "")
					{
						break;
					}
					Transform transform = _objMap.transform.FindLoop(text2);
					if (!(transform == null))
					{
						visibleObject.listObj.Add(transform.gameObject);
					}
				}
				lstMapVanish.Add(visibleObject);
			}
		}
		return true;
	}

	public void CameraDataSaveBinary(BinaryWriter bw)
	{
		bw.Write(CamDat.Pos.x);
		bw.Write(CamDat.Pos.y);
		bw.Write(CamDat.Pos.z);
		bw.Write(CamDat.Dir.x);
		bw.Write(CamDat.Dir.y);
		bw.Write(CamDat.Dir.z);
		bw.Write(CamDat.Rot.x);
		bw.Write(CamDat.Rot.y);
		bw.Write(CamDat.Rot.z);
		bw.Write(CamDat.Fov);
	}

	public bool CameraDataLoadBinary(BinaryReader br, bool isUpdate)
	{
		CameraData cameraData = new CameraData
		{
			Pos = 
			{
				x = br.ReadSingle(),
				y = br.ReadSingle(),
				z = br.ReadSingle()
			},
			Dir = 
			{
				x = br.ReadSingle(),
				y = br.ReadSingle(),
				z = br.ReadSingle()
			},
			Rot = 
			{
				x = br.ReadSingle(),
				y = br.ReadSingle(),
				z = br.ReadSingle()
			},
			Fov = br.ReadSingle()
		};
		CamReset.Copy(cameraData, Quaternion.identity);
		if (isUpdate)
		{
			CamDat.Copy(cameraData);
			if (base.thisCamera != null)
			{
				base.thisCamera.fieldOfView = cameraData.Fov;
			}
			CameraUpdate();
			if (!isInit)
			{
				isInit = true;
			}
		}
		return true;
	}

	public void visibleForceVanish(bool _visible)
	{
		foreach (VisibleObject item in lstMapVanish)
		{
			foreach (GameObject item2 in item.listObj)
			{
				if ((bool)item2)
				{
					item2.SetActive(_visible);
				}
			}
			item.isVisible = _visible;
			item.delay = (_visible ? 0.3f : 0f);
		}
	}

	private void visibleFroceVanish(VisibleObject _obj, bool _visible)
	{
		if (_obj == null || _obj.listObj == null)
		{
			return;
		}
		for (int i = 0; i < _obj.listObj.Count; i++)
		{
			if (!(_obj.listObj[i] == null))
			{
				int num = 0;
				if (Singleton<HSceneFlagCtrl>.IsInstance() && Singleton<HSceneFlagCtrl>.Instance.nowHPoint != null)
				{
					num = Singleton<HSceneFlagCtrl>.Instance.nowHPoint.CheckVisible(_obj.listObj[i]);
				}
				switch (num)
				{
				case 0:
					_obj.listObj[i].SetActive(_visible);
					break;
				case 1:
					_obj.listObj[i].SetActive(_visible);
					break;
				case 2:
					_obj.listObj[i].SetActive(value: false);
					break;
				}
			}
		}
		_obj.delay = (_visible ? 0.3f : 0f);
		_obj.isVisible = _visible;
	}

	private bool VanishProc()
	{
		if (!isConfigVanish)
		{
			return false;
		}
		int i = 0;
		while (i < lstMapVanish.Count)
		{
			if (listCollider.Find((Collider x) => x != null && lstMapVanish[i].nameCollider == x.name) == null)
			{
				VanishDelayVisible(lstMapVanish[i]);
			}
			else if (lstMapVanish[i].isVisible)
			{
				visibleFroceVanish(lstMapVanish[i], _visible: false);
			}
			int num = i + 1;
			i = num;
		}
		return true;
	}

	private bool VanishDelayVisible(VisibleObject _visible)
	{
		if (_visible.isVisible)
		{
			return false;
		}
		_visible.delay += Time.deltaTime;
		if (_visible.delay >= 0.3f)
		{
			visibleFroceVanish(_visible, _visible: true);
		}
		return true;
	}
}
