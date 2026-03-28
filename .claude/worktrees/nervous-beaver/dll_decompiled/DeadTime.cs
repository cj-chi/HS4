using UnityEngine;

public class DeadTime : MonoBehaviour
{
	public float deadTime = 1.5f;

	public bool destroyRoot;

	private void Awake()
	{
		Object.Destroy((!destroyRoot) ? base.gameObject : base.transform.root.gameObject, deadTime);
	}
}
