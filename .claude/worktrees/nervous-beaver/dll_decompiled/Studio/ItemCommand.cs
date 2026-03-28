using UnityEngine;

namespace Studio;

public static class ItemCommand
{
	public class ColorInfo
	{
		public int dicKey { get; protected set; }

		public Color oldValue { get; protected set; }

		public Color newValue { get; protected set; }

		public ColorInfo(int _dicKey, Color _oldValue, Color _newValue)
		{
			dicKey = _dicKey;
			oldValue = _oldValue;
			newValue = _newValue;
		}
	}
}
