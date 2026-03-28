using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HOptionButton : MonoBehaviour
{
	[SerializeField]
	private Sprite defaultSprite;

	[SerializeField]
	private Sprite clicked;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Image targetImage;

	public bool useClicked;

	public PointerDownAction downAction;

	public bool Enable
	{
		set
		{
			if (!(button == null) && button.interactable != value)
			{
				button.interactable = value;
			}
		}
	}

	private void Update()
	{
		if (useClicked && targetImage.sprite != clicked)
		{
			targetImage.sprite = clicked;
		}
		else if (!useClicked && targetImage.sprite != defaultSprite)
		{
			targetImage.sprite = defaultSprite;
		}
	}

	private void Start()
	{
		if (button == null)
		{
			button = GetComponent<Button>();
		}
	}
}
