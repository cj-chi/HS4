using PicoGames.QuickRopes;
using UnityEngine;

[RequireComponent(typeof(QuickRope))]
public class RopeController : MonoBehaviour
{
	[Min(1f)]
	public int minJointCount = 3;

	[Min(0.001f)]
	public float maxSpeed = 5f;

	[Range(0f, 1f)]
	public float acceleration = 1f;

	[Range(0.001f, 1f)]
	public float dampening = 1f;

	private QuickRope rope;

	private void Awake()
	{
		rope = GetComponent<QuickRope>();
		if (rope.Spline.IsLooped)
		{
			base.enabled = false;
			return;
		}
		rope.minLinkCount = minJointCount;
		if (!rope.canResize)
		{
			rope.maxLinkCount = rope.Links.Length + 1;
			rope.canResize = true;
			rope.Generate();
		}
		rope.Links[rope.Links.Length - 1].Rigidbody.isKinematic = true;
	}

	private void Update()
	{
		rope.velocityAccel = acceleration;
		rope.velocityDampen = dampening;
		if (Input.GetKey(KeyCode.UpArrow))
		{
			rope.Velocity = maxSpeed;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			rope.Velocity = 0f - maxSpeed;
		}
		else
		{
			rope.Velocity = 0f;
		}
	}
}
