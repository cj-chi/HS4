using Illusion.Extensions;
using UnityEngine;

namespace Studio;

[DefaultExecutionOrder(30000)]
public class ParticleParent : MonoBehaviour
{
	private Transform _transform;

	public GameObject ObjOriginal { get; set; }

	private Transform Transform => _transform ?? (_transform = base.transform);

	private void OnEnable()
	{
		if (ObjOriginal != null)
		{
			ObjOriginal.SetActiveIfDifferent(active: true);
		}
	}

	private void OnDisable()
	{
		if (ObjOriginal != null)
		{
			ObjOriginal.SetActiveIfDifferent(active: false);
		}
	}

	private void OnDestroy()
	{
		if (ObjOriginal != null)
		{
			Object.Destroy(ObjOriginal);
		}
		ObjOriginal = null;
	}

	private void LateUpdate()
	{
		Transform obj = ObjOriginal.transform;
		obj.position = Transform.position;
		obj.rotation = Transform.rotation;
		obj.localScale = Transform.lossyScale;
	}
}
