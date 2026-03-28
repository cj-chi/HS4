using UnityEngine;

public class MinAttribute : PropertyAttribute
{
	private float min;

	public MinAttribute(float _min = 0f)
	{
		min = _min;
	}

	public float GetValue(float _value)
	{
		return Mathf.Max(min, _value);
	}

	public int GetValue(int _value)
	{
		return (int)Mathf.Max(min, _value);
	}
}
