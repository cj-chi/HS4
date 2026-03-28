using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class AspDataFieldAttribute : Attribute
{
}
