using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Studio;

public class ParticleAssist : MonoBehaviour
{
	[Serializable]
	public class TransformC
	{
		public Vector3 pos = Vector3.zero;

		public Vector3 rot = Vector3.zero;

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

	private Transform _transform;

	private TransformC original = new TransformC();

	private TransformC particle = new TransformC();

	private Transform Transform => _transform ?? (_transform = base.transform);

	private TransformC Local
	{
		set
		{
			Transform.localPosition = value.pos;
			Transform.localEulerAngles = value.rot;
			Transform.localScale = value.scale;
		}
	}

	private void Start()
	{
		Observable.FromCoroutine(YieldNull).Subscribe().AddTo(this);
		(from _ in this.LateUpdateAsObservable()
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			Local = original;
		}).AddTo(this);
		Observable.FromCoroutine(EndOfFrame).Subscribe().AddTo(this);
	}

	private IEnumerator YieldNull()
	{
		while (true)
		{
			yield return null;
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
			particle.Transform = Transform;
		}
	}
}
