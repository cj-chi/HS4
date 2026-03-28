using System;
using UnityEngine;

public class RayChara : CollisionCamera
{
	[Serializable]
	public class Parts
	{
		public Transform target;

		public void Update(Vector3 pos, string tag)
		{
			Vector3 vector = target.position - pos;
			RaycastHit[] array = Physics.RaycastAll(pos, vector.normalized, vector.magnitude);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if (raycastHit.collider.gameObject.tag == tag)
				{
					raycastHit.collider.gameObject.GetComponent<Renderer>().enabled = false;
				}
			}
		}
	}

	public Parts[] parts;

	private new void Start()
	{
		base.Start();
	}

	private void Update()
	{
		if (parts != null && objDels != null)
		{
			GameObject[] array = objDels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetComponent<Renderer>().enabled = true;
			}
			Parts[] array2 = parts;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Update(base.transform.position, tagName);
			}
		}
	}
}
