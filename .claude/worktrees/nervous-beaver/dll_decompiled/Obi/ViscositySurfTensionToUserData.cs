using UnityEngine;

namespace Obi;

[RequireComponent(typeof(ObiEmitter))]
public class ViscositySurfTensionToUserData : MonoBehaviour
{
	private ObiEmitter emitter;

	private void Awake()
	{
		emitter = GetComponent<ObiEmitter>();
		emitter.OnEmitParticle += Emitter_OnEmitParticle;
	}

	private void Emitter_OnEmitParticle(object sender, ObiEmitter.ObiParticleEventArgs e)
	{
		if (emitter.Solver != null)
		{
			int index = emitter.particleIndices[e.index];
			Vector4 value = emitter.Solver.userData[index];
			value[0] = emitter.Solver.viscosities[index];
			value[1] = emitter.Solver.surfaceTension[index];
			emitter.Solver.userData[index] = value;
		}
	}
}
