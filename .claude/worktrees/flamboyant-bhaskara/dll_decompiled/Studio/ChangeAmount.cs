using System;
using System.IO;
using UnityEngine;

namespace Studio;

public class ChangeAmount
{
	protected Vector3 m_Pos = Vector3.zero;

	protected Vector3 m_Rot = Vector3.zero;

	protected Vector3 m_Scale = Vector3.one;

	public Action onChangePos;

	public Action onChangePosAfter;

	public Action onChangeRot;

	public Action<Vector3> onChangeScale;

	public Vector3 pos
	{
		get
		{
			return m_Pos;
		}
		set
		{
			if (Utility.SetStruct(ref m_Pos, value) && onChangePos != null)
			{
				onChangePos();
				onChangePosAfter?.Invoke();
			}
		}
	}

	public Vector3 rot
	{
		get
		{
			return m_Rot;
		}
		set
		{
			if (Utility.SetStruct(ref m_Rot, value) && onChangeRot != null)
			{
				onChangeRot();
			}
		}
	}

	public Vector3 scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			if (Utility.SetStruct(ref m_Scale, value) && onChangeScale != null)
			{
				onChangeScale(value);
			}
		}
	}

	public Vector3 defRot { get; set; } = Vector3.zero;

	public ChangeAmount()
	{
		m_Pos = Vector3.zero;
		m_Rot = Vector3.zero;
		m_Scale = Vector3.one;
	}

	public ChangeAmount(Vector3 pos, Vector3 rot, Vector3 scale)
	{
		m_Pos = pos;
		m_Rot = rot;
		m_Scale = scale;
	}

	public void Save(BinaryWriter _writer)
	{
		_writer.Write(m_Pos.x);
		_writer.Write(m_Pos.y);
		_writer.Write(m_Pos.z);
		_writer.Write(m_Rot.x);
		_writer.Write(m_Rot.y);
		_writer.Write(m_Rot.z);
		_writer.Write(m_Scale.x);
		_writer.Write(m_Scale.y);
		_writer.Write(m_Scale.z);
	}

	public void Load(BinaryReader _reader)
	{
		m_Pos.Set(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		m_Rot.Set(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		m_Scale.Set(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
	}

	public ChangeAmount Clone()
	{
		return new ChangeAmount(m_Pos, m_Rot, m_Scale);
	}

	public void Copy(ChangeAmount _source, bool _pos = true, bool _rot = true, bool _scale = true)
	{
		if (_pos)
		{
			pos = _source.pos;
		}
		if (_rot)
		{
			rot = _source.rot;
		}
		if (_scale)
		{
			scale = _source.scale;
		}
	}

	public void OnChange()
	{
		if (onChangePos != null)
		{
			onChangePos();
			onChangePosAfter?.Invoke();
		}
		onChangeRot?.Invoke();
		onChangeScale?.Invoke(scale);
	}

	public void Reset()
	{
		pos = Vector3.zero;
		rot = Vector3.zero;
		scale = Vector3.one;
	}
}
