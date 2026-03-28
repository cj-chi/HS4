using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
internal sealed class AspMvcControllerAttribute : Attribute
{
	[CanBeNull]
	public string AnonymousProperty { get; private set; }

	public AspMvcControllerAttribute()
	{
	}

	public AspMvcControllerAttribute([NotNull] string anonymousProperty)
	{
		AnonymousProperty = anonymousProperty;
	}
}
