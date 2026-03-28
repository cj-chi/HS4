using System;

namespace PlaceholderSoftware.WetStuff;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AssertionConditionAttribute : Attribute
{
	public AssertionConditionType ConditionType { get; private set; }

	public AssertionConditionAttribute(AssertionConditionType conditionType)
	{
		ConditionType = conditionType;
	}
}
