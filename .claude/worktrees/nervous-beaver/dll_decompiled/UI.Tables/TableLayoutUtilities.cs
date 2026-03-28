using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Tables;

public static class TableLayoutUtilities
{
	public static GameObject InstantiatePrefab(string name, bool playMode = false, bool generateUndo = true)
	{
		GameObject gameObject = Resources.Load<GameObject>("Prefabs/" + name);
		if (gameObject == null)
		{
			throw new UnityException($"Could not find prefab '{name}'!");
		}
		Transform transform = null;
		GameObject gameObject2 = Object.Instantiate(gameObject);
		gameObject2.name = name;
		if (transform == null || !(transform is RectTransform))
		{
			transform = GetCanvasTransform();
		}
		gameObject2.transform.SetParent(transform);
		FixInstanceTransform(instanceTransform: (RectTransform)gameObject2.transform, baseTransform: (RectTransform)gameObject.transform);
		return gameObject2;
	}

	public static Transform GetCanvasTransform()
	{
		Canvas canvas = null;
		if (canvas == null)
		{
			canvas = Object.FindObjectOfType<Canvas>();
			if (canvas != null)
			{
				return canvas.transform;
			}
		}
		GameObject obj = new GameObject("Canvas")
		{
			layer = LayerMask.NameToLayer("UI")
		};
		canvas = obj.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		obj.AddComponent<CanvasScaler>();
		obj.AddComponent<GraphicRaycaster>();
		if (Object.FindObjectOfType<EventSystem>() == null)
		{
			GameObject gameObject = new GameObject("EventSystem");
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();
		}
		return canvas.transform;
	}

	public static void FixInstanceTransform(RectTransform baseTransform, RectTransform instanceTransform)
	{
		instanceTransform.localPosition = Vector3.zero;
		instanceTransform.position = Vector3.zero;
		instanceTransform.rotation = baseTransform.rotation;
		instanceTransform.localScale = baseTransform.localScale;
		instanceTransform.anchoredPosition3D = new Vector3(baseTransform.anchoredPosition3D.x, baseTransform.anchoredPosition3D.y, 0f);
		instanceTransform.sizeDelta = baseTransform.sizeDelta;
	}

	public static T FindParentOfType<T>(GameObject childObject) where T : Object
	{
		Transform transform = childObject.transform;
		while (transform.parent != null)
		{
			T component = transform.parent.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent.transform;
		}
		return null;
	}
}
