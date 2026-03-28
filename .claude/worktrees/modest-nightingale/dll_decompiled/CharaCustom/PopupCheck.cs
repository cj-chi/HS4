using System;
using Illusion.Extensions;
using Illusion.Game;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class PopupCheck : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Button btnYes;

	[SerializeField]
	private Text textYes;

	[SerializeField]
	private Button btnYes2;

	[SerializeField]
	private Text textYes2;

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private Text textNo;

	[SerializeField]
	private Text textMsg;

	public Action actYes;

	public Action actYes2;

	public Action actNo;

	public void SetupWindow(string msg = "", string yes = "", string yes2 = "", string no = "")
	{
		if (!msg.IsNullOrEmpty())
		{
			textMsg.text = msg;
		}
		if (!yes.IsNullOrEmpty())
		{
			textYes.text = yes;
		}
		if (!yes2.IsNullOrEmpty())
		{
			textYes2.text = yes2;
		}
		if (!no.IsNullOrEmpty())
		{
			textNo.text = no;
		}
		canvasGroup.Enable(enable: true);
	}

	private void Start()
	{
		if ((bool)btnYes)
		{
			btnYes.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				actYes?.Invoke();
				canvasGroup.Enable(enable: false);
			});
		}
		if ((bool)btnYes2)
		{
			btnYes2.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.ok_l);
				actYes2?.Invoke();
				canvasGroup.Enable(enable: false);
			});
		}
		if ((bool)btnNo)
		{
			btnNo.OnClickAsObservable().Subscribe(delegate
			{
				Utils.Sound.Play(SystemSE.cancel);
				actNo?.Invoke();
				canvasGroup.Enable(enable: false);
			});
		}
	}

	private void Update()
	{
		if (1f == canvasGroup.alpha && Input.GetMouseButtonDown(1))
		{
			Utils.Sound.Play(SystemSE.cancel);
			actNo?.Invoke();
			canvasGroup.Enable(enable: false);
		}
	}
}
