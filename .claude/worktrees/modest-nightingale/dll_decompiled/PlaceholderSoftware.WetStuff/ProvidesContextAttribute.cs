using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
internal sealed class ProvidesContextAttribute : Attribute
{
}
