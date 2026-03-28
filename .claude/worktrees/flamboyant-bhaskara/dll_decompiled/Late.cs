using UnityEngine;

[DefaultExecutionOrder(15000)]
public class Late : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Vector3 posLocal = Vector3.zero;

	private void LateUpdate()
	{
		if (target != null)
		{
			target.localPosition = posLocal;
		}
	}
}
