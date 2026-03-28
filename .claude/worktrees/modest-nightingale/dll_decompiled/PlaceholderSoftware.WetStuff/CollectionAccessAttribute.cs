using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class CollectionAccessAttribute : Attribute
{
	public CollectionAccessType CollectionAccessType { get; private set; }

	public CollectionAccessAttribute(CollectionAccessType collectionAccessType)
	{
		CollectionAccessType = collectionAccessType;
	}
}
