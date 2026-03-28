using System.Collections.Generic;
using UnityEngine;

namespace SpriteToParticlesAsset;

public class LoadingTimeSpritesPool : MonoBehaviour
{
	[Tooltip("Drag here all the sprites to be loaded in the pool.")]
	public List<Sprite> spritesToLoad;

	[Tooltip("If enabled the load will be called on this GameObject’s Awake method. Otherwise it can be called by the method LoadAll() ")]
	public bool loadAllOnAwake;

	private void Awake()
	{
		if (loadAllOnAwake)
		{
			LoadAll();
		}
	}

	public void LoadAll()
	{
		foreach (Sprite item in spritesToLoad)
		{
			Rect rect = item.rect;
			SpritesDataPool.GetSpriteColors(item, (int)rect.position.x, (int)rect.position.y, (int)rect.size.x, (int)rect.size.y);
		}
	}
}
