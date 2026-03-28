using UnityEngine;

namespace SpriteParticleEmitter;

public class StaticEmitterContinuous : StaticSpriteEmitter
{
	public override event SimpleEvent OnAvailableToPlay;

	protected override void Update()
	{
		base.Update();
		if (isPlaying && hasCachingEnded)
		{
			Emit();
		}
	}

	public override void CacheSprite(bool relativeToParent = false)
	{
		base.CacheSprite();
		if (OnAvailableToPlay != null)
		{
			OnAvailableToPlay();
		}
	}

	protected void Emit()
	{
		if (!hasCachingEnded)
		{
			return;
		}
		ParticlesToEmitThisFrame += EmissionRate * Time.deltaTime;
		Vector3 position = spriteRenderer.gameObject.transform.position;
		Vector3 vector = position;
		Quaternion rotation = spriteRenderer.gameObject.transform.rotation;
		Vector3 lossyScale = spriteRenderer.gameObject.transform.lossyScale;
		ParticleSystemSimulationSpace simulationSpace = SimulationSpace;
		int max = particlesCacheCount;
		float startSize = particleStartSize;
		int num = (int)ParticlesToEmitThisFrame;
		if (particlesCacheCount <= 0)
		{
			return;
		}
		Color[] array = particleInitColorCache;
		Vector3[] array2 = particleInitPositionsCache;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < num; i++)
		{
			int num2 = Random.Range(0, max);
			if (useBetweenFramesPrecision)
			{
				float t = Random.Range(0f, 1f);
				vector = Vector3.Lerp(lastTransformPosition, position, t);
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array[num2];
			}
			emitParams.startSize = startSize;
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				Vector3 vector2 = array2[num2];
				zero.x = vector2.x * lossyScale.x;
				zero.y = vector2.y * lossyScale.y;
				emitParams.position = rotation * zero + vector;
				particlesSystem.Emit(emitParams, 1);
			}
			else
			{
				emitParams.position = array2[num2];
				particlesSystem.Emit(emitParams, 1);
			}
		}
		ParticlesToEmitThisFrame -= num;
		lastTransformPosition = position;
	}

	public override void Play()
	{
		if (!isPlaying)
		{
			particlesSystem.Play();
		}
		isPlaying = true;
	}

	public override void Stop()
	{
		isPlaying = false;
	}

	public override void Pause()
	{
		if (isPlaying)
		{
			particlesSystem.Pause();
		}
		isPlaying = false;
	}
}
