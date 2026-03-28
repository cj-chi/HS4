using UnityEngine.UI;

namespace Illusion.Component.UI;

public class ButtonExtension : Button
{
	public void SetDoStateTransition(int state, bool instant)
	{
		base.DoStateTransition((SelectionState)state, instant);
	}
}
