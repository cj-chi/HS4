using System;
using System.IO;
using UnityEngine;

namespace Studio;

public class ColorInfo
{
	public Color mainColor = Color.white;

	public float metallic;

	public float glossiness;

	public PatternInfo pattern = new PatternInfo();

	public virtual void Save(BinaryWriter _writer, Version _version)
	{
		_writer.Write(JsonUtility.ToJson(mainColor));
		_writer.Write(metallic);
		_writer.Write(glossiness);
		pattern.Save(_writer, _version);
	}

	public virtual void Load(BinaryReader _reader, Version _version)
	{
		mainColor = JsonUtility.FromJson<Color>(_reader.ReadString());
		metallic = _reader.ReadSingle();
		glossiness = _reader.ReadSingle();
		pattern.Load(_reader, _version);
	}
}
