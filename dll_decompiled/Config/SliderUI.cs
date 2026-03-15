using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

public class SliderUI : Slider
{
	public override void OnPointerDown(PointerEventData eventData)
	{
		if (base.onValueChanged != null)
		{
			base.onValueChanged.Invoke(value);
		}
		base.OnPointerDown(eventData);
	}
}
