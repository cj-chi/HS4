using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AspMvcAreaAttribute : Attribute
{
	[CanBeNull]
	public string AnonymousProperty { get; private set; }

	public AspMvcAreaAttribute()
	{
	}

	public AspMvcAreaAttribute([NotNull] string anonymousProperty)
	{
		AnonymousProperty = anonymousProperty;
	}
}
