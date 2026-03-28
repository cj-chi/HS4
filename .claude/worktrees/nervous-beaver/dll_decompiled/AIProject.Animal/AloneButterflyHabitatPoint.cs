using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace AIProject.Animal;

public class AloneButterflyHabitatPoint : MonoBehaviour
{
	private List<AloneButterfly> _use = new List<AloneButterfly>();

	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private Transform _center;

	[SerializeField]
	private float _moveRadius = 10f;

	[SerializeField]
	private float _moveHeight = 5f;

	[SerializeField]
	private float _maxDelayTime = 1f;

	[SerializeField]
	private float _moveSpeed = 1f;

	[SerializeField]
	private float _addAngle = 90f;

	[SerializeField]
	private float _turnAngle = 170f;

	[SerializeField]
	private float _changeTargetDistance = 1f;

	[SerializeField]
	private float _nextPointMaxDistance = 5f;

	[SerializeField]
	private float _speedDownDistance = 2f;

	[SerializeField]
	private Vector2Int _createNumRange = Vector2Int.one;

	private int _userMaxCount;

	public Transform Center => _center;

	public float MoveRadius => _moveRadius;

	public float MoveHeight => _moveHeight;

	public float MaxDelayTime => _maxDelayTime;

	public float MoveSpeed => _moveSpeed;

	public float AddAngle => _addAngle;

	public float TurnAngle => _turnAngle;

	public float ChangeTargetDistance => _changeTargetDistance;

	public float NextPointMaxDistance => _nextPointMaxDistance;

	public float SpeedDownDistance => _speedDownDistance;

	public bool Available => _center != null;

	public Vector2Int CreateNumRange => _createNumRange;

	private void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (_prefab == null)
		{
			return;
		}
		if (_center == null)
		{
			_center = base.transform;
		}
		_userMaxCount = _createNumRange.RandomRange();
		for (int i = 0; i < _userMaxCount; i++)
		{
			GameObject gameObject = new GameObject("alone_butterfly_" + i.ToString("00"));
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			AloneButterfly butterfly = gameObject.GetOrAddComponent<AloneButterfly>();
			butterfly.Initialize(this, _prefab);
			butterfly.OnDestroyAsObservable().TakeUntilDestroy(this).Subscribe(delegate
			{
				_use.Remove(butterfly);
			});
			_use.Add(butterfly);
		}
	}
}
