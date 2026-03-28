using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class RazorImportNamespaceAttribute : Attribute
{
	[NotNull]
	public string Name { get; private set; }

	public RazorImportNamespaceAttribute([NotNull] string name)
	{
		Name = name;
	}
}
