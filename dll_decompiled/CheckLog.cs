using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class CheckLog : MonoBehaviour
{
	[SerializeField]
	private RectTransform rtfScroll;

	[SerializeField]
	private RectTransform rtfContent;

	[SerializeField]
	private Text tmpText;

	private ScrollRect scrollR;

	private bool updateNormalizePosition;

	private List<Text> lstText = new List<Text>();

	private void Start()
	{
		scrollR = rtfScroll.GetComponent<ScrollRect>();
		if ((bool)scrollR)
		{
			(from _ in scrollR.UpdateAsObservable()
				where updateNormalizePosition
				select _).Subscribe(delegate
			{
				updateNormalizePosition = false;
				scrollR.verticalNormalizedPosition = 0f;
			});
		}
	}

	private Text CloneText(string str)
	{
		Text text = Object.Instantiate(tmpText);
		text.transform.SetParent(rtfContent.transform, worldPositionStays: false);
		text.text = str;
		text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, text.preferredHeight);
		text.gameObject.SetActive(value: true);
		updateNormalizePosition = true;
		return text;
	}

	public int AddLog(string format, params object[] args)
	{
		string text = format;
		for (int i = 0; i < args.Length; i++)
		{
			text = text.Replace("{" + i + "}", args[i].ToString());
		}
		Text item = CloneText(text);
		lstText.Add(item);
		return lstText.Count - 1;
	}

	public void UpdateLog(int index, string format, params object[] args)
	{
		if (index < lstText.Count)
		{
			string text = format;
			for (int i = 0; i < args.Length; i++)
			{
				text = text.Replace("{" + i + "}", args[i].ToString());
			}
			lstText[index].text = text;
		}
	}

	public int AddLog(Color color, string format, params object[] args)
	{
		string text = format;
		for (int i = 0; i < args.Length; i++)
		{
			text = text.Replace("{" + i + "}", args[i].ToString());
		}
		text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>\n";
		Text item = CloneText(text);
		lstText.Add(item);
		return lstText.Count - 1;
	}

	public void UpdateLog(int index, Color color, string format, params object[] args)
	{
		if (index < lstText.Count)
		{
			string text = format;
			for (int i = 0; i < args.Length; i++)
			{
				text = text.Replace("{" + i + "}", args[i].ToString());
			}
			text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>\n";
			lstText[index].text = text;
		}
	}

	public void StartLog()
	{
		for (int num = rtfContent.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(rtfContent.GetChild(num).gameObject);
		}
		Resources.UnloadUnusedAssets();
		lstText.Clear();
	}

	public void EndLog()
	{
		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
	}
}
