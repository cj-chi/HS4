using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Studio;

public class IconComponent : MonoBehaviour
{
	[SerializeField]
	private Renderer renderer;

	private Transform transTarget;

	private Transform transRender;

	public bool Active
	{
		set
		{
			renderer.gameObject.SetActive(value);
		}
	}

	public bool Visible
	{
		set
		{
			renderer.enabled = value;
		}
	}

	public int Layer
	{
		set
		{
			renderer.gameObject.layer = value;
		}
	}

	private void Awake()
	{
		transRender = renderer.transform;
		transTarget = Camera.main.transform;
		(from _ in this.UpdateAsObservable()
			where renderer.enabled
			select _).Subscribe(delegate
		{
			transRender.LookAt(transTarget.position);
		});
	}
}
