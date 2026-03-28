using System;
using Illusion.Extensions;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UploaderSystem;

[Serializable]
public class NetFileComponent : MonoBehaviour
{
	[Header("---< 基本 >--------------------------")]
	[SerializeField]
	private GameObject objSortInfo;

	[SerializeField]
	private RawImage rawImage;

	public Toggle tglItem;

	[Header("---< ランク >------------------------")]
	[SerializeField]
	private GameObject objRank;

	[SerializeField]
	private Text textRank;

	[SerializeField]
	private Image[] imgRank;

	[Header("---< 拍手 >--------------------")]
	[SerializeField]
	private GameObject objApplause;

	[SerializeField]
	private UI_ButtonEx btnLike;

	[SerializeField]
	private Text textApplauseNum;

	public Action actApplause;

	[Header("---< 更新日 >------------------------")]
	[SerializeField]
	private GameObject objDate;

	[SerializeField]
	private Text textDateTitle;

	[SerializeField]
	private Text textDate;

	public void SetState(bool interactable, bool enable)
	{
		if (null != tglItem)
		{
			tglItem.interactable = interactable;
		}
		if (null != rawImage)
		{
			rawImage.enabled = enable;
		}
		if (null != objSortInfo)
		{
			objSortInfo.SetActiveIfDifferent(interactable);
		}
		if (null != btnLike)
		{
			btnLike.gameObject.SetActiveIfDifferent(enable);
		}
	}

	public void SetImage(Texture tex)
	{
		if (null != rawImage)
		{
			rawImage.enabled = null != tex;
			if (null != rawImage.texture)
			{
				UnityEngine.Object.Destroy(rawImage.texture);
			}
			rawImage.texture = tex;
		}
	}

	public void UpdateSortType(int type)
	{
		bool[,] array = new bool[3, 7]
		{
			{ false, false, false, false, true, true, false },
			{ false, false, false, false, false, false, true },
			{ true, true, true, true, false, false, false }
		};
		if (null != objRank)
		{
			objRank.SetActiveIfDifferent(array[0, type]);
		}
		if (null != objApplause)
		{
			objApplause.SetActiveIfDifferent(array[1, type]);
		}
		if (null != objDate)
		{
			objDate.SetActiveIfDifferent(array[2, type]);
		}
	}

	public void SetRanking(int no)
	{
		GameSystem.Language language = Singleton<GameSystem>.Instance.language;
		for (int i = 0; i < imgRank.Length; i++)
		{
			imgRank[i].enabled = false;
		}
		if (1 <= no && no <= 3)
		{
			imgRank[no - 1].enabled = true;
			if (null != textRank)
			{
				textRank.text = "";
			}
		}
		else if (null != textRank)
		{
			switch (language)
			{
			case GameSystem.Language.Japanese:
				textRank.text = $"{no}位";
				break;
			case GameSystem.Language.English:
				textRank.text = $"{no}";
				break;
			case GameSystem.Language.SimplifiedChinese:
				textRank.text = $"第{no}";
				break;
			case GameSystem.Language.TraditionalChinese:
				textRank.text = $"第{no}名";
				break;
			}
		}
	}

	public void SetUpdateTime(DateTime time, int kind)
	{
		if (null != textDate)
		{
			if (Singleton<GameSystem>.Instance.language == GameSystem.Language.English)
			{
				textDate.text = time.ToString("MM/dd/yyyy");
			}
			else
			{
				textDate.text = time.ToString("yyyy/MM/dd");
			}
		}
	}

	public void SetApplauseNum(int num)
	{
		if (null != textApplauseNum)
		{
			textApplauseNum.text = num.ToString();
		}
	}

	public void EnableApplause(bool enable)
	{
		if (null != btnLike)
		{
			btnLike.interactable = enable;
		}
	}

	private void Awake()
	{
	}

	private void Reset()
	{
		Transform transform = null;
		transform = base.transform.Find("imgBack/sortinfo");
		if (null != transform)
		{
			objSortInfo = transform.gameObject;
		}
		transform = base.transform.Find("Image/RawImage");
		if (null != transform)
		{
			rawImage = transform.GetComponent<RawImage>();
		}
		tglItem = GetComponent<Toggle>();
		transform = base.transform.Find("imgBack/sortinfo/rank");
		if (null != transform)
		{
			objRank = transform.gameObject;
		}
		transform = base.transform.Find("imgBack/sortinfo/rank/textRank");
		if (null != transform)
		{
			textRank = transform.GetComponent<Text>();
		}
		imgRank = new Image[3];
		transform = base.transform.Find("imgBack/sortinfo/rank/imgRank00");
		if (null != transform)
		{
			imgRank[0] = transform.GetComponent<Image>();
		}
		transform = base.transform.Find("imgBack/sortinfo/rank/imgRank01");
		if (null != transform)
		{
			imgRank[1] = transform.GetComponent<Image>();
		}
		transform = base.transform.Find("imgBack/sortinfo/rank/imgRank02");
		if (null != transform)
		{
			imgRank[2] = transform.GetComponent<Image>();
		}
		transform = base.transform.Find("imgBack/sortinfo/applausenum");
		if (null != transform)
		{
			objApplause = transform.gameObject;
		}
		transform = base.transform.Find("imgBack/sortinfo/applausenum/textApplauseNum");
		if (null != transform)
		{
			textApplauseNum = transform.GetComponent<Text>();
		}
		transform = base.transform.Find("imgBack/sortinfo/applausenum/btnLike");
		if (null != transform)
		{
			btnLike = transform.GetComponent<UI_ButtonEx>();
		}
		transform = base.transform.Find("imgBack/sortinfo/date");
		if (null != transform)
		{
			objDate = transform.gameObject;
		}
		transform = base.transform.Find("imgBack/sortinfo/date/textUpTimeTitle");
		if (null != transform)
		{
			textDateTitle = transform.GetComponent<Text>();
		}
		transform = base.transform.Find("imgBack/sortinfo/date/textUpTime");
		if (null != transform)
		{
			textDate = transform.GetComponent<Text>();
		}
	}

	private void Start()
	{
		if (null != btnLike)
		{
			btnLike.OnClickAsObservable().Subscribe(delegate
			{
				actApplause?.Invoke();
			});
		}
	}

	private void Update()
	{
	}
}
