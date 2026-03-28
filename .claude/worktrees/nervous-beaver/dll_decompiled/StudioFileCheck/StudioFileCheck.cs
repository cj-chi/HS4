using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIChara;
using Illusion.Extensions;
using Studio;
using UniRx;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.UI;

namespace StudioFileCheck;

public class StudioFileCheck : MonoBehaviour
{
	private class NodePool : ObjectPool<Node>
	{
		private readonly Node studioNode;

		private readonly Transform parentTransform;

		public NodePool(Node _prefab, Transform _parentTransform)
		{
			studioNode = _prefab;
			parentTransform = _parentTransform;
		}

		protected override Node CreateInstance()
		{
			return UnityEngine.Object.Instantiate(studioNode, parentTransform);
		}
	}

	[Serializable]
	private class ScrollViewInfo
	{
		public GameObject objectNode;

		public Transform transformRoot;

		public ScrollRect scrollRect;

		private NodePool nodePool;

		private List<Node> rentList = new List<Node>();

		public bool IsInit { get; private set; }

		public void Init(int _size = 0)
		{
			nodePool = new NodePool(objectNode.GetComponent<Node>(), transformRoot);
			if (_size > 0)
			{
				nodePool.PreloadAsync(_size, Mathf.Min(_size, 100)).Subscribe(delegate
				{
					IsInit = true;
				});
			}
			else
			{
				IsInit = true;
			}
		}

		public Node Rent()
		{
			Node node = nodePool.Rent();
			rentList.Add(node);
			node.transform.SetAsLastSibling();
			node.Select = false;
			node.RemoveAllListeners();
			return node;
		}

		public Node Rent(string _text, Color _color, bool _button = false)
		{
			Node node = Rent();
			node.Text = _text;
			node.TextColor = _color;
			node.Button.enabled = _button;
			return node;
		}

		public void Return()
		{
			foreach (Node rent in rentList)
			{
				nodePool.Return(rent);
			}
			rentList.Clear();
		}
	}

	private class ModCharaCheck
	{
		private string name = "";

		public bool isMod { get; private set; }

		public List<string> lstResult { get; private set; }

		public string text
		{
			get
			{
				if (lstResult.IsNullOrEmpty())
				{
					return "";
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(name);
				foreach (string item in lstResult)
				{
					stringBuilder.AppendLine(item);
				}
				return stringBuilder.ToString();
			}
		}

		public ModCharaCheck(OICharInfo oIChar)
		{
			List<string> checkInfo = new List<string>();
			isMod = ChaFileControl.CheckDataRange(oIChar.charFile, chk_custom: true, chk_clothes: true, chk_parameter: true, checkInfo);
			lstResult = checkInfo;
			name = oIChar.charFile.parameter.fullname;
		}
	}

	private class ModCheckInfo
	{
		public string file = "";

		public Version version;

		public bool isLoad;

		public SceneInfo info;

		public List<ModCharaCheck> lstChar;

		public bool isMod
		{
			get
			{
				if (!lstChar.IsNullOrEmpty())
				{
					return lstChar.Any((ModCharaCheck v) => v.isMod);
				}
				return false;
			}
		}
	}

	private const string fileDir = "CheckStudio";

	[SerializeField]
	private Button buttonModCheck;

	[SerializeField]
	private ScrollViewInfo fileList = new ScrollViewInfo();

	[SerializeField]
	private Text textFile;

	[SerializeField]
	private ScrollViewInfo fileInfo = new ScrollViewInfo();

	[Header("作業中関係")]
	[SerializeField]
	private CanvasGroup canvasGroupOver;

	private List<ModCheckInfo> listModCheck;

	private string fileName = "";

	private float m_NextFrameTime;

	private float nextFrameTime
	{
		set
		{
			m_NextFrameTime = Time.realtimeSinceStartup + value;
		}
	}

	private bool isOver => Time.realtimeSinceStartup >= m_NextFrameTime;

	private void MoveFile(string _directory, string _file)
	{
		string sourceFileName = UserData.Create("CheckStudio") + _file;
		string destFileName = UserData.Create("CheckStudio/" + _directory) + _file;
		try
		{
			File.Move(sourceFileName, destFileName);
		}
		catch (Exception)
		{
		}
	}

	private void OnSelect(int _idx)
	{
		ModCheckInfo modCheckInfo = listModCheck[_idx];
		fileName = modCheckInfo.file;
		textFile.text = fileName;
		DeleteInfoNode();
		if (!modCheckInfo.isLoad)
		{
			fileInfo.Rent("読み込めない", Color.red);
			return;
		}
		if (modCheckInfo.isMod)
		{
			fileInfo.Rent("Mod情報", Color.red);
			foreach (ModCharaCheck item in modCheckInfo.lstChar.Where((ModCharaCheck v) => v.isMod))
			{
				fileInfo.Rent(item.text, Color.white);
			}
		}
		if (modCheckInfo.info.dicObject.Count != 0)
		{
			fileInfo.Rent("シーン情報", Color.blue);
			List<string> list = new List<string>();
			GetInfoLoop(modCheckInfo.info.dicObject.Values.ToList(), list, 0);
			foreach (string item2 in list)
			{
				fileInfo.Rent(item2, Color.white);
			}
		}
		else
		{
			fileInfo.Rent("キャラ、アイテム等が存在しない", Color.red);
		}
		fileInfo.scrollRect.normalizedPosition = Vector2.one;
	}

	private void DeleteInfoNode()
	{
		fileInfo.Return();
	}

	private void GetInfoLoop(List<ObjectInfo> _infoList, List<string> _list, int _indent)
	{
		string text = "";
		for (int i = 0; i < _indent; i++)
		{
			text += "\t";
		}
		foreach (ObjectInfo _info in _infoList)
		{
			switch (_info.kind)
			{
			case 0:
			{
				OICharInfo oICharInfo = _info as OICharInfo;
				_list.Add(string.Format("{1}Ch : {0}", oICharInfo.charFile.parameter.fullname, text));
				foreach (KeyValuePair<int, List<ObjectInfo>> item in oICharInfo.child)
				{
					GetInfoLoop(item.Value, _list, _indent + 1);
				}
				break;
			}
			case 1:
			{
				OIItemInfo oIItemInfo = _info as OIItemInfo;
				if (Singleton<Info>.IsInstance())
				{
					Info.ItemLoadInfo value2 = null;
					Dictionary<int, Dictionary<int, Info.ItemLoadInfo>> value3 = null;
					if (Singleton<Info>.Instance.dicItemLoadInfo.TryGetValue(oIItemInfo.group, out value3))
					{
						Dictionary<int, Info.ItemLoadInfo> value4 = null;
						if (value3.TryGetValue(oIItemInfo.category, out value4))
						{
							value4.TryGetValue(oIItemInfo.no, out value2);
						}
					}
					_list.Add(string.Format("{1}I : {0}", (value2 != null) ? value2.name : "存在しない", text));
				}
				else
				{
					_list.Add(string.Format("{1}I : {0}", oIItemInfo.no, text));
				}
				GetInfoLoop(oIItemInfo.child, _list, _indent + 1);
				break;
			}
			case 2:
			{
				OILightInfo oILightInfo = _info as OILightInfo;
				if (Singleton<Info>.IsInstance())
				{
					Info.LightLoadInfo value = null;
					_list.Add(string.Format("{1}L : {0}", Singleton<Info>.Instance.dicLightLoadInfo.TryGetValue(oILightInfo.no, out value) ? value.name : "存在しない", text));
				}
				else
				{
					_list.Add(string.Format("{1}L : {0}", oILightInfo.no, text));
				}
				break;
			}
			case 3:
			{
				OIFolderInfo oIFolderInfo = _info as OIFolderInfo;
				_list.Add(string.Format("{1}F : {0}", oIFolderInfo.name, text));
				GetInfoLoop(oIFolderInfo.child, _list, _indent + 1);
				break;
			}
			case 4:
			{
				OIRouteInfo oIRouteInfo = _info as OIRouteInfo;
				_list.Add(string.Format("{1}R : {0}", oIRouteInfo.name, text));
				GetInfoLoop(oIRouteInfo.child, _list, _indent + 1);
				break;
			}
			case 5:
			{
				OICameraInfo oICameraInfo = _info as OICameraInfo;
				_list.Add(string.Format("{1}Ca : {0}", oICameraInfo.name, text));
				break;
			}
			}
		}
	}

	private IEnumerator ModCheckCoroutine()
	{
		nextFrameTime = 0.01f;
		canvasGroupOver.Enable(enable: true);
		fileList.Return();
		if (isOver)
		{
			yield return null;
			nextFrameTime = 0.01f;
		}
		DeleteInfoNode();
		if (isOver)
		{
			yield return null;
			nextFrameTime = 0.01f;
		}
		string[] files = Directory.GetFiles(UserData.Create("CheckStudio"), "*.png");
		if (isOver)
		{
			yield return null;
			nextFrameTime = 0.01f;
		}
		listModCheck = new List<ModCheckInfo>();
		string[] array = files;
		foreach (string s in array)
		{
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			ModCheckInfo info = new ModCheckInfo();
			info.file = Path.GetFileName(s);
			info.info = new SceneInfo();
			info.isLoad = info.info.Load(s, out info.version);
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			if (!info.isLoad)
			{
				listModCheck.Add(info);
				continue;
			}
			if (info.info.dicObject.Count() == 0)
			{
				listModCheck.Add(info);
				continue;
			}
			List<OICharInfo> targets = info.info.dicObject.Values.OfType<OICharInfo>().ToList();
			if (targets.IsNullOrEmpty())
			{
				listModCheck.Add(info);
				continue;
			}
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			info.lstChar = targets.Select((OICharInfo oIChar) => new ModCharaCheck(oIChar)).ToList();
			listModCheck.Add(info);
		}
		foreach (ModCheckInfo info in listModCheck.Where((ModCheckInfo modCheckInfo) => modCheckInfo.isMod))
		{
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			MoveFile("MOD", info.file);
		}
		foreach (ModCheckInfo info in listModCheck.Where((ModCheckInfo modCheckInfo) => !modCheckInfo.isLoad))
		{
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			MoveFile("Unknown", info.file);
		}
		foreach (var v in listModCheck.Select((ModCheckInfo v2, int i2) => new
		{
			v = v2,
			i = i2
		}))
		{
			if (isOver)
			{
				yield return null;
				nextFrameTime = 0.01f;
			}
			Node node = fileList.Rent(v.v.file, v.v.isLoad ? (v.v.isMod ? Color.red : Color.white) : Color.yellow, _button: true);
			int idx = v.i;
			node.Button.OnClickAsObservable().Subscribe(delegate
			{
				node.Select = true;
				OnSelect(idx);
			});
		}
		canvasGroupOver.Enable(enable: false);
	}

	private IEnumerator Start()
	{
		canvasGroupOver.Enable(enable: true);
		yield return new WaitWhile(() => !AssetBundleManager.initialized);
		if (!Singleton<Info>.IsInstance())
		{
			yield return new WaitWhile(() => !Singleton<Info>.IsInstance());
		}
		if (!Singleton<Info>.Instance.isLoadList)
		{
			yield return Singleton<Info>.Instance.LoadExcelDataCoroutine();
		}
		fileList.Init(500);
		fileInfo.Init(5000);
		buttonModCheck.OnClickAsObservable().Subscribe(delegate
		{
			Observable.FromMicroCoroutine(ModCheckCoroutine).Subscribe();
		});
		canvasGroupOver.Enable(enable: false);
	}
}
