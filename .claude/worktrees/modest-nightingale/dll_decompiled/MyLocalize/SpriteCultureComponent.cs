using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyLocalize;

[DisallowMultipleComponent]
public class SpriteCultureComponent : MonoBehaviour
{
	public int id = -1;

	public Image image;

	public void ChangeCulture(Dictionary<int, SpriteInfo.SrcInfo> dictSpriteInfo)
	{
		if (!(null == image) && dictSpriteInfo.TryGetValue(id, out var value))
		{
			image.sprite = value.sprite;
		}
	}
}
