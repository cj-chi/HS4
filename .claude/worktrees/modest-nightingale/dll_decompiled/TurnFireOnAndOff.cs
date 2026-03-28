using System.Collections;
using UnityEngine;

public class TurnFireOnAndOff : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(OnOffForever());
	}

	private IEnumerator OnOffForever()
	{
		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			while (true)
			{
				animator.SetBool("Fire", value: true);
				yield return new WaitForSeconds(2f);
				animator.SetBool("Fire", value: false);
				yield return new WaitForSeconds(2f);
			}
		}
	}
}
