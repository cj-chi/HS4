using UnityEngine;

public class BoundingBoxChecker : MonoBehaviour
{
	[SerializeField]
	private Vector3 size;

	[SerializeField]
	private Vector3 center;

	private void OnDrawGizmosSelected()
	{
		Renderer component = GetComponent<Renderer>();
		if (!(component == null))
		{
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			bounds.Encapsulate(component.bounds);
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			Gizmos.DrawCube(bounds.center, bounds.size);
			size = bounds.size;
			center = bounds.center;
		}
	}
}
