using System;
using UnityEngine;

public class MoveOnGround : MonoBehaviour
{
	public bool IsRootMove = true;

	private EffectSettings effectSettings;

	private Transform tRoot;

	private Transform tTarget;

	private Vector3 targetPos;

	private bool isInitialized;

	private bool isFinished;

	private ParticleSystem[] particles;

	public event EventHandler<CollisionInfo> OnCollision;

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
		particles = effectSettings.GetComponentsInChildren<ParticleSystem>();
		InitDefaultVariables();
		isInitialized = true;
	}

	private void OnEnable()
	{
		if (isInitialized)
		{
			InitDefaultVariables();
		}
	}

	private void InitDefaultVariables()
	{
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		isFinished = false;
		tTarget = effectSettings.Target.transform;
		if (IsRootMove)
		{
			tRoot = effectSettings.transform;
		}
		else
		{
			tRoot = base.transform.parent;
			tRoot.localPosition = Vector3.zero;
		}
		targetPos = tRoot.position + Vector3.Normalize(tTarget.position - tRoot.position) * effectSettings.MoveDistance;
		Physics.Raycast(tRoot.position, Vector3.down, out var hitInfo);
		tRoot.position = hitInfo.point;
		array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	private void Update()
	{
		if (!(tTarget == null) && !isFinished)
		{
			Vector3 position = tRoot.position;
			Physics.Raycast(new Vector3(position.x, 0.5f, position.z), Vector3.down, out var hitInfo);
			tRoot.position = hitInfo.point;
			position = tRoot.position;
			Vector3 vector = (effectSettings.IsHomingMove ? tTarget.position : targetPos);
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
			if (Vector3.Distance(new Vector3(position.x, 0f, position.z), vector2) <= effectSettings.ColliderRadius)
			{
				effectSettings.OnCollisionHandler(new CollisionInfo());
				isFinished = true;
			}
			tRoot.position = Vector3.MoveTowards(position, vector2, effectSettings.MoveSpeed * Time.deltaTime);
		}
	}
}
