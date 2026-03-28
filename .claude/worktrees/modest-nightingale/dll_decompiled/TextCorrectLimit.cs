using UnityEngine;
using UnityEngine.UI;

public static class TextCorrectLimit
{
	public static string CorrectString(Text text, string baseStr, string endStr)
	{
		GameObject gameObject = Object.Instantiate(text.gameObject, null, worldPositionStays: false);
		Text component = gameObject.GetComponent<Text>();
		float spaceWidth = GetSpaceWidth(component);
		int length = endStr.Length;
		float num = 0f;
		for (int i = 0; i < length; i++)
		{
			num += ((' ' == endStr[i]) ? spaceWidth : GetTextWidth(component, endStr.Substring(i, 1)));
		}
		float num2 = text.rectTransform.rect.width - num;
		length = baseStr.Length;
		int num3 = 0;
		num = 0f;
		for (int j = 0; j < length; j++)
		{
			num += ((' ' == baseStr[j]) ? spaceWidth : GetTextWidth(component, baseStr.Substring(j, 1)));
			if (num >= num2)
			{
				break;
			}
			num3++;
		}
		Object.Destroy(gameObject);
		return baseStr.Substring(0, num3) + ((num3 == length) ? "" : endStr);
	}

	public static void Correct(Text text, string baseStr, string endStr)
	{
		text.text = CorrectString(text, baseStr, endStr);
	}

	private static float GetSpaceWidth(Text textComp)
	{
		float textWidth = GetTextWidth(textComp, "m m");
		float textWidth2 = GetTextWidth(textComp, "mm");
		return textWidth - textWidth2;
	}

	private static float GetTextWidth(Text textComp, string message)
	{
		textComp.text = message;
		return textComp.preferredWidth;
	}
}
