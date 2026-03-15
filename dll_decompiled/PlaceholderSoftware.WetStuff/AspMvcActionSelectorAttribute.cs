using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal sealed class AspMvcActionSelectorAttribute : Attribute
{
}
