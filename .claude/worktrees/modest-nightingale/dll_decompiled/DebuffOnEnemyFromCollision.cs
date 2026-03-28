using UnityEngine;

public class DebuffOnEnemyFromCollision : MonoBehaviour
{
	public EffectSettings EffectSettings;

	public GameObject Effect;

	private void Start()
	{
		EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
	}

	private void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
	{
		if (!(Effect == null))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, EffectSettings.EffectRadius, EffectSettings.LayerMask);
			foreach (Collider collider in array)
			{
				Renderer componentInChildren = collider.transform.GetComponentInChildren<Renderer>();
				GameObject obj = Object.Instantiate(Effect);
				obj.transform.parent = componentInChildren.transform;
				obj.transform.localPosition = Vector3.zero;
				obj.GetComponent<AddMaterialOnHit>().UpdateMaterial(collider.transform);
			}
		}
	}

	private void Update()
	{
	}
}
