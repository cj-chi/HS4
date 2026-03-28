using UnityEngine;

namespace AIProject;

public class ValueInfo
{
	private float _cycle;

	public float Term { get; set; }

	public ValueInfo(float term)
	{
		Term = term;
		_cycle = 0f;
	}

	private int Add()
	{
		return Add(1f);
	}

	private int Add(float speed)
	{
		if (Term > 0f)
		{
			float num = 1f / Term;
			if (_cycle < 1f)
			{
				_cycle += Time.deltaTime * num * speed;
				return 0;
			}
			_cycle = 0f;
			return 1;
		}
		return 0;
	}

	public static int operator *(ValueInfo a, float d)
	{
		return a.Add(d);
	}

	public static implicit operator int(ValueInfo a)
	{
		return a.Add();
	}
}
