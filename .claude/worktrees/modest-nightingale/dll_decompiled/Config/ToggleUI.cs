using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

public class ToggleUI : Toggle
{
	public delegate void OnClickDelegate(bool _value);

	public event OnClickDelegate onPointerClick;

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		if (this.onPointerClick != null)
		{
			this.onPointerClick(base.isOn);
		}
	}
}
