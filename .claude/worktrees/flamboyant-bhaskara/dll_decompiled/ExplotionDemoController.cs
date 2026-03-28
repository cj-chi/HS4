using SpriteToParticlesAsset;
using UnityEngine;

public class ExplotionDemoController : MonoBehaviour
{
	public GameObject[] rangerPrefabs;

	public EffectorExplode currentRanger;

	public RadialFillCursor cursor;

	public Transform spawnPosition;

	private int lastRangerIndex;

	private void Start()
	{
		cursor.Show(b: true);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SpawnRanger(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SpawnRanger(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SpawnRanger(2);
		}
		if (Input.GetMouseButtonUp(0))
		{
			Explode();
		}
	}

	public void SpawnRanger(int index)
	{
		CancelInvoke("SpawnRanger");
		if (index >= 0)
		{
			lastRangerIndex = index;
		}
		if ((bool)currentRanger && !currentRanger.exploded)
		{
			Object.DestroyImmediate(currentRanger.gameObject);
		}
		GameObject gameObject = Object.Instantiate(rangerPrefabs[lastRangerIndex], spawnPosition.position, Quaternion.identity);
		currentRanger = gameObject.GetComponent<EffectorExplode>();
		cursor.Show(b: true);
	}

	public void SpawnRanger()
	{
		SpawnRanger(-1);
	}

	public void Explode()
	{
		if ((bool)currentRanger)
		{
			currentRanger.ExplodeAt(cursor.transform.position, cursor.radius, cursor.angle, cursor.rotationAngle, cursor.strenght);
			currentRanger.GetComponent<BoxCollider2D>().enabled = false;
		}
		Invoke("SpawnRanger", 1f);
		cursor.Show(b: false);
	}
}
