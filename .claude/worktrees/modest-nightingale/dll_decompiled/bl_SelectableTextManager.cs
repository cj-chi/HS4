using UnityEngine;

public class bl_SelectableTextManager : MonoBehaviour
{
	private bl_SelectableText[] AllSelectables;

	private void Awake()
	{
		AllSelectables = GetComponentsInChildren<bl_SelectableText>();
	}

	public void OnEnter()
	{
		if (AllSelectables.Length != 0)
		{
			bl_SelectableText[] allSelectables = AllSelectables;
			for (int i = 0; i < allSelectables.Length; i++)
			{
				allSelectables[i].OnEnter();
			}
		}
	}

	public void OnExit()
	{
		if (AllSelectables.Length != 0)
		{
			bl_SelectableText[] allSelectables = AllSelectables;
			for (int i = 0; i < allSelectables.Length; i++)
			{
				allSelectables[i].OnExit();
			}
		}
	}
}
