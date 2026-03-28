using System.Collections.Generic;
using UnityEngine;

namespace SpriteToParticlesAsset;

public class SpritesDataPool
{
	private static Dictionary<Sprite, Color[]> spritesShared = new Dictionary<Sprite, Color[]>();

	public static Color[] GetSpriteColors(Sprite sprite, int x, int y, int blockWidth, int blockHeight)
	{
		if (spritesShared == null)
		{
			spritesShared = new Dictionary<Sprite, Color[]>();
		}
		Color[] array;
		if (!spritesShared.ContainsKey(sprite))
		{
			array = sprite.texture.GetPixels(x, y, blockWidth, blockHeight);
			spritesShared.Add(sprite, array);
		}
		else
		{
			array = spritesShared[sprite];
		}
		return array;
	}

	public static void ReleaseMemory()
	{
		spritesShared = null;
	}
}
