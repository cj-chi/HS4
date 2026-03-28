using UnityEngine;
using UnityEngine.UI;

namespace StudioFileCheck;

public class Node : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Text text;

	[SerializeField]
	private Toggle toggle;

	public Button Button => button;

	public string Text
	{
		set
		{
			text.text = value;
		}
	}

	public Color TextColor
	{
		set
		{
			text.color = value;
		}
	}

	public bool Select
	{
		set
		{
			toggle.isOn = value;
		}
	}

	public void RemoveAllListeners()
	{
		button.onClick.RemoveAllListeners();
	}
}
