using AIChara;
using UniRx;
using UnityEngine;

namespace Illusion.Component.Correct.Process;

[RequireComponent(typeof(BaseData))]
public abstract class BaseProcess : MonoBehaviour
{
	public enum Type
	{
		Target,
		Sync
	}

	private BaseData _data;

	public Type type;

	public bool noRestore;

	private ChaControl _chaCtrl;

	private Vector3 pos = Vector3.zero;

	private Quaternion rot = Quaternion.identity;

	public BaseData data => this.GetComponentCache(ref _data);

	public ChaControl chaCtrl => this.GetCacheObject(ref _chaCtrl, () => GetComponentInParent<ChaControl>());

	private void Start()
	{
		(from _ in Observable.EveryEndOfFrame().TakeUntilDestroy(base.gameObject)
			where this != null && base.isActiveAndEnabled
			where type == Type.Target && !noRestore
			select _).Subscribe(delegate
		{
			Restore();
		});
		void Restore()
		{
			Transform bone = data.bone;
			if (!(bone == null))
			{
				bone.localPosition = pos;
				bone.localRotation = rot;
			}
		}
	}

	protected virtual void LateUpdate()
	{
		Transform bone = data.bone;
		if (bone == null)
		{
			return;
		}
		switch (type)
		{
		case Type.Target:
			pos = bone.localPosition;
			rot = bone.localRotation;
			bone.localPosition = pos + data.pos;
			bone.localRotation = rot * data.rot;
			break;
		case Type.Sync:
			if (chaCtrl != null)
			{
				base.transform.SetPositionAndRotation(bone.position + chaCtrl.objBodyBone.transform.TransformDirection(data.pos), bone.rotation * data.rot);
			}
			break;
		}
	}
}
