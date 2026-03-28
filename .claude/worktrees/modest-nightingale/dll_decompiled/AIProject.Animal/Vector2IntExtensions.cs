using UnityEngine;

namespace AIProject.Animal;

public static class Vector2IntExtensions
{
	public static int RandomRange(this Vector2Int vec)
	{
		return Random.Range(vec.x, vec.y + 1);
	}
}
