using System;
using UnityEngine;

namespace AIProject.Animal;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field)]
public class CreateRangeAttribute : PropertyAttribute
{
	public string label = "";

	public CreateRangeAttribute(string _label)
	{
		label = _label;
	}
}
