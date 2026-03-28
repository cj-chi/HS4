using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[BaseTypeRequired(typeof(Attribute))]
internal sealed class BaseTypeRequiredAttribute : Attribute
{
	[NotNull]
	public Type BaseType { get; private set; }

	public BaseTypeRequiredAttribute([NotNull] Type baseType)
	{
		BaseType = baseType;
	}
}
