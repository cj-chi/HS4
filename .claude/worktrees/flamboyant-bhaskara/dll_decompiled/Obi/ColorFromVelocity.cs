using System;
using UnityEngine;

namespace Obi;

[RequireComponent(typeof(ObiActor))]
public class ColorFromVelocity : MonoBehaviour
{
	private ObiActor actor;

	public float sensibility = 0.2f;

	private void Awake()
	{
		actor = GetComponent<ObiActor>();
		actor.OnAddedToSolver += Actor_OnAddedToSolver;
		actor.OnRemovedFromSolver += Actor_OnRemovedFromSolver;
	}

	private void Actor_OnAddedToSolver(object sender, ObiActor.ObiActorSolverArgs e)
	{
		e.Solver.OnFrameEnd += E_Solver_OnFrameEnd;
	}

	private void Actor_OnRemovedFromSolver(object sender, ObiActor.ObiActorSolverArgs e)
	{
		e.Solver.OnFrameEnd -= E_Solver_OnFrameEnd;
	}

	public void OnEnable()
	{
	}

	private void E_Solver_OnFrameEnd(object sender, EventArgs e)
	{
		if (base.isActiveAndEnabled && actor.colors != null)
		{
			for (int i = 0; i < actor.colors.Length; i++)
			{
				int index = actor.particleIndices[i];
				Vector4 vector = actor.Solver.velocities[index];
				actor.colors[i] = new Color(Mathf.Clamp(vector.x / sensibility, -1f, 1f) * 0.5f + 0.5f, Mathf.Clamp(vector.y / sensibility, -1f, 1f) * 0.5f + 0.5f, Mathf.Clamp(vector.z / sensibility, -1f, 1f) * 0.5f + 0.5f, 1f);
			}
		}
	}
}
