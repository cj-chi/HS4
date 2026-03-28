using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Studio;

public class OIItemInfo : ObjectInfo
{
	public int animePattern;

	public float animeSpeed = 1f;

	public ColorInfo[] colors;

	public float alpha = 1f;

	public Color emissionColor;

	public float emissionPower;

	public float lightCancel;

	public PatternInfo panel;

	public bool enableFK;

	public Dictionary<string, OIBoneInfo> bones;

	public bool enableDynamicBone = true;

	public List<bool> option;

	public float animeNormalizedTime;

	private Color32 emissionColor32;

	private float emissionColorIntensity;

	public override int kind => 1;

	public int group { get; private set; }

	public int category { get; private set; }

	public int no { get; private set; }

	public List<ObjectInfo> child { get; private set; }

	public Color EmissionColor
	{
		get
		{
			DecomposeHDRColor(emissionColor, out emissionColor32, out emissionColorIntensity);
			return emissionColor32;
		}
		set
		{
			emissionColor = (Mathf.Approximately(emissionColorIntensity, 0f) ? value : (value * Mathf.Pow(2f, emissionColorIntensity)));
		}
	}

	public override void Save(BinaryWriter _writer, Version _version)
	{
		base.Save(_writer, _version);
		_writer.Write(group);
		_writer.Write(category);
		_writer.Write(no);
		_writer.Write(animePattern);
		_writer.Write(animeSpeed);
		for (int i = 0; i < 4; i++)
		{
			colors[i].Save(_writer, _version);
		}
		_writer.Write(alpha);
		_writer.Write(JsonUtility.ToJson(emissionColor));
		_writer.Write(emissionPower);
		_writer.Write(lightCancel);
		panel.Save(_writer, _version);
		_writer.Write(enableFK);
		_writer.Write(bones.Count);
		foreach (KeyValuePair<string, OIBoneInfo> bone in bones)
		{
			_writer.Write(bone.Key);
			bone.Value.Save(_writer, _version);
		}
		_writer.Write(enableDynamicBone);
		_writer.Write(option.Count);
		foreach (bool item in option)
		{
			_writer.Write(item);
		}
		_writer.Write(animeNormalizedTime);
		int count = child.Count;
		_writer.Write(count);
		for (int j = 0; j < count; j++)
		{
			child[j].Save(_writer, _version);
		}
	}

	public override void Load(BinaryReader _reader, Version _version, bool _import, bool _tree = true)
	{
		base.Load(_reader, _version, _import);
		group = _reader.ReadInt32();
		category = _reader.ReadInt32();
		no = _reader.ReadInt32();
		if (_version.CompareTo(new Version(1, 0, 1)) >= 0)
		{
			animePattern = _reader.ReadInt32();
		}
		animeSpeed = _reader.ReadSingle();
		for (int i = 0; i < 4; i++)
		{
			colors[i].Load(_reader, _version);
		}
		alpha = _reader.ReadSingle();
		emissionColor = JsonUtility.FromJson<Color>(_reader.ReadString());
		emissionPower = _reader.ReadSingle();
		lightCancel = _reader.ReadSingle();
		panel.Load(_reader, _version);
		if (_version.CompareTo(new Version(1, 1, 0)) <= 0 && !panel.filePath.IsNullOrEmpty())
		{
			panel.filePath = BackgroundListAssist.GetFilePath(panel.filePath);
		}
		enableFK = _reader.ReadBoolean();
		int num = _reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			string key = _reader.ReadString();
			bones[key] = new OIBoneInfo((!_import) ? (-1) : Studio.GetNewIndex());
			bones[key].Load(_reader, _version, _import);
		}
		enableDynamicBone = _reader.ReadBoolean();
		num = _reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			option.Add(_reader.ReadBoolean());
		}
		animeNormalizedTime = _reader.ReadSingle();
		ObjectInfoAssist.LoadChild(_reader, _version, child, _import);
	}

	public override void DeleteKey()
	{
		Studio.DeleteIndex(base.dicKey);
		int count = child.Count;
		for (int i = 0; i < count; i++)
		{
			child[i].DeleteKey();
		}
	}

	public OIItemInfo(int _group, int _category, int _no, int _key)
		: base(_key)
	{
		group = _group;
		category = _category;
		no = _no;
		child = new List<ObjectInfo>();
		colors = new ColorInfo[4]
		{
			new ColorInfo(),
			new ColorInfo(),
			new ColorInfo(),
			new ColorInfo()
		};
		emissionColor = Utility.ConvertColor(255, 255, 255);
		panel = new PatternInfo();
		bones = new Dictionary<string, OIBoneInfo>();
		option = new List<bool>();
		animeNormalizedTime = 0f;
	}

	internal void DecomposeHDRColor(Color _colorHDR, out Color32 _baseColor, out float _intensity)
	{
		_baseColor = Color.black;
		float maxColorComponent = _colorHDR.maxColorComponent;
		byte b = 191;
		if (Mathf.Approximately(maxColorComponent, 0f) || (maxColorComponent <= 1f && maxColorComponent > 0.003921569f))
		{
			_intensity = 0f;
			_baseColor.r = (byte)Mathf.RoundToInt(_colorHDR.r * 255f);
			_baseColor.g = (byte)Mathf.RoundToInt(_colorHDR.g * 255f);
			_baseColor.b = (byte)Mathf.RoundToInt(_colorHDR.b * 255f);
		}
		else
		{
			float num = (float)(int)b / maxColorComponent;
			_intensity = Mathf.Log(255f / num) / Mathf.Log(2f);
			_baseColor.r = Math.Min(b, (byte)Mathf.CeilToInt(num * _colorHDR.r));
			_baseColor.g = Math.Min(b, (byte)Mathf.CeilToInt(num * _colorHDR.g));
			_baseColor.b = Math.Min(b, (byte)Mathf.CeilToInt(num * _colorHDR.b));
		}
	}
}
