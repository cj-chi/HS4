using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
internal sealed class ItemCanBeNullAttribute : Attribute
{
}
