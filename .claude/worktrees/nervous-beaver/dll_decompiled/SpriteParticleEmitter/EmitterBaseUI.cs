using SpriteToParticlesAsset;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteParticleEmitter;

[SerializeField]
public abstract class EmitterBaseUI : MonoBehaviour
{
	public bool verboseDebug;

	[Header("References")]
	[Tooltip("Must be provided by other GameObject's ImageRenderer.")]
	public Image imageRenderer;

	[Tooltip("If none is provided the script will look for one in this game object.")]
	public ParticleSystem particlesSystem;

	[Header("Color Emission Options")]
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

	[Tooltip("Should new particles override ParticleSystem's startColor and use the color in the pixel they're emitting from?")]
	public bool UsePixelSourceColor;

	[Tooltip("Must match Particle System's same option")]
	protected ParticleSystemSimulationSpace SimulationSpace;

	protected bool isPlaying;

	protected UIParticleRenderer uiParticleSystem;

	protected ParticleSystem.MainModule mainModule;

	[Tooltip("Should the transform match target Image Renderer Position?")]
	public bool matchImageRendererPostionData = true;

	[Tooltip("Should the transform match target Image Renderer Scale?")]
	public bool matchImageRendererScale = true;

	[Header("Advanced")]
	[Tooltip("This will save memory size when dealing with same sprite being loaded repeatedly by different GameObjects.")]
	public bool useSpritesSharingCache;

	public virtual event SimpleEvent OnCacheEnded;

	public virtual event SimpleEvent OnAvailableToPlay;

	protected virtual void Awake()
	{
		uiParticleSystem = GetComponent<UIParticleRenderer>();
		if (!imageRenderer)
		{
			_ = verboseDebug;
			isPlaying = false;
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
