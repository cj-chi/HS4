using UnityEngine;

public class TransformInfoData : ScriptableObject
{
	public Vector3 trans = Vector3.zero;

	public Quaternion rot = Quaternion.identity;

	public void Reflect(Transform _target, bool _isLocal = true)
	{
		if (_isLocal)
		{
			_target.localPosition = trans;
			_target.localRotation = rot;
		}
		else
		{
			_target.position = trans;
			_target.rotation = rot;
		}
	}
}
