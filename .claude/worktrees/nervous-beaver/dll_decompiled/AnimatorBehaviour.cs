using UnityEngine;

public class AnimatorBehaviour : MonoBehaviour
{
	public Animator anim;

	private EffectSettings effectSettings;

	private bool isInitialized;

	private float oldSpeed;

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

	private void Start()
	{
		oldSpeed = anim.speed;
		GetEffectSettingsComponent(base.transform);
		if (effectSettings != null)
		{
			effectSettings.CollisionEnter += prefabSettings_CollisionEnter;
		}
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			anim.speed = oldSpeed;
		}
	}

	private void prefabSettings_CollisionEnter(object sender, CollisionInfo e)
	{
		anim.speed = 0f;
	}

	private void Update()
	{
	}
}
