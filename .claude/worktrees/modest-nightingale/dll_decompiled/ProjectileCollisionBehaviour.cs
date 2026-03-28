using System;
using UnityEngine;

public class ProjectileCollisionBehaviour : MonoBehaviour
{
	public float RandomMoveRadius;

	public float RandomMoveSpeed;

	public float RandomRange;

	public RandomMoveCoordinates RandomMoveCoordinates;

	public GameObject EffectOnHitObject;

	public GameObject GoLight;

	public AnimationCurve Acceleration;

	public float AcceleraionTime = 1f;

	public bool IsCenterLightPosition;

	public bool IsLookAt;

	public bool AttachAfterCollision;

	public bool IsRootMove = true;

	public bool IsLocalSpaceRandomMove;

	public bool IsDeviation;

	public bool SendCollisionMessage = true;

	public bool ResetParentPositionOnDisable;

	private EffectSettings effectSettings;

	private Transform tRoot;

	private Transform tTarget;

	private Transform t;

	private Transform tLight;

	private Vector3 forwardDirection;

	private Vector3 startPosition;

	private Vector3 startParentPosition;

	private RaycastHit hit;

	private Vector3 smootRandomPos;

	private Vector3 oldSmootRandomPos;

	private float deltaSpeed;

	private float startTime;

	private float randomSpeed;

	private float randomRadiusX;

	private float randomRadiusY;

	private int randomDirection1;

	private int randomDirection2;

	private int randomDirection3;

	private bool onCollision;

	private bool isInitializedOnStart;

	private Vector3 randomTargetOffsetXZVector;

	private bool frameDroped;

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
		GetEffectSettingsComponent(t);
		_ = effectSettings == null;
		if (!IsRootMove)
		{
			startParentPosition = base.transform.parent.position;
		}
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
		if (ResetParentPositionOnDisable && isInitializedOnStart && !IsRootMove)
		{
			base.transform.parent.position = startParentPosition;
		}
	}

	private void InitializeDefault()
	{
		hit = default(RaycastHit);
		onCollision = false;
		smootRandomPos = default(Vector3);
		oldSmootRandomPos = default(Vector3);
		deltaSpeed = 0f;
		startTime = 0f;
		randomSpeed = 0f;
		randomRadiusX = 0f;
		randomRadiusY = 0f;
		randomDirection1 = 0;
		randomDirection2 = 0;
		randomDirection3 = 0;
		frameDroped = false;
		tRoot = (IsRootMove ? effectSettings.transform : base.transform.parent);
		startPosition = tRoot.position;
		if (effectSettings.Target != null)
		{
			tTarget = effectSettings.Target.transform;
		}
		else
		{
			_ = effectSettings.UseMoveVector;
		}
		if ((double)effectSettings.EffectRadius > 0.001)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * effectSettings.EffectRadius;
			randomTargetOffsetXZVector = new Vector3(vector.x, 0f, vector.y);
		}
		else
		{
			randomTargetOffsetXZVector = Vector3.zero;
		}
		if (!effectSettings.UseMoveVector)
		{
			forwardDirection = tRoot.position + (tTarget.position + randomTargetOffsetXZVector - tRoot.position).normalized * effectSettings.MoveDistance;
			GetTargetHit();
		}
		else
		{
			forwardDirection = tRoot.position + effectSettings.MoveVector * effectSettings.MoveDistance;
		}
		if (IsLookAt)
		{
			if (!effectSettings.UseMoveVector)
			{
				tRoot.LookAt(tTarget);
			}
			else
			{
				tRoot.LookAt(forwardDirection);
			}
		}
		InitRandomVariables();
	}

	private void Update()
	{
		if (!frameDroped)
		{
			frameDroped = true;
		}
		else if (((effectSettings.UseMoveVector || !(tTarget == null)) && !onCollision) || !frameDroped)
		{
			Vector3 vector = (effectSettings.UseMoveVector ? forwardDirection : (effectSettings.IsHomingMove ? tTarget.position : forwardDirection));
			float num = Vector3.Distance(tRoot.position, vector);
			float num2 = effectSettings.MoveSpeed * Time.deltaTime;
			if (num2 > num)
			{
				num2 = num;
			}
			if (num <= effectSettings.ColliderRadius)
			{
				hit = default(RaycastHit);
				CollisionEnter();
			}
			Vector3 normalized = (vector - tRoot.position).normalized;
			if (Physics.Raycast(tRoot.position, normalized, out var hitInfo, num2 + effectSettings.ColliderRadius, effectSettings.LayerMask))
			{
				hit = hitInfo;
				vector = hitInfo.point - normalized * effectSettings.ColliderRadius;
				CollisionEnter();
			}
			if (IsCenterLightPosition && GoLight != null)
			{
				tLight.position = (startPosition + tRoot.position) / 2f;
			}
			Vector3 vector2 = default(Vector3);
			if (RandomMoveCoordinates != RandomMoveCoordinates.None)
			{
				UpdateSmootRandomhPos();
				vector2 = smootRandomPos - oldSmootRandomPos;
			}
			float num3 = 1f;
			if (Acceleration.length > 0)
			{
				float time = (Time.time - startTime) / AcceleraionTime;
				num3 = Acceleration.Evaluate(time);
			}
			Vector3 vector3 = Vector3.MoveTowards(tRoot.position, vector, effectSettings.MoveSpeed * Time.deltaTime * num3);
			Vector3 vector4 = vector3 + vector2;
			if (IsLookAt && effectSettings.IsHomingMove)
			{
				tRoot.LookAt(vector4);
			}
			if (IsLocalSpaceRandomMove && IsRootMove)
			{
				tRoot.position = vector3;
				t.localPosition += vector2;
			}
			else
			{
				tRoot.position = vector4;
			}
			oldSmootRandomPos = smootRandomPos;
		}
	}

	private void CollisionEnter()
	{
		if (EffectOnHitObject != null && hit.transform != null)
		{
			Renderer componentInChildren = hit.transform.GetComponentInChildren<Renderer>();
			GameObject obj = UnityEngine.Object.Instantiate(EffectOnHitObject);
			obj.transform.parent = componentInChildren.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.GetComponent<AddMaterialOnHit>().UpdateMaterial(hit);
		}
		if (AttachAfterCollision)
		{
			tRoot.parent = hit.transform;
		}
		if (SendCollisionMessage)
		{
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
		onCollision = true;
	}

	private void InitRandomVariables()
	{
		deltaSpeed = RandomMoveSpeed * UnityEngine.Random.Range(1f, 1000f * RandomRange + 1f) / 1000f - 1f;
		startTime = Time.time;
		randomRadiusX = UnityEngine.Random.Range(RandomMoveRadius / 20f, RandomMoveRadius * 100f) / 100f;
		randomRadiusY = UnityEngine.Random.Range(RandomMoveRadius / 20f, RandomMoveRadius * 100f) / 100f;
		randomSpeed = UnityEngine.Random.Range(RandomMoveSpeed / 20f, RandomMoveSpeed * 100f) / 100f;
		randomDirection1 = ((UnityEngine.Random.Range(0, 2) > 0) ? 1 : (-1));
		randomDirection2 = ((UnityEngine.Random.Range(0, 2) > 0) ? 1 : (-1));
		randomDirection3 = ((UnityEngine.Random.Range(0, 2) > 0) ? 1 : (-1));
	}

	private void GetTargetHit()
	{
		Ray ray = new Ray(tRoot.position, Vector3.Normalize(tTarget.position + randomTargetOffsetXZVector - tRoot.position));
		Collider componentInChildren = tTarget.GetComponentInChildren<Collider>();
		if (componentInChildren != null && componentInChildren.Raycast(ray, out var hitInfo, effectSettings.MoveDistance))
		{
			hit = hitInfo;
		}
	}

	private void UpdateSmootRandomhPos()
	{
		float num = Time.time - startTime;
		float num2 = num * randomSpeed;
		float f = num * deltaSpeed;
		float num4;
		float num5;
		if (IsDeviation)
		{
			float num3 = Vector3.Distance(tRoot.position, hit.point) / effectSettings.MoveDistance;
			num4 = (float)randomDirection2 * Mathf.Sin(num2) * randomRadiusX * num3;
			num5 = (float)randomDirection3 * Mathf.Sin(num2 + (float)randomDirection1 * (float)Math.PI / 2f * num + Mathf.Sin(f)) * randomRadiusY * num3;
		}
		else
		{
			num4 = (float)randomDirection2 * Mathf.Sin(num2) * randomRadiusX;
			num5 = (float)randomDirection3 * Mathf.Sin(num2 + (float)randomDirection1 * (float)Math.PI / 2f * num + Mathf.Sin(f)) * randomRadiusY;
		}
		if (RandomMoveCoordinates == RandomMoveCoordinates.XY)
		{
			smootRandomPos = new Vector3(num4, num5, 0f);
		}
		if (RandomMoveCoordinates == RandomMoveCoordinates.XZ)
		{
			smootRandomPos = new Vector3(num4, 0f, num5);
		}
		if (RandomMoveCoordinates == RandomMoveCoordinates.YZ)
		{
			smootRandomPos = new Vector3(0f, num4, num5);
		}
		if (RandomMoveCoordinates == RandomMoveCoordinates.XYZ)
		{
			smootRandomPos = new Vector3(num4, num5, (num4 + num5) / 2f * (float)randomDirection1);
		}
	}
}
