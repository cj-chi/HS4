using System;
using UnityEngine;

[Serializable]
public class H_LookAtDan_Info
{
	public enum AxisType
	{
		X,
		Y,
		Z,
		RevX,
		RevY,
		RevZ,
		None
	}

	public enum RotationOrder
	{
		XYZ,
		XZY,
		YXZ,
		YZX,
		ZXY,
		ZYX
	}

	public string lookAtName = "";

	public string targetName = "";

	public AxisType targetAxisType = AxisType.Z;

	public string upAxisName = "";

	public AxisType upAxisType = AxisType.Y;

	public AxisType sourceAxisType = AxisType.Y;

	public AxisType limitAxisType = AxisType.None;

	public RotationOrder rotOrder = RotationOrder.ZXY;

	[Range(-180f, 180f)]
	public float limitMin;

	[Range(-180f, 180f)]
	public float limitMax;

	private Quaternion oldRotation = Quaternion.identity;

	public Transform trfLookAt { get; private set; }

	public Transform trfTarget { get; private set; }

	public Transform trfUpAxis { get; private set; }

	public H_LookAtDan_Info()
	{
		trfLookAt = null;
		trfTarget = null;
		trfUpAxis = null;
	}

	public void SetLookAtTransform(Transform trf)
	{
		trfLookAt = trf;
	}

	public void SetTargetTransform(Transform trf)
	{
		trfTarget = trf;
	}

	public void SetUpAxisTransform(Transform trf)
	{
		trfUpAxis = trf;
	}

	public void SetOldRotation(Quaternion q)
	{
		oldRotation = q;
	}

	public void ManualCalc()
	{
		if (null == trfTarget || null == trfLookAt)
		{
			return;
		}
		Vector3 upVector = GetUpVector();
		Vector3 vector = Vector3.Normalize(trfTarget.position - trfLookAt.position);
		Vector3 vector2 = Vector3.Normalize(Vector3.Cross(upVector, vector));
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		if (targetAxisType == AxisType.RevX || targetAxisType == AxisType.RevY || targetAxisType == AxisType.RevZ)
		{
			vector = -vector;
			vector2 = -vector2;
		}
		Vector3 xvec = Vector3.zero;
		Vector3 yvec = Vector3.zero;
		Vector3 zvec = Vector3.zero;
		switch (targetAxisType)
		{
		case AxisType.X:
		case AxisType.RevX:
			xvec = vector;
			if (sourceAxisType == AxisType.Y)
			{
				yvec = vector3;
				zvec = -vector2;
			}
			else if (sourceAxisType == AxisType.RevY)
			{
				yvec = -vector3;
				zvec = vector2;
			}
			else if (sourceAxisType == AxisType.Z)
			{
				yvec = vector2;
				zvec = vector3;
			}
			else if (sourceAxisType == AxisType.RevZ)
			{
				yvec = -vector2;
				zvec = -vector3;
			}
			break;
		case AxisType.Y:
		case AxisType.RevY:
			yvec = vector;
			if (sourceAxisType == AxisType.X)
			{
				xvec = vector3;
				zvec = vector2;
			}
			else if (sourceAxisType == AxisType.RevX)
			{
				xvec = -vector3;
				zvec = -vector2;
			}
			else if (sourceAxisType == AxisType.Z)
			{
				xvec = -vector2;
				zvec = vector3;
			}
			else if (sourceAxisType == AxisType.RevZ)
			{
				xvec = vector2;
				zvec = -vector3;
			}
			break;
		case AxisType.Z:
		case AxisType.RevZ:
			zvec = vector;
			if (sourceAxisType == AxisType.X)
			{
				xvec = vector3;
				yvec = -vector2;
			}
			else if (sourceAxisType == AxisType.RevX)
			{
				xvec = -vector3;
				yvec = vector2;
			}
			else if (sourceAxisType == AxisType.Y)
			{
				xvec = vector2;
				yvec = vector3;
			}
			else if (sourceAxisType == AxisType.RevY)
			{
				xvec = -vector2;
				yvec = -vector3;
			}
			break;
		}
		if (limitAxisType == AxisType.None)
		{
			Quaternion q = default(Quaternion);
			if (LookAtQuat(xvec, yvec, zvec, ref q))
			{
				trfLookAt.rotation = q;
			}
			else
			{
				trfLookAt.rotation = oldRotation;
			}
			oldRotation = trfLookAt.rotation;
			return;
		}
		Quaternion q2 = default(Quaternion);
		if (LookAtQuat(xvec, yvec, zvec, ref q2))
		{
			trfLookAt.rotation = q2;
		}
		else
		{
			trfLookAt.rotation = oldRotation;
		}
		ConvertRotation.RotationOrder order = (ConvertRotation.RotationOrder)rotOrder;
		Quaternion localRotation = trfLookAt.localRotation;
		Vector3 vector4 = ConvertRotation.ConvertDegreeFromQuaternion(order, localRotation);
		Quaternion q3 = Quaternion.Slerp(localRotation, Quaternion.identity, 0.5f);
		Vector3 vector5 = ConvertRotation.ConvertDegreeFromQuaternion(order, q3);
		if (limitAxisType == AxisType.X)
		{
			if ((vector4.x < 0f && vector5.x > 0f) || (vector4.x > 0f && vector5.x < 0f))
			{
				vector4.x *= -1f;
			}
			vector4.x = Mathf.Clamp(vector4.x, limitMin, limitMax);
		}
		else if (limitAxisType == AxisType.Y)
		{
			if ((vector4.y < 0f && vector5.y > 0f) || (vector4.y > 0f && vector5.y < 0f))
			{
				vector4.y *= -1f;
			}
			vector4.y = Mathf.Clamp(vector4.y, limitMin, limitMax);
		}
		else if (limitAxisType == AxisType.Z)
		{
			if ((vector4.z < 0f && vector5.z > 0f) || (vector4.z > 0f && vector5.z < 0f))
			{
				vector4.z *= -1f;
			}
			vector4.z = Mathf.Clamp(vector4.z, limitMin, limitMax);
		}
		trfLookAt.localRotation = ConvertRotation.ConvertDegreeToQuaternion(order, vector4.x, vector4.y, vector4.z);
		oldRotation = trfLookAt.rotation;
	}

	private Vector3 GetUpVector()
	{
		Vector3 result = Vector3.up;
		if ((bool)trfUpAxis)
		{
			switch (upAxisType)
			{
			case AxisType.X:
				result = trfUpAxis.right;
				break;
			case AxisType.Y:
				result = trfUpAxis.up;
				break;
			case AxisType.Z:
				result = trfUpAxis.forward;
				break;
			}
		}
		return result;
	}

	private bool LookAtQuat(Vector3 xvec, Vector3 yvec, Vector3 zvec, ref Quaternion q)
	{
		float num = 1f + xvec.x + yvec.y + zvec.z;
		if (num == 0f)
		{
			GlobalMethod.DebugLog("LookAt 計算不可 値0", 1);
			return false;
		}
		float num2 = Mathf.Sqrt(num) / 2f;
		if (float.IsNaN(num2))
		{
			GlobalMethod.DebugLog("LookAt 計算不可 NaN", 1);
			return false;
		}
		float num3 = 4f * num2;
		if (num3 == 0f)
		{
			GlobalMethod.DebugLog("LookAt 計算不可 w=0", 1);
			return false;
		}
		float x = (yvec.z - zvec.y) / num3;
		float y = (zvec.x - xvec.z) / num3;
		float z = (xvec.y - yvec.x) / num3;
		q = new Quaternion(x, y, z, num2);
		return true;
	}
}
