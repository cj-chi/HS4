using System;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace SpriteParticleEmitter;

[ExecuteInEditMode]
[RequireComponent(typeof(UIParticleRenderer))]
public class StaticUIImageEmitter : EmitterBaseUI
{
	[Header("Awake Options")]
	[Tooltip("Activating this will force CacheOnAwake")]
	public bool PlayOnAwake = true;

	[Tooltip("Should the system cache on Awake method? - Static emission needs to be cached first, if this property is not checked the CacheSprite() method should be called by code. (Refer to manual for further explanation)")]
	public bool CacheOnAwake = true;

	protected bool hasCachingEnded;

	protected int particlesCacheCount;

	protected float particleStartSize;

	protected Vector3[] particleInitPositionsCache;

	protected Color[] particleInitColorCache;

	public override event SimpleEvent OnCacheEnded;

	public override event SimpleEvent OnAvailableToPlay;

	protected override void Awake()
	{
		base.Awake();
		if (PlayOnAwake)
		{
			isPlaying = true;
			CacheOnAwake = true;
		}
		if (CacheOnAwake)
		{
			CacheSprite();
		}
	}

	public virtual void CacheSprite(bool relativeToParent = false)
	{
		hasCachingEnded = false;
		particlesCacheCount = 0;
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
		if (imageRenderer == null || imageRenderer.sprite == null)
		{
			_ = verboseDebug;
		}
		float num = (int)sprite.rect.size.x;
		float num2 = (int)sprite.rect.size.y;
		particleStartSize = mainModule.startSize.constant;
		float num3 = sprite.pivot.x / pixelsPerUnit;
		float num4 = sprite.pivot.y / pixelsPerUnit;
		Color[] array = ((!useSpritesSharingCache || !Application.isPlaying) ? sprite.texture.GetPixels((int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2) : SpritesDataPool.GetSpriteColors(sprite, (int)sprite.rect.position.x, (int)sprite.rect.position.y, (int)num, (int)num2));
		float redTolerance = RedTolerance;
		float greenTolerance = GreenTolerance;
		float blueTolerance = BlueTolerance;
		float num5 = num * num2;
		List<Color> list = new List<Color>();
		List<Vector3> list2 = new List<Vector3>();
		for (int i = 0; (float)i < num5; i++)
		{
			Color item = array[i];
			if (!(item.a <= 0f) && (!UseEmissionFromColor || (FloatComparer.AreEqual(r, item.r, redTolerance) && FloatComparer.AreEqual(g, item.g, greenTolerance) && FloatComparer.AreEqual(b, item.b, blueTolerance))))
			{
				float x = (float)i % num / pixelsPerUnit - num3;
				float y = (float)i / num / pixelsPerUnit - num4;
				Vector3 item2 = new Vector3(x, y, 0f);
				list2.Add(item2);
				list.Add(item);
				particlesCacheCount++;
			}
		}
		particleInitPositionsCache = list2.ToArray();
		particleInitColorCache = list.ToArray();
		if (particlesCacheCount <= 0)
		{
			_ = verboseDebug;
			return;
		}
		array = null;
		list2.Clear();
		list2 = null;
		list.Clear();
		list = null;
		GC.Collect();
		hasCachingEnded = true;
		if (OnCacheEnded != null)
		{
			OnCacheEnded();
		}
	}

	protected virtual void Update()
	{
	}

	public override void Play()
	{
	}

	public override void Stop()
	{
	}

	public override void Pause()
	{
	}

	public override bool IsPlaying()
	{
		return isPlaying;
	}

	public override bool IsAvailableToPlay()
	{
		return hasCachingEnded;
	}

	private void DummyMethod()
	{
		if (OnAvailableToPlay != null)
		{
			OnAvailableToPlay();
		}
		if (OnCacheEnded != null)
		{
			OnCacheEnded();
		}
	}
}
