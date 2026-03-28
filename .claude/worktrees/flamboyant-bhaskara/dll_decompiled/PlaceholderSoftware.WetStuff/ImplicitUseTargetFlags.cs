using System;

namespace PlaceholderSoftware.WetStuff;

[Flags]
internal enum ImplicitUseTargetFlags
{
	Default = 1,
	Itself = 1,
	Members = 2,
	WithMembers = 3
}
