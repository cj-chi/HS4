using System.Collections.Generic;
using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class ObiParticleCounter : MonoBehaviour
{
	private ObiSolver solver;

	public int counter;

	public Collider2D targetCollider;

	private ObiSolver.ObiCollisionEventArgs frame;

	private HashSet<int> particles = new HashSet<int>();

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
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < e.contacts.Count; i++)
		{
			if (e.contacts.Data[i].distance < 0.001f && ObiColliderBase.idToCollider.TryGetValue(e.contacts.Data[i].other, out var value) && value == targetCollider)
			{
				hashSet.Add(e.contacts.Data[i].particle);
			}
		}
		particles.ExceptWith(hashSet);
		counter += particles.Count;
		particles = hashSet;
	}
}
