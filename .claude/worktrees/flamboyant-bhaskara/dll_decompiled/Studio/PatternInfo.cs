using System;
using System.IO;
using UniRx;
using UnityEngine;

namespace Studio;

public class PatternInfo
{
	private IntReactiveProperty _key = new IntReactiveProperty(0);

	private StringReactiveProperty _filePath = new StringReactiveProperty("");

	public Color color = Color.white;

	public bool clamp = true;

	public Vector4 uv = new Vector4(0f, 0f, 1f, 1f);

	public float rot;

	public int key
	{
		get
		{
			return _key.Value;
		}
		set
		{
			_key.Value = value;
		}
	}

	public string filePath
	{
		get
		{
			return _filePath.Value;
		}
		set
		{
			_filePath.Value = value;
		}
	}

	public float ut
	{
		get
		{
			return uv.z;
		}
		set
		{
			uv.z = value;
		}
	}

	public float vt
	{
		get
		{
			return uv.w;
		}
		set
		{
			uv.w = value;
		}
	}

	public float us
	{
		get
		{
			return uv.x;
		}
		set
		{
			uv.x = value;
		}
	}

	public float vs
	{
		get
		{
			return uv.y;
		}
		set
		{
			uv.y = value;
		}
	}

	public string name
	{
		get
		{
			int _key = key;
			if (_key != -1)
			{
				PatternSelectInfo patternSelectInfo = Singleton<Studio>.Instance.patternSelectListCtrl.lstSelectInfo.Find((PatternSelectInfo p) => p.index == _key);
				if (patternSelectInfo == null)
				{
					return "なし";
				}
				return patternSelectInfo.name;
			}
			if (!filePath.IsNullOrEmpty())
			{
				return Path.GetFileNameWithoutExtension(filePath);
			}
			return "なし";
		}
	}

	public PatternInfo()
	{
		_key.Subscribe(delegate(int _)
		{
			if (_ != -1)
			{
				_filePath.Value = "";
			}
		});
		_filePath.Subscribe(delegate(string _)
		{
			if (!_.IsNullOrEmpty())
			{
				_key.Value = -1;
			}
		});
	}

	public void Save(BinaryWriter _writer, Version _version)
	{
		_writer.Write(JsonUtility.ToJson(color));
		_writer.Write(_key.Value);
		_writer.Write(_filePath.Value);
		_writer.Write(clamp);
		_writer.Write(JsonUtility.ToJson(uv));
		_writer.Write(rot);
	}

	public void Load(BinaryReader _reader, Version _version)
	{
		color = JsonUtility.FromJson<Color>(_reader.ReadString());
		_key.Value = _reader.ReadInt32();
		_filePath.Value = _reader.ReadString();
		clamp = _reader.ReadBoolean();
		uv = JsonUtility.FromJson<Vector4>(_reader.ReadString());
		rot = _reader.ReadSingle();
	}
}
