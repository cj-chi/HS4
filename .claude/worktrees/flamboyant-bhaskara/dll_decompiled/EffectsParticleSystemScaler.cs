using UnityEngine;

[ExecuteInEditMode]
public class EffectsParticleSystemScaler : MonoBehaviour
{
	public float particlesScale = 1f;

	private float oldScale;

	private void Start()
	{
		oldScale = particlesScale;
	}

	private void Update()
	{
	}
}
