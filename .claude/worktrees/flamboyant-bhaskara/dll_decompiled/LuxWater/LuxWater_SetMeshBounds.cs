using UnityEngine;

namespace LuxWater;

public class LuxWater_SetMeshBounds : MonoBehaviour
{
	[Space(6f)]
	[LuxWater_HelpBtn("h.s0d0kaaphhix")]
	public float Expand_XZ;

	public float Expand_Y;

	private Renderer rend;

	private void OnEnable()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.RecalculateBounds();
		Bounds bounds = sharedMesh.bounds;
		bounds.Expand(new Vector3(Expand_XZ, Expand_Y, Expand_XZ));
		sharedMesh.bounds = bounds;
	}

	private void OnDisable()
	{
		GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
	}

	private void OnValidate()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.RecalculateBounds();
		Bounds bounds = sharedMesh.bounds;
		bounds.Expand(new Vector3(Expand_XZ, Expand_Y, Expand_XZ));
		sharedMesh.bounds = bounds;
	}

	private void OnDrawGizmosSelected()
	{
		rend = GetComponent<Renderer>();
		Vector3 center = rend.bounds.center;
		_ = rend.bounds.extents;
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(center, rend.bounds.size);
	}
}
