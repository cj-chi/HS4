using Obi;
using UnityEngine;

public class ActorSpawner : MonoBehaviour
{
	public ObiActor template;

	public int basePhase = 2;

	public int maxInstances = 15;

	public float spawnDelay = 0.3f;

	private int phase;

	private int instances;

	private float timeFromLastSpawn;

	private void Update()
	{
		timeFromLastSpawn += Time.deltaTime;
		if (Input.GetMouseButtonDown(0) && instances < maxInstances && timeFromLastSpawn > spawnDelay)
		{
			Object.Instantiate(template.gameObject, base.transform.position, Quaternion.identity).GetComponent<ObiActor>().SetPhase(Oni.MakePhase(basePhase + phase, (Oni.ParticlePhase)0));
			phase++;
			instances++;
			timeFromLastSpawn = 0f;
		}
	}
}
