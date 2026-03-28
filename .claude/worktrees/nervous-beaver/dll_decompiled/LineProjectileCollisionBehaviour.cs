using UnityEngine;

public class LineProjectileCollisionBehaviour : MonoBehaviour
{
	public GameObject EffectOnHit;

	public GameObject EffectOnHitObject;

	public GameObject ParticlesScale;

	public GameObject GoLight;

	public bool IsCenterLightPosition;

	public LineRenderer[] LineRenderers;

	private EffectSettings effectSettings;

	private Transform t;

	private Transform tLight;

	private Transform tEffectOnHit;

	private Transform tParticleScale;

	private RaycastHit hit;

	private RaycastHit oldRaycastHit;

	private bool isInitializedOnStart;

	private bool frameDroped;

	private ParticleSystem[] effectOnHitParticles;

	private EffectSettings effectSettingsInstance;

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
		t = base.transform;
		if (EffectOnHit != null)
		{
			tEffectOnHit = EffectOnHit.transform;
			effectOnHitParticles = EffectOnHit.GetComponentsInChildren<ParticleSystem>();
		}
		if (ParticlesScale != null)
		{
			tParticleScale = ParticlesScale.transform;
		}
		GetEffectSettingsComponent(t);
		_ = effectSettings == null;
		if (GoLight != null)
		{
			tLight = GoLight.transform;
		}
		InitializeDefault();
		isInitializedOnStart = true;
	}

	private void OnEnable()
	{
		if (isInitializedOnStart)
		{
			InitializeDefault();
		}
	}

	private void OnDisable()
	{
		CollisionLeave();
	}

	private void InitializeDefault()
	{
		hit = default(RaycastHit);
		frameDroped = false;
	}

	private void Update()
	{
		if (!frameDroped)
		{
			frameDroped = true;
			return;
		}
		Vector3 vector = t.position + t.forward * effectSettings.MoveDistance;
		if (Physics.Raycast(t.position, t.forward, out var hitInfo, effectSettings.MoveDistance + 1f, effectSettings.LayerMask))
		{
			hit = hitInfo;
			vector = hitInfo.point;
			if (oldRaycastHit.collider != hit.collider)
			{
				CollisionLeave();
				oldRaycastHit = hit;
				CollisionEnter();
				if (EffectOnHit != null)
				{
					ParticleSystem[] array = effectOnHitParticles;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Play();
					}
				}
			}
			if (EffectOnHit != null)
			{
				tEffectOnHit.position = hit.point - t.forward * effectSettings.ColliderRadius;
			}
		}
		else if (EffectOnHit != null)
		{
			ParticleSystem[] array = effectOnHitParticles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}
		if (EffectOnHit != null)
		{
			tEffectOnHit.LookAt(hit.point + hit.normal);
		}
		if (IsCenterLightPosition && GoLight != null)
		{
			tLight.position = (t.position + vector) / 2f;
		}
		LineRenderer[] lineRenderers = LineRenderers;
		foreach (LineRenderer obj in lineRenderers)
		{
			obj.SetPosition(0, vector);
			obj.SetPosition(1, t.position);
		}
		if (ParticlesScale != null)
		{
			float x = Vector3.Distance(t.position, vector) / 2f;
			tParticleScale.localScale = new Vector3(x, 1f, 1f);
		}
	}

	private void CollisionEnter()
	{
		if (EffectOnHitObject != null && hit.transform != null)
		{
			AddMaterialOnHit componentInChildren = hit.transform.GetComponentInChildren<AddMaterialOnHit>();
			effectSettingsInstance = null;
			if (componentInChildren != null)
			{
				effectSettingsInstance = componentInChildren.gameObject.GetComponent<EffectSettings>();
			}
			if (effectSettingsInstance != null)
			{
				effectSettingsInstance.IsVisible = true;
			}
			else
			{
				Renderer componentInChildren2 = hit.transform.GetComponentInChildren<Renderer>();
				GameObject gameObject = Object.Instantiate(EffectOnHitObject);
				gameObject.transform.parent = componentInChildren2.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.GetComponent<AddMaterialOnHit>().UpdateMaterial(hit);
				effectSettingsInstance = gameObject.GetComponent<EffectSettings>();
			}
		}
		effectSettings.OnCollisionHandler(new CollisionInfo
		{
			Hit = hit
		});
	}

	private void CollisionLeave()
	{
		if (effectSettingsInstance != null)
		{
			effectSettingsInstance.IsVisible = false;
		}
	}
}
