using System;
using System.IO;

namespace Studio;

public class OIRoutePointInfo : ObjectInfo
{
	public enum Connection
	{
		Line,
		Curve
	}

	public float speed = 2f;

	public StudioTween.EaseType easeType = StudioTween.EaseType.linear;

	public Connection connection;

	public OIRoutePointAidInfo aidInfo;

	public bool link;

	public override int kind => 6;

	public string name { get; private set; }

	public int number
	{
		set
		{
			name = ((value == 0) ? "スタート" : $"ポイント{value}");
		}
	}

	public override int[] kinds => new int[2] { 6, 4 };

	public override void Save(BinaryWriter _writer, Version _version)
	{
		_writer.Write(base.dicKey);
		base.changeAmount.Save(_writer);
		_writer.Write(speed);
		_writer.Write((int)easeType);
		_writer.Write((int)connection);
		aidInfo.Save(_writer, _version);
		_writer.Write(link);
	}

	public override void Load(BinaryReader _reader, Version _version, bool _import, bool _tree = true)
	{
		base.Load(_reader, _version, _import, _other: false);
		speed = _reader.ReadSingle();
		easeType = (StudioTween.EaseType)_reader.ReadInt32();
		connection = (Connection)_reader.ReadInt32();
		if (aidInfo == null)
		{
			aidInfo = new OIRoutePointAidInfo((!_import) ? (-1) : Studio.GetNewIndex());
		}
		aidInfo.Load(_reader, _version, _import);
		link = _reader.ReadBoolean();
	}

	public override void DeleteKey()
	{
		Studio.DeleteIndex(base.dicKey);
	}

	public OIRoutePointInfo(int _key)
		: base(_key)
	{
		number = 0;
		speed = 2f;
		easeType = StudioTween.EaseType.linear;
	}
}
