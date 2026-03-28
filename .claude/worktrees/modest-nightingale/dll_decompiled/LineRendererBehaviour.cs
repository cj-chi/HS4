using UnityEngine;

public class LineRendererBehaviour : MonoBehaviour
{
	public bool IsVertical;

	public float LightHeightOffset = 0.3f;

	public float ParticlesHeightOffset = 0.2f;

	public float TimeDestroyLightAfterCollision = 4f;

	public float TimeDestroyThisAfterCollision = 4f;

	public float TimeDestroyRootAfterCollision = 4f;

	public GameObject EffectOnHitObject;

	public GameObject Explosion;

	public GameObject StartGlow;

	public GameObject HitGlow;

	public GameObject Particles;

	public GameObject GoLight;

	private EffectSettings effectSettings;

	private Transform tRoot;

	private Transform tTarget;

	private bool isInitializedOnStart;

	private LineRenderer line;

	private int currentShaderIndex;

	private RaycastHit hit;

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
		GetEffectSettingsComponent(base.transform);
		_ = effectSettings == null;
		tRoot = effectSettings.transform;
		line = GetComponent<LineRenderer>();
		InitializeDefault();
		isInitializedOnStart = true;
	}

	private void InitializeDefault()
	{
		GetComponent<Renderer>().material.SetFloat("_Chanel", currentShaderIndex);
		currentShaderIndex++;
		if (currentShaderIndex == 3)
		{
			currentShaderIndex = 0;
		}
		line.SetPosition(0, tRoot.position);
		if (IsVertical)
		{
			if (Physics.Raycast(tRoot.position, Vector3.down, out hit))
			{
				line.SetPosition(1, hit.point);
				if (StartGlow != null)
				{
					StartGlow.transform.position = tRoot.position;
				}
				if (HitGlow != null)
				{
					HitGlow.transform.position = hit.point;
				}
				if (GoLight != null)
				{
					GoLight.transform.position = hit.point + new Vector3(0f, LightHeightOffset, 0f);
				}
				if (Particles != null)
				{
					Particles.transform.position = hit.point + new Vector3(0f, ParticlesHeightOffset, 0f);
				}
				if (Explosion != null)
				{
					Explosion.transform.position = hit.point + new Vector3(0f, ParticlesHeightOffset, 0f);
				}
			}
		}
		else
		{
			if (effectSettings.Target != null)
			{
				tTarget = effectSettings.Target.transform;
			}
			else
			{
				_ = effectSettings.UseMoveVector;
			}
			Vector3 vector = (effectSettings.UseMoveVector ? (tRoot.position + effectSettings.MoveVector * effectSettings.MoveDistance) : (tTarget.position - tRoot.position).normalized);
			Vector3 vector2 = tRoot.position + vector * effectSettings.MoveDistance;
			if (Physics.Raycast(tRoot.position, vector, out hit, effectSettings.MoveDistance + 1f, effectSettings.LayerMask))
			{
				vector2 = (tRoot.position + Vector3.Normalize(hit.point - tRoot.position) * (effectSettings.MoveDistance + 1f)).normalized;
			}
			line.SetPosition(1, hit.point - effectSettings.ColliderRadius * vector2);
			Vector3 vector3 = hit.point - vector2 * ParticlesHeightOffset;
			if (StartGlow != null)
			{
				StartGlow.transform.position = tRoot.position;
			}
			if (HitGlow != null)
			{
				HitGlow.transform.position = vector3;
			}
			if (GoLight != null)
			{
				GoLight.transform.position = hit.point - vector2 * LightHeightOffset;
			}
			if (Particles != null)
			{
				Particles.transform.position = vector3;
			}
			if (Explosion != null)
			{
				Explosion.transform.position = vector3;
				Explosion.transform.LookAt(vector3 + hit.normal);
			}
		}
		CollisionInfo e = new CollisionInfo
		{
			Hit = hit
		};
		effectSettings.OnCollisionHandler(e);
		if (hit.transform != null)
		{
			ShieldCollisionBehaviour component = hit.transform.GetComponent<ShieldCollisionBehaviour>();
			if (component != null)
			{
				component.ShieldCollisionEnter(e);
			}
		}
	}

	private void OnEnable()
	{
		if (isInitializedOnStart)
		{
			InitializeDefault();
		}
	}
}
