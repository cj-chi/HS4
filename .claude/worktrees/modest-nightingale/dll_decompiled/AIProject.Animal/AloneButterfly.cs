using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace AIProject.Animal;

public class AloneButterfly : MonoBehaviour
{
	[SerializeField]
	private float _speed = 4f;

	private List<Vector3> _pointList = new List<Vector3>();

	private Vector3? TargetPoint;

	private Animation _animation;

	private GameObject _bodyObject;

	public AloneButterflyHabitatPoint _habitatPoint { get; set; }

	public bool IsReached
	{
		get
		{
			if (!TargetPoint.HasValue)
			{
				return true;
			}
			return Vector3.Distance(Position, _habitatPoint.Center.position + TargetPoint.Value) <= _habitatPoint.ChangeTargetDistance;
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
			base.transform.rotation = value;
		}
	}

	public Vector3 Forward => base.transform.forward;

	public void SetAnimationSpeed(float speed)
	{
		if (!(_animation != null) || !_animation.isActiveAndEnabled)
		{
			return;
		}
		foreach (AnimationState item in _animation)
		{
			item.speed = speed;
		}
	}

	public void Initialize(AloneButterflyHabitatPoint habitatPoint, GameObject prefabObj)
	{
		if (habitatPoint == null)
		{
			return;
		}
		_habitatPoint = habitatPoint;
		Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(0f, _habitatPoint.MaxDelayTime))).TakeUntilDestroy(habitatPoint).TakeUntilDestroy(this)
			.Subscribe(delegate
			{
				_bodyObject = UnityEngine.Object.Instantiate(prefabObj, base.transform);
				Transform obj = _bodyObject.transform;
				Vector3 localPosition = (_bodyObject.transform.localEulerAngles = Vector3.zero);
				obj.localPosition = localPosition;
				_bodyObject.SetActive(value: true);
				_animation = GetComponentInChildren<Animation>(includeInactive: true);
				SetAnimationSpeed(_speed);
				Position = _habitatPoint.Center.position + GetRandomMoveAreaPoint();
				(from __ in Observable.EveryUpdate().TakeUntilDestroy(_habitatPoint).TakeUntilDestroy(this)
					where base.isActiveAndEnabled
					select __).Subscribe(delegate
				{
					AddWaypoint();
					OnUpdate();
				});
			});
	}

	public float GetDiameter(float moveSpeed, float addAngle)
	{
		if (moveSpeed == 0f || addAngle == 0f)
		{
			return float.PositiveInfinity;
		}
		return 360f / (addAngle / moveSpeed) / (float)Math.PI;
	}

	protected Vector3 GetRandomPosOnCircle(float radius)
	{
		float f = UnityEngine.Random.value * (float)Math.PI * 2f;
		float num = radius * Mathf.Sqrt(UnityEngine.Random.value);
		return new Vector3(num * Mathf.Cos(f), 0f, num * Mathf.Sin(f));
	}

	private Vector3 GetRandomMoveAreaPoint()
	{
		if (_habitatPoint == null || _habitatPoint.Center == null)
		{
			return Vector3.zero;
		}
		float moveHeight = _habitatPoint.MoveHeight;
		float moveRadius = _habitatPoint.MoveRadius;
		moveHeight = UnityEngine.Random.Range((0f - moveHeight) / 2f, moveHeight / 2f);
		Vector3 randomPosOnCircle = GetRandomPosOnCircle(moveRadius);
		randomPosOnCircle.y = moveHeight;
		return randomPosOnCircle;
	}

	private void AddWaypoint()
	{
		if (3 > _pointList.Count)
		{
			Vector3 b = (_pointList.IsNullOrEmpty() ? Position : (_habitatPoint.Center.position + _pointList.Back()));
			Vector3 randomMoveAreaPoint = GetRandomMoveAreaPoint();
			float num = Vector3.Distance(_habitatPoint.Center.position + randomMoveAreaPoint, b);
			if (_habitatPoint.NextPointMaxDistance < num && GetDiameter(_habitatPoint.MoveSpeed, _habitatPoint.AddAngle) < num)
			{
				_pointList.Add(randomMoveAreaPoint);
			}
		}
	}

	private void OnUpdate()
	{
		if (IsReached)
		{
			TargetPoint = null;
		}
		bool hasValue = TargetPoint.HasValue;
		bool flag = _pointList.IsNullOrEmpty();
		if (!hasValue && flag)
		{
			return;
		}
		if (!hasValue && !flag)
		{
			TargetPoint = _pointList.PopFront();
		}
		if (!TargetPoint.HasValue)
		{
			return;
		}
		Vector3 vector = _habitatPoint.Center.position + TargetPoint.Value;
		float f = Vector3.SignedAngle(Forward, vector - Position, Vector3.up);
		float deltaTime = Time.deltaTime;
		float num = Vector3.Distance(vector, Position);
		float num2 = _habitatPoint.MoveSpeed * deltaTime;
		float b = _habitatPoint.AddAngle * deltaTime;
		b = Mathf.Min(Mathf.Abs(f), b);
		Vector3 vector2 = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		identity = Quaternion.Euler(0f, b * Mathf.Sign(f), 0f);
		Rotation *= identity;
		if (Mathf.Abs(f) <= _habitatPoint.TurnAngle)
		{
			if (num < _habitatPoint.SpeedDownDistance)
			{
				num2 *= 1f - Mathf.Abs(f) / _habitatPoint.TurnAngle;
			}
			Vector3 vector3 = vector - Position;
			vector2 = vector3.normalized * num2;
			f = Vector3.SignedAngle(vector3, Forward, Vector3.up);
			vector2 = Quaternion.Euler(0f, f, 0f) * vector2;
		}
		Position += vector2;
	}
}
