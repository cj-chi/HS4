using UnityEngine.UI;

namespace Illusion.Extensions;

public static class UGUIToggleExtensions
{
	public static void SetIsOnWithoutCallback(this Toggle self, bool isOn)
	{
		Toggle.ToggleEvent onValueChanged = self.onValueChanged;
		self.onValueChanged = new Toggle.ToggleEvent();
		self.isOn = isOn;
		self.onValueChanged = onValueChanged;
	}
}
