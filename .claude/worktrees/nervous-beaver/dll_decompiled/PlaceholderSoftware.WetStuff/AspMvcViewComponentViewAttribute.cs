using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
internal sealed class AspMvcViewComponentViewAttribute : Attribute
{
}
