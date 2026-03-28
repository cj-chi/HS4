using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HDropdownScrollTemplate : MonoBehaviour
{
	private HSceneSprite hUI;

	private ScrollRect sr;

	private PointerAction[] items;

	public void Awake()
	{
		sr = base.gameObject.GetComponent<ScrollRect>();
	}

	public void Start()
	{
		Dropdown componentInParent = GetComponentInParent<Dropdown>();
		if (!(componentInParent != null))
		{
			return;
		}
		RectTransform component = base.transform.Find("Viewport").GetComponent<RectTransform>();
		RectTransform component2 = base.transform.Find("Viewport/Content").GetComponent<RectTransform>();
		RectTransform component3 = base.transform.Find("Viewport/Content/Item").GetComponent<RectTransform>();
		float num = component2.rect.height - component.rect.height;
		float value = component3.rect.height * (float)componentInParent.value / num;
		sr.verticalNormalizedPosition = 1f - Mathf.Clamp(value, 0f, 1f);
		items = GetComponentsInChildren<PointerAction>();
		hUI = (Singleton<HSceneSprite>.IsInstance() ? Singleton<HSceneSprite>.Instance : null);
		if (hUI != null)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].listDownAction.Clear();
				items[i].listDownAction.Add(hUI.OnClickSliderSelect);
			}
		}
	}
}
