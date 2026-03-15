using Illusion.CustomAttributes;
using UnityEngine;

namespace AIChara;

public abstract class CmpBase : MonoBehaviour
{
	private bool baseDB;

	private DynamicBone[] dynamicBones;

	[Header("カメラ内判定用")]
	public Renderer[] rendCheckVisible;

	[Button("Reacquire", "再取得", new object[] { })]
	public int reacquire;

	public bool isVisible
	{
		get
		{
			if (rendCheckVisible == null || rendCheckVisible.Length == 0)
			{
				return false;
			}
			Renderer[] array = rendCheckVisible;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].isVisible)
				{
					return true;
				}
			}
			return false;
		}
	}

	public CmpBase(bool _baseDB)
	{
		baseDB = _baseDB;
	}

	protected virtual void Reacquire()
	{
		rendCheckVisible = GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	private void Reset()
	{
		SetReferenceObject();
		rendCheckVisible = GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	public abstract void SetReferenceObject();

	public void InitDynamicBones()
	{
		dynamicBones = null;
		if (baseDB)
		{
			dynamicBones = GetComponentsInChildren<DynamicBone>(includeInactive: true);
		}
	}

	public void ResetDynamicBones(bool includeInactive = false)
	{
		if (!baseDB || dynamicBones == null)
		{
			return;
		}
		DynamicBone[] array = dynamicBones;
		foreach (DynamicBone dynamicBone in array)
		{
			if (dynamicBone.enabled || includeInactive)
			{
				dynamicBone.ResetParticlesPosition();
			}
		}
	}

	public void EnableDynamicBones(bool enable)
	{
		if (!baseDB || dynamicBones == null)
		{
			return;
		}
		DynamicBone[] array = dynamicBones;
		foreach (DynamicBone dynamicBone in array)
		{
			if (dynamicBone.enabled != enable)
			{
				dynamicBone.enabled = enable;
				if (enable)
				{
					dynamicBone.ResetParticlesPosition();
				}
			}
		}
	}
}
