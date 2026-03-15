using UnityEngine;
using UnityEngine.UI;

public class ChachoiceNode : MonoBehaviour
{
	public int ChoiseNo;

	public string Name;

	public Text label;

	private void Start()
	{
		if (label != null)
		{
			Name = label.text;
		}
	}
}
