using Illusion.Extensions;
using UniRx;
using UnityEngine;

namespace Studio;

public class Wait : SingletonInitializer<Wait>
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Animator animator;

	private static BoolReactiveProperty activeReactive = new BoolReactiveProperty(initialValue: false);

	public static bool Active
	{
		get
		{
			return activeReactive.Value;
		}
		set
		{
			activeReactive.Value = value;
		}
	}

	protected override void Initialize()
	{
		activeReactive.Subscribe(delegate(bool _b)
		{
			canvasGroup.Enable(_b);
			animator.enabled = _b;
		});
	}
}
