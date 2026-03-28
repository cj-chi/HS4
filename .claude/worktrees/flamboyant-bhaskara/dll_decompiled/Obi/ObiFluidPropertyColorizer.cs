using System;
using UnityEngine;

namespace Obi;

[RequireComponent(typeof(ObiEmitter))]
public class ObiFluidPropertyColorizer : MonoBehaviour
{
	private ObiEmitter emitter;

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
				float time = emitter.Solver.userData[emitter.particleIndices[i]][0];
				emitter.colors[i] = grad.Evaluate(time);
			}
		}
	}
}
