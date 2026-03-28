using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Illusion.Elements.Reference;
using Manager;
using UnityEngine;

namespace ADV;

public class CommandController : MonoBehaviour
{
	public class FontColor : AutoIndexer<(int key, Color color)>
	{
		public new Color this[string key]
		{
			get
			{
				switch (key)
				{
				case "[P]":
				case "[P名前]":
					return GetConfigColor(0).Value;
				case "[H]":
				case "[H名前]":
					return GetConfigColor(1).Value;
				default:
				{
					(int, Color) tuple = base[key];
					return GetConfigColor(tuple.Item1) ?? tuple.Item2;
				}
				}
			}
			private set
			{
			}
		}

		public FontColor()
			: base((-1, Color.white))
		{
		}

		public void Set(string key, Color color)
		{
			Set(key, base.initializeValue.key, color);
		}

		public void Set(string key, int configIndex)
		{
			Set(key, configIndex, base.initializeValue.color);
		}

		private void Set(string key, int configIndex, Color color)
		{
			base.table[key] = (configIndex, color);
		}

		private Color? GetConfigColor(int configIndex)
		{
			return configIndex switch
			{
				0 => Manager.Config.TextData.Font0Color, 
				1 => Manager.Config.TextData.Font1Color, 
				2 => Manager.Config.TextData.Font2Color, 
				_ => null, 
			};
		}
	}

	[Serializable]
	private class CharaCorrectHeightCamera
	{
		[Serializable]
		private struct Pair
		{
			[SerializeField]
			private Vector3 _min;

			[SerializeField]
			private Vector3 _max;

			public Vector3 min
			{
				get
				{
					return _min;
				}
				set
				{
					_min = value;
				}
			}

			public Vector3 max
			{
				get
				{
					return _max;
				}
				set
				{
					_max = value;
				}
			}

			public Pair(Vector3 min, Vector3 max)
			{
				_min = min;
				_max = max;
			}
		}

		[SerializeField]
		private Pair pos;

		[SerializeField]
		private Pair ang;

		public bool Calculate(IEnumerable<CharaData> datas, out Vector3 pos, out Vector3 ang)
		{
			if (datas == null || !datas.Any())
			{
				pos = Vector3.zero;
				ang = Vector3.zero;
				return false;
			}
			float shape = datas.Average((CharaData item) => item.chaCtrl.GetShapeBodyValue(0));
			pos = MathfEx.GetShapeLerpPositionValue(shape, this.pos.min, this.pos.max);
			ang = MathfEx.GetShapeLerpAngleValue(shape, this.ang.min, this.ang.max);
			return true;
		}
	}

	[SerializeField]
	private Transform character;

	private Dictionary<string, Transform> _characterStandNulls;

	private int charaStartIndex;

	[SerializeField]
	private Transform eventCGRoot;

	[SerializeField]
	private Transform objectRoot;

	[SerializeField]
	private Transform nullRoot;

	[SerializeField]
	private Transform basePosition;

	[SerializeField]
	private Transform cameraPosition;

	private Action correctCameraReset;

	[SerializeField]
	private bool _useCorrectCamera = true;

	[SerializeField]
	private CharaCorrectHeightCamera correctCamera = new CharaCorrectHeightCamera();

	[SerializeField]
	private bool _isCharaReleaseBackUpPos = true;

	private bool useCorrectCameraBakup;

	public Transform Character => character;

	public Dictionary<string, Transform> characterStandNulls => this.GetCache(ref _characterStandNulls, () => (from i in Enumerable.Range(0, charaStartIndex)
		select character.GetChild(i)).ToDictionary((Transform v) => v.name, (Transform v) => v, StringComparer.InvariantCultureIgnoreCase));

	private BackupPosRot cameraRoot { get; set; }

	private BackupPosRot charaRoot { get; set; }

	public Transform EventCGRoot => eventCGRoot;

	public Transform ObjectRoot => objectRoot;

	public Transform NullRoot => nullRoot;

	public Transform BasePositon => basePosition;

	public Transform CameraPosition => cameraPosition;

	public bool useCorrectCamera
	{
		get
		{
			return _useCorrectCamera;
		}
		set
		{
			_useCorrectCamera = value;
			if (!value)
			{
				correctCameraReset?.Invoke();
				correctCameraReset = null;
			}
		}
	}

	public bool IsCharaReleaseBackUpPos
	{
		get
		{
			return _isCharaReleaseBackUpPos;
		}
		set
		{
			_isCharaReleaseBackUpPos = value;
		}
	}

	public CommandList NowCommandList => nowCommandList;

	private CommandList nowCommandList { get; set; }

	public CommandList BackGroundCommandList => backGroundCommandList;

	private CommandList backGroundCommandList { get; set; }

	public List<CharaData> LoadingCharaList => loadingCharaList;

	private List<CharaData> loadingCharaList { get; } = new List<CharaData>();

	public Dictionary<int, CharaData> Characters { get; } = new Dictionary<int, CharaData>();

	public Dictionary<string, GameObject> Objects { get; } = new Dictionary<string, GameObject>();

	public Dictionary<string, Vector3> V3Dic { get; } = new Dictionary<string, Vector3>();

	public Dictionary<string, Transform> NullTable { get; } = new Dictionary<string, Transform>();

	public FontColor fontColor { get; } = new FontColor();

	private TextScenario scenario { get; set; }

	public bool GetV3Dic(string arg, out Vector3 pos)
	{
		pos = Vector3.zero;
		if (!arg.IsNullOrEmpty() && !float.TryParse(arg, out var _))
		{
			return V3Dic.TryGetValue(arg, out pos);
		}
		return false;
	}

	public CharaData GetChara(int no)
	{
		if (no < 0)
		{
			TextScenario.ParamData paramData = null;
			paramData = ((no != -1) ? scenario.heroineList[Mathf.Abs(no + 2)] : scenario.player);
			if (paramData != null)
			{
				foreach (KeyValuePair<int, CharaData> character in Characters)
				{
					if (character.Value.data == paramData)
					{
						return character.Value;
					}
				}
				return new CharaData(paramData, scenario, isParent: true);
			}
		}
		if (!Characters.TryGetValue(no, out var value))
		{
			return scenario.currentChara;
		}
		return value;
	}

	public void AddChara(int no, CharaData data)
	{
		RemoveChara(no);
		Characters[no] = data;
	}

	public void RemoveChara(int no)
	{
		if (Characters.TryGetValue(no, out var value))
		{
			value.Release();
			loadingCharaList.Remove(value);
		}
		Characters.Remove(no);
	}

	public void Initialize()
	{
		useCorrectCameraBakup = _useCorrectCamera;
		if (scenario == null)
		{
			scenario = GetComponent<TextScenario>();
			nowCommandList = new CommandList(scenario);
			backGroundCommandList = new CommandList(scenario);
		}
		else
		{
			nowCommandList.Clear();
			backGroundCommandList.Clear();
		}
		loadingCharaList.Clear();
		Objects.Clear();
		Characters.Clear();
		V3Dic.Clear();
		NullTable.Clear();
		fontColor.Clear();
		if (cameraRoot != null)
		{
			cameraRoot.Set(cameraPosition);
		}
		if (charaRoot != null)
		{
			charaRoot.Set(character);
		}
	}

	public void SetObject(GameObject go)
	{
		if (Objects.TryGetValue(go.name, out var value))
		{
			UnityEngine.Object.Destroy(value);
		}
		go.transform.SetParent(ObjectRoot, worldPositionStays: false);
		Objects[go.name] = go;
	}

	public void SetNull(Transform nullT)
	{
		if (NullTable.TryGetValue(nullT.name, out var value) && value != null)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		nullT.SetParent(NullRoot, worldPositionStays: false);
		NullTable[nullT.name] = nullT;
	}

	public void ReleaseObject()
	{
		foreach (GameObject value in Objects.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
		Objects.Clear();
	}

	public void ReleaseNull()
	{
		foreach (Transform value in NullTable.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
		NullTable.Clear();
	}

	public void ReleaseChara()
	{
		foreach (CharaData value in Characters.Values)
		{
			value.Release(_isCharaReleaseBackUpPos);
		}
		Characters.Clear();
	}

	public void ReleaseEventCG()
	{
		if (!(eventCGRoot == null))
		{
			for (int i = 0; i < eventCGRoot.childCount; i++)
			{
				UnityEngine.Object.Destroy(eventCGRoot.GetChild(i).gameObject);
			}
		}
	}

	public void Release()
	{
		ReleaseObject();
		ReleaseNull();
		ReleaseChara();
		ReleaseEventCG();
		_useCorrectCamera = useCorrectCameraBakup;
	}

	private IEnumerator RestoreCameraPosition(Vector3 pos, Quaternion rot)
	{
		correctCameraReset = delegate
		{
			cameraPosition.SetPositionAndRotation(pos, rot);
		};
		yield return new WaitForEndOfFrame();
		if (correctCameraReset != null)
		{
			correctCameraReset();
			correctCameraReset = null;
		}
	}

	private void OnDestroy()
	{
		Release();
	}

	private void Awake()
	{
		if (cameraPosition != null)
		{
			cameraRoot = new BackupPosRot(cameraPosition);
		}
		if (character != null)
		{
			charaStartIndex = character.childCount;
			charaRoot = new BackupPosRot(character);
		}
	}

	private void Update()
	{
		if (_useCorrectCamera && cameraPosition != null && correctCamera.Calculate(from p in Characters
			where p.Key >= 0 && p.Value.chaCtrl != null
			select p.Value, out var pos, out var ang))
		{
			StartCoroutine(RestoreCameraPosition(cameraPosition.position, cameraPosition.rotation));
			cameraPosition.SetPositionAndRotation(cameraPosition.position + pos, cameraPosition.rotation * Quaternion.Euler(ang));
		}
	}
}
