using System;

namespace GameLoadCharaFileSystem;

[Flags]
public enum CategoryKind
{
	Male = 1,
	Female = 2,
	MyData = 4,
	Download = 8,
	Preset = 0x10
}
