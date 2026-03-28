using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpriteToParticlesAsset;

[ExecuteInEditMode]
public class SpriteToParticles : MonoBehaviour
{
	public enum BorderEmission
	{
		Off,
		Fast,
		Precise
	}

	[Tooltip("Weather the system is being used for Sprite or Image component. ")]
	public RenderSystemUsing renderSystemType;

	[Tooltip("Weather the system is using static or dynamic mode.")]
	public SpriteMode mode = SpriteMode.Static;

	[Tooltip("Should log warnings and errors?")]
	public bool verboseDebug;

	[Tooltip("If none is provided the script will look for one in this game object.")]
	public SpriteRenderer spriteRenderer;

	[Tooltip("Must be provided by other GameObject's ImageRenderer.")]
	public Image imageRenderer;

	[Tooltip("If none is provided the script will look for one in this game object.")]
	public ParticleSystem particlesSystem;

	[Tooltip("Start emitting as soon as able. (On static emission activating this will force CacheOnAwake)")]
	public bool PlayOnAwake = true;

	[Tooltip("Particles to emit per second")]
	public float EmissionRate = 1000f;

	[Tooltip("Should new particles override ParticleSystem's startColor and use the color in the pixel they're emitting from?")]
	public bool UsePixelSourceColor;

	[Tooltip("Emit from sprite border. Fast will work on the x axis only. Precise works on both x and y axis but is more performance heavy. (Border emission only works in dynamic mode currently)")]
	public BorderEmission borderEmission;

	[Tooltip("Activating this will make the Emitter only emit from selected color")]
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

	[Tooltip("This will save memory size when dealing with same sprite being loaded repeatedly by different GameObjects.")]
	public bool useSpritesSharingPool;

	[Tooltip("Weather use BetweenFrames precision or not. (Refer to manual for further explanation)")]
	public bool useBetweenFramesPrecision;

	[Tooltip("Should the system cache sprites data? (Refer to manual for further explanation)")]
	public bool CacheSprites = true;

	[Tooltip("Should the transform match target Renderer GameObject Position? (For Image Component(UI) StP Object must have same parent as the Renderer Image component Transform)")]
	[FormerlySerializedAs("matchImageRendererPostionData")]
	public bool matchTargetGOPostionData;

	[Tooltip("Should the transform match target Renderer Renderer Scale? (For Image Component(UI) StP Object must have same parent as the Image component Transform. For Sprite Component it will match local scale data)")]
	[FormerlySerializedAs("matchImageRendererScale")]
	public bool matchTargetGOScale;

	private ParticleSystemSimulationSpace SimulationSpace;

	private bool isPlaying;

	public UIParticleRenderer uiParticleSystem;

	private ParticleSystem.MainModule mainModule;

	private float ParticlesToEmitThisFrame;

	private Vector3 lastTransformPosition;

	private Transform spriteTransformReference;

	private Color[] colorCache = new Color[1];

	private int[] indexCache = new int[1];

	private Dictionary<Sprite, Color[]> spritesSoFar = new Dictionary<Sprite, Color[]>();

	private RectTransform targetRectTransform;

	private RectTransform currentRectTransform;

	private Vector2 offsetXY;

	private float wMult = 100f;

	private float hMult = 100f;

	[Tooltip("Should the system cache on Awake method? - Static emission needs to be cached first, if this property is not checked the CacheSprite() method should be called by code. (Refer to manual for further explanation)")]
	public bool CacheOnAwake = true;

	private bool hasCachingEnded;

	private int particlesCacheCount;

	private float particleStartSize;

	private Vector3[] particleInitPositionsCache;

	private Color[] particleInitColorCache;

	private bool forceHack = true;

	public event SimpleEvent OnCacheEnded;

	public event SimpleEvent OnAvailableToPlay;

	protected virtual void Awake()
	{
		spritesSoFar = new Dictionary<Sprite, Color[]>();
		colorCache = new Color[1];
		indexCache = new int[1];
		particleInitPositionsCache = null;
		particleInitColorCache = null;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
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
			if ((bool)spriteRenderer)
			{
				spriteTransformReference = spriteRenderer.gameObject.transform;
				lastTransformPosition = spriteTransformReference.position;
			}
		}
		else
		{
			uiParticleSystem = GetComponent<UIParticleRenderer>();
			if (!uiParticleSystem)
			{
				_ = verboseDebug;
				isPlaying = false;
				return;
			}
			if (!imageRenderer)
			{
				_ = verboseDebug;
				isPlaying = false;
				return;
			}
		}
		if (!particlesSystem)
		{
			particlesSystem = GetComponent<ParticleSystem>();
			if (!particlesSystem)
			{
				_ = verboseDebug;
				isPlaying = false;
				return;
			}
		}
		mainModule = particlesSystem.main;
		mainModule.loop = true;
		mainModule.playOnAwake = true;
		particlesSystem.Stop();
		SimulationSpace = mainModule.simulationSpace;
		if (PlayOnAwake)
		{
			isPlaying = true;
			particlesSystem.Emit(1);
			particlesSystem.Clear();
			if (Application.isPlaying)
			{
				particlesSystem.Play();
			}
		}
		if (renderSystemType == RenderSystemUsing.ImageRenderer)
		{
			currentRectTransform = GetComponent<RectTransform>();
			targetRectTransform = imageRenderer.GetComponent<RectTransform>();
		}
		if ((float)mainModule.maxParticles < EmissionRate)
		{
			mainModule.maxParticles = Mathf.CeilToInt(EmissionRate);
		}
		if (mode == SpriteMode.Static)
		{
			if (PlayOnAwake)
			{
				CacheOnAwake = true;
			}
			if (CacheOnAwake)
			{
				CacheSprite();
			}
		}
	}

	public void Update()
	{
		bool flag = isPlaying;
		if (mode == SpriteMode.Static)
		{
			flag = isPlaying && hasCachingEnded;
		}
		if (!flag)
		{
			return;
		}
		if (renderSystemType == RenderSystemUsing.ImageRenderer)
		{
			HandlePositionAndScaleForUI();
			if (!isPlaying)
			{
				return;
			}
		}
		else
		{
			HandlePosition();
		}
		ParticlesToEmitThisFrame += EmissionRate * Time.deltaTime;
		int num = (int)ParticlesToEmitThisFrame;
		if (num > 0)
		{
			Emit(num);
		}
		ParticlesToEmitThisFrame -= num;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			if (!spriteTransformReference)
			{
				spriteTransformReference = spriteRenderer.transform;
			}
			lastTransformPosition = spriteTransformReference.position;
		}
	}

	public void Emit(int emitCount)
	{
		HackUnityCrash2017();
		if (mode == SpriteMode.Dynamic)
		{
			EmitDynamic(emitCount);
		}
		else
		{
			EmitStatic(emitCount);
		}
	}

	public void EmitAll(bool hideSprite = true)
	{
		if (hideSprite)
		{
			if (renderSystemType == RenderSystemUsing.SpriteRenderer)
			{
				spriteRenderer.enabled = false;
			}
			else
			{
				imageRenderer.enabled = false;
			}
		}
		if (mode == SpriteMode.Dynamic)
		{
			EmitAllDynamic();
		}
		else
		{
			EmitAllStatic();
		}
	}

	public void RestoreSprite()
	{
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			spriteRenderer.enabled = true;
		}
	}

	public void Play()
	{
		if (!isPlaying)
		{
			particlesSystem.Play();
		}
		isPlaying = true;
	}

	public void Pause()
	{
		if (isPlaying)
		{
			particlesSystem.Pause();
		}
		isPlaying = false;
	}

	public void Stop()
	{
		isPlaying = false;
	}

	public bool IsPlaying()
	{
		return isPlaying;
	}

	public bool IsAvailableToPlay()
	{
		if (mode == SpriteMode.Static)
		{
			return hasCachingEnded;
		}
		return true;
	}

	public void ClearCachedSprites()
	{
		spritesSoFar = new Dictionary<Sprite, Color[]>();
	}

	private void HandlePositionAndScaleForUI()
	{
		if (mode == SpriteMode.Dynamic)
		{
			ProcessPositionAndScaleDynamic();
		}
		else
		{
			ProcessPositionAndScaleStatic();
		}
	}

	private void HandlePosition()
	{
		if (matchTargetGOPostionData && spriteRenderer != null)
		{
			base.transform.position = spriteRenderer.transform.position;
		}
		if (matchTargetGOScale && spriteRenderer != null)
		{
			base.transform.localScale = spriteRenderer.transform.lossyScale;
		}
	}

	private void ProcessPositionAndScaleDynamic()
	{
		if (imageRenderer == null)
		{
			_ = verboseDebug;
			isPlaying = false;
			return;
		}
		if (matchTargetGOPostionData)
		{
			currentRectTransform.position = new Vector3(targetRectTransform.position.x, targetRectTransform.position.y, targetRectTransform.position.z);
		}
		currentRectTransform.pivot = targetRectTransform.pivot;
		if (matchTargetGOPostionData)
		{
			currentRectTransform.anchoredPosition = targetRectTransform.anchoredPosition;
			currentRectTransform.anchorMin = targetRectTransform.anchorMin;
			currentRectTransform.anchorMax = targetRectTransform.anchorMax;
			currentRectTransform.offsetMin = targetRectTransform.offsetMin;
			currentRectTransform.offsetMax = targetRectTransform.offsetMax;
		}
		if (matchTargetGOScale)
		{
			currentRectTransform.localScale = targetRectTransform.localScale;
		}
		currentRectTransform.rotation = targetRectTransform.rotation;
		currentRectTransform.sizeDelta = new Vector2(targetRectTransform.rect.width, targetRectTransform.rect.height);
		float x = (1f - targetRectTransform.pivot.x) * targetRectTransform.rect.width - targetRectTransform.rect.width / 2f;
		float y = (1f - targetRectTransform.pivot.y) * (0f - targetRectTransform.rect.height) + targetRectTransform.rect.height / 2f;
		offsetXY = new Vector2(x, y);
		Sprite sprite = GetSprite();
		if (imageRenderer.preserveAspect)
		{
			float num = sprite.rect.size.y / sprite.rect.size.x;
			float num2 = targetRectTransform.rect.height / targetRectTransform.rect.width;
			if (num > num2)
			{
				wMult = sprite.pixelsPerUnit * (targetRectTransform.rect.width / sprite.rect.size.x) * (num2 / num);
				hMult = sprite.pixelsPerUnit * (targetRectTransform.rect.height / sprite.rect.size.y);
			}
			else
			{
				wMult = sprite.pixelsPerUnit * (targetRectTransform.rect.width / sprite.rect.size.x);
				hMult = sprite.pixelsPerUnit * (targetRectTransform.rect.height / sprite.rect.size.y) * (num / num2);
			}
		}
		else
		{
			wMult = sprite.pixelsPerUnit * (targetRectTransform.rect.width / sprite.rect.size.x);
			hMult = sprite.pixelsPerUnit * (targetRectTransform.rect.height / sprite.rect.size.y);
		}
	}

	private void EmitDynamic(int emitCount)
	{
		Sprite sprite = GetSprite();
		if (!sprite)
		{
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		float startSize = GetParticleStartSize(pixelsPerUnit);
		float num3 = sprite.pivot.x / pixelsPerUnit;
		float num4 = sprite.pivot.y / pixelsPerUnit;
		Color[] spriteColorsData = GetSpriteColorsData(sprite);
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num5 = num * num2;
		Color[] array = colorCache;
		int[] array2 = indexCache;
		if ((float)array.Length < num5)
		{
			colorCache = new Color[(int)num5];
			indexCache = new int[(int)num5];
			array = colorCache;
			array2 = indexCache;
		}
		int num6 = (int)num;
		float num7 = 1f / pixelsPerUnit / 2f;
		Vector3 vector = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		Vector3 vector2 = Vector3.one;
		bool flag = false;
		bool flag2 = false;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			if (SimulationSpace != ParticleSystemSimulationSpace.Local)
			{
				vector = spriteTransformReference.position;
				quaternion = spriteTransformReference.rotation;
				vector2 = spriteTransformReference.lossyScale;
			}
			flag = spriteRenderer.flipX;
			flag2 = spriteRenderer.flipY;
		}
		int num8 = 0;
		bool useEmissionFromColor = UseEmissionFromColor;
		bool flag3 = borderEmission == BorderEmission.Fast || borderEmission == BorderEmission.Precise;
		if (flag3)
		{
			bool flag4 = false;
			Color color = spriteColorsData[0];
			int num9 = (int)num;
			bool flag5 = borderEmission == BorderEmission.Precise;
			for (int i = 0; (float)i < num5; i++)
			{
				Color color2 = spriteColorsData[i];
				bool flag6 = color2.a > 0f;
				if (flag5)
				{
					int num10 = i - num9;
					if (num10 > 0)
					{
						Color color3 = spriteColorsData[num10];
						bool flag7 = color3.a > 0f;
						if (flag6)
						{
							if (!flag7)
							{
								if (!useEmissionFromColor || (FloatComparer.AreEqual(r, color2.r, redTolerance) && FloatComparer.AreEqual(g, color2.g, greenTolerance) && FloatComparer.AreEqual(b, color2.b, blueTolerance)))
								{
									array[num8] = color2;
									array2[num8] = i;
									num8++;
									color = color2;
									flag4 = true;
								}
								continue;
							}
						}
						else if (flag7)
						{
							if (useEmissionFromColor && (!FloatComparer.AreEqual(r, color3.r, redTolerance) || !FloatComparer.AreEqual(g, color3.g, greenTolerance) || !FloatComparer.AreEqual(b, color3.b, blueTolerance)))
							{
								continue;
							}
							array[num8] = color3;
							array2[num8] = num10;
							num8++;
						}
					}
				}
				if (!flag6 && flag4)
				{
					if (useEmissionFromColor && (!FloatComparer.AreEqual(r, color.r, redTolerance) || !FloatComparer.AreEqual(g, color.g, greenTolerance) || !FloatComparer.AreEqual(b, color.b, blueTolerance)))
					{
						continue;
					}
					array[num8] = color;
					array2[num8] = i - 1;
					num8++;
					flag4 = true;
				}
				color = color2;
				if (!flag6)
				{
					flag4 = false;
				}
				else if ((!flag3 || (flag6 && !flag4)) && (!useEmissionFromColor || (FloatComparer.AreEqual(r, color2.r, redTolerance) && FloatComparer.AreEqual(g, color2.g, greenTolerance) && FloatComparer.AreEqual(b, color2.b, blueTolerance))))
				{
					array[num8] = color2;
					array2[num8] = i;
					num8++;
					flag4 = true;
				}
			}
		}
		else
		{
			for (int j = 0; (float)j < num5; j++)
			{
				Color color4 = spriteColorsData[j];
				if (!(color4.a <= 0f) && (!useEmissionFromColor || (FloatComparer.AreEqual(r, color4.r, redTolerance) && FloatComparer.AreEqual(g, color4.g, greenTolerance) && FloatComparer.AreEqual(b, color4.b, blueTolerance))))
				{
					array[num8] = color4;
					array2[num8] = j;
					num8++;
				}
			}
		}
		if (num8 <= 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 vector3 = vector;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			for (int k = 0; k < emitCount; k++)
			{
				int num11 = UnityEngine.Random.Range(0, num8 - 1);
				int num12 = array2[num11];
				if (useBetweenFramesPrecision)
				{
					vector3 = Vector3.Lerp(t: UnityEngine.Random.Range(0f, 1f), a: lastTransformPosition, b: vector);
				}
				float num13 = (float)num12 % num / pixelsPerUnit - num3 + num7;
				float num14 = (float)(num12 / num6) / pixelsPerUnit - num4 + num7;
				if (flag)
				{
					num13 = num / pixelsPerUnit - num13 - num3 * 2f;
				}
				if (flag2)
				{
					num14 = num2 / pixelsPerUnit - num14 - num4 * 2f;
				}
				zero.x = num13 * vector2.x;
				zero.y = num14 * vector2.y;
				ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
				{
					position = quaternion * zero + vector3
				};
				if (UsePixelSourceColor)
				{
					emitParams.startColor = array[num11];
				}
				emitParams.startSize = startSize;
				particlesSystem.Emit(emitParams, 1);
			}
			return;
		}
		for (int l = 0; l < emitCount; l++)
		{
			int num15 = UnityEngine.Random.Range(0, num8 - 1);
			int num16 = array2[num15];
			float num17 = (float)num16 % num / pixelsPerUnit - num3 + num7;
			float num18 = (float)(num16 / num6) / pixelsPerUnit - num4 + num7;
			ParticleSystem.EmitParams emitParams2 = default(ParticleSystem.EmitParams);
			zero.x = num17 * wMult + offsetXY.x;
			zero.y = num18 * hMult - offsetXY.y;
			emitParams2.position = zero;
			if (UsePixelSourceColor)
			{
				emitParams2.startColor = array[num15];
			}
			emitParams2.startSize = startSize;
			particlesSystem.Emit(emitParams2, 1);
		}
	}

	private void EmitAllDynamic()
	{
		Sprite sprite = GetSprite();
		if (!sprite)
		{
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		float startSize = GetParticleStartSize(pixelsPerUnit);
		float num3 = sprite.pivot.x / pixelsPerUnit;
		float num4 = sprite.pivot.y / pixelsPerUnit;
		Color[] spriteColorsData = GetSpriteColorsData(sprite);
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num5 = num * num2;
		int num6 = (int)num;
		float num7 = 1f / pixelsPerUnit / 2f;
		Vector3 vector = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		Vector3 vector2 = Vector3.one;
		bool flag = false;
		bool flag2 = false;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer && SimulationSpace != ParticleSystemSimulationSpace.Local)
		{
			vector = spriteTransformReference.position;
			quaternion = spriteTransformReference.rotation;
			vector2 = spriteTransformReference.lossyScale;
			flag = spriteRenderer.flipX;
			flag2 = spriteRenderer.flipY;
		}
		Vector3 zero = Vector3.zero;
		Vector3 vector3 = vector;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			for (int i = 0; (float)i < num5; i++)
			{
				Color color = spriteColorsData[i];
				if (!(color.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, color.r, redTolerance) && FloatComparer.AreEqual(g, color.g, greenTolerance) && FloatComparer.AreEqual(b, color.b, blueTolerance))))
				{
					float num8 = (float)i % num / pixelsPerUnit - num3;
					float num9 = (float)(i / num6) / pixelsPerUnit - num4;
					if (flag)
					{
						num8 = num / pixelsPerUnit - num8 - num3 * 2f;
					}
					if (flag2)
					{
						num9 = num2 / pixelsPerUnit - num9 - num4 * 2f;
					}
					zero.x = num8 * vector2.x + num7;
					zero.y = num9 * vector2.y + num7;
					ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
					{
						position = quaternion * zero + vector3
					};
					if (UsePixelSourceColor)
					{
						emitParams.startColor = color;
					}
					emitParams.startSize = startSize;
					particlesSystem.Emit(emitParams, 1);
				}
			}
			return;
		}
		for (int j = 0; (float)j < num5; j++)
		{
			Color color2 = spriteColorsData[j];
			if (!(color2.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, color2.r, redTolerance) && FloatComparer.AreEqual(g, color2.g, greenTolerance) && FloatComparer.AreEqual(b, color2.b, blueTolerance))))
			{
				float num10 = (float)j % num / pixelsPerUnit - num3;
				float num11 = (float)(j / num6) / pixelsPerUnit - num4;
				ParticleSystem.EmitParams emitParams2 = default(ParticleSystem.EmitParams);
				zero.x = num10 * wMult + offsetXY.x + num7;
				zero.y = num11 * hMult - offsetXY.y + num7;
				emitParams2.position = zero;
				if (UsePixelSourceColor)
				{
					emitParams2.startColor = color2;
				}
				emitParams2.startSize = startSize;
				particlesSystem.Emit(emitParams2, 1);
			}
		}
	}

	public void CacheSprite(bool relativeToParent = false)
	{
		if (!particlesSystem)
		{
			return;
		}
		hasCachingEnded = false;
		particlesCacheCount = 0;
		Sprite sprite = GetSprite();
		if (!sprite)
		{
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		int num3 = (int)num;
		float num4 = 1f / pixelsPerUnit / 2f;
		particleStartSize = GetParticleStartSize(pixelsPerUnit);
		float num5 = sprite.pivot.x / pixelsPerUnit;
		float num6 = sprite.pivot.y / pixelsPerUnit;
		Color[] spriteColorsData = GetSpriteColorsData(sprite);
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num7 = num * num2;
		Vector3 vector = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		Vector3 vector2 = Vector3.one;
		bool flag = false;
		bool flag2 = false;
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			vector = spriteTransformReference.position;
			quaternion = spriteTransformReference.rotation;
			vector2 = spriteTransformReference.lossyScale;
			flag = spriteRenderer.flipX;
			flag2 = spriteRenderer.flipY;
		}
		List<Color> list = new List<Color>();
		List<Vector3> list2 = new List<Vector3>();
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			for (int i = 0; (float)i < num7; i++)
			{
				Color item = spriteColorsData[i];
				if (!(item.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, item.r, redTolerance) && FloatComparer.AreEqual(g, item.g, greenTolerance) && FloatComparer.AreEqual(b, item.b, blueTolerance))))
				{
					float num8 = (float)i % num / pixelsPerUnit - num5 + num4;
					float num9 = (float)(i / num3) / pixelsPerUnit - num6 + num4;
					if (flag)
					{
						num8 = num / pixelsPerUnit - num8 - num5 * 2f;
					}
					if (flag2)
					{
						num9 = num2 / pixelsPerUnit - num9 - num6 * 2f;
					}
					Vector3 item2 = ((!relativeToParent) ? new Vector3(num8, num9, 0f) : (quaternion * new Vector3(num8 * vector2.x, num9 * vector2.y, 0f) + vector));
					list2.Add(item2);
					list.Add(item);
					particlesCacheCount++;
				}
			}
		}
		else
		{
			for (int j = 0; (float)j < num7; j++)
			{
				Color item3 = spriteColorsData[j];
				if (!(item3.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, item3.r, redTolerance) && FloatComparer.AreEqual(g, item3.g, greenTolerance) && FloatComparer.AreEqual(b, item3.b, blueTolerance))))
				{
					float x = (float)j % num / pixelsPerUnit - num5 + num4;
					float y = (float)(j / num3) / pixelsPerUnit - num6 + num4;
					Vector3 item4 = new Vector3(x, y, 0f);
					list2.Add(item4);
					list.Add(item3);
					particlesCacheCount++;
				}
			}
		}
		particleInitPositionsCache = list2.ToArray();
		particleInitColorCache = list.ToArray();
		if (particlesCacheCount <= 0)
		{
			_ = verboseDebug;
			return;
		}
		spriteColorsData = null;
		list2.Clear();
		list2 = null;
		list.Clear();
		list = null;
		GC.Collect();
		hasCachingEnded = true;
		if (this.OnCacheEnded != null)
		{
			this.OnCacheEnded();
		}
		if (this.OnAvailableToPlay != null)
		{
			this.OnAvailableToPlay();
		}
	}

	private void ProcessPositionAndScaleStatic()
	{
		if (matchTargetGOPostionData)
		{
			currentRectTransform.position = new Vector3(targetRectTransform.position.x, targetRectTransform.position.y, targetRectTransform.position.z);
		}
		currentRectTransform.pivot = targetRectTransform.pivot;
		if (matchTargetGOPostionData)
		{
			currentRectTransform.anchoredPosition = targetRectTransform.anchoredPosition;
			currentRectTransform.anchorMin = targetRectTransform.anchorMin;
			currentRectTransform.anchorMax = targetRectTransform.anchorMax;
			currentRectTransform.offsetMin = targetRectTransform.offsetMin;
			currentRectTransform.offsetMax = targetRectTransform.offsetMax;
		}
		if (matchTargetGOScale)
		{
			currentRectTransform.localScale = targetRectTransform.localScale;
		}
		currentRectTransform.rotation = targetRectTransform.rotation;
		currentRectTransform.sizeDelta = new Vector2(targetRectTransform.rect.width, targetRectTransform.rect.height);
		float x = (1f - currentRectTransform.pivot.x) * currentRectTransform.rect.width - currentRectTransform.rect.width / 2f;
		float y = (1f - currentRectTransform.pivot.y) * (0f - currentRectTransform.rect.height) + currentRectTransform.rect.height / 2f;
		offsetXY = new Vector2(x, y);
		Sprite sprite = GetSprite();
		if ((bool)sprite)
		{
			wMult = sprite.pixelsPerUnit * (currentRectTransform.rect.width / sprite.rect.size.x);
			hMult = sprite.pixelsPerUnit * (currentRectTransform.rect.height / sprite.rect.size.y);
		}
	}

	private void EmitStatic(int emitCount)
	{
		if (!hasCachingEnded)
		{
			return;
		}
		int max = particlesCacheCount;
		float startSize = particleStartSize;
		bool flag = renderSystemType == RenderSystemUsing.SpriteRenderer;
		if (particlesCacheCount <= 0)
		{
			return;
		}
		Vector3 position;
		Quaternion rotation;
		Vector3 lossyScale;
		if (flag)
		{
			position = spriteTransformReference.position;
			rotation = spriteTransformReference.rotation;
			lossyScale = spriteTransformReference.lossyScale;
		}
		else
		{
			position = currentRectTransform.position;
			rotation = currentRectTransform.rotation;
			lossyScale = currentRectTransform.lossyScale;
		}
		Vector3 vector = position;
		ParticleSystemSimulationSpace simulationSpace = SimulationSpace;
		Color[] array = particleInitColorCache;
		Vector3[] array2 = particleInitPositionsCache;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < emitCount; i++)
		{
			int num = UnityEngine.Random.Range(0, max);
			if (useBetweenFramesPrecision)
			{
				float t = UnityEngine.Random.Range(0f, 1f);
				vector = Vector3.Lerp(lastTransformPosition, position, t);
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array[num];
			}
			emitParams.startSize = startSize;
			Vector3 vector2 = array2[num];
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				if (flag)
				{
					zero.x = vector2.x * lossyScale.x;
					zero.y = vector2.y * lossyScale.y;
				}
				else
				{
					zero.x = vector2.x * wMult * lossyScale.x + offsetXY.x;
					zero.y = vector2.y * hMult * lossyScale.y - offsetXY.y;
				}
				emitParams.position = rotation * zero + vector;
				particlesSystem.Emit(emitParams, 1);
			}
			else
			{
				if (flag)
				{
					emitParams.position = array2[num];
				}
				else
				{
					zero.x = vector2.x * wMult + offsetXY.x;
					zero.y = vector2.y * hMult - offsetXY.y;
					emitParams.position = zero;
				}
				particlesSystem.Emit(emitParams, 1);
			}
		}
	}

	private void EmitAllStatic()
	{
		if (!hasCachingEnded)
		{
			return;
		}
		int num = particlesCacheCount;
		float startSize = particleStartSize;
		bool flag = renderSystemType == RenderSystemUsing.SpriteRenderer;
		if (particlesCacheCount <= 0)
		{
			return;
		}
		Vector3 position;
		Quaternion rotation;
		Vector3 lossyScale;
		if (flag)
		{
			position = spriteTransformReference.position;
			rotation = spriteTransformReference.rotation;
			lossyScale = spriteTransformReference.lossyScale;
		}
		else
		{
			position = currentRectTransform.position;
			rotation = currentRectTransform.rotation;
			lossyScale = currentRectTransform.lossyScale;
		}
		Vector3 vector = position;
		ParticleSystemSimulationSpace simulationSpace = SimulationSpace;
		Color[] array = particleInitColorCache;
		Vector3[] array2 = particleInitPositionsCache;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < num; i++)
		{
			if (useBetweenFramesPrecision)
			{
				float t = UnityEngine.Random.Range(0f, 1f);
				vector = Vector3.Lerp(lastTransformPosition, position, t);
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array[i];
			}
			emitParams.startSize = startSize;
			Vector3 vector2 = array2[i];
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				if (flag)
				{
					zero.x = vector2.x * lossyScale.x;
					zero.y = vector2.y * lossyScale.y;
				}
				else
				{
					zero.x = vector2.x * wMult * lossyScale.x + offsetXY.x;
					zero.y = vector2.y * hMult * lossyScale.y - offsetXY.y;
				}
				emitParams.position = rotation * zero + vector;
				particlesSystem.Emit(emitParams, 1);
			}
			else
			{
				if (flag)
				{
					emitParams.position = array2[i];
				}
				else
				{
					zero.x = vector2.x * wMult + offsetXY.x;
					zero.y = vector2.y * hMult - offsetXY.y;
					emitParams.position = zero;
				}
				particlesSystem.Emit(emitParams, 1);
			}
		}
	}

	public Sprite GetSprite()
	{
		Sprite sprite;
		if (renderSystemType == RenderSystemUsing.ImageRenderer)
		{
			if (!imageRenderer)
			{
				_ = verboseDebug;
				return null;
			}
			sprite = imageRenderer.sprite;
			if ((bool)imageRenderer.overrideSprite)
			{
				sprite = imageRenderer.overrideSprite;
			}
		}
		else
		{
			if (!spriteRenderer)
			{
				_ = verboseDebug;
				return null;
			}
			sprite = spriteRenderer.sprite;
		}
		if (!sprite)
		{
			_ = verboseDebug;
			isPlaying = false;
			return null;
		}
		return sprite;
	}

	private float GetParticleStartSize(float PixelsPerUnit)
	{
		if (renderSystemType == RenderSystemUsing.SpriteRenderer)
		{
			float num = 1f / PixelsPerUnit;
			return num * mainModule.startSize.constant;
		}
		return mainModule.startSize.constant;
	}

	private Color[] GetSpriteColorsData(Sprite sprite)
	{
		Rect rect = sprite.rect;
		Color[] array;
		if (useSpritesSharingPool && Application.isPlaying)
		{
			array = SpritesDataPool.GetSpriteColors(sprite, (int)rect.position.x, (int)rect.position.y, (int)rect.size.x, (int)rect.size.y);
		}
		else if (CacheSprites && mode == SpriteMode.Dynamic)
		{
			if (spritesSoFar.ContainsKey(sprite))
			{
				array = spritesSoFar[sprite];
			}
			else
			{
				array = sprite.texture.GetPixels((int)rect.position.x, (int)rect.position.y, (int)rect.size.x, (int)rect.size.y);
				spritesSoFar.Add(sprite, array);
			}
		}
		else
		{
			array = sprite.texture.GetPixels((int)rect.position.x, (int)rect.position.y, (int)rect.size.x, (int)rect.size.y);
		}
		return array;
	}

	private void HackUnityCrash2017()
	{
		if (forceHack && !particlesSystem.isStopped)
		{
			particlesSystem.Emit(1);
			particlesSystem.Clear();
			forceHack = false;
		}
	}

	private void ForceNextUseOfHack()
	{
		forceHack = true;
	}
}
