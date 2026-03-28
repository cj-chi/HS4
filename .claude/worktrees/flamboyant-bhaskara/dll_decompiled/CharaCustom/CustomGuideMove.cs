using UnityEngine;
using UnityEngine.EventSystems;

namespace CharaCustom;

public class CustomGuideMove : CustomGuideBase, IInitializePotentialDragHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public enum MoveAxis
	{
		X,
		Y,
		Z,
		XYZ
	}

	[SerializeField]
	private CustomGuideLimit guidLimit;

	public MoveAxis axis;

	private Vector2 oldPos = Vector2.zero;

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

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (!CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			oldPos = eventData.pressPosition;
		}
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			return;
		}
		base.OnDrag(eventData);
		bool _snap = false;
		Vector3 vector = ((axis == MoveAxis.XYZ) ? (WorldPos(eventData.position) - WorldPos(oldPos)) : AxisMove(eventData.delta, ref _snap));
		Vector3 position = base.guideObject.amount.position;
		position += vector;
		if (null != guidLimit && guidLimit.limited)
		{
			Vector3 position2 = guidLimit.trfParent.InverseTransformPoint(position);
			float x = guidLimit.limitMin.x;
			float x2 = guidLimit.limitMax.x;
			if (guidLimit.limitMin.x > guidLimit.limitMax.x)
			{
				x = guidLimit.limitMax.x;
				x2 = guidLimit.limitMin.x;
			}
			position2.x = Mathf.Clamp(position2.x, x, x2);
			x = guidLimit.limitMin.y;
			x2 = guidLimit.limitMax.y;
			if (guidLimit.limitMin.y > guidLimit.limitMax.y)
			{
				x = guidLimit.limitMax.y;
				x2 = guidLimit.limitMin.y;
			}
			position2.y = Mathf.Clamp(position2.y, x, x2);
			x = guidLimit.limitMin.z;
			x2 = guidLimit.limitMax.z;
			if (guidLimit.limitMin.z > guidLimit.limitMax.z)
			{
				x = guidLimit.limitMax.z;
				x2 = guidLimit.limitMin.z;
			}
			position2.z = Mathf.Clamp(position2.z, x, x2);
			Vector3 vector2 = guidLimit.trfParent.TransformPoint(position2);
			if (axis == MoveAxis.XYZ || axis == MoveAxis.X)
			{
				position.x = vector2.x;
			}
			if (axis == MoveAxis.XYZ || axis == MoveAxis.Y)
			{
				position.y = vector2.y;
			}
			if (axis == MoveAxis.XYZ || axis == MoveAxis.Z)
			{
				position.z = vector2.z;
			}
		}
		base.guideObject.amount.position = ((axis == MoveAxis.XYZ) ? position : (_snap ? Parse(position) : position));
		base.guideObject.ctrlAxisType = (int)axis;
		oldPos = eventData.position;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!CustomGuideAssist.IsCameraActionFlag(base.guideObject.ccv2))
		{
			CustomGuideAssist.SetCameraMoveFlag(base.guideObject.ccv2, _bPlay: false);
			base.guideObject.isDrag = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		CustomGuideAssist.SetCameraMoveFlag(base.guideObject.ccv2, _bPlay: true);
		base.guideObject.isDrag = false;
	}

	private Vector3 WorldPos(Vector2 _screenPos)
	{
		Plane plane = new Plane(camera.transform.forward * -1f, base.transform.position);
		Ray ray = RectTransformUtility.ScreenPointToRay(camera, _screenPos);
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return base.transform.position;
		}
		return ray.GetPoint(enter);
	}

	private Vector3 AxisPos(Vector2 _screenPos)
	{
		Vector3 position = base.transform.position;
		Plane plane = new Plane(base.transform.forward, position);
		if (!plane.GetSide(camera.transform.position))
		{
			plane = new Plane(base.transform.forward * -1f, position);
		}
		Vector3 up = base.transform.up;
		Ray ray = RectTransformUtility.ScreenPointToRay(camera, _screenPos);
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return Vector3.Project(position, up);
		}
		return Vector3.Project(ray.GetPoint(enter), up);
	}

	private Vector3 AxisMove(Vector2 _delta, ref bool _snap)
	{
		Vector3 vector = camera.transform.TransformVector(_delta.x * 0.01f, _delta.y * 0.01f, 0f);
		Vector3 up = base.transform.up;
		return up * vector.magnitude * base.guideObject.speedMove * Vector3.Dot(vector.normalized, up);
	}

	private Vector3 Parse(Vector3 _src)
	{
		return _src;
	}
}
