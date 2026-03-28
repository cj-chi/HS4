using UnityEngine;

public class TransformCompositeInfoData : ScriptableObject
{
	public TransformInfoData[] transformInfoData = new TransformInfoData[3];

	public void Reflect(Transform _target, float _value, bool _isLocal = true)
	{
		float num = Mathf.Clamp(_value, 0f, 1f);
		Vector3 vector;
		Quaternion quaternion;
		if (MathfEx.RangeEqualOn(0f, num, 0.5f))
		{
			float t = Mathf.InverseLerp(0f, 0.5f, num);
			vector = Vector3.Lerp(transformInfoData[0].trans, transformInfoData[1].trans, t);
			quaternion = Quaternion.Lerp(transformInfoData[0].rot, transformInfoData[1].rot, t);
		}
		else
		{
			float t2 = Mathf.InverseLerp(0.5f, 1f, num);
			vector = Vector3.Lerp(transformInfoData[1].trans, transformInfoData[2].trans, t2);
			quaternion = Quaternion.Lerp(transformInfoData[1].rot, transformInfoData[2].rot, t2);
		}
		if (_isLocal)
		{
			_target.localPosition = vector;
			_target.localRotation = quaternion;
		}
		else
		{
			_target.position = vector;
			_target.rotation = quaternion;
		}
	}
}
