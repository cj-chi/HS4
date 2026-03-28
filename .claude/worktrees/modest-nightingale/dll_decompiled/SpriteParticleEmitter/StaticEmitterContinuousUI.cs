using UnityEngine;

namespace SpriteParticleEmitter;

public class StaticEmitterContinuousUI : StaticUIImageEmitter
{
	[Header("Emission")]
	[Tooltip("Particles to emit per second")]
	public float EmissionRate = 1000f;

	protected float ParticlesToEmitThisFrame;

	private RectTransform targetRectTransform;

	private RectTransform currentRectTransform;

	protected Vector2 offsetXY;

	protected float wMult = 100f;

	protected float hMult = 100f;

	public override event SimpleEvent OnAvailableToPlay;

	protected override void Awake()
	{
		base.Awake();
		currentRectTransform = GetComponent<RectTransform>();
		targetRectTransform = imageRenderer.GetComponent<RectTransform>();
	}

	protected override void Update()
	{
		base.Update();
		if (isPlaying && hasCachingEnded)
		{
			ProcessPositionAndScale();
			Emit();
		}
	}

	private void ProcessPositionAndScale()
	{
		if (matchImageRendererPostionData)
		{
			currentRectTransform.position = new Vector3(targetRectTransform.position.x, targetRectTransform.position.y, targetRectTransform.position.z);
		}
		currentRectTransform.pivot = targetRectTransform.pivot;
		if (matchImageRendererPostionData)
		{
			currentRectTransform.anchoredPosition = targetRectTransform.anchoredPosition;
			currentRectTransform.anchorMin = targetRectTransform.anchorMin;
			currentRectTransform.anchorMax = targetRectTransform.anchorMax;
			currentRectTransform.offsetMin = targetRectTransform.offsetMin;
			currentRectTransform.offsetMax = targetRectTransform.offsetMax;
		}
		if (matchImageRendererScale)
		{
			currentRectTransform.localScale = targetRectTransform.localScale;
		}
		currentRectTransform.rotation = targetRectTransform.rotation;
		currentRectTransform.sizeDelta = new Vector2(targetRectTransform.rect.width, targetRectTransform.rect.height);
		float x = (1f - currentRectTransform.pivot.x) * currentRectTransform.rect.width - currentRectTransform.rect.width / 2f;
		float y = (1f - currentRectTransform.pivot.y) * (0f - currentRectTransform.rect.height) + currentRectTransform.rect.height / 2f;
		offsetXY = new Vector2(x, y);
		Sprite sprite = imageRenderer.sprite;
		wMult = sprite.pixelsPerUnit * (currentRectTransform.rect.width / sprite.rect.size.x);
		hMult = sprite.pixelsPerUnit * (currentRectTransform.rect.height / sprite.rect.size.y);
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
		Vector3 position = currentRectTransform.position;
		Quaternion rotation = currentRectTransform.rotation;
		Vector3 localScale = currentRectTransform.localScale;
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
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array[num2];
			}
			emitParams.startSize = startSize;
			Vector3 vector = array2[num2];
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				zero.x = vector.x * wMult * localScale.x + offsetXY.x;
				zero.y = vector.y * hMult * localScale.y - offsetXY.y;
				emitParams.position = rotation * zero + position;
				particlesSystem.Emit(emitParams, 1);
			}
			else
			{
				zero.x = vector.x * wMult + offsetXY.x;
				zero.y = vector.y * hMult - offsetXY.y;
				emitParams.position = zero;
				particlesSystem.Emit(emitParams, 1);
			}
		}
		ParticlesToEmitThisFrame -= num;
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
