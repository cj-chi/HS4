using UnityEngine;

public class ShieldCollisionBehaviour : MonoBehaviour
{
	public GameObject EffectOnHit;

	public GameObject ExplosionOnHit;

	public bool IsWaterInstance;

	public float ScaleWave = 0.89f;

	public bool CreateMechInstanceOnHit;

	public Vector3 AngleFix;

	public int currentQueue = 3001;

	public void ShieldCollisionEnter(CollisionInfo e)
	{
		if (!(e.Hit.transform != null))
		{
			return;
		}
		if (IsWaterInstance)
		{
			Transform obj = Object.Instantiate(ExplosionOnHit).transform;
			obj.parent = base.transform;
			float num = base.transform.localScale.x * ScaleWave;
			obj.localScale = new Vector3(num, num, num);
			obj.localPosition = new Vector3(0f, 0.001f, 0f);
			obj.LookAt(e.Hit.point);
			return;
		}
		if (EffectOnHit != null)
		{
			if (!CreateMechInstanceOnHit)
			{
				Renderer componentInChildren = e.Hit.transform.GetComponentInChildren<Renderer>();
				GameObject obj2 = Object.Instantiate(EffectOnHit);
				obj2.transform.parent = componentInChildren.transform;
				obj2.transform.localPosition = Vector3.zero;
				AddMaterialOnHit component = obj2.GetComponent<AddMaterialOnHit>();
				component.SetMaterialQueue(currentQueue);
				component.UpdateMaterial(e.Hit);
			}
			else
			{
				GameObject obj3 = Object.Instantiate(EffectOnHit);
				Transform obj4 = obj3.transform;
				obj4.parent = GetComponent<Renderer>().transform;
				obj4.localPosition = Vector3.zero;
				obj4.localScale = base.transform.localScale * ScaleWave;
				obj4.LookAt(e.Hit.point);
				obj4.Rotate(AngleFix);
				obj3.GetComponent<Renderer>().material.renderQueue = currentQueue - 1000;
			}
		}
		if (currentQueue > 4000)
		{
			currentQueue = 3001;
		}
		else
		{
			currentQueue++;
		}
		if (ExplosionOnHit != null)
		{
			Object.Instantiate(ExplosionOnHit, e.Hit.point, default(Quaternion)).transform.parent = base.transform;
		}
	}

	private void Update()
	{
	}
}
