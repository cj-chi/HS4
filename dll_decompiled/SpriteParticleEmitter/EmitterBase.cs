using UnityEngine;

namespace SpriteParticleEmitter;

[SerializeField]
public abstract class EmitterBase : MonoBehaviour
{
	public enum BorderEmission
	{
		Off,
		Fast,
		Precise
	}

	public bool verboseDebug;

	[Header("References")]
	[Tooltip("If none is provided the script will look for one in this game object.")]
	public SpriteRenderer spriteRenderer;

	[Tooltip("If none is provided the script will look for one in this game object.")]
	public ParticleSystem particlesSystem;

	[Header("Emission Options")]
	[Tooltip("Start emitting as soon as able. (On static emission activating this will force CacheOnAwake)")]
	public bool PlayOnAwake = true;

	[Tooltip("Particles to emit per second")]
	public float EmissionRate = 1000f;

	[Tooltip("Should new particles override ParticleSystem's startColor and use the color in the pixel they're emitting from?")]
	public bool UsePixelSourceColor;

	public BorderEmission borderEmission;

	[Space(10f)]
	public bool UseEmissionFromColor;

	[Tooltip("Emission will take this color as only source position")]
	public Color EmitFromColor;

	[Range(0.01f, 1f)]
	[Tooltip("In conjunction with EmitFromColor. Defines how much can it deviate from red spectrum for selected color.")]
	public float RedTolerance = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("In conjunction with EmitFromColor. Defines how much can it deviate from green spectrum for selected color.")]
	public float GreenTolerance = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("In conjunction with EmitFromColor. Defines how much can it deviate from blue spectrum for selected color.")]
	public float BlueTolerance = 0.05f;

	protected ParticleSystemSimulationSpace SimulationSpace;

	protected bool isPlaying;

	protected ParticleSystem.MainModule mainModule;

	[Header("Advanced")]
	[Tooltip("This will save memory size when dealing with same sprite being loaded repeatedly by different GameObjects.")]
	public bool useSpritesSharingCache;

	public bool useBetweenFramesPrecision;

	protected Vector3 lastTransformPosition;

	protected float ParticlesToEmitThisFrame;

	public virtual event SimpleEvent OnCacheEnded;

	public virtual event SimpleEvent OnAvailableToPlay;

	protected virtual void Awake()
	{
		if (!spriteRenderer)
		{
			_ = verboseDebug;
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (!spriteRenderer)
			{
				_ = verboseDebug;
			}
		}
		if (!particlesSystem)
		{
			particlesSystem = GetComponent<ParticleSystem>();
			if (!particlesSystem)
			{
				_ = verboseDebug;
				return;
			}
		}
		mainModule = particlesSystem.main;
		mainModule.loop = false;
		mainModule.playOnAwake = false;
		particlesSystem.Stop();
		SimulationSpace = mainModule.simulationSpace;
	}

	public abstract void Play();

	public abstract void Pause();

	public abstract void Stop();

	public abstract bool IsPlaying();

	public abstract bool IsAvailableToPlay();

	private void DummyMethod()
	{
		if (this.OnAvailableToPlay != null)
		{
			this.OnAvailableToPlay();
		}
		if (this.OnCacheEnded != null)
		{
			this.OnCacheEnded();
		}
	}
}
