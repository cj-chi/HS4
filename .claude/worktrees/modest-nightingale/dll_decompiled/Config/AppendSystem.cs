using UnityEngine;

namespace Config;

public class AppendSystem : BaseSystem
{
	private Color colorDefault_M = new Color(0f, 0f, 1f, 0.5f);

	private Color colorDefault_F = new Color(1f, 0f, 0f, 0.5f);

	public Color MobMColor_M = Color.blue;

	public Color MobMColor_F = Color.red;

	public AppendSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		MobMColor_M = colorDefault_M;
		MobMColor_F = colorDefault_F;
	}
}
