using UnityEngine;

namespace SpriteToParticlesAsset;

[ExecuteInEditMode]
public class EffectorRepeler : MonoBehaviour
{
	[Tooltip("Repeler force intensity. A negative strength will attract particles instead of repeling them.")]
	public float strength = 1f;

	[Tooltip("Transform at which the particles will repel from. If none is set it will use the current Sprite position.")]
	public Transform repelerCenter;

	private SpriteToParticles emitter;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private Vector3 center;

	private void Awake()
	{
		emitter = GetComponent<SpriteToParticles>();
		if ((bool)emitter && (bool)emitter.particlesSystem)
		{
			ps = emitter.particlesSystem;
		}
		if (!repelerCenter)
		{
			repelerCenter = base.transform;
		}
	}

	public void SetRepelCenterTransform(Transform repeler)
	{
		repelerCenter = repeler;
	}

	private void LateUpdate()
	{
		if (!ps)
		{
			if (!emitter || !emitter.particlesSystem)
			{
				return;
			}
			ps = emitter.particlesSystem;
		}
		if (particles == null || particles.Length < ps.particleCount)
		{
			particles = new ParticleSystem.Particle[ps.particleCount];
		}
		int num = ps.GetParticles(particles);
		if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			center = repelerCenter.localPosition;
		}
		else
		{
			center = repelerCenter.position;
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = particles[i].position - center;
			particles[i].velocity = vector * strength;
		}
		ps.SetParticles(particles, num);
	}
}
