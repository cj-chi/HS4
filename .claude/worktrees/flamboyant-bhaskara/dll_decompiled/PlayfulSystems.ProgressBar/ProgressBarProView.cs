using UnityEngine;

namespace PlayfulSystems.ProgressBar;

public abstract class ProgressBarProView : MonoBehaviour
{
	public virtual void NewChangeStarted(float currentValue, float targetValue)
	{
	}

	public virtual void SetBarColor(Color color)
	{
	}

	public virtual bool CanUpdateView(float currentValue, float targetValue)
	{
		return base.isActiveAndEnabled;
	}

	public abstract void UpdateView(float currentValue, float targetValue);
}
