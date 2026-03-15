using System.Collections;
using UnityEngine;

namespace SpriteParticleEmitter;

public class StaticEmitterOneShot : StaticSpriteEmitter
{
	[Tooltip("Must the script disable referenced spriteRenderer component?")]
	public bool HideOriginalSpriteOnPlay = true;

	[Header("Silent Emission")]
	[Tooltip("Should start Silent Emitting as soon as has cache ended? (Refer to manual for further explanation)")]
	public bool SilentEmitOnAwake = true;

	[Tooltip("Silent emission can be expensive. This defines the lower limit fps can go before continue silent emission on next frame (Refer to manual for further explanation)")]
	public float WantedFPSDuringSilentEmission = 60f;

	protected bool SilentEmissionEnded;

	protected bool hasSilentEmissionAlreadyBeenShot;

	public override event SimpleEvent OnAvailableToPlay;

	protected override void Awake()
	{
		base.Awake();
		SilentEmissionEnded = false;
		if (SilentEmitOnAwake)
		{
			EmitSilently();
		}
	}

	public override void CacheSprite(bool relativeToParent = false)
	{
		base.CacheSprite(SimulationSpace == ParticleSystemSimulationSpace.World);
		if (mainModule.maxParticles < particlesCacheCount)
		{
			mainModule.maxParticles = Mathf.CeilToInt(particlesCacheCount);
		}
		SilentEmissionEnded = false;
		hasSilentEmissionAlreadyBeenShot = false;
	}

	public void EmitSilently()
	{
		StartCoroutine(EmitParticlesSilently());
	}

	private IEnumerator EmitParticlesSilently()
	{
		hasSilentEmissionAlreadyBeenShot = false;
		SilentEmissionEnded = false;
		isPlaying = false;
		float time = Time.realtimeSinceStartup;
		float LastTimeSaved = Time.realtimeSinceStartup;
		float waitTimeMax = 1000f / WantedFPSDuringSilentEmission;
		particlesSystem.Clear();
		particlesSystem.Pause();
		Color[] colorCache = particleInitColorCache;
		Vector3[] posCache = particleInitPositionsCache;
		float pStartSize = particleStartSize;
		int length = particlesCacheCount;
		ParticleSystem ps = particlesSystem;
		for (int i = 0; i < length; i++)
		{
			if (i % 3 == 0)
			{
				LastTimeSaved = Time.realtimeSinceStartup;
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			if (UsePixelSourceColor)
			{
				emitParams.startColor = colorCache[i];
			}
			emitParams.startSize = pStartSize;
			emitParams.position = posCache[i];
			ps.Emit(emitParams, 1);
			if (LastTimeSaved - time > waitTimeMax)
			{
				particlesSystem.Pause();
				time = LastTimeSaved;
				yield return null;
			}
		}
		particlesSystem.Pause();
		SilentEmissionEnded = true;
		if (OnAvailableToPlay != null)
		{
			OnAvailableToPlay();
		}
	}

	public void SetHideSpriteOnPlay(bool hideOriginalSprite)
	{
		HideOriginalSpriteOnPlay = hideOriginalSprite;
	}

	private bool PlayOneShot()
	{
		if (HideOriginalSpriteOnPlay)
		{
			spriteRenderer.enabled = false;
		}
		if (!SilentEmissionEnded)
		{
			return false;
		}
		particlesSystem.Play();
		isPlaying = true;
		hasSilentEmissionAlreadyBeenShot = true;
		return true;
	}

	public override void Play()
	{
		if (!IsAvailableToPlay())
		{
			return;
		}
		if (!hasSilentEmissionAlreadyBeenShot)
		{
			if (!isPlaying)
			{
				PlayOneShot();
			}
		}
		else if (!isPlaying)
		{
			particlesSystem.Play();
			isPlaying = true;
		}
	}

	public override void Stop()
	{
		if (isPlaying)
		{
			particlesSystem.Pause();
		}
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

	public void Reset()
	{
		EmitSilently();
	}

	public override bool IsAvailableToPlay()
	{
		if (hasCachingEnded && !isPlaying)
		{
			return SilentEmissionEnded;
		}
		return false;
	}
}
