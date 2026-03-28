using UnityEngine;

[ExecuteInEditMode]
public class ME_ParticleGravityPoint : MonoBehaviour
{
	public Transform target;

	public float Force = 1f;

	public bool DistanceRelative;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private ParticleSystem.MainModule mainModule;

	private Vector3 prevPos;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		mainModule = ps.main;
	}

	private void LateUpdate()
	{
		int maxParticles = mainModule.maxParticles;
		if (particles == null || particles.Length < maxParticles)
		{
			particles = new ParticleSystem.Particle[maxParticles];
		}
		int num = ps.GetParticles(particles);
		Vector3 vector = Vector3.zero;
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = base.transform.InverseTransformPoint(target.position);
		}
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.World)
		{
			vector = target.position;
		}
		float num2 = Time.deltaTime * Force;
		if (DistanceRelative)
		{
			num2 *= Mathf.Abs((prevPos - vector).magnitude);
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = Vector3.Normalize(vector - particles[i].position);
			if (DistanceRelative)
			{
				vector2 = Vector3.Normalize(vector - prevPos);
			}
			Vector3 vector3 = vector2 * num2;
			particles[i].velocity += vector3;
		}
		ps.SetParticles(particles, num);
		prevPos = vector;
	}
}
