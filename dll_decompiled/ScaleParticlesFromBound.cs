using UnityEngine;

public class ScaleParticlesFromBound : MonoBehaviour
{
	private Collider targetCollider;

	private void GetMeshFilterParent(Transform t)
	{
		Collider component = t.parent.GetComponent<Collider>();
		if (component == null)
		{
			GetMeshFilterParent(t.parent);
		}
		else
		{
			targetCollider = component;
		}
	}

	private void Start()
	{
		GetMeshFilterParent(base.transform);
		if (!(targetCollider == null))
		{
			Vector3 size = targetCollider.bounds.size;
			base.transform.localScale = size;
		}
	}

	private void Update()
	{
	}
}
