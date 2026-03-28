using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
internal sealed class CannotApplyEqualityOperatorAttribute : Attribute
{
}
