using System;
using System.Collections.Generic;
using SceneAssist;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HColorPickerCtrl : MonoBehaviour
{
	public enum ScreenEffectName
	{
		AOcolor,
		BloomColor,
		FogColor,
		SunColor,
		EnvironmentLight
	}

	[Serializable]
	public struct ScreenEffectPos
	{
		public ScreenEffectName kind;

		public Vector3 pos;
	}

	[Tooltip("このキャンバスグループ")]
	[SerializeField]
	private CanvasGroup cgWindow;

	[Tooltip("閉じるボタン")]
	[SerializeField]
	private Button btnClose;

	[Tooltip("サンプルカラーScript")]
	[SerializeField]
	private UI_SampleColor sampleColor;

	[SerializeField]
	private Vector3 LightViewPos = Vector3.zero;

	[SerializeField]
	private ScreenEffectPos[] screenEffectPos;

	[SerializeField]
	private RectTransform PanelRct;

	[SerializeField]
	private CameraControl_Ver2 HCamera;

	private string fromOpen = "";

	private PointerAction PointerAction;

	private static readonly Dictionary<ScreenEffectName, string[]> colorPickerKind = new Dictionary<ScreenEffectName, string[]>
	{
		{
			ScreenEffectName.AOcolor,
			new string[1] { "AOcolor" }
		},
		{
			ScreenEffectName.BloomColor,
			new string[1] { "BloomColor" }
		},
		{
			ScreenEffectName.FogColor,
			new string[1] { "FogColor" }
		},
		{
			ScreenEffectName.SunColor,
			new string[2] { "SunColor", "SunThreshold" }
		},
		{
			ScreenEffectName.EnvironmentLight,
			new string[3] { "SkyColor", "EquatorColor", "GroundColor" }
		}
	};

	public bool isOpen
	{
		get
		{
			if (0f != cgWindow.alpha)
			{
				return true;
			}
			return false;
		}
	}

	private void Start()
	{
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			});
		}
		Toggle[] componentsInChildren = GetComponentsInChildren<Toggle>(includeInactive: true);
		Image[][] array = null;
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			array = new Image[componentsInChildren.Length][];
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				array[num] = componentsInChildren[num].GetComponentsInChildren<Image>(includeInactive: true);
				PointerAction = componentsInChildren[num].GetComponent<PointerAction>();
				if (PointerAction == null)
				{
					PointerAction = componentsInChildren[num].gameObject.AddComponent<PointerAction>();
				}
				PointerAction.listDownAction.Clear();
				PointerAction.listDownAction.Add(delegate
				{
					GlobalMethod.setCameraMoveFlag(HCamera, _bPlay: false);
				});
			}
		}
		Image[] componentsInChildren2 = GetComponentsInChildren<Image>(includeInactive: true);
		if (componentsInChildren2 == null || componentsInChildren2.Length == 0)
		{
			return;
		}
		for (int num2 = 0; num2 < componentsInChildren2.Length; num2++)
		{
			if ((array == null && array.Length == 0) || !CheckToggleImage(array, componentsInChildren2[num2].gameObject))
			{
				PointerAction = componentsInChildren2[num2].GetComponent<PointerAction>();
				if (PointerAction == null)
				{
					PointerAction = componentsInChildren2[num2].gameObject.AddComponent<PointerAction>();
				}
				PointerAction.listDownAction.Clear();
				PointerAction.listDownAction.Add(delegate
				{
					GlobalMethod.setCameraMoveFlag(HCamera, _bPlay: false);
				});
			}
		}
	}

	public bool Check(string strCheck)
	{
		return fromOpen == strCheck;
	}

	public void Open(Color color, Action<Color> _actUpdateColor, string open, int mode = 0)
	{
		if (mode == 0 || this.screenEffectPos == null || this.screenEffectPos.Length == 0)
		{
			PanelRct.anchoredPosition = LightViewPos;
		}
		else
		{
			ScreenEffectPos[] array = this.screenEffectPos;
			for (int i = 0; i < array.Length; i++)
			{
				ScreenEffectPos screenEffectPos = array[i];
				if (!colorPickerKind.ContainsKey(screenEffectPos.kind))
				{
					continue;
				}
				bool flag = false;
				string[] array2 = colorPickerKind[screenEffectPos.kind];
				for (int j = 0; j < array2.Length; j++)
				{
					if (!(array2[j] != open))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					PanelRct.anchoredPosition = screenEffectPos.pos;
					break;
				}
			}
		}
		cgWindow.alpha = 1f;
		cgWindow.interactable = true;
		cgWindow.blocksRaycasts = true;
		if (null != sampleColor)
		{
			sampleColor.SetColor(color);
			sampleColor.actUpdateColor = _actUpdateColor;
		}
		fromOpen = open;
	}

	public void Close()
	{
		cgWindow.alpha = 0f;
		cgWindow.interactable = false;
		cgWindow.blocksRaycasts = false;
		if (null != sampleColor)
		{
			sampleColor.actUpdateColor = null;
		}
		fromOpen = "";
	}

	private bool CheckToggleImage(Image[][] sorce, GameObject target)
	{
		foreach (Image[] array in sorce)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].gameObject == target)
				{
					return true;
				}
			}
		}
		return false;
	}
}
