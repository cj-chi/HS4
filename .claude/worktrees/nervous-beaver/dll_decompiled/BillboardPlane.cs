using UnityEngine;

public class BillboardPlane : MonoBehaviour
{
	public float BillboardBlendMax = 0.9f;

	public Transform[] Billboards;

	private Transform _CameraTransform;

	private Vector3[] _StoredUp;

	private void Start()
	{
		if (Camera.main != null)
		{
			_CameraTransform = Camera.main.transform;
			_StoredUp = new Vector3[Billboards.Length];
			for (int i = 0; i < Billboards.Length; i++)
			{
				_StoredUp[i] = Billboards[i].up;
			}
		}
	}

	private void Update()
	{
		if (_CameraTransform != null)
		{
			for (int i = 0; i < Billboards.Length; i++)
			{
				Transform obj = Billboards[i];
				Vector3 vector = _StoredUp[i];
				Vector3 normalized = (obj.position - _CameraTransform.position).normalized;
				Vector3 normalized2 = Vector3.Cross(_CameraTransform.right, vector).normalized;
				Quaternion a = Quaternion.LookRotation(normalized, vector);
				Quaternion b = Quaternion.LookRotation(normalized2, vector);
				float t = Mathf.Min(BillboardBlendMax, 2f * (Mathf.Max(0.5f, 0f - Vector3.Dot(vector, normalized)) - 0.5f));
				obj.rotation = Quaternion.Lerp(a, b, t);
			}
		}
	}
}
