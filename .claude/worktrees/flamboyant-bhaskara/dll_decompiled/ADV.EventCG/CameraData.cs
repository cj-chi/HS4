using System.Collections;
using System.Linq;
using AIChara;
using UniRx;
using UnityEngine;

namespace ADV.EventCG;

public class CameraData : MonoBehaviour
{
	[Header("カメラデータ")]
	[SerializeField]
	private float _fieldOfView;

	private float? baseFieldOfView;

	[Header("身長補正座標")]
	[SerializeField]
	private Vector3 _minPos;

	[SerializeField]
	private Vector3 _maxPos;

	[Header("身長補正角度")]
	[SerializeField]
	private Vector3 _minAng;

	[SerializeField]
	private Vector3 _maxAng;

	private ReactiveCollection<ChaControl> _chaCtrlList = new ReactiveCollection<ChaControl>();

	private Vector3 basePos;

	private Vector3 baseAng;

	private Transform parent;

	public bool initialized { get; private set; }

	public float fieldOfView
	{
		get
		{
			return _fieldOfView;
		}
		set
		{
			_fieldOfView = value;
		}
	}

	public ReactiveCollection<ChaControl> chaCtrlList => _chaCtrlList;

	public void SetCameraData(Camera cam)
	{
		baseFieldOfView = cam.fieldOfView;
	}

	public void RepairCameraData(Camera cam)
	{
		if (baseFieldOfView.HasValue)
		{
			cam.fieldOfView = baseFieldOfView.Value;
		}
	}

	private void Calculate()
	{
		if (_chaCtrlList.Any())
		{
			float shape = _chaCtrlList.Average((ChaControl p) => p.GetShapeBodyValue(0));
			Vector3 vector = MathfEx.GetShapeLerpPositionValue(shape, _minPos, _maxPos);
			Vector3 shapeLerpAngleValue = MathfEx.GetShapeLerpAngleValue(shape, _minAng, _maxAng);
			if (parent != null)
			{
				vector = parent.TransformDirection(vector);
			}
			base.transform.SetPositionAndRotation(basePos + vector, Quaternion.Euler(baseAng + shapeLerpAngleValue));
		}
	}

	private void OnEnable()
	{
		basePos = base.transform.position;
		baseAng = base.transform.eulerAngles;
	}

	private void OnDisable()
	{
		base.transform.position = basePos;
		base.transform.eulerAngles = baseAng;
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		parent = base.transform.parent;
		GetComponentInParent<Data>().withoutPlayerList.ForEach(delegate(ChaControl item)
		{
			_chaCtrlList.Add(item);
		});
		_chaCtrlList.ObserveAdd().Subscribe(delegate
		{
			Calculate();
		});
		_chaCtrlList.ObserveRemove().Subscribe(delegate
		{
			Calculate();
		});
		_chaCtrlList.AddTo(this);
		base.enabled = true;
		initialized = true;
		Calculate();
		yield break;
	}
}
