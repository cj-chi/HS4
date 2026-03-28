using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace SpriteParticleEmitter;

[ExecuteInEditMode]
public class DynamicEmitter : EmitterBase
{
	[Tooltip("Should the system cache sprites data? (Refer to manual for further explanation)")]
	public bool CacheSprites = true;

	private Color[] colorCache = new Color[1];

	private int[] indexCache = new int[1];

	protected Dictionary<Sprite, Color[]> spritesSoFar = new Dictionary<Sprite, Color[]>();

	protected override void Awake()
	{
		base.Awake();
		if (PlayOnAwake)
		{
			isPlaying = true;
		}
		if ((float)mainModule.maxParticles < EmissionRate)
		{
			mainModule.maxParticles = Mathf.CeilToInt(EmissionRate);
		}
	}

	protected void Update()
	{
		if (isPlaying)
		{
			ParticlesToEmitThisFrame += EmissionRate * Time.deltaTime;
			int num = (int)ParticlesToEmitThisFrame;
			if (num > 0)
			{
				Emit(num);
			}
			ParticlesToEmitThisFrame -= num;
		}
	}

	public void Emit(int emitCount)
	{
		Sprite sprite = spriteRenderer.sprite;
		if (!sprite)
		{
			_ = verboseDebug;
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		Vector3 vector = spriteRenderer.gameObject.transform.position;
		Quaternion quaternion = spriteRenderer.gameObject.transform.rotation;
		Vector3 vector2 = spriteRenderer.gameObject.transform.lossyScale;
		if (SimulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = Vector3.zero;
			vector2 = Vector3.one;
			quaternion = Quaternion.identity;
		}
		bool flipX = spriteRenderer.flipX;
		bool flipY = spriteRenderer.flipY;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		int num3 = (int)num;
		float num4 = 1f / pixelsPerUnit / 2f;
		float num5 = 1f / pixelsPerUnit;
		num5 *= mainModule.startSize.constant;
		float num6 = sprite.pivot.x / pixelsPerUnit;
		float num7 = sprite.pivot.y / pixelsPerUnit;
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
		float num8 = num * num2;
		Color[] array2 = colorCache;
		int[] array3 = indexCache;
		if ((float)array2.Length < num8)
		{
			colorCache = new Color[(int)num8];
			indexCache = new int[(int)num8];
			array2 = colorCache;
			array3 = indexCache;
		}
		bool useEmissionFromColor = UseEmissionFromColor;
		int num9 = 0;
		bool flag = borderEmission == BorderEmission.Fast || borderEmission == BorderEmission.Precise;
		if (flag)
		{
			bool flag2 = false;
			Color color = array[0];
			int num10 = (int)num;
			bool flag3 = borderEmission == BorderEmission.Precise;
			for (int i = 0; (float)i < num8; i++)
			{
				Color color2 = array[i];
				bool flag4 = color2.a > 0f;
				if (flag3)
				{
					int num11 = i - num10;
					if (num11 > 0)
					{
						Color color3 = array[num11];
						bool flag5 = color3.a > 0f;
						if (flag4)
						{
							if (!flag5)
							{
								if (!useEmissionFromColor || (FloatComparer.AreEqual(r, color2.r, redTolerance) && FloatComparer.AreEqual(g, color2.g, greenTolerance) && FloatComparer.AreEqual(b, color2.b, blueTolerance)))
								{
									array2[num9] = color2;
									array3[num9] = i;
									num9++;
									color = color2;
									flag2 = true;
								}
								continue;
							}
						}
						else if (flag5)
						{
							if (useEmissionFromColor && (!FloatComparer.AreEqual(r, color3.r, redTolerance) || !FloatComparer.AreEqual(g, color3.g, greenTolerance) || !FloatComparer.AreEqual(b, color3.b, blueTolerance)))
							{
								continue;
							}
							array2[num9] = color3;
							array3[num9] = num11;
							num9++;
						}
					}
				}
				if (flag && !flag4 && flag2)
				{
					if (useEmissionFromColor && (!FloatComparer.AreEqual(r, color.r, redTolerance) || !FloatComparer.AreEqual(g, color.g, greenTolerance) || !FloatComparer.AreEqual(b, color.b, blueTolerance)))
					{
						continue;
					}
					array2[num9] = color;
					array3[num9] = i - 1;
					num9++;
					flag2 = true;
				}
				color = color2;
				if (!flag4)
				{
					flag2 = false;
				}
				else if ((!flag || (flag4 && !flag2)) && (!useEmissionFromColor || (FloatComparer.AreEqual(r, color2.r, redTolerance) && FloatComparer.AreEqual(g, color2.g, greenTolerance) && FloatComparer.AreEqual(b, color2.b, blueTolerance))))
				{
					array2[num9] = color2;
					array3[num9] = i;
					num9++;
					flag2 = true;
				}
			}
		}
		else
		{
			for (int j = 0; (float)j < num8; j++)
			{
				Color color4 = array[j];
				if (!(color4.a <= 0f) && (!useEmissionFromColor || (FloatComparer.AreEqual(r, color4.r, redTolerance) && FloatComparer.AreEqual(g, color4.g, greenTolerance) && FloatComparer.AreEqual(b, color4.b, blueTolerance))))
				{
					array2[num9] = color4;
					array3[num9] = j;
					num9++;
				}
			}
		}
		if (num9 <= 0)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		for (int k = 0; k < emitCount; k++)
		{
			int num12 = Random.Range(0, num9 - 1);
			int num13 = array3[num12];
			float num14 = (float)num13 % num / pixelsPerUnit - num6;
			float num15 = (float)(num13 / num3) / pixelsPerUnit - num7;
			if (flipX)
			{
				num14 = num / pixelsPerUnit - num14 - num6 * 2f;
			}
			if (flipY)
			{
				num15 = num2 / pixelsPerUnit - num15 - num7 * 2f;
			}
			zero.x = num14 * vector2.x - num4;
			zero.y = num15 * vector2.y + num4;
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
			{
				position = quaternion * zero + vector
			};
			if (UsePixelSourceColor)
			{
				emitParams.startColor = array2[num12];
			}
			emitParams.startSize = num5;
			particlesSystem.Emit(emitParams, 1);
		}
	}

	public void EmitAll(bool hideSprite = true)
	{
		if (hideSprite)
		{
			spriteRenderer.enabled = false;
		}
		Sprite sprite = spriteRenderer.sprite;
		if (!sprite)
		{
			_ = verboseDebug;
			return;
		}
		float r = EmitFromColor.r;
		float g = EmitFromColor.g;
		float b = EmitFromColor.b;
		Vector3 vector = spriteRenderer.gameObject.transform.position;
		Quaternion quaternion = spriteRenderer.gameObject.transform.rotation;
		Vector3 vector2 = spriteRenderer.gameObject.transform.lossyScale;
		if (SimulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = Vector3.zero;
			vector2 = Vector3.one;
			quaternion = Quaternion.identity;
		}
		bool flipX = spriteRenderer.flipX;
		bool flipY = spriteRenderer.flipY;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		float num3 = 1f / pixelsPerUnit;
		num3 *= mainModule.startSize.constant;
		float num4 = sprite.pivot.x / pixelsPerUnit;
		float num5 = sprite.pivot.y / pixelsPerUnit;
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
		float num6 = num * num2;
		Vector3 zero = Vector3.zero;
		for (int i = 0; (float)i < num6; i++)
		{
			Color color = array[i];
			if (!(color.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, color.r, redTolerance) && FloatComparer.AreEqual(g, color.g, greenTolerance) && FloatComparer.AreEqual(b, color.b, blueTolerance))))
			{
				float num7 = (float)i % num / pixelsPerUnit - num4;
				float num8 = (float)i / num / pixelsPerUnit - num5;
				if (flipX)
				{
					num7 = num / pixelsPerUnit - num7 - num4 * 2f;
				}
				if (flipY)
				{
					num8 = num2 / pixelsPerUnit - num8 - num5 * 2f;
				}
				zero.x = num7 * vector2.x;
				zero.y = num8 * vector2.y;
				ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
				{
					position = quaternion * zero + vector
				};
				if (UsePixelSourceColor)
				{
					emitParams.startColor = color;
				}
				emitParams.startSize = num3;
				particlesSystem.Emit(emitParams, 1);
			}
		}
	}

	public void RestoreSprite()
	{
		spriteRenderer.enabled = true;
	}

	public override void Play()
	{
		if (!isPlaying)
		{
			particlesSystem.Play();
		}
		isPlaying = true;
	}

	public override void Pause()
	{
		if (isPlaying)
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

	private void DummyMethod()
	{
	}
}
