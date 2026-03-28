using System;
using System.IO;

namespace Studio;

public class OIBoneInfo : ObjectInfo
{
	public enum BoneGroup
	{
		Body = 1,
		RightLeg = 2,
		LeftLeg = 4,
		RightArm = 8,
		LeftArm = 0x10,
		RightHand = 0x20,
		LeftHand = 0x40,
		Hair = 0x80,
		Neck = 0x100,
		Breast = 0x200,
		Skirt = 0x400
	}

	public override int kind => -1;

	public BoneGroup group { get; set; }

	public int level { get; set; }

	public override void Save(BinaryWriter _writer, Version _version)
	{
		_writer.Write(base.dicKey);
		base.changeAmount.Save(_writer);
	}

	public override void Load(BinaryReader _reader, Version _version, bool _import, bool _tree = true)
	{
		base.Load(_reader, _version, _import, _other: false);
	}

	public override void DeleteKey()
	{
		Studio.DeleteIndex(base.dicKey);
	}

	public OIBoneInfo(int _key)
		: base(_key)
	{
		group = (BoneGroup)0;
		level = 0;
	}
}
