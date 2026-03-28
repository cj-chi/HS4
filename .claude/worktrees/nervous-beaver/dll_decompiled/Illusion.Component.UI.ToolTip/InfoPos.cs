using UnityEngine;

namespace Illusion.Component.UI.ToolTip;

public class InfoPos : MonoBehaviour
{
	[Tooltip("表示する場所(指定なし:マウス座標)")]
	[SerializeField]
	private Transform _target;

	[Tooltip("表示ウィンドウ")]
	[SerializeField]
	protected RectTransform info;

	[Tooltip("ずらす値")]
	[SerializeField]
	private Vector3 offsetPos = Vector3.zero;

	[Tooltip("中心からの計算の有効")]
	[SerializeField]
	private bool isCalcCenter;

	[Tooltip("中心からずらす大きさ")]
	[SerializeField]
	private Vector2 offsetSize = Vector3.zero;

	public Transform target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	private Vector2 targetPos
	{
		get
		{
			if (target != null)
			{
				RectTransform rectTransform = target as RectTransform;
				if (rectTransform != null)
				{
					return rectTransform.position + offsetPos;
				}
				return RectTransformUtility.WorldToScreenPoint(Camera.main, target.position + offsetPos);
			}
			return Input.mousePosition + offsetPos;
		}
	}

	private static Vector2 ScreenSize => new Vector2(Screen.width, Screen.height);

	protected virtual void Update()
	{
		Vector2 pos = ClipPos(info, targetPos);
		CenterOffsetSize(ref pos);
		info.anchoredPosition = ClipPos(info, pos);
	}

	private void CenterOffsetSize(ref Vector2 pos)
	{
		pos += ((!isCalcCenter) ? Vector2.zero : OffsetSize(new Vector2(offsetSize.x, offsetSize.y + info.sizeDelta.y), (ScreenSize * 0.5f - pos).normalized));
	}

	private static Vector2 ClipPos(RectTransform rt, Vector2 pos)
	{
		Vector2 screenSize = ScreenSize;
		for (int i = 0; i < 2; i++)
		{
			pos[i] = Mathf.Clamp(pos[i], rt.sizeDelta[i] * rt.pivot[i], screenSize[i] - rt.sizeDelta[i] * (1f - rt.pivot[i]));
		}
		return pos;
	}

	private static Vector2 OffsetSize(Vector2 size, Vector2 v)
	{
		return new Vector2(size.x * (float)((!(Vector2.Dot(Vector2.right, v) < 0f)) ? 1 : (-1)), size.y * (float)((!(Vector2.Dot(Vector2.up, v) < 0f)) ? 1 : (-1))) * 0.5f;
	}
}
