using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Component;

internal class InteractableAlphaChangerBaseButton : MonoBehaviour
{
	[Header("Interactable参照用ボタン")]
	[SerializeField]
	private Button flagButton;

	[Header("カラー変更対象TextMesh")]
	[SerializeField]
	private List<TextMeshProUGUI> targetTextMesh = new List<TextMeshProUGUI>();

	[Header("カラー変更対象Text")]
	[SerializeField]
	private List<Text> targetText = new List<Text>();

	[Header("カラー変更対象Image")]
	[SerializeField]
	private List<Image> targetImage = new List<Image>();

	[Header("カラー変更対象RawImage")]
	[SerializeField]
	private List<RawImage> targetRawImage = new List<RawImage>();

	private void Awake()
	{
		if (flagButton == null)
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		List<Color> baseTextMeshColor = targetTextMesh.Select((TextMeshProUGUI t) => t.color).ToList();
		List<Color> baseTextColor = targetText.Select((Text t) => t.color).ToList();
		List<Color> baseImageColor = targetImage.Select((Image t) => t.color).ToList();
		Color[] baseRawImageColor = targetRawImage.Select((RawImage t) => t.color).ToArray();
		BoolReactiveProperty isInteract = new BoolReactiveProperty(flagButton.interactable);
		isInteract.Subscribe(delegate(bool isOn)
		{
			ColorBlock colors = flagButton.colors;
			List<Color> list = new List<Color>(baseTextMeshColor);
			List<Color> list2 = new List<Color>(baseTextColor);
			List<Color> list3 = new List<Color>(baseImageColor);
			List<Color> list4 = new List<Color>(baseRawImageColor);
			if (!isOn)
			{
				for (int i = 0; i < targetTextMesh.Count; i++)
				{
					list[i] = new Color(Mathf.Clamp01(list[i].r * colors.disabledColor.r), Mathf.Clamp01(list[i].g * colors.disabledColor.g), Mathf.Clamp01(list[i].b * colors.disabledColor.b), Mathf.Clamp01(list[i].a * colors.disabledColor.a));
				}
				for (int j = 0; j < targetText.Count; j++)
				{
					list2[j] = new Color(Mathf.Clamp01(list2[j].r * colors.disabledColor.r), Mathf.Clamp01(list2[j].g * colors.disabledColor.g), Mathf.Clamp01(list2[j].b * colors.disabledColor.b), Mathf.Clamp01(list2[j].a * colors.disabledColor.a));
				}
				for (int k = 0; k < targetImage.Count; k++)
				{
					list3[k] = new Color(Mathf.Clamp01(list3[k].r * colors.disabledColor.r), Mathf.Clamp01(list3[k].g * colors.disabledColor.g), Mathf.Clamp01(list3[k].b * colors.disabledColor.b), Mathf.Clamp01(list3[k].a * colors.disabledColor.a));
				}
				for (int l = 0; l < targetRawImage.Count; l++)
				{
					list4[l] = new Color(Mathf.Clamp01(list4[l].r * colors.disabledColor.r), Mathf.Clamp01(list4[l].g * colors.disabledColor.g), Mathf.Clamp01(list4[l].b * colors.disabledColor.b), Mathf.Clamp01(list4[l].a * colors.disabledColor.a));
				}
			}
			for (int m = 0; m < targetTextMesh.Count; m++)
			{
				targetTextMesh[m].color = list[m];
			}
			for (int n = 0; n < targetText.Count; n++)
			{
				targetText[n].color = list2[n];
			}
			for (int num = 0; num < targetImage.Count; num++)
			{
				targetImage[num].color = list3[num];
			}
			for (int num2 = 0; num2 < targetRawImage.Count; num2++)
			{
				targetRawImage[num2].color = list4[num2];
			}
		});
		this.OnEnableAsObservable().Subscribe(delegate
		{
			isInteract.Value = flagButton.IsInteractable();
		});
		(from _ in this.UpdateAsObservable()
			select flagButton.IsInteractable()).DistinctUntilChanged().Subscribe(delegate(bool interactable)
		{
			isInteract.Value = interactable;
		});
	}
}
