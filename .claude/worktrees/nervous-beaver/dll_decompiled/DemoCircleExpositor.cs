using System;
using UnityEngine;

public class DemoCircleExpositor : MonoBehaviour
{
	[SerializeField]
	private float radius = 40f;

	[SerializeField]
	private float rotateSpeed = 10f;

	private Transform[] items;

	private int count;

	private int currentTarget;

	private float offsetRotation;

	private float iniY;

	private Quaternion dummyRotation;

	private void Start()
	{
		dummyRotation = base.transform.rotation;
		iniY = base.transform.position.y;
		items = new Transform[base.transform.childCount];
		foreach (Transform item in base.transform)
		{
			items[count] = item;
			count++;
		}
		offsetRotation = 360f / (float)count;
		for (int i = 0; i < count; i++)
		{
			float f = (float)i * (float)Math.PI * 2f / (float)count;
			Vector3 position = new Vector3(Mathf.Sin(f) * radius, iniY, (0f - Mathf.Cos(f)) * radius);
			items[i].position = position;
		}
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, dummyRotation, rotateSpeed * Time.deltaTime);
	}

	public void ChangeTarget(int offset)
	{
		currentTarget += offset;
		if (currentTarget > items.Length - 1)
		{
			currentTarget = 0;
		}
		else if (currentTarget < 0)
		{
			currentTarget = items.Length - 1;
		}
		dummyRotation *= Quaternion.Euler(Vector3.up * offset * offsetRotation);
	}
}
