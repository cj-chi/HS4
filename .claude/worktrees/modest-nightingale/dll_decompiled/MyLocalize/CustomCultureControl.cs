using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MyLocalize;

public class CustomCultureControl : CultureControl
{
	protected override void Reset()
	{
		base.Reset();
		SpriteCultureComponent[] componentsInChildren = GetComponentsInChildren<SpriteCultureComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i]);
		}
		Image[] array = (from img in GetComponentsInChildren<Image>(includeInactive: true)
			where img.gameObject.name == "imgEntry" || img.gameObject.name == "imgShortcut"
			select img).ToArray();
		lstCmpCultureSprite = new List<SpriteCultureComponent>();
		int num = 0;
		Image[] array2 = array;
		foreach (Image image in array2)
		{
			SpriteCultureComponent spriteCultureComponent = image.gameObject.AddComponent<SpriteCultureComponent>();
			spriteCultureComponent.id = num++;
			spriteCultureComponent.image = image;
			lstCmpCultureSprite.Add(spriteCultureComponent);
		}
	}

	protected override void Start()
	{
		base.Start();
	}
}
