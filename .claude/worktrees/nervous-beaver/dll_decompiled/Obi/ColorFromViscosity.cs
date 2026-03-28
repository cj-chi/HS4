using System;
using UnityEngine;

namespace Obi;

[RequireComponent(typeof(ObiEmitter))]
public class ColorFromViscosity : MonoBehaviour
{
	private ObiEmitter emitter;

	public float min;

	public float max = 1f;

	public Gradient grad;

	private void Awake()
	{
		emitter = GetComponent<ObiEmitter>();
		emitter.OnAddedToSolver += Emitter_OnAddedToSolver;
		emitter.OnRemovedFromSolver += Emitter_OnRemovedFromSolver;
	}

	private void Emitter_OnAddedToSolver(object sender, ObiActor.ObiActorSolverArgs e)
	{
		e.Solver.OnFrameEnd += E_Solver_OnFrameEnd;
	}

	private void Emitter_OnRemovedFromSolver(object sender, ObiActor.ObiActorSolverArgs e)
	{
		e.Solver.OnFrameEnd -= E_Solver_OnFrameEnd;
	}

	public void OnEnable()
	{
	}

	private void E_Solver_OnFrameEnd(object sender, EventArgs e)
	{
		if (base.isActiveAndEnabled)
		{
			for (int i = 0; i < emitter.particleIndices.Length; i++)
			{
				int index = emitter.particleIndices[i];
				emitter.colors[i] = grad.Evaluate((emitter.Solver.viscosities[index] - min) / (max - min));
				emitter.Solver.viscosities[index] = emitter.Solver.userData[index][0];
				emitter.Solver.surfaceTension[index] = emitter.Solver.userData[index][1];
			}
		}
	}
}
