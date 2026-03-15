using System.Collections;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class LateBefore : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	private GameObject objTarget;

	private Vector3 pos = Vector3.zero;

	private Quaternion rot = Quaternion.identity;

	private Vector3 scale = Vector3.one;

	private Vector3 localPos = Vector3.zero;

	private Quaternion localRot = Quaternion.identity;

	private Vector3 localScale = Vector3.one;

	private Transform parent;

	public Transform Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
			localPos = target.localPosition;
			localRot = target.localRotation;
			localScale = target.localScale;
		}
	}

	private GameObject ObjTarget => objTarget ?? (objTarget = target.gameObject);

	private void Start()
	{
		Observable.FromCoroutine((CancellationToken _) => SetYield()).Subscribe().AddTo(this);
		Observable.FromCoroutine((CancellationToken _) => SetEnd()).Subscribe().AddTo(this);
		this.OnEnableAsObservable().Subscribe(delegate
		{
			target.localPosition = localPos;
			target.localRotation = localRot;
			target.localScale = localScale;
		}).AddTo(this);
	}

	private IEnumerator SetYield()
	{
		yield return null;
		while (true)
		{
			if (target != null && ObjTarget.activeSelf)
			{
				if (target.parent != null)
				{
					Matrix4x4 matrix4x = Matrix4x4.TRS(target.parent.position, target.parent.rotation, target.parent.lossyScale);
					Matrix4x4 matrix4x2 = Matrix4x4.TRS(pos, rot, scale);
					matrix4x2 = matrix4x.inverse * matrix4x2;
					target.localPosition = new Vector3(matrix4x2.m03, matrix4x2.m13, matrix4x2.m23);
					target.localRotation = matrix4x2.rotation;
					target.localScale = matrix4x2.lossyScale;
				}
				else
				{
					target.position = pos;
					target.rotation = rot;
				}
			}
			yield return null;
		}
	}

	private IEnumerator SetEnd()
	{
		WaitForEndOfFrame waitEnd = new WaitForEndOfFrame();
		while (true)
		{
			yield return waitEnd;
			if (target != null && ObjTarget.activeSelf)
			{
				pos = target.position;
				rot = target.rotation;
				scale = target.lossyScale;
			}
		}
	}

	private void LateUpdate()
	{
		if (target != null)
		{
			target.localPosition = localPos;
			target.localRotation = localRot;
			target.localScale = localScale;
		}
	}
}
