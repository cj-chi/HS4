using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class bl_BrightnessImage : MonoBehaviour
{
	private float Value = 1f;

	private CanvasGroup _Alpha;

	private CanvasGroup Alpha
	{
		get
		{
			if (_Alpha == null)
			{
				_Alpha = GetComponent<CanvasGroup>();
			}
			return _Alpha;
		}
	}

	private void Start()
	{
		base.transform.SetAsLastSibling();
	}

	public void SetValue(float val)
	{
		Value = val;
		Alpha.alpha = 1f - Value;
	}
}
