using System.Linq;
using UnityEngine;

namespace ADV.Commands.Object;

internal static class ObjectEx
{
	public static Transform FindRoot(string findType, CommandController commandController)
	{
		Transform result = null;
		if (!findType.IsNullOrEmpty())
		{
			result = ((!int.TryParse(findType, out var result2)) ? commandController.Objects[findType].transform : commandController.Character.GetChild(result2));
		}
		return result;
	}

	public static Transform FindChild(Transform root, string name)
	{
		return root.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == name);
	}

	public static Transform FindGet(string findType, string childName, string otherRootName, CommandController commandController)
	{
		Transform transform = FindRoot(findType, commandController);
		if (transform == null)
		{
			transform = GameObject.Find(otherRootName).transform;
		}
		if (!childName.IsNullOrEmpty())
		{
			transform = FindChild(transform, childName);
		}
		return transform;
	}
}
