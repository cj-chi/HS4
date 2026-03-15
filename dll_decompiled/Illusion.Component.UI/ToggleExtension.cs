using UnityEngine.UI;

namespace Illusion.Component.UI;

public class ToggleExtension : Toggle
{
	public void SetDoStateTransition(int state, bool instant)
	{
		base.DoStateTransition((SelectionState)state, instant);
	}
}
