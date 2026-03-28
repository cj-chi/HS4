using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
internal sealed class NoReorderAttribute : Attribute
{
}
