using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class Melt : MonoBehaviour
{
	public float heat = 0.1f;

	public float cooling = 0.1f;

	private ObiSolver solver;

	public Collider hotCollider;

	public Collider coldCollider;

	private void Awake()
	{
		solver = GetComponent<ObiSolver>();
	}

	private void OnEnable()
	{
		solver.OnCollision += Solver_OnCollision;
	}

	private void OnDisable()
	{
		solver.OnCollision -= Solver_OnCollision;
	}

	private void Solver_OnCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
	{
		for (int i = 0; i < e.contacts.Count; i++)
		{
			if (e.contacts.Data[i].distance < 0.001f && ObiColliderBase.idToCollider.TryGetValue(e.contacts.Data[i].other, out var value))
			{
				int particle = e.contacts.Data[i].particle;
				Vector4 value2 = solver.userData[particle];
				if (value == hotCollider)
				{
					value2[0] = Mathf.Max(0.05f, value2[0] - heat * Time.fixedDeltaTime);
					value2[1] = Mathf.Max(0.5f, value2[1] - heat * Time.fixedDeltaTime);
				}
				else if (value == coldCollider)
				{
					value2[0] = Mathf.Min(10f, value2[0] + cooling * Time.fixedDeltaTime);
					value2[1] = Mathf.Min(2f, value2[1] + cooling * Time.fixedDeltaTime);
				}
				solver.userData[particle] = value2;
			}
		}
	}
}
