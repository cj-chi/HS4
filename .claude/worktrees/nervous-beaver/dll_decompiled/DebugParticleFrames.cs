using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiActor))]
public class DebugParticleFrames : MonoBehaviour
{
	private ObiActor actor;

	public float size = 0.1f;

	public void Awake()
	{
		actor = GetComponent<ObiActor>();
	}

	private void OnDrawGizmos()
	{
		if (!(actor == null) && actor.InSolver && actor.orientations != null)
		{
			for (int i = 0; i < actor.orientations.Length; i++)
			{
				int index = actor.particleIndices[i];
				Vector3 vector = actor.Solver.positions[index];
				Quaternion quaternion = actor.Solver.orientations[index];
				Gizmos.color = Color.red;
				Gizmos.DrawRay(vector, quaternion * Vector3.right * size);
				Gizmos.color = Color.green;
				Gizmos.DrawRay(vector, quaternion * Vector3.up * size);
				Gizmos.color = Color.blue;
				Gizmos.DrawRay(vector, quaternion * Vector3.forward * size);
			}
		}
	}
}
