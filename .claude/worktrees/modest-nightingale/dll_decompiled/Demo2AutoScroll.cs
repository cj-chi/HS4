using System.Collections;
using UnityEngine;

public class Demo2AutoScroll : MonoBehaviour
{
	private Transform[] children;

	public float totalTime;

	public GameObject sceneDescription;

	private void Start()
	{
		sceneDescription.SetActive(value: false);
		Camera.main.fieldOfView = 60f;
		children = GetComponentsInChildren<Transform>();
		for (int i = 0; i < children.Length; i++)
		{
			if (children[i].gameObject != base.gameObject)
			{
				children[i].gameObject.SetActive(value: false);
				children[i].localPosition = Vector3.zero;
			}
		}
		totalTime /= children.Length;
		StartCoroutine(ScrollElements());
	}

	private IEnumerator ScrollElements()
	{
		int i = 0;
		while (true)
		{
			if (children[i].gameObject == base.gameObject)
			{
				i = (i + 1) % children.Length;
				continue;
			}
			children[i].gameObject.SetActive(value: true);
			yield return new WaitForSeconds(totalTime);
			children[i].gameObject.SetActive(value: false);
			i = (i + 1) % children.Length;
		}
	}
}
