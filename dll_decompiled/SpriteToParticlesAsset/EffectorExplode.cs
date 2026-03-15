using System;
using UnityEngine;

namespace SpriteToParticlesAsset;

public class EffectorExplode : MonoBehaviour
{
	[Tooltip("Weather the system is being used for Sprite or Image component. ")]
	public float destroyObjectAfterExplosionIn = 10f;

	private SpriteToParticles emitter;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	[HideInInspector]
	public bool exploded;

	private void Awake()
	{
		emitter = GetComponent<SpriteToParticles>();
		if ((bool)emitter && (bool)emitter.particlesSystem)
		{
			ps = emitter.particlesSystem;
		}
	}

	public void ExplodeAt(Vector3 sourcePos, float radius, float angle, float startRot, float strenght)
	{
		if (!ps)
		{
			if (!emitter || !emitter.particlesSystem)
			{
				return;
			}
			ps = emitter.particlesSystem;
		}
		emitter.EmitAll();
		if (particles == null || particles.Length < ps.particleCount)
		{
			particles = new ParticleSystem.Particle[ps.particleCount];
		}
		int num = ps.GetParticles(particles);
		float num2 = radius / 2f;
		Vector2 vector = new Vector2(Mathf.Cos((float)Math.PI / 180f * startRot), Mathf.Sin((float)Math.PI / 180f * startRot));
		for (int i = 0; i < num; i++)
		{
			ParticleSystem.Particle particle = particles[i];
			float num3 = Vector3.Distance(sourcePos, particle.position);
			if (num3 < num2)
			{
				Vector3 vector2 = particle.position - sourcePos;
				float num4 = Vector3.Angle(vector, vector2);
				if (Vector3.Cross(vector, vector2).z < 0f)
				{
					num4 = 360f - num4;
				}
				if (num4 < angle)
				{
					vector2.Normalize();
					float num5 = radius - num3;
					float num6 = UnityEngine.Random.Range(num5 / 2f, num5);
					particle.velocity += vector2 * num6 * strenght;
					particles[i] = particle;
				}
			}
		}
		ps.SetParticles(particles, num);
		exploded = true;
		if (destroyObjectAfterExplosionIn >= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject, destroyObjectAfterExplosionIn);
		}
	}

	public void ExplodeTest()
	{
		ExplodeAt(base.transform.position, 10f, 360f, 0f, 2f);
	}
}
