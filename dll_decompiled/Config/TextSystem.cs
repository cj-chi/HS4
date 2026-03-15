using UnityEngine;

namespace Config;

public class TextSystem : BaseSystem
{
	private static Color initWindowColor = new Color(1f, 1f, 1f, 1f);

	private static Color initFont1Color = new Color(1f, 0.8f, 1f, 1f);

	public int FontSpeed = 40;

	public bool NextVoiceStop = true;

	public bool ChoicesSkip;

	public bool ChoicesAuto;

	public float WindowAlpha = 0.8f;

	public Color WindowColor = initWindowColor;

	public Color Font0Color = Color.white;

	public Color Font1Color = initFont1Color;

	public Color Font2Color = Color.white;

	public TextSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		FontSpeed = 40;
		NextVoiceStop = true;
		ChoicesSkip = false;
		ChoicesAuto = false;
		WindowAlpha = 0.8f;
		WindowColor = initWindowColor;
		Font0Color = Color.white;
		Font1Color = initFont1Color;
		Font2Color = Color.white;
	}
}
