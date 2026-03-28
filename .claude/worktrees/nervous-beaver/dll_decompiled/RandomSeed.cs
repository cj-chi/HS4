using UnityEngine;
using UnityEngine.UI;

public class RandomSeed : MonoBehaviour
{
	private void Start()
	{
		if (GetComponent<SpriteRenderer>() != null)
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null && component.material != null)
			{
				component.material.SetFloat("_RandomSeed", Random.Range(0f, 1000f));
			}
		}
		else
		{
			Image component2 = GetComponent<Image>();
			if (component2 != null && component2.material != null)
			{
				component2.material.SetFloat("_RandomSeed", Random.Range(0f, 1000f));
			}
		}
	}
}
