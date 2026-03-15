using System.Collections;
using UnityEngine;

public class SpawnFireballs : MonoBehaviour
{
	public GameObject[] FireballPrefabs;

	public float Delay = 2f;

	private void Start()
	{
		StartCoroutine(SpawnForever());
	}

	private IEnumerator SpawnForever()
	{
		while (true)
		{
			GameObject original = FireballPrefabs[Random.Range(0, FireballPrefabs.Length)];
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			Object.Instantiate(original, base.transform.position + insideUnitSphere, base.transform.rotation).transform.SetParent(base.transform);
			yield return new WaitForSeconds(Delay);
		}
	}
}
