using System.Collections;
using UnityEngine;

public class DemoCamera : MonoBehaviour
{
	[SerializeField]
	private Transform targetedItem;

	[SerializeField]
	private DemoController demoController;

	[SerializeField]
	private float speed = 1f;

	private Vector3 offset;

	private Vector3 target;

	private bool canUpdate;

	private void Awake()
	{
		offset = base.transform.position - targetedItem.position;
		StartCoroutine(SetCamAfterStart());
	}

	private void Update()
	{
		if (canUpdate)
		{
			target.y = (float)demoController.GetCurrExpositor() * demoController.expositorDistance;
			base.transform.position = Vector3.Lerp(base.transform.position, target, speed * Time.deltaTime);
		}
	}

	private IEnumerator SetCamAfterStart()
	{
		yield return null;
		base.transform.position = targetedItem.position + offset;
		target = base.transform.position;
		canUpdate = true;
	}
}
