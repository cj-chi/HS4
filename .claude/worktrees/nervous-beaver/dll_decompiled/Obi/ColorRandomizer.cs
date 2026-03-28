using UnityEngine;

namespace Obi;

[RequireComponent(typeof(ObiActor))]
public class ColorRandomizer : MonoBehaviour
{
	private ObiActor actor;

	public Gradient gradient = new Gradient();

	private void Start()
	{
		actor = GetComponent<ObiActor>();
		if (actor.colors != null)
		{
			for (int i = 0; i < actor.colors.Length; i++)
			{
				actor.colors[i] = gradient.Evaluate(Random.value);
			}
		}
	}
}
