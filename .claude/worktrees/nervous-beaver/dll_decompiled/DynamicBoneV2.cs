using System.Collections.Generic;
using UniRx;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone V2")]
public class DynamicBoneV2 : MonoBehaviour
{
	public enum UpdateMode
	{
		Normal,
		AnimatePhysics,
		UnscaledTime
	}

	public enum FreezeAxis
	{
		None,
		X,
		Y,
		Z
	}

	private class Particle
	{
		public Transform m_Transform;

		public int m_ParentIndex = -1;

		public float m_Damping;

		public float m_Elasticity;

		public float m_Stiffness;

		public float m_Inert;

		public float m_Radius;

		public float m_BoneLength;

		public Vector3 m_Position = Vector3.zero;

		public Vector3 m_PrevPosition = Vector3.zero;

		public Vector3 m_EndOffset = Vector3.zero;

		public Vector3 m_InitLocalPosition = Vector3.zero;

		public Quaternion m_InitLocalRotation = Quaternion.identity;
	}

	private struct ColliderJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<DynamicBoneColliderBase.Bound> boundAry;

		public Vector3 particlePosition;

		[ReadOnly]
		public NativeArray<float> particleRadiusAry;

		[ReadOnly]
		public NativeArray<float> capsuleHeightAry;

		[ReadOnly]
		public NativeArray<Vector3> centerAry;

		[ReadOnly]
		public NativeArray<Vector3> c0Ary;

		[ReadOnly]
		public NativeArray<Vector3> c1Ary;

		[ReadOnly]
		public NativeArray<float> radiusAry;

		[ReadOnly]
		public NativeArray<bool> enabledAry;

		public void Execute(int index)
		{
			if (capsuleHeightAry[index] <= 0f)
			{
				Vector3 vector = particlePosition - centerAry[index];
				float sqrMagnitude = vector.sqrMagnitude;
				if (boundAry[index] == DynamicBoneColliderBase.Bound.Outside)
				{
					float num = radiusAry[index] + particleRadiusAry[index];
					float num2 = num * num;
					if (sqrMagnitude > 0f && sqrMagnitude < num2)
					{
						float num3 = Mathf.Sqrt(sqrMagnitude);
						particlePosition = centerAry[index] + vector * (num / num3);
					}
				}
				else
				{
					float num4 = radiusAry[index] - particleRadiusAry[index];
					float num5 = num4 * num4;
					if (sqrMagnitude > num5)
					{
						float num6 = Mathf.Sqrt(sqrMagnitude);
						particlePosition = centerAry[index] + vector * (num4 / num6);
					}
				}
				return;
			}
			if (boundAry[index] == DynamicBoneColliderBase.Bound.Outside)
			{
				float num7 = radiusAry[index] + particleRadiusAry[index];
				float num8 = num7 * num7;
				Vector3 vector2 = c1Ary[index] - c0Ary[index];
				Vector3 vector3 = particlePosition - c0Ary[index];
				float num9 = Vector3.Dot(vector3, vector2);
				if (num9 <= 0f)
				{
					float sqrMagnitude2 = vector3.sqrMagnitude;
					if (sqrMagnitude2 > 0f && sqrMagnitude2 < num8)
					{
						float num10 = Mathf.Sqrt(sqrMagnitude2);
						particlePosition = c0Ary[index] + vector3 * (num7 / num10);
					}
					return;
				}
				float sqrMagnitude3 = vector2.sqrMagnitude;
				if (num9 >= sqrMagnitude3)
				{
					vector3 = particlePosition - c1Ary[index];
					float sqrMagnitude4 = vector3.sqrMagnitude;
					if (sqrMagnitude4 > 0f && sqrMagnitude4 < num8)
					{
						float num11 = Mathf.Sqrt(sqrMagnitude4);
						particlePosition = c1Ary[index] + vector3 * (num7 / num11);
					}
				}
				else if (sqrMagnitude3 > 0f)
				{
					num9 /= sqrMagnitude3;
					vector3 -= vector2 * num9;
					float sqrMagnitude5 = vector3.sqrMagnitude;
					if (sqrMagnitude5 > 0f && sqrMagnitude5 < num8)
					{
						float num12 = Mathf.Sqrt(sqrMagnitude5);
						particlePosition += vector3 * ((num7 - num12) / num12);
					}
				}
				return;
			}
			float num13 = radiusAry[index] - particleRadiusAry[index];
			float num14 = num13 * num13;
			Vector3 vector4 = c1Ary[index] - c0Ary[index];
			Vector3 vector5 = particlePosition - c0Ary[index];
			float num15 = Vector3.Dot(vector5, vector4);
			if (num15 <= 0f)
			{
				float sqrMagnitude6 = vector5.sqrMagnitude;
				if (sqrMagnitude6 > num14)
				{
					float num16 = Mathf.Sqrt(sqrMagnitude6);
					particlePosition = c0Ary[index] + vector5 * (num13 / num16);
				}
				return;
			}
			float sqrMagnitude7 = vector4.sqrMagnitude;
			if (num15 >= sqrMagnitude7)
			{
				vector5 = particlePosition - c1Ary[index];
				float sqrMagnitude8 = vector5.sqrMagnitude;
				if (sqrMagnitude8 > num14)
				{
					float num17 = Mathf.Sqrt(sqrMagnitude8);
					particlePosition = c1Ary[index] + vector5 * (num13 / num17);
				}
			}
			else if (sqrMagnitude7 > 0f)
			{
				num15 /= sqrMagnitude7;
				vector5 -= vector4 * num15;
				float sqrMagnitude9 = vector5.sqrMagnitude;
				if (sqrMagnitude9 > num14)
				{
					float num18 = Mathf.Sqrt(sqrMagnitude9);
					particlePosition += vector5 * ((num13 - num18) / num18);
				}
			}
		}
	}

	public Transform m_Root;

	public float m_UpdateRate = 60f;

	public UpdateMode m_UpdateMode;

	[Range(0f, 1f)]
	public float m_Damping = 0.1f;

	public AnimationCurve m_DampingDistrib;

	[Range(0f, 1f)]
	public float m_Elasticity = 0.1f;

	public AnimationCurve m_ElasticityDistrib;

	[Range(0f, 1f)]
	public float m_Stiffness = 0.1f;

	public AnimationCurve m_StiffnessDistrib;

	[Range(0f, 1f)]
	public float m_Inert;

	public AnimationCurve m_InertDistrib;

	public float m_Radius;

	public AnimationCurve m_RadiusDistrib;

	public float m_EndLength;

	public Vector3 m_EndOffset = Vector3.zero;

	public Vector3 m_Gravity = Vector3.zero;

	public Vector3 m_Force = Vector3.zero;

	public List<DynamicBoneCollider> m_Colliders;

	public List<Transform> m_Exclusions;

	public FreezeAxis m_FreezeAxis;

	public bool m_DistantDisable;

	public Transform m_ReferenceObject;

	public float m_DistanceToObject = 20f;

	public List<Transform> m_notRolls;

	private Vector3 m_LocalGravity = Vector3.zero;

	private Vector3 m_ObjectMove = Vector3.zero;

	private Vector3 m_ObjectPrevPosition = Vector3.zero;

	private float m_BoneTotalLength;

	private float m_ObjectScale = 1f;

	private float m_Time;

	private float m_Weight = 1f;

	private bool m_DistantDisabled;

	private List<Particle> m_Particles = new List<Particle>();

	private void Start()
	{
		SetupParticles();
		(from _ in Observable.EveryUpdate().TakeUntilDestroy(base.gameObject)
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			OnUpdate();
		});
		(from _ in Observable.EveryLateUpdate().TakeUntilDestroy(base.gameObject)
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			OnLateUpdate();
		});
	}

	private void FixedUpdate()
	{
		if (m_UpdateMode == UpdateMode.AnimatePhysics)
		{
			PreUpdate();
		}
	}

	private void OnUpdate()
	{
		if (m_UpdateMode != UpdateMode.AnimatePhysics)
		{
			PreUpdate();
		}
	}

	private void OnLateUpdate()
	{
		if (m_DistantDisable)
		{
			CheckDistance();
		}
		if (m_Weight > 0f && (!m_DistantDisable || !m_DistantDisabled))
		{
			float deltaTime = Time.deltaTime;
			UpdateDynamicBones(deltaTime);
		}
	}

	private void PreUpdate()
	{
		if (m_Weight > 0f && (!m_DistantDisable || !m_DistantDisabled))
		{
			InitTransforms();
		}
	}

	private void CheckDistance()
	{
		Transform referenceObject = m_ReferenceObject;
		if (referenceObject == null && Camera.main != null)
		{
			referenceObject = Camera.main.transform;
		}
		if (!(referenceObject != null))
		{
			return;
		}
		bool flag = (referenceObject.position - base.transform.position).sqrMagnitude > m_DistanceToObject * m_DistanceToObject;
		if (flag != m_DistantDisabled)
		{
			if (!flag)
			{
				ResetParticlesPosition();
			}
			m_DistantDisabled = flag;
		}
	}

	private void OnEnable()
	{
		ResetParticlesPosition();
	}

	private void OnDisable()
	{
		InitTransforms();
	}

	private void OnValidate()
	{
		m_UpdateRate = Mathf.Max(m_UpdateRate, 0f);
		m_Damping = Mathf.Clamp01(m_Damping);
		m_Elasticity = Mathf.Clamp01(m_Elasticity);
		m_Stiffness = Mathf.Clamp01(m_Stiffness);
		m_Inert = Mathf.Clamp01(m_Inert);
		m_Radius = Mathf.Max(m_Radius, 0f);
		if (Application.isEditor && Application.isPlaying)
		{
			InitTransforms();
			SetupParticles();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!base.enabled || m_Root == null)
		{
			return;
		}
		if (Application.isEditor && !Application.isPlaying && base.transform.hasChanged)
		{
			InitTransforms();
			SetupParticles();
		}
		Gizmos.color = Color.white;
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				Particle particle2 = m_Particles[particle.m_ParentIndex];
				Gizmos.DrawLine(particle.m_Position, particle2.m_Position);
			}
			if (particle.m_Radius > 0f)
			{
				Gizmos.DrawWireSphere(particle.m_Position, particle.m_Radius * m_ObjectScale);
			}
		}
	}

	public void SetWeight(float w)
	{
		if (m_Weight != w)
		{
			if (w == 0f)
			{
				InitTransforms();
			}
			else if (m_Weight == 0f)
			{
				ResetParticlesPosition();
			}
			m_Weight = w;
		}
	}

	public float GetWeight()
	{
		return m_Weight;
	}

	private void UpdateDynamicBones(float t)
	{
		if (m_Root == null)
		{
			return;
		}
		m_ObjectScale = Mathf.Abs(base.transform.lossyScale.x);
		m_ObjectMove = base.transform.position - m_ObjectPrevPosition;
		m_ObjectPrevPosition = base.transform.position;
		int num = 1;
		if (m_UpdateRate > 0f)
		{
			float num2 = 1f / m_UpdateRate;
			m_Time += t;
			num = 0;
			while (m_Time >= num2)
			{
				m_Time -= num2;
				if (++num >= 3)
				{
					m_Time = 0f;
					break;
				}
			}
		}
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				UpdateParticles1();
				UpdateParticles2();
				m_ObjectMove = Vector3.zero;
			}
		}
		else
		{
			SkipUpdateParticles();
		}
		ApplyParticlesToTransforms();
	}

	private void SetupParticles()
	{
		m_Particles.Clear();
		if (m_Root == null)
		{
			return;
		}
		m_LocalGravity = m_Root.InverseTransformDirection(m_Gravity);
		m_ObjectScale = Mathf.Abs(base.transform.lossyScale.x);
		m_ObjectPrevPosition = base.transform.position;
		m_ObjectMove = Vector3.zero;
		m_BoneTotalLength = 0f;
		AppendParticles(m_Root, -1, 0f);
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			particle.m_Damping = m_Damping;
			particle.m_Elasticity = m_Elasticity;
			particle.m_Stiffness = m_Stiffness;
			particle.m_Inert = m_Inert;
			particle.m_Radius = m_Radius;
			if (m_BoneTotalLength > 0f)
			{
				float time = particle.m_BoneLength / m_BoneTotalLength;
				if (m_DampingDistrib != null && m_DampingDistrib.keys.Length != 0)
				{
					particle.m_Damping *= m_DampingDistrib.Evaluate(time);
				}
				if (m_ElasticityDistrib != null && m_ElasticityDistrib.keys.Length != 0)
				{
					particle.m_Elasticity *= m_ElasticityDistrib.Evaluate(time);
				}
				if (m_StiffnessDistrib != null && m_StiffnessDistrib.keys.Length != 0)
				{
					particle.m_Stiffness *= m_StiffnessDistrib.Evaluate(time);
				}
				if (m_InertDistrib != null && m_InertDistrib.keys.Length != 0)
				{
					particle.m_Inert *= m_InertDistrib.Evaluate(time);
				}
				if (m_RadiusDistrib != null && m_RadiusDistrib.keys.Length != 0)
				{
					particle.m_Radius *= m_RadiusDistrib.Evaluate(time);
				}
			}
			particle.m_Damping = Mathf.Clamp01(particle.m_Damping);
			particle.m_Elasticity = Mathf.Clamp01(particle.m_Elasticity);
			particle.m_Stiffness = Mathf.Clamp01(particle.m_Stiffness);
			particle.m_Inert = Mathf.Clamp01(particle.m_Inert);
			particle.m_Radius = Mathf.Max(particle.m_Radius, 0f);
		}
	}

	private void AppendParticles(Transform b, int parentIndex, float boneLength)
	{
		Particle particle = new Particle();
		particle.m_Transform = b;
		particle.m_ParentIndex = parentIndex;
		if (b != null)
		{
			particle.m_Position = (particle.m_PrevPosition = b.position);
			particle.m_InitLocalPosition = b.localPosition;
			particle.m_InitLocalRotation = b.localRotation;
		}
		else
		{
			Transform transform = m_Particles[parentIndex].m_Transform;
			if (m_EndLength > 0f)
			{
				Transform parent = transform.parent;
				if (parent != null)
				{
					particle.m_EndOffset = transform.InverseTransformPoint(transform.position * 2f - parent.position) * m_EndLength;
				}
				else
				{
					particle.m_EndOffset = new Vector3(m_EndLength, 0f, 0f);
				}
			}
			else
			{
				particle.m_EndOffset = transform.InverseTransformPoint(base.transform.TransformDirection(m_EndOffset) + transform.position);
			}
			particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset));
		}
		if (parentIndex >= 0)
		{
			boneLength += (m_Particles[parentIndex].m_Transform.position - particle.m_Position).magnitude;
			particle.m_BoneLength = boneLength;
			m_BoneTotalLength = Mathf.Max(m_BoneTotalLength, boneLength);
		}
		int count = m_Particles.Count;
		m_Particles.Add(particle);
		bool flag = false;
		int index = 0;
		if (!(b != null))
		{
			return;
		}
		for (int i = 0; i < b.childCount; i++)
		{
			bool flag2 = false;
			if (m_Exclusions != null)
			{
				for (int j = 0; j < m_Exclusions.Count; j++)
				{
					if (m_Exclusions[j] == b.GetChild(i))
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int k = 0; k < m_notRolls.Count; k++)
				{
					if (m_notRolls[k] == b.GetChild(i))
					{
						flag = true;
						flag2 = true;
						index = i;
						break;
					}
				}
			}
			if (!flag2)
			{
				AppendParticles(b.GetChild(i), count, boneLength);
			}
		}
		if (flag)
		{
			for (int l = 0; l < b.GetChild(index).childCount; l++)
			{
				bool flag3 = false;
				for (int m = 0; m < m_Exclusions.Count; m++)
				{
					if (m_Exclusions[m] == b.GetChild(index).GetChild(l))
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					for (int n = 0; n < m_notRolls.Count; n++)
					{
						if (m_notRolls[n] == b.GetChild(index).GetChild(l))
						{
							flag = true;
							flag3 = true;
							break;
						}
					}
				}
				if (!flag3)
				{
					AppendParticles(b.GetChild(index).GetChild(l), count, boneLength);
				}
			}
		}
		if (b.childCount == 0 && (m_EndLength > 0f || m_EndOffset != Vector3.zero))
		{
			AppendParticles(null, count, boneLength);
		}
	}

	private void InitTransforms()
	{
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_Transform != null)
			{
				particle.m_Transform.localPosition = particle.m_InitLocalPosition;
				particle.m_Transform.localRotation = particle.m_InitLocalRotation;
			}
		}
	}

	public void ResetParticlesPosition()
	{
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_Transform != null)
			{
				particle.m_Position = (particle.m_PrevPosition = particle.m_Transform.position);
				continue;
			}
			Transform transform = m_Particles[particle.m_ParentIndex].m_Transform;
			particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset));
		}
		m_ObjectPrevPosition = base.transform.position;
	}

	private void UpdateParticles1()
	{
		Vector3 gravity = m_Gravity;
		Vector3 normalized = m_Gravity.normalized;
		Vector3 lhs = m_Root.TransformDirection(m_LocalGravity);
		Vector3 vector = normalized * Mathf.Max(Vector3.Dot(lhs, normalized), 0f);
		gravity -= vector;
		gravity = (gravity + m_Force) * m_ObjectScale;
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				Vector3 vector2 = particle.m_Position - particle.m_PrevPosition;
				Vector3 vector3 = m_ObjectMove * particle.m_Inert;
				particle.m_PrevPosition = particle.m_Position + vector3;
				particle.m_Position += vector2 * (1f - particle.m_Damping) + gravity + vector3;
			}
			else
			{
				particle.m_PrevPosition = particle.m_Position;
				particle.m_Position = particle.m_Transform.position;
			}
		}
	}

	private void UpdateParticles2WithJob()
	{
		Plane plane = default(Plane);
		for (int i = 1; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			Particle particle2 = m_Particles[particle.m_ParentIndex];
			float num = ((!(particle.m_Transform != null)) ? particle2.m_Transform.localToWorldMatrix.MultiplyVector(particle.m_EndOffset).magnitude : (particle2.m_Transform.position - particle.m_Transform.position).magnitude);
			float num2 = Mathf.Lerp(1f, particle.m_Stiffness, m_Weight);
			if (num2 > 0f || particle.m_Elasticity > 0f)
			{
				Matrix4x4 localToWorldMatrix = particle2.m_Transform.localToWorldMatrix;
				localToWorldMatrix.SetColumn(3, particle2.m_Position);
				Vector3 vector = ((!(particle.m_Transform != null)) ? localToWorldMatrix.MultiplyPoint3x4(particle.m_EndOffset) : localToWorldMatrix.MultiplyPoint3x4(particle.m_Transform.localPosition));
				Vector3 vector2 = vector - particle.m_Position;
				particle.m_Position += vector2 * particle.m_Elasticity;
				if (num2 > 0f)
				{
					vector2 = vector - particle.m_Position;
					float magnitude = vector2.magnitude;
					float num3 = num * (1f - num2) * 2f;
					if (magnitude > num3)
					{
						particle.m_Position += vector2 * ((magnitude - num3) / magnitude);
					}
				}
			}
			if (m_Colliders != null)
			{
				int count = m_Colliders.Count;
				NativeArray<DynamicBoneColliderBase.Bound> boundAry = new NativeArray<DynamicBoneColliderBase.Bound>(count, Allocator.Temp);
				NativeArray<float> particleRadiusAry = new NativeArray<float>(count, Allocator.Temp);
				NativeArray<float> capsuleHeightAry = new NativeArray<float>(count, Allocator.Temp);
				NativeArray<Vector3> centerAry = new NativeArray<Vector3>(count, Allocator.Temp);
				NativeArray<Vector3> c0Ary = new NativeArray<Vector3>(count, Allocator.Temp);
				NativeArray<Vector3> c1Ary = new NativeArray<Vector3>(count, Allocator.Temp);
				NativeArray<float> radiusAry = new NativeArray<float>(count, Allocator.Temp);
				NativeArray<bool> enabledAry = new NativeArray<bool>(count, Allocator.Temp);
				ColliderJob jobData = new ColliderJob
				{
					boundAry = boundAry,
					particlePosition = particle.m_Position,
					particleRadiusAry = particleRadiusAry,
					capsuleHeightAry = capsuleHeightAry,
					centerAry = centerAry,
					c0Ary = c0Ary,
					c1Ary = c1Ary,
					radiusAry = radiusAry,
					enabledAry = enabledAry
				};
				jobData.Schedule(count, 0).Complete();
				particle.m_Position = jobData.particlePosition;
				boundAry.Dispose();
				particleRadiusAry.Dispose();
				capsuleHeightAry.Dispose();
				centerAry.Dispose();
				c0Ary.Dispose();
				c1Ary.Dispose();
				radiusAry.Dispose();
				enabledAry.Dispose();
			}
			if (m_FreezeAxis != FreezeAxis.None)
			{
				switch (m_FreezeAxis)
				{
				case FreezeAxis.X:
					plane.SetNormalAndPosition(particle2.m_Transform.right, particle2.m_Position);
					break;
				case FreezeAxis.Y:
					plane.SetNormalAndPosition(particle2.m_Transform.up, particle2.m_Position);
					break;
				case FreezeAxis.Z:
					plane.SetNormalAndPosition(particle2.m_Transform.forward, particle2.m_Position);
					break;
				}
				particle.m_Position -= plane.normal * plane.GetDistanceToPoint(particle.m_Position);
			}
			Vector3 vector3 = particle2.m_Position - particle.m_Position;
			float magnitude2 = vector3.magnitude;
			if (magnitude2 > 0f)
			{
				particle.m_Position += vector3 * ((magnitude2 - num) / magnitude2);
			}
		}
	}

	private void UpdateParticles2()
	{
		Plane plane = default(Plane);
		for (int i = 1; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			Particle particle2 = m_Particles[particle.m_ParentIndex];
			float num = ((!(particle.m_Transform != null)) ? particle2.m_Transform.localToWorldMatrix.MultiplyVector(particle.m_EndOffset).magnitude : (particle2.m_Transform.position - particle.m_Transform.position).magnitude);
			float num2 = Mathf.Lerp(1f, particle.m_Stiffness, m_Weight);
			if (num2 > 0f || particle.m_Elasticity > 0f)
			{
				Matrix4x4 localToWorldMatrix = particle2.m_Transform.localToWorldMatrix;
				localToWorldMatrix.SetColumn(3, particle2.m_Position);
				Vector3 vector = ((!(particle.m_Transform != null)) ? localToWorldMatrix.MultiplyPoint3x4(particle.m_EndOffset) : localToWorldMatrix.MultiplyPoint3x4(particle.m_Transform.localPosition));
				Vector3 vector2 = vector - particle.m_Position;
				particle.m_Position += vector2 * particle.m_Elasticity;
				if (num2 > 0f)
				{
					vector2 = vector - particle.m_Position;
					float magnitude = vector2.magnitude;
					float num3 = num * (1f - num2) * 2f;
					if (magnitude > num3)
					{
						particle.m_Position += vector2 * ((magnitude - num3) / magnitude);
					}
				}
			}
			if (m_Colliders != null)
			{
				float particleRadius = particle.m_Radius * m_ObjectScale;
				for (int j = 0; j < m_Colliders.Count; j++)
				{
					DynamicBoneCollider dynamicBoneCollider = m_Colliders[j];
					if (dynamicBoneCollider != null && dynamicBoneCollider.enabled)
					{
						dynamicBoneCollider.Collide(ref particle.m_Position, particleRadius);
					}
				}
			}
			if (m_FreezeAxis != FreezeAxis.None)
			{
				switch (m_FreezeAxis)
				{
				case FreezeAxis.X:
					plane.SetNormalAndPosition(particle2.m_Transform.right, particle2.m_Position);
					break;
				case FreezeAxis.Y:
					plane.SetNormalAndPosition(particle2.m_Transform.up, particle2.m_Position);
					break;
				case FreezeAxis.Z:
					plane.SetNormalAndPosition(particle2.m_Transform.forward, particle2.m_Position);
					break;
				}
				particle.m_Position -= plane.normal * plane.GetDistanceToPoint(particle.m_Position);
			}
			Vector3 vector3 = particle2.m_Position - particle.m_Position;
			float magnitude2 = vector3.magnitude;
			if (magnitude2 > 0f)
			{
				particle.m_Position += vector3 * ((magnitude2 - num) / magnitude2);
			}
		}
	}

	private void SkipUpdateParticles()
	{
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				particle.m_PrevPosition += m_ObjectMove;
				particle.m_Position += m_ObjectMove;
				Particle particle2 = m_Particles[particle.m_ParentIndex];
				float num = ((!(particle.m_Transform != null)) ? particle2.m_Transform.localToWorldMatrix.MultiplyVector(particle.m_EndOffset).magnitude : (particle2.m_Transform.position - particle.m_Transform.position).magnitude);
				float num2 = Mathf.Lerp(1f, particle.m_Stiffness, m_Weight);
				if (num2 > 0f)
				{
					Matrix4x4 localToWorldMatrix = particle2.m_Transform.localToWorldMatrix;
					localToWorldMatrix.SetColumn(3, particle2.m_Position);
					Vector3 vector = ((!(particle.m_Transform != null)) ? localToWorldMatrix.MultiplyPoint3x4(particle.m_EndOffset) : localToWorldMatrix.MultiplyPoint3x4(particle.m_Transform.localPosition));
					Vector3 vector2 = vector - particle.m_Position;
					float magnitude = vector2.magnitude;
					float num3 = num * (1f - num2) * 2f;
					if (magnitude > num3)
					{
						particle.m_Position += vector2 * ((magnitude - num3) / magnitude);
					}
				}
				Vector3 vector3 = particle2.m_Position - particle.m_Position;
				float magnitude2 = vector3.magnitude;
				if (magnitude2 > 0f)
				{
					particle.m_Position += vector3 * ((magnitude2 - num) / magnitude2);
				}
			}
			else
			{
				particle.m_PrevPosition = particle.m_Position;
				particle.m_Position = particle.m_Transform.position;
			}
		}
	}

	private static Vector3 MirrorVector(Vector3 v, Vector3 axis)
	{
		return v - axis * (Vector3.Dot(v, axis) * 2f);
	}

	private void ApplyParticlesToTransforms()
	{
		for (int i = 1; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			Particle particle2 = m_Particles[particle.m_ParentIndex];
			if (particle2.m_Transform.childCount <= 1)
			{
				Vector3 direction = ((!(particle.m_Transform != null)) ? particle.m_EndOffset : particle.m_Transform.localPosition);
				Vector3 toDirection = particle.m_Position - particle2.m_Position;
				Quaternion quaternion = Quaternion.FromToRotation(particle2.m_Transform.TransformDirection(direction), toDirection);
				particle2.m_Transform.rotation = quaternion * particle2.m_Transform.rotation;
			}
			if (particle.m_Transform != null)
			{
				particle.m_Transform.position = particle.m_Position;
			}
		}
	}
}
