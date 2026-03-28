using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
	public float Speed = 10f;

	public float Distance = 20f;

	private void Start()
	{
		StartCoroutine(FlyAndDie());
	}

	private IEnumerator FlyAndDie()
	{
		float dist = 0f;
		Vector3 position = base.transform.localPosition;
		while (dist < Distance)
		{
			float num = Speed * Time.deltaTime;
			dist += num;
			position.z += num;
			base.transform.localPosition = position;
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		Object.Destroy(base.gameObject);
	}
}
