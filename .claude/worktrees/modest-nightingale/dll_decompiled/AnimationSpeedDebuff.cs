using UnityEngine;

public class AnimationSpeedDebuff : MonoBehaviour
{
	public AnimationCurve AnimationSpeenOnTime;

	public float MaxTime = 1f;

	private Animator myAnimator;

	private Transform root;

	private float oldSpeed;

	private float time;

	private void GetAnimatorOnParent(Transform t)
	{
		Animator component = t.parent.GetComponent<Animator>();
		if (component == null)
		{
			if (!(root == t.parent))
			{
				GetAnimatorOnParent(t.parent);
			}
		}
		else
		{
			myAnimator = component;
		}
	}

	private void Start()
	{
		root = base.transform.root;
		GetAnimatorOnParent(base.transform);
		if (!(myAnimator == null))
		{
			oldSpeed = myAnimator.speed;
		}
	}

	private void Update()
	{
		if (!(myAnimator == null) && AnimationSpeenOnTime.length != 0)
		{
			time += Time.deltaTime;
			myAnimator.speed = Mathf.Clamp01(AnimationSpeenOnTime.Evaluate(time / MaxTime) * oldSpeed);
		}
	}
}
