using UnityEngine;

namespace LuxWater;

public class LuxWater_HelpBtn : PropertyAttribute
{
	public string URL;

	public LuxWater_HelpBtn(string URL)
	{
		this.URL = URL;
	}
}
