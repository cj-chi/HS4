using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AspMvcSuppressViewErrorAttribute : Attribute
{
}
