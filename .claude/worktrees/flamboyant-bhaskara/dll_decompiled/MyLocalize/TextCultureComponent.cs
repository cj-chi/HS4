using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyLocalize;

[DisallowMultipleComponent]
public class TextCultureComponent : MonoBehaviour
{
	public int id = -1;

	public Text text;

	public void ChangeCulture(Dictionary<int, FontInfo.Info> dictFontInfo, Dictionary<int, TextInfo.Info> dictTextInfo)
	{
		if (!(null == text) && dictTextInfo.TryGetValue(id, out var value) && dictFontInfo.TryGetValue(value.fontId, out var value2))
		{
			text.font = value2.font;
			text.fontSize = value.size;
			text.text = value.str;
		}
	}
}
