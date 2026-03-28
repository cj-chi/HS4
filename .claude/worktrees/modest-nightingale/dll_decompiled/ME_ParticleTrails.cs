using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ME_ParticleTrails : MonoBehaviour
{
	public GameObject TrailPrefab;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private Dictionary<uint, GameObject> hashTrails = new Dictionary<uint, GameObject>();

	private Dictionary<uint, GameObject> newHashTrails = new Dictionary<uint, GameObject>();

	private List<GameObject> currentGO = new List<GameObject>();

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		particles = new ParticleSystem.Particle[ps.main.maxParticles];
	}

	private void OnEnable()
	{
		InvokeRepeating("ClearEmptyHashes", 1f, 1f);
	}

	private void OnDisable()
	{
		Clear();
		CancelInvoke("ClearEmptyHashes");
	}

	public void Clear()
	{
		foreach (GameObject item in currentGO)
		{
			Object.Destroy(item);
		}
		currentGO.Clear();
	}

	private void Update()
	{
		UpdateTrail();
	}

	private void UpdateTrail()
	{
		newHashTrails.Clear();
		int num = ps.GetParticles(particles);
		for (int i = 0; i < num; i++)
		{
			if (!hashTrails.ContainsKey(particles[i].randomSeed))
			{
				GameObject gameObject = Object.Instantiate(TrailPrefab, base.transform.position, default(Quaternion));
				gameObject.transform.parent = base.transform;
				currentGO.Add(gameObject);
				newHashTrails.Add(particles[i].randomSeed, gameObject);
				gameObject.GetComponent<LineRenderer>().widthMultiplier *= particles[i].startSize;
				continue;
			}
			GameObject gameObject2 = hashTrails[particles[i].randomSeed];
			if (gameObject2 != null)
			{
				LineRenderer component = gameObject2.GetComponent<LineRenderer>();
				component.startColor *= (Color)particles[i].GetCurrentColor(ps);
				component.endColor *= (Color)particles[i].GetCurrentColor(ps);
				if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
				{
					gameObject2.transform.position = particles[i].position;
				}
				if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
				{
					gameObject2.transform.position = ps.transform.TransformPoint(particles[i].position);
				}
				newHashTrails.Add(particles[i].randomSeed, gameObject2);
			}
			hashTrails.Remove(particles[i].randomSeed);
		}
		foreach (KeyValuePair<uint, GameObject> hashTrail in hashTrails)
		{
			if (hashTrail.Value != null)
			{
				hashTrail.Value.GetComponent<ME_TrailRendererNoise>().IsActive = false;
			}
		}
		AddRange(hashTrails, newHashTrails);
	}

	public void AddRange<T, S>(Dictionary<T, S> source, Dictionary<T, S> collection)
	{
		if (collection == null)
		{
			return;
		}
		foreach (KeyValuePair<T, S> item in collection)
		{
			if (!source.ContainsKey(item.Key))
			{
				source.Add(item.Key, item.Value);
			}
		}
	}

	private void ClearEmptyHashes()
	{
		hashTrails = hashTrails.Where((KeyValuePair<uint, GameObject> h) => h.Value != null).ToDictionary((KeyValuePair<uint, GameObject> h) => h.Key, (KeyValuePair<uint, GameObject> h) => h.Value);
	}
}
