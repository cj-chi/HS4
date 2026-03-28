using System.Collections.Generic;
using Illusion.CustomAttributes;
using UnityEngine;

namespace Map;

public class MapVisibleList : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _list = new List<GameObject>();

	[SerializeField]
	[Button("Set", "ON", new object[] { true })]
	private int onButton;

	[SerializeField]
	[Button("Set", "OFF", new object[] { false })]
	private int offButton;

	public List<GameObject> list => _list;

	public void ON()
	{
		Set(visible: true);
	}

	public void OFF()
	{
		Set(visible: false);
	}

	public void Set(bool visible)
	{
		list.ForEach(delegate(GameObject item)
		{
			item.SetActive(visible);
		});
	}
}
