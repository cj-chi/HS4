using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class ColliderHighlighter : MonoBehaviour
{
	private ObiSolver solver;

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
		Oni.Contact[] data = e.contacts.Data;
		for (int i = 0; i < e.contacts.Count; i++)
		{
			Oni.Contact contact = data[i];
			if (!(contact.distance < 0.01f))
			{
				continue;
			}
			Collider collider = ObiColliderBase.idToCollider[contact.other] as Collider;
			if (collider != null)
			{
				Blinker component = collider.GetComponent<Blinker>();
				if ((bool)component)
				{
					component.Blink();
				}
			}
		}
	}
}
