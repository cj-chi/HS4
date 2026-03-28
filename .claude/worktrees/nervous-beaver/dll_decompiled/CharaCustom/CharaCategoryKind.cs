using System;

namespace CharaCustom;

[Flags]
public enum CharaCategoryKind
{
	Male = 1,
	Female = 2,
	MyData = 4,
	Download = 8,
	Preset = 0x10
}
