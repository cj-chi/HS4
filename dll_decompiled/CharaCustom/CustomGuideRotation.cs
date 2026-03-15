using UnityEngine;
using UnityEngine.EventSystems;

namespace CharaCustom;

public class CustomGuideRotation : CustomGuideBase, IInitializePotentialDragHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public enum RotationAxis
	{
		X,
		Y,
		Z
	}

	public RotationAxis axis;

	[SerializeField]
	private CustomGuideLimit guidLimit;

	private Vector2 prevScreenPos = Vector2.zero;

	private Vector3 prevPlanePos = Vector3.zero;

	private Camera m_Camera;

	private Camera camera
	{
		get
		{
			if (m_Camera == null)
			{
				m_Camera = Camera.main;
			}
			return m_Camera;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			CustomGuideAssist.SetCameraMoveFlag(base.guideObject.ccv2, _bPlay: false);
		}
	}

	public void OnInitializePotentialDrag(PointerEventData _eventData)
	{
		if (!CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			base.guideObject.isDrag = true;
			prevScreenPos = _eventData.position;
			prevPlanePos = PlanePos(_eventData.position);
		}
	}

	private float LimitedValue(RotationAxis axis, float val)
	{
		float num = 0f;
		float num2 = 0f;
		float result = 0f;
		switch (axis)
		{
		case RotationAxis.X:
			num = guidLimit.limitMin.x;
			num2 = guidLimit.limitMax.x;
			if (guidLimit.limitMin.x > guidLimit.limitMax.x)
			{
				num = guidLimit.limitMax.x;
				num2 = guidLimit.limitMin.x;
			}
			val = ((val > 180f) ? (val - 360f) : val);
			result = Mathf.Clamp(val, num, num2);
			break;
		case RotationAxis.Y:
			num = guidLimit.limitMin.y;
			num2 = guidLimit.limitMax.y;
			if (guidLimit.limitMin.y > guidLimit.limitMax.y)
			{
				num = guidLimit.limitMax.y;
				num2 = guidLimit.limitMin.y;
			}
			val = ((val > 180f) ? (val - 360f) : val);
			result = Mathf.Clamp(val, num, num2);
			break;
		case RotationAxis.Z:
			num = guidLimit.limitMin.z;
			num2 = guidLimit.limitMax.z;
			if (guidLimit.limitMin.z > guidLimit.limitMax.z)
			{
				num = guidLimit.limitMax.z;
				num2 = guidLimit.limitMin.z;
			}
			val = ((val > 180f) ? (val - 360f) : val);
			result = Mathf.Clamp(val, num, num2);
			break;
		}
		return result;
	}

	public override void OnDrag(PointerEventData _eventData)
	{
		if (CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			return;
		}
		base.OnDrag(_eventData);
		Vector3 zero = Vector3.zero;
		if (Mathf.Abs(Vector3.Dot(camera.transform.forward, base.transform.right)) > 0.1f)
		{
			Vector3 position = PlanePos(_eventData.position);
			Vector3 vector = Quaternion.Euler(0f, 90f, 0f) * base.transform.InverseTransformPoint(prevPlanePos);
			Vector3 vector2 = Quaternion.Euler(0f, 90f, 0f) * base.transform.InverseTransformPoint(position);
			float num = VectorToAngle(new Vector2(vector.x, vector.y), new Vector2(vector2.x, vector2.y));
			if (null != guidLimit && guidLimit.limited)
			{
				zero[(int)axis] = LimitedValue(axis, num);
			}
			else
			{
				zero[(int)axis] = num;
			}
			prevPlanePos = position;
		}
		else
		{
			Vector3 position2 = _eventData.position;
			position2.z = Vector3.Distance(prevPlanePos, camera.transform.position);
			Vector3 position3 = prevScreenPos;
			position3.z = Vector3.Distance(prevPlanePos, camera.transform.position);
			Vector3 vector3 = camera.ScreenToWorldPoint(position2) - camera.ScreenToWorldPoint(position3);
			Vector3 position4 = prevPlanePos + vector3;
			Vector3 vector4 = Quaternion.Euler(0f, 90f, 0f) * base.transform.InverseTransformPoint(prevPlanePos);
			Vector3 vector5 = Quaternion.Euler(0f, 90f, 0f) * base.transform.InverseTransformPoint(position4);
			prevPlanePos = position4;
			float num2 = VectorToAngle(new Vector2(vector4.x, vector4.y), new Vector2(vector5.x, vector5.y));
			if (null != guidLimit && guidLimit.limited)
			{
				zero[(int)axis] = LimitedValue(axis, num2);
			}
			else
			{
				zero[(int)axis] = num2;
			}
			prevPlanePos = position4;
		}
		prevScreenPos = _eventData.position;
		Vector3 eulerAngles = (Quaternion.Euler(base.guideObject.amount.rotation) * Quaternion.Euler(zero)).eulerAngles;
		eulerAngles.x %= 360f;
		eulerAngles.y %= 360f;
		eulerAngles.z %= 360f;
		base.guideObject.amount.rotation = eulerAngles;
		base.guideObject.ctrlAxisType = (int)axis;
	}

	public void OnPointerUp(PointerEventData _eventData)
	{
		CustomGuideAssist.SetCameraMoveFlag(base.guideObject.ccv2, _bPlay: true);
		base.guideObject.isDrag = false;
	}

	private Vector3 PlanePos(Vector2 _screenPos)
	{
		Plane plane = new Plane(base.transform.right, base.transform.position);
		if (!plane.GetSide(camera.transform.position))
		{
			plane.SetNormalAndPosition(base.transform.right * -1f, base.transform.position);
		}
		Ray ray = RectTransformUtility.ScreenPointToRay(camera, _screenPos);
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return base.transform.position;
		}
		return ray.GetPoint(enter);
	}

	private float VectorToAngle(Vector2 _v1, Vector2 _v2)
	{
		float current = Mathf.Atan2(_v1.x, _v1.y) * 57.29578f;
		float target = Mathf.Atan2(_v2.x, _v2.y) * 57.29578f;
		return Mathf.DeltaAngle(current, target);
	}
}
