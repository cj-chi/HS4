using UnityEngine;

public class Fixed : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Vector3 posLocal = Vector3.zero;

	private void Update()
	{
		if (target != null)
		{
			target.localPosition = posLocal;
		}
	}
}
