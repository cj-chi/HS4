using UnityEngine;

public class CollisionActiveBehaviour : MonoBehaviour
{
	public bool IsReverse;

	public float TimeDelay;

	public bool IsLookAt;

	private EffectSettings effectSettings;

	private void Start()
	{
		GetEffectSettingsComponent(base.transform);
		if (IsReverse)
		{
			effectSettings.RegistreInactiveElement(base.gameObject, TimeDelay);
			base.gameObject.SetActive(value: false);
		}
		else
		{
			effectSettings.RegistreActiveElement(base.gameObject, TimeDelay);
		}
		if (IsLookAt)
		{
			effectSettings.CollisionEnter += effectSettings_CollisionEnter;
		}
	}

	private void effectSettings_CollisionEnter(object sender, CollisionInfo e)
	{
		base.transform.LookAt(effectSettings.transform.position + e.Hit.normal);
	}

	private void GetEffectSettingsComponent(Transform tr)
	{
		Transform parent = tr.parent;
		if (parent != null)
		{
			effectSettings = parent.GetComponentInChildren<EffectSettings>();
			if (effectSettings == null)
			{
				GetEffectSettingsComponent(parent.transform);
			}
		}
	}
}
