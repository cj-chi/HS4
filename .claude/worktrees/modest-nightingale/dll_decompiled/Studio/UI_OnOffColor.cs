using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class UI_OnOffColor : MonoBehaviour
{
	public Image[] images;

	public Color onColor = Color.white;

	public Color offColor = Color.white;

	private void Start()
	{
		Toggle component = GetComponent<Toggle>();
		if ((bool)component)
		{
			OnChange(component.isOn);
		}
	}

	public void OnChange(bool check)
	{
		if (images != null)
		{
			Color color = (check ? onColor : offColor);
			Image[] array = images;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
		}
	}
}
