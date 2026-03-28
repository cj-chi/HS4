using System;
using System.IO;

namespace Studio;

public class OICameraInfo : ObjectInfo
{
	public string name = "";

	public bool active;

	public override int kind => 5;

	public override void Save(BinaryWriter _writer, Version _version)
	{
		base.Save(_writer, _version);
		_writer.Write(name);
		_writer.Write(active);
	}

	public override void Load(BinaryReader _reader, Version _version, bool _import, bool _tree = true)
	{
		base.Load(_reader, _version, _import);
		name = _reader.ReadString();
		active = _reader.ReadBoolean();
	}

	public override void DeleteKey()
	{
		Studio.DeleteIndex(base.dicKey);
	}

	public OICameraInfo(int _key)
		: base(_key)
	{
		name = $"カメラ{Singleton<Studio>.Instance.cameraCount}";
		active = false;
	}
}
