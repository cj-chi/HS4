using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIExampleController : MonoBehaviour
{
	public Toggle maskToggle;

	public Image maskBorder;

	public List<Image> maskImages;

	public Sprite[] windowSprites;

	public Image window;

	public Image health;

	public Image mana;

	public Slider healthSlider;

	public Slider manaSlider;

	private void Start()
	{
		maskImages = new List<Image>();
		Mask[] array = Object.FindObjectsOfType<Mask>();
		foreach (Mask mask in array)
		{
			maskImages.Add(mask.GetComponent<Image>());
		}
	}

	private void Update()
	{
	}

	public void ToggleMask()
	{
		foreach (Image maskImage in maskImages)
		{
			maskImage.enabled = maskToggle.isOn;
		}
		maskBorder.enabled = maskToggle.isOn;
	}

	public void ChangeWindowType(int i)
	{
		window.sprite = windowSprites[i];
	}

	public void OnSlidersChanged()
	{
		mana.rectTransform.sizeDelta = new Vector2(mana.rectTransform.sizeDelta.x, manaSlider.value * 240f);
		health.rectTransform.sizeDelta = new Vector2(health.rectTransform.sizeDelta.x, healthSlider.value * 240f);
	}
}
