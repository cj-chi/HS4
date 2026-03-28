using System;
using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class StepWorld : MonoBehaviour
{
	[Serializable]
	public class TransformC
	{
		[ReadOnly]
		public int order;

		[ReadOnly]
		public Vector3 pos = Vector3.zero;

		[ReadOnly]
		public Vector3 rot = Vector3.zero;

		[ReadOnly]
		public Vector3 scale = Vector3.one;

		public Quaternion Rottation
		{
			get
			{
				return Quaternion.Euler(rot);
			}
			set
			{
				rot = value.eulerAngles;
			}
		}

		public Transform Transform
		{
			set
			{
				pos = value.position;
				rot = value.eulerAngles;
				scale = value.lossyScale;
			}
		}

		public Transform Local
		{
			set
			{
				pos = value.localPosition;
				rot = value.localEulerAngles;
				scale = value.localScale;
			}
		}

		public TransformC WorldToLocal(Transform _parent)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(pos, Rottation, scale);
			matrix4x = _parent.worldToLocalMatrix * matrix4x;
			pos = new Vector3(matrix4x.m03, matrix4x.m13, matrix4x.m23);
			Rottation = matrix4x.rotation;
			scale = matrix4x.lossyScale;
			return this;
		}
	}

	public TransformC tFixed = new TransformC();

	public TransformC tUpdate = new TransformC();

	public TransformC tYieldNull = new TransformC();

	public TransformC tLate = new TransformC();

	public TransformC tEndOfFrame = new TransformC();

	private Transform _transform;

	private int count;

	private TransformC original = new TransformC();

	private TransformC particle = new TransformC();

	private Transform Transform => _transform ?? (_transform = base.transform);

	private TransformC Local
	{
		set
		{
			_transform.localPosition = value.pos;
			_transform.localEulerAngles = value.rot;
			_transform.localScale = value.scale;
		}
	}

	private void Start()
	{
		(from _ in this.FixedUpdateAsObservable()
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			tFixed.Transform = Transform;
			tFixed.order = count++;
		});
		(from _ in this.UpdateAsObservable()
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			tUpdate.Transform = Transform;
			tUpdate.order = count++;
		});
		Observable.FromCoroutine(YieldNull).Subscribe();
		(from _ in this.LateUpdateAsObservable()
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			tLate.Transform = Transform;
			tLate.order = count++;
			Local = original;
		});
		Observable.FromCoroutine(EndOfFrame).Subscribe();
	}

	private IEnumerator YieldNull()
	{
		while (true)
		{
			yield return null;
			tYieldNull.Transform = Transform;
			tYieldNull.order = count++;
			original.Local = Transform;
			Local = particle.WorldToLocal(Transform.parent);
		}
	}

	private IEnumerator EndOfFrame()
	{
		WaitForEndOfFrame end = new WaitForEndOfFrame();
		while (true)
		{
			yield return end;
			tEndOfFrame.Transform = Transform;
			tEndOfFrame.order = count++;
			count = 0;
			particle.Transform = Transform;
		}
	}
}
