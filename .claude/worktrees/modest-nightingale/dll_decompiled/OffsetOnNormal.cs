using UnityEngine;

public class OffsetOnNormal : MonoBehaviour
{
	public float offset = 1f;

	public GameObject offsetGameObject;

	private Vector3 startPosition;

	private void Awake()
	{
		startPosition = base.transform.position;
	}

	private void OnEnable()
	{
		Physics.Raycast(startPosition, Vector3.down, out var hitInfo);
		if (offsetGameObject != null)
		{
			base.transform.position = offsetGameObject.transform.position + hitInfo.normal * offset;
		}
		else
		{
			base.transform.position = hitInfo.point + hitInfo.normal * offset;
		}
	}

	private void Update()
	{
	}
}
