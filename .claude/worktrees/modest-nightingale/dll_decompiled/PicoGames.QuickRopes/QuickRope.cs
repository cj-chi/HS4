using System;
using PicoGames.Utilities;
using UnityEngine;

namespace PicoGames.QuickRopes;

[AddComponentMenu("PicoGames/QuickRopes/QuickRope")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Spline))]
public class QuickRope : MonoBehaviour
{
	public enum RenderType
	{
		Prefab,
		Rendered
	}

	public enum ColliderType
	{
		None,
		Sphere,
		Box,
		Capsule
	}

	private const int MAX_JOINT_COUNT = 128;

	private const float MIN_JOINT_SCALE = 0.001f;

	private const float MIN_JOINT_SPACING = 0.001f;

	private const string VERSION = "3.1.6";

	[SerializeField]
	[Min(0.001f)]
	public float linkScale = 1f;

	[SerializeField]
	[Min(0.001f)]
	public float linkSpacing = 0.5f;

	[SerializeField]
	[Min(3f)]
	public int maxLinkCount = 50;

	[SerializeField]
	[Min(1f)]
	public int minLinkCount = 1;

	[SerializeField]
	public bool alternateRotation = true;

	[SerializeField]
	public bool usePhysics = true;

	[SerializeField]
	public bool canResize;

	[SerializeField]
	public GameObject defaultPrefab;

	[SerializeField]
	public TransformSettings defaultPrefabOffsets = new TransformSettings
	{
		position = new Vector3(0f, 0.25f, 0f),
		scale = Vector3.one
	};

	[SerializeField]
	public RigidbodySettings defaultRigidbodySettings = new RigidbodySettings
	{
		mass = 1f,
		drag = 0.1f,
		angularDrag = 0.05f,
		useGravity = true,
		isKinematic = false,
		solverCount = 6
	};

	[SerializeField]
	public JointSettings defaultJointSettings = new JointSettings
	{
		breakForce = float.PositiveInfinity,
		breakTorque = float.PositiveInfinity,
		swingLimit = 90f,
		twistLimit = 40f
	};

	[SerializeField]
	public ColliderSettings defaultColliderSettings = new ColliderSettings
	{
		size = Vector3.one,
		height = 2f,
		center = Vector3.zero,
		radius = 1f
	};

	[SerializeField]
	public float velocityAccel = 1f;

	[SerializeField]
	public float velocityDampen = 0.98f;

	[SerializeField]
	[HideInInspector]
	private float velocity;

	[SerializeField]
	[HideInInspector]
	private float kVelocity;

	[SerializeField]
	[HideInInspector]
	private int activeLinkCount;

	[SerializeField]
	private Link[] links = new Link[0];

	[SerializeField]
	[HideInInspector]
	private Spline spline;

	public Spline Spline
	{
		get
		{
			if (spline == null)
			{
				spline = base.gameObject.GetComponent<Spline>();
				if (spline == null)
				{
					spline = base.gameObject.AddComponent<Spline>();
					spline.Reset();
				}
			}
			return spline;
		}
	}

	public int ActiveLinkCount => activeLinkCount;

	public Link[] Links => links;

	public float Velocity
	{
		get
		{
			return velocity;
		}
		set
		{
			velocity = value;
		}
	}

	private void Reset()
	{
		if (!Application.isPlaying)
		{
			ClearLinks();
			Spline.Reset();
		}
	}

	public void Generate()
	{
		if (spline.IsLooped)
		{
			canResize = false;
		}
		SplinePoint[] array = ResizeLinkArray();
		if (array.Length == 0)
		{
			return;
		}
		Rigidbody connectedBody = null;
		for (int num = links.Length - 1; num >= 0; num--)
		{
			links[num].IsActive = num < activeLinkCount || num == links.Length - 1;
			links[num].transform.localScale = Vector3.one * linkScale;
			links[num].gameObject.layer = base.gameObject.layer;
			links[num].gameObject.tag = base.gameObject.tag;
			if (num < array.Length - 1)
			{
				links[num].transform.rotation = base.transform.rotation * array[num].rotation;
				links[num].transform.position = base.transform.TransformPoint(array[num].position);
			}
			else if (num == links.Length - 1)
			{
				links[num].transform.rotation = base.transform.rotation * array[array.Length - 1].rotation;
				links[num].transform.position = base.transform.TransformPoint(array[array.Length - 1].position);
			}
			if (num != links.Length - 1)
			{
				if (!links[num].overridePrefab)
				{
					links[num].prefab = defaultPrefab;
				}
				if (!links[num].overrideOffsetSettings)
				{
					links[num].offsetSettings = defaultPrefabOffsets;
				}
				if (links[num].AttachedPrefab() != null)
				{
					links[num].AttachedPrefab().hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
				}
			}
			links[num].alternateJoints = alternateRotation;
			links[num].ApplyPrefabSettings();
			if (!links[num].overrideRigidbodySettings)
			{
				links[num].rigidbodySettings = defaultRigidbodySettings;
			}
			links[num].ApplyRigidbodySettings();
			if (num != links.Length - 1)
			{
				if (!links[num].overrideJointSettings)
				{
					links[num].jointSettings = defaultJointSettings;
				}
				links[num].ApplyJointSettings(linkSpacing * (1f / linkScale));
			}
			if (!links[num].overrideColliderSettings)
			{
				links[num].colliderSettings = defaultColliderSettings;
			}
			links[num].ApplyColliderSettings();
			if (links[num].TogglePhysicsEnabled(usePhysics))
			{
				links[num].ConnectedBody = connectedBody;
				connectedBody = links[num].Rigidbody;
			}
		}
		if (!usePhysics)
		{
			return;
		}
		if (spline.IsLooped)
		{
			links[links.Length - 1].IsActive = false;
			activeLinkCount--;
			links[links.Length - 2].ConnectedBody = links[0].Rigidbody;
			return;
		}
		links[links.Length - 1].RemoveJoint();
		links[links.Length - 1].IsPrefabActive = false;
		if (canResize && activeLinkCount != links.Length)
		{
			links[activeLinkCount - 1].ConnectedBody = links[links.Length - 1].Rigidbody;
		}
	}

	private SplinePoint[] ResizeLinkArray()
	{
		maxLinkCount = Mathf.Min(maxLinkCount, 128);
		SplinePoint[] array = Spline.GetSpacedPointsReversed(linkSpacing);
		activeLinkCount = Mathf.Min(maxLinkCount, array.Length) - 1;
		int num = (canResize ? maxLinkCount : array.Length);
		if (num <= 0 && links.Length != 0)
		{
			for (int i = 0; i < links.Length; i++)
			{
				links[i].Destroy();
			}
			links = new Link[0];
			array = new SplinePoint[0];
		}
		else if (links.Length != num)
		{
			if (num > links.Length)
			{
				int num2 = links.Length;
				Array.Resize(ref links, num);
				for (int j = num2; j < num; j++)
				{
					links[j] = new Link(new GameObject("Link_[" + j + "]"), j);
					links[j].Parent = base.transform;
				}
			}
			else if (num > 0)
			{
				for (int num3 = links.Length - 1; num3 >= num; num3--)
				{
					links[num3].Destroy();
				}
				Array.Resize(ref links, num);
			}
		}
		return array;
	}

	private void ClearLinks()
	{
		if (Application.isPlaying)
		{
			return;
		}
		while (base.transform.childCount > 0)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(base.transform.GetChild(0).gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.transform.GetChild(0).gameObject);
			}
		}
		links = new Link[0];
	}

	private void FixedUpdate()
	{
		if (velocity != 0f)
		{
			kVelocity = Mathf.Lerp(kVelocity, velocity, Time.deltaTime * velocityAccel);
		}
		else
		{
			kVelocity = Mathf.Lerp(kVelocity, velocity, Time.deltaTime * velocityDampen);
		}
		if (kVelocity > 0f)
		{
			kVelocity = (Extend(kVelocity) ? kVelocity : 0f);
		}
		if (kVelocity < 0f)
		{
			kVelocity = (Retract(kVelocity, minLinkCount) ? kVelocity : 0f);
		}
	}

	public bool Extend(float _speed)
	{
		if (!canResize)
		{
			return false;
		}
		Link link = links[links.Length - 1];
		Link link2 = links[activeLinkCount - 1];
		Vector3 target = link.transform.position - link2.transform.up * linkSpacing * 2f;
		if (activeLinkCount < maxLinkCount - 1)
		{
			link2.ConnectedBody = null;
			link2.transform.position = Vector3.MoveTowards(link2.transform.position, target, Mathf.Abs(_speed) * Time.deltaTime);
			if (Vector3.SqrMagnitude(link2.transform.position - link.transform.position) > linkSpacing * linkSpacing)
			{
				Link link3 = links[activeLinkCount];
				link3.transform.position = link2.transform.position + (link.transform.position - link2.transform.position).normalized * linkSpacing;
				link3.transform.rotation = link2.transform.rotation;
				activeLinkCount++;
				link3.IsActive = true;
				link2.ConnectedBody = link3.Rigidbody;
				link2 = link3;
			}
			link2.ApplyJointSettings(Vector3.Distance(link.transform.position, link2.transform.position) * (1f / linkScale));
			link2.ConnectedBody = link.Rigidbody;
		}
		else
		{
			kVelocity = 0f;
		}
		return true;
	}

	public bool Retract(float _speed, int _minJointCount)
	{
		if (!canResize)
		{
			return false;
		}
		Link link = links[links.Length - 1];
		Link link2 = links[activeLinkCount - 1];
		link2.ConnectedBody = null;
		link2.transform.position = Vector3.MoveTowards(link2.transform.position, link.transform.position, Mathf.Abs(_speed) * Time.deltaTime);
		if (activeLinkCount > _minJointCount)
		{
			if (Vector3.SqrMagnitude(link.transform.position - link2.transform.position) <= 0.01f)
			{
				link2.IsActive = false;
				activeLinkCount--;
				link2 = links[activeLinkCount - 1];
			}
		}
		else
		{
			kVelocity = 0f;
			link2.transform.position = link.transform.position - link2.transform.up * linkSpacing;
		}
		link2.Joint.anchor = new Vector3(0f, Vector3.Distance(link.transform.position, link2.transform.position) * (1f / linkScale), 0f);
		link2.ConnectedBody = link.Rigidbody;
		return true;
	}

	public static QuickRope Create(Vector3 _position, GameObject _prefab, float _linkSpacing = 1f, float _prefabScale = 0.5f)
	{
		return Create(_position, _position + new Vector3(0f, 5f, 0f), _prefab, _linkSpacing, _prefabScale);
	}

	public static QuickRope Create(Vector3 _pointA, Vector3 _pointB, GameObject _prefab, float _linkSpacing = 1f, float _prefabScale = 0.5f)
	{
		GameObject obj = new GameObject("Rope");
		obj.transform.position = _pointA;
		QuickRope quickRope = obj.AddComponent<QuickRope>();
		quickRope.defaultPrefab = _prefab;
		Vector3 normalized = (_pointB - _pointA).normalized;
		quickRope.Spline.SetControlPoint(1, _pointB, Space.World);
		quickRope.Spline.SetPoint(1, _pointA + normalized, Space.World);
		quickRope.Spline.SetPoint(2, _pointB - normalized, Space.World);
		quickRope.Generate();
		return quickRope;
	}
}
