using UnityEngine;

public class CursorPositionDrawer : MonoBehaviour
{
	public bool visible = true;

	private GUIStyle style;

	private void Awake()
	{
		style = new GUIStyle();
		style.fontSize = 20;
		GUIStyleState gUIStyleState = new GUIStyleState();
		gUIStyleState.textColor = Color.white;
		style.normal = gUIStyleState;
	}

	private void OnGUI()
	{
		if (!visible)
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Space(110f);
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				GUILayout.Label(string.Format("PosX:{0} PosY:{1}", Input.mousePosition.x.ToString("0000.00"), Input.mousePosition.y.ToString("0000.00")), style);
			}
		}
	}
}
