using System;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class LogView : MonoBehaviour
{
	[SerializeField]
	private Processing processing;

	[SerializeField]
	private Canvas canvasLog;

	[SerializeField]
	private RectTransform rtfScroll;

	[SerializeField]
	private RectTransform rtfContent;

	[SerializeField]
	private Text textLog;

	[SerializeField]
	private Button btnClose;

	private Dictionary<int, Text> dictTextLog = new Dictionary<int, Text>();

	private StringBuilder sbAdd = new StringBuilder(4096);

	public Action onClose;

	public bool IsActive => processing.update;

	private void Start()
	{
		if ((bool)btnClose)
		{
			btnClose.OnClickAsObservable().Subscribe(delegate
			{
				SetActiveCanvas(active: false);
				onClose?.Invoke();
			});
		}
		this.UpdateAsObservable().Subscribe(delegate
		{
			if (!(null == rtfScroll) && !(null == rtfContent) && !(null == textLog) && sbAdd.Length != 0)
			{
				Text text = UnityEngine.Object.Instantiate(textLog);
				text.transform.SetParent(rtfContent.transform, worldPositionStays: false);
				text.text = sbAdd.ToString().TrimEnd('\r', '\n');
				text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, text.preferredHeight);
				text.gameObject.SetActive(value: true);
				sbAdd.Length = 0;
			}
		});
	}

	public void SetActiveCanvas(bool active)
	{
		if ((bool)canvasLog)
		{
			canvasLog.gameObject.SetActive(active);
		}
	}

	public void AddLog(string format, params object[] args)
	{
		string text = format;
		for (int i = 0; i < args.Length; i++)
		{
			text = text.Replace("{" + i + "}", args[i].ToString());
		}
		sbAdd.Append($"{text}\n");
	}

	public void AddLog(Color color, string format, params object[] args)
	{
		string text = format;
		for (int i = 0; i < args.Length; i++)
		{
			text = text.Replace("{" + i + "}", args[i].ToString());
		}
		sbAdd.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>\n");
	}

	public void StartLog()
	{
		for (int num = rtfContent.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(rtfContent.GetChild(num).gameObject);
		}
		Resources.UnloadUnusedAssets();
		dictTextLog.Clear();
		sbAdd.Length = 0;
		processing.update = true;
		if ((bool)btnClose)
		{
			btnClose.interactable = false;
		}
		SetActiveCanvas(active: true);
	}

	public void EndLog()
	{
		processing.update = false;
		if ((bool)btnClose)
		{
			btnClose.interactable = true;
		}
		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
	}
}
