using System.Collections.Generic;
using Manager;
using UnityEngine;

public class WetCameraCol : MonoBehaviour
{
	public CameraWetEffect CameraWetEffect;

	private List<Collider> colliders = new List<Collider>();

	private List<int> delIdxes = new List<int>();

	private List<Collider> mapWaters = new List<Collider>();

	private int upState;

	private void Start()
	{
		colliders.Clear();
		delIdxes.Clear();
		mapWaters.Clear();
	}

	private void Update()
	{
		delIdxes.Clear();
		for (int i = 0; i < colliders.Count; i++)
		{
			int num = i;
			if (!colliders[num].enabled)
			{
				delIdxes.Add(num);
			}
		}
		for (int num2 = delIdxes.Count - 1; num2 >= 0; num2--)
		{
			colliders.RemoveAt(delIdxes[num2]);
		}
		if (CameraWetEffect != null)
		{
			if (colliders.Count == 0)
			{
				CameraWetEffect.HitWaterSiru = false;
			}
			if (mapWaters.Count == 0 && upState == 1)
			{
				CameraWetEffect.HitWaterMap = true;
				upState = 0;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (HSceneManager.HResourceTables.hParticle == null)
		{
			return;
		}
		List<GameObject> playParticles = HSceneManager.HResourceTables.hParticle.GetPlayParticles();
		if (playParticles == null)
		{
			return;
		}
		if (!(other.transform.parent != null) || !(other.transform.parent.parent != null) || !(other.transform.parent.parent.parent != null) || !playParticles.Contains(other.transform.parent.parent.parent.gameObject))
		{
			if (mapWaters.Contains(other) || !Singleton<CameraWetMapCol>.IsInstance())
			{
				return;
			}
			Collider[] hitWater = Singleton<CameraWetMapCol>.Instance.HitWater;
			bool flag = false;
			for (int i = 0; i < hitWater.Length; i++)
			{
				if (!(other != hitWater[i]))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				mapWaters.Add(other);
				upState = 0;
			}
			return;
		}
		Renderer component = other.gameObject.GetComponent<Renderer>();
		if (!(component == null) && component.enabled)
		{
			if (CameraWetEffect != null)
			{
				CameraWetEffect.HitWaterSiru = true;
			}
			colliders.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		List<GameObject> playParticles = HSceneManager.HResourceTables.hParticle.GetPlayParticles();
		if (playParticles == null)
		{
			return;
		}
		if (!(other.transform.parent != null) || !(other.transform.parent.parent != null) || !playParticles.Contains(other.transform.parent.parent.gameObject))
		{
			if (mapWaters.Contains(other))
			{
				mapWaters.Remove(other);
				float y = base.transform.position.y;
				float y2 = other.transform.position.y;
				if (y > y2)
				{
					upState = 1;
				}
				else
				{
					upState = -1;
				}
			}
		}
		else
		{
			Renderer component = other.gameObject.GetComponent<Renderer>();
			if (!(component == null) && component.enabled && colliders.Contains(other))
			{
				colliders.Remove(other);
			}
		}
	}
}
