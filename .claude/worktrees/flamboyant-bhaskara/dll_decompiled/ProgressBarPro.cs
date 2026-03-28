using System.Collections;
using PlayfulSystems;
using PlayfulSystems.ProgressBar;
using UnityEngine;

[ExecuteInEditMode]
public class ProgressBarPro : MonoBehaviour
{
	public enum AnimationType
	{
		FixedTimeForChange,
		ChangeSpeed
	}

	[SerializeField]
	[Range(0f, 1f)]
	private float m_value = 1f;

	private float displayValue = -1f;

	[Space(10f)]
	[Tooltip("Smoothes out the animation of the bar.")]
	[SerializeField]
	private bool animateBar = true;

	[SerializeField]
	private AnimationType animationType;

	[SerializeField]
	private float animTime = 0.25f;

	[Space(10f)]
	[SerializeField]
	private ProgressBarProView[] views;

	private Coroutine sizeAnim;

	public float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value != m_value)
			{
				SetValue(value);
			}
		}
	}

	public void Start()
	{
		if (views == null || views.Length == 0)
		{
			views = GetComponentsInChildren<ProgressBarProView>();
		}
	}

	private void OnEnable()
	{
		SetDisplayValue(m_value, forceUpdate: true);
	}

	public void SetValue(float value, float maxValue)
	{
		if (maxValue != 0f)
		{
			SetValue(value / maxValue);
		}
		else
		{
			SetValue(0f);
		}
	}

	public void SetValue(int value, int maxValue)
	{
		if (maxValue != 0)
		{
			SetValue((float)value / (float)maxValue);
		}
		else
		{
			SetValue(0f);
		}
	}

	public void SetValue(float percentage, bool forceUpdate = false)
	{
		if (forceUpdate || !Mathf.Approximately(m_value, percentage))
		{
			m_value = Mathf.Clamp01(percentage);
			for (int i = 0; i < views.Length; i++)
			{
				views[i].NewChangeStarted(displayValue, m_value);
			}
			if (animateBar && Application.isPlaying && base.gameObject.activeInHierarchy)
			{
				StartSizeAnim(percentage);
			}
			else
			{
				SetDisplayValue(percentage);
			}
		}
	}

	public bool IsAnimating()
	{
		if (!animateBar)
		{
			return false;
		}
		return !Mathf.Approximately(displayValue, m_value);
	}

	public void SetBarColor(Color color)
	{
		for (int i = 0; i < views.Length; i++)
		{
			views[i].SetBarColor(color);
		}
	}

	private void StartSizeAnim(float percentage)
	{
		if (sizeAnim != null)
		{
			StopCoroutine(sizeAnim);
		}
		sizeAnim = StartCoroutine(DoBarSizeAnim());
	}

	private IEnumerator DoBarSizeAnim()
	{
		float startValue = displayValue;
		float time = 0f;
		float change = m_value - displayValue;
		float duration = ((animationType == AnimationType.FixedTimeForChange) ? animTime : (Mathf.Abs(change) / animTime));
		while (time < duration)
		{
			time += Time.deltaTime;
			SetDisplayValue(Utils.EaseSinInOut(time / duration, startValue, change));
			yield return null;
		}
		SetDisplayValue(m_value, forceUpdate: true);
		sizeAnim = null;
	}

	private void SetDisplayValue(float value, bool forceUpdate = false)
	{
		if (forceUpdate || !(displayValue >= 0f) || !Mathf.Approximately(displayValue, value))
		{
			displayValue = value;
			UpdateBarViews(displayValue, m_value, forceUpdate);
		}
	}

	private void UpdateBarViews(float currentValue, float targetValue, bool forceUpdate = false)
	{
		if (views == null)
		{
			return;
		}
		for (int i = 0; i < views.Length; i++)
		{
			if (views[i] != null && (forceUpdate || views[i].CanUpdateView(currentValue, targetValue)))
			{
				views[i].UpdateView(currentValue, targetValue);
			}
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		SetValue(m_value, forceUpdate: true);
	}
}
