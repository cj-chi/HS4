using UnityEngine;

public class LazyLoad : MonoBehaviour
{
	public GameObject GO;

	public float TimeDelay = 0.3f;

	private void Awake()
	{
		GO.SetActive(value: false);
	}

	private void LazyEnable()
	{
		GO.SetActive(value: true);
	}

	private void OnEnable()
	{
		Invoke("LazyEnable", TimeDelay);
	}

	private void OnDisable()
	{
		CancelInvoke("LazyEnable");
		GO.SetActive(value: false);
	}
}
