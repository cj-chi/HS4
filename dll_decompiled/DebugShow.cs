using UnityEngine;

public class DebugShow : Singleton<DebugShow>
{
	[SerializeField]
	private KeyCode _iconDelKey = KeyCode.Delete;

	[SerializeField]
	private bool _isDel;

	private KeyCode IconDelKey => _iconDelKey;

	private bool isDel
	{
		get
		{
			return _isDel;
		}
		set
		{
			_isDel = value;
		}
	}
}
