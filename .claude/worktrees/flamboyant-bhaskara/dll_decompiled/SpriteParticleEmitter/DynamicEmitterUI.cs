using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace SpriteParticleEmitter;

[ExecuteInEditMode]
[RequireComponent(typeof(UIParticleRenderer))]
public class DynamicEmitterUI : EmitterBaseUI
{
	[Tooltip("Start emitting as soon as able")]
	public bool PlayOnAwake = true;

	[Header("Emission")]
	[Tooltip("Particles to emit per second")]
	public float EmissionRate = 1000f;

	protected float ParticlesToEmitThisFrame;

	[Tooltip("Should the system cache sprites data? (Refer to manual for further explanation)")]
	public bool CacheSprites = true;

	private Color[] colorCache = new Color[1];

	private int[] indexCache = new int[1];

	protected Dictionary<Sprite, Color[]> spritesSoFar = new Dictionary<Sprite, Color[]>();

	private RectTransform targetRectTransform;

	private RectTransform currentRectTransform;

	protected Vector2 offsetXY;

	protected float wMult = 100f;

	protected float hMult = 100f;

	protected override void Awake()
	{
		base.Awake();
		if (PlayOnAwake)
		{
			isPlaying = true;
		}
		currentRectTransform = GetComponent<RectTransform>();
		targetRectTransform = imageRenderer.GetComponent<RectTransform>();
		if ((float)mainModule.maxParticles < EmissionRate)
		{
			mainModule.maxParticles = Mathf.CeilToInt(EmissionRate);
		}
	}

	protected void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		if (imageRenderer == null)
		{
			_ = verboseDebug;
			isPlaying = false;
			return;
		}
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
		float x = (1f - targetRectTransform.pivot.x) * targetRectTransform.rect.width - targetRectTransform.rect.width / 2f;
		float y = (1f - targetRectTransform.pivot.y) * (0f - targetRectTransform.rect.height) + targetRectTransform.rect.height / 2f;
		offsetXY = new Vector2(x, y);
		Sprite sprite = imageRenderer.sprite;
		if (!sprite)
		{
			_ = verboseDebug;
			return;
		}
		wMult = sprite.pixelsPerUnit * (targetRectTransform.rect.width / sprite.rect.size.x);
		hMult = sprite.pixelsPerUnit * (targetRectTransform.rect.height / sprite.rect.size.y);
		ParticlesToEmitThisFrame += EmissionRate * Time.deltaTime;
		int num = (int)ParticlesToEmitThisFrame;
		if (num > 0)
		{
			Emit(num);
		}
		ParticlesToEmitThisFrame -= num;
	}

	public void Emit(int emitCount)
	{
		Sprite sprite = imageRenderer.sprite;
		if ((bool)imageRenderer.overrideSprite)
		{
			sprite = imageRenderer.overrideSprite;
		}
		if (!sprite)
		{
			_ = verboseDebug;
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		ParticleSystem.MinMaxCurve startSize = mainModule.startSize;
		float num3 = sprite.pivot.x / pixelsPerUnit;
		float num4 = sprite.pivot.y / pixelsPerUnit;
		Color[] array;
		if (useSpritesSharingCache && Application.isPlaying)
		{
			array = SpritesDataPool.GetSpriteColors(sprite, (int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
		}
		else if (CacheSprites)
		{
			if (spritesSoFar.ContainsKey(sprite))
			{
				array = spritesSoFar[sprite];
			}
			else
			{
				array = sprite.texture.GetPixels((int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
				spritesSoFar.Add(sprite, array);
			}
		}
		else
		{
			array = sprite.texture.GetPixels((int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
		}
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num5 = num * num2;
		Color[] array2 = colorCache;
		int[] array3 = indexCache;
		if ((float)array2.Length < num5)
		{
			colorCache = new Color[(int)num5];
			indexCache = new int[(int)num5];
			array2 = colorCache;
			array3 = indexCache;
		}
		int num6 = 0;
		for (int i = 0; (float)i < num5; i++)
		{
			Color color = array[i];
			if (color.a > 0f && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, color.r, redTolerance) && FloatComparer.AreEqual(g, color.g, greenTolerance) && FloatComparer.AreEqual(b, color.b, blueTolerance))))
			{
				array2[num6] = color;
				array3[num6] = i;
				num6++;
			}
		}
		if (num6 <= 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < emitCount; j++)
		{
			int num7 = Random.Range(0, num6 - 1);
			int num8 = array3[num7];
			float num9 = (float)num8 % num / pixelsPerUnit - num3;
			float num10 = (float)num8 / num / pixelsPerUnit - num4;
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			zero.x = num9 * wMult + offsetXY.x;
			zero.y = num10 * hMult - offsetXY.y;
			emitParams.position = zero;
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array2[num7];
			}
			emitParams.startSize = startSize.constant;
			particlesSystem.Emit(emitParams, 1);
		}
	}

	public void EmitAll(bool hideSprite = true)
	{
		if (hideSprite)
		{
			imageRenderer.enabled = false;
		}
		Sprite sprite = imageRenderer.sprite;
		if (!sprite)
		{
			_ = verboseDebug;
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		float constant = mainModule.startSize.constant;
		float num3 = sprite.pivot.x / pixelsPerUnit;
		float num4 = sprite.pivot.y / pixelsPerUnit;
		Color[] array;
		if (useSpritesSharingCache && Application.isPlaying)
		{
			array = SpritesDataPool.GetSpriteColors(sprite, (int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
		}
		else if (CacheSprites)
		{
			if (spritesSoFar.ContainsKey(sprite))
			{
				array = spritesSoFar[sprite];
			}
			else
			{
				array = sprite.texture.GetPixels((int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
				spritesSoFar.Add(sprite, array);
			}
		}
		else
		{
			array = sprite.texture.GetPixels((int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2);
		}
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num5 = num * num2;
		Vector3 zero = Vector3.zero;
		for (int i = 0; (float)i < num5; i++)
		{
			Color color = array[i];
			if (!(color.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, color.r, redTolerance) && FloatComparer.AreEqual(g, color.g, greenTolerance) && FloatComparer.AreEqual(b, color.b, blueTolerance))))
			{
				float num6 = (float)i % num / pixelsPerUnit - num3;
				float num7 = (float)i / num / pixelsPerUnit - num4;
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				zero.x = num6 * wMult + offsetXY.x;
				zero.y = num7 * hMult - offsetXY.y;
				emitParams.position = zero;
				if (UsePixelSourceColor)
				{
					emitParams.startColor = color;
				}
				emitParams.startSize = constant;
				particlesSystem.Emit(emitParams, 1);
			}
		}
	}

	public void RestoreSprite()
	{
		if ((bool)imageRenderer)
		{
			imageRenderer.enabled = true;
		}
	}

	public override void Play()
	{
		if (!isPlaying && (bool)particlesSystem)
		{
			particlesSystem.Play();
		}
		isPlaying = true;
	}

	public override void Pause()
	{
		if (isPlaying && (bool)particlesSystem)
		{
			particlesSystem.Pause();
		}
		isPlaying = false;
	}

	public override void Stop()
	{
		isPlaying = false;
	}

	public override bool IsPlaying()
	{
		return isPlaying;
	}

	public override bool IsAvailableToPlay()
	{
		return true;
	}

	public void ClearCachedSprites()
	{
		spritesSoFar = new Dictionary<Sprite, Color[]>();
	}
}
