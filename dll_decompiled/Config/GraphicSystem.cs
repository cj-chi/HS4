using UnityEngine;

namespace Config;

public class GraphicSystem : BaseSystem
{
	public const byte GRAPHIC_QUALITY = 0;

	public bool SelfShadow = true;

	public bool Bloom = true;

	public bool SSAO = true;

	public bool SSR = true;

	public bool RP = true;

	public bool DepthOfField = true;

	public bool Fog = true;

	public bool Vignette = true;

	public bool SunShaft = true;

	public bool Rain = true;

	public byte GraphicQuality;

	public bool AmbientLight = true;

	public bool Map = true;

	public bool Shield = true;

	public Color BackColor = Color.black;

	public GraphicSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		SelfShadow = true;
		Bloom = true;
		SSAO = true;
		SSR = true;
		RP = true;
		DepthOfField = true;
		Fog = true;
		Vignette = true;
		SunShaft = true;
		Rain = true;
		GraphicQuality = 0;
		AmbientLight = true;
		Map = true;
		Shield = true;
		BackColor = Color.black;
	}
}
