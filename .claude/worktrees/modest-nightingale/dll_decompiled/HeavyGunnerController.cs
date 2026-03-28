using System;
using System.Collections.Generic;
using SpriteToParticlesAsset;
using UnityEngine;

public class HeavyGunnerController : MonoBehaviour
{
	public List<SpriteToParticles> ShadowFxs;

	public List<SpriteToParticles> WeirdFxs;

	public SpriteToParticles GunPrep;

	public float Speed = 20f;

	public GameObject LookAtAim;

	public float RotationVelocity = 5f;

	private float wantedRotation;

	public float angleDisplacement = 180f;

	public Rigidbody2D rig;

	private Animator animator;

	private float ShootPrepTime;

	public GameObject BulletPrefab;

	public Transform BulletStartPos;

	public float bulletSpeed = 20f;

	public float bulletRotationSpeed = 20f;

	private void Start()
	{
		rig = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		float axis = Input.GetAxis("Vertical");
		float axis2 = Input.GetAxis("Horizontal");
		Vector2 vector = new Vector2(axis2, axis);
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		rig.velocity = new Vector3(vector.x * Speed, vector.y * Speed, 0f);
		animator.SetFloat("Speed", rig.velocity.magnitude);
		float num = Mathf.Atan2(LookAtAim.transform.position.y - base.transform.position.y, LookAtAim.transform.position.x - base.transform.position.x);
		wantedRotation = 180f / (float)Math.PI * num + angleDisplacement;
		Quaternion to = Quaternion.Euler(0f, 0f, wantedRotation);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, RotationVelocity * Time.deltaTime);
		if (Input.GetMouseButton(0))
		{
			ShootPrep();
		}
		if (Input.GetMouseButtonUp(0))
		{
			Shoot();
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			ShadowFXToggle();
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			WeirdFXToggle();
		}
	}

	public void ShootPrep()
	{
		ShootPrepTime += Time.deltaTime;
		if (ShootPrepTime > 0.1f)
		{
			if (!GunPrep.IsPlaying())
			{
				GunPrep.Play();
			}
			GunPrep.EmissionRate = ShootPrepTime * 1000f;
			if (GunPrep.EmissionRate > 10000f)
			{
				GunPrep.EmissionRate = 10000f;
			}
		}
	}

	public void Shoot()
	{
		animator.SetTrigger("Shoot");
		ShootPrepTime = 0f;
		GunPrep.Stop();
		GameObject obj = UnityEngine.Object.Instantiate(BulletPrefab);
		obj.transform.position = BulletStartPos.position;
		obj.GetComponent<Rigidbody2D>().AddForce(base.transform.up * bulletSpeed);
		obj.GetComponent<Rigidbody2D>().AddTorque(bulletRotationSpeed);
		UnityEngine.Object.Destroy(obj, 10f);
	}

	public void ShadowFXToggle()
	{
		if (ShadowFxs[0].IsPlaying())
		{
			foreach (SpriteToParticles shadowFx in ShadowFxs)
			{
				shadowFx.Stop();
			}
			return;
		}
		foreach (SpriteToParticles shadowFx2 in ShadowFxs)
		{
			shadowFx2.Play();
		}
	}

	public void WeirdFXToggle()
	{
		if (WeirdFxs[0].IsPlaying())
		{
			foreach (SpriteToParticles weirdFx in WeirdFxs)
			{
				weirdFx.Stop();
			}
			return;
		}
		foreach (SpriteToParticles weirdFx2 in WeirdFxs)
		{
			weirdFx2.Play();
		}
	}
}
