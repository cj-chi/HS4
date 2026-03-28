using System;
using UnityEngine;

namespace PicoGames.QuickRopes;

[Serializable]
public class Link
{
	[SerializeField]
	public bool overridePrefab;

	[SerializeField]
	public bool overrideOffsetSettings;

	[SerializeField]
	public bool overrideRigidbodySettings;

	[SerializeField]
	public bool overrideJointSettings;

	[SerializeField]
	public bool overrideColliderSettings;

	[SerializeField]
	public GameObject prefab;

	[SerializeField]
	public TransformSettings offsetSettings;

	[SerializeField]
	public RigidbodySettings rigidbodySettings;

	[SerializeField]
	public JointSettings jointSettings;

	[SerializeField]
	public ColliderSettings colliderSettings;

	[SerializeField]
	public bool alternateJoints = true;

	[SerializeField]
	public GameObject gameObject;

	[SerializeField]
	public Collider collider;

	[SerializeField]
	private GameObject attachedPrefab;

	[SerializeField]
	private Rigidbody rigidbody;

	[SerializeField]
	private ConfigurableJoint joint;

	[SerializeField]
	private int linkIndex = -1;

	[SerializeField]
	private bool prevIsKinematic;

	[SerializeField]
	public Transform transform => gameObject.transform;

	public GameObject Prefab
	{
		get
		{
			return prefab;
		}
		set
		{
			prefab = value;
		}
	}

	public Transform Parent
	{
		get
		{
			return transform.parent;
		}
		set
		{
			transform.parent = value;
		}
	}

	public Rigidbody ConnectedBody
	{
		get
		{
			if (joint == null)
			{
				return null;
			}
			return joint.connectedBody;
		}
		set
		{
			if (!(joint == null))
			{
				joint.connectedBody = value;
			}
		}
	}

	public bool IsActive
	{
		get
		{
			if (gameObject == null)
			{
				return false;
			}
			return gameObject.activeSelf;
		}
		set
		{
			if (!(gameObject == null))
			{
				gameObject.hideFlags = ((!value) ? HideFlags.HideInHierarchy : HideFlags.None);
				gameObject.SetActive(value);
				IsPrefabActive = true;
			}
		}
	}

	public bool IsPrefabActive
	{
		get
		{
			if (attachedPrefab != null)
			{
				return attachedPrefab.activeSelf;
			}
			return false;
		}
		set
		{
			if (!(attachedPrefab == null))
			{
				attachedPrefab.SetActive(value);
			}
		}
	}

	public Rigidbody Rigidbody => rigidbody;

	public ConfigurableJoint Joint => joint;

	public Link(GameObject _gameObject, int _index)
	{
		gameObject = _gameObject;
		linkIndex = _index;
		IsActive = false;
	}

	public bool TogglePhysicsEnabled(bool _enabled)
	{
		if (_enabled)
		{
			Rigidbody.isKinematic = prevIsKinematic;
		}
		else
		{
			prevIsKinematic = Rigidbody.isKinematic;
			Rigidbody.isKinematic = true;
		}
		return _enabled;
	}

	public void ApplyPrefabSettings()
	{
		if (transform.childCount > 0)
		{
			if (!Application.isPlaying)
			{
				while (transform.childCount > 0)
				{
					UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject, allowDestroyingAssets: false);
				}
			}
			else
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
				}
			}
		}
		if (prefab != null)
		{
			if (prefab.activeInHierarchy)
			{
				attachedPrefab = prefab;
			}
			else
			{
				attachedPrefab = UnityEngine.Object.Instantiate(prefab);
			}
			attachedPrefab.name = prefab.name;
			attachedPrefab.transform.parent = transform;
			attachedPrefab.transform.localPosition = offsetSettings.position;
			attachedPrefab.transform.localRotation = offsetSettings.rotation * ((!alternateJoints) ? Quaternion.identity : Quaternion.AngleAxis((linkIndex % 2 == 0) ? 90 : 0, Vector3.up));
			attachedPrefab.transform.localScale = offsetSettings.scale;
		}
	}

	public void ApplyColliderSettings()
	{
		Collider[] components = gameObject.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(components[i]);
			}
		}
		switch (colliderSettings.type)
		{
		case QuickRope.ColliderType.None:
			collider = null;
			break;
		case QuickRope.ColliderType.Box:
		{
			collider = gameObject.AddComponent<BoxCollider>();
			BoxCollider obj3 = collider as BoxCollider;
			obj3.material = colliderSettings.physicsMaterial;
			obj3.size = colliderSettings.size;
			obj3.center = colliderSettings.center;
			break;
		}
		case QuickRope.ColliderType.Capsule:
		{
			collider = gameObject.AddComponent<CapsuleCollider>();
			CapsuleCollider obj2 = collider as CapsuleCollider;
			obj2.material = colliderSettings.physicsMaterial;
			obj2.radius = colliderSettings.radius;
			obj2.height = colliderSettings.height;
			obj2.direction = (int)colliderSettings.direction;
			obj2.center = colliderSettings.center;
			break;
		}
		case QuickRope.ColliderType.Sphere:
		{
			collider = gameObject.AddComponent<SphereCollider>();
			SphereCollider obj = collider as SphereCollider;
			obj.material = colliderSettings.physicsMaterial;
			obj.radius = colliderSettings.radius;
			obj.center = colliderSettings.center;
			break;
		}
		}
	}

	public void ApplyRigidbodySettings()
	{
		if (rigidbody == null)
		{
			rigidbody = gameObject.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = gameObject.AddComponent<Rigidbody>();
			}
		}
		prevIsKinematic = rigidbodySettings.isKinematic;
		rigidbody.mass = rigidbodySettings.mass;
		rigidbody.drag = rigidbodySettings.drag;
		rigidbody.angularDrag = rigidbodySettings.angularDrag;
		rigidbody.useGravity = rigidbodySettings.useGravity;
		rigidbody.isKinematic = rigidbodySettings.isKinematic;
		rigidbody.interpolation = rigidbodySettings.interpolate;
		rigidbody.collisionDetectionMode = rigidbodySettings.collisionDetection;
		rigidbody.constraints = rigidbodySettings.constraints;
		rigidbody.solverIterations = rigidbodySettings.solverCount;
		if (!rigidbody.isKinematic)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.Sleep();
		}
	}

	public void ApplyJointSettings(float _anchorOffset)
	{
		joint = gameObject.GetComponent<ConfigurableJoint>();
		if (joint == null)
		{
			joint = gameObject.AddComponent<ConfigurableJoint>();
		}
		joint.xMotion = ConfigurableJointMotion.Limited;
		joint.yMotion = ConfigurableJointMotion.Limited;
		joint.zMotion = ConfigurableJointMotion.Limited;
		joint.linearLimit = new SoftJointLimit
		{
			limit = jointSettings.swingLimit
		};
		joint.linearLimitSpring = new SoftJointLimitSpring
		{
			spring = jointSettings.spring,
			damper = jointSettings.damper
		};
	}

	public void RemoveJoint()
	{
		if (!(joint == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(joint);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(joint);
			}
			joint = null;
		}
	}

	public void RemoveRigidbody()
	{
		if (!(rigidbody == null))
		{
			RemoveJoint();
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(rigidbody);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(rigidbody);
			}
			rigidbody = null;
		}
	}

	public GameObject AttachedPrefab()
	{
		return attachedPrefab;
	}

	public void Destroy()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
		gameObject = null;
	}
}
