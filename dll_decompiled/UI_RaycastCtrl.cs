using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_RaycastCtrl : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup[] canvasGrp;

	[SerializeField]
	private Image[] imgRaycastTargetOn;

	private void GetImageComponents()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>(includeInactive: true);
		imgRaycastTargetOn = (from img in componentsInChildren
			where img.gameObject.name != "Viewport"
			where img.raycastTarget
			select img).ToArray();
	}

	private void GetCanvasGroup()
	{
		List<CanvasGroup> list = new List<CanvasGroup>();
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (null != component)
		{
			list.Add(component);
		}
		Transform parent = base.transform.parent;
		if (null == parent)
		{
			return;
		}
		while (true)
		{
			component = parent.GetComponent<CanvasGroup>();
			if (null != component)
			{
				list.Add(component);
			}
			if (null == parent.parent)
			{
				break;
			}
			parent = parent.parent;
		}
		canvasGrp = list.ToArray();
	}

	public void ChangeRaycastTarget(bool enable)
	{
		Image[] array = imgRaycastTargetOn;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].raycastTarget = enable;
		}
	}

	public void Reset()
	{
		GetCanvasGroup();
		GetImageComponents();
	}

	private void Update()
	{
		if (canvasGrp == null || imgRaycastTargetOn == null)
		{
			return;
		}
		bool enable = true;
		CanvasGroup[] array = canvasGrp;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].blocksRaycasts)
			{
				enable = false;
				break;
			}
		}
		ChangeRaycastTarget(enable);
	}
}
