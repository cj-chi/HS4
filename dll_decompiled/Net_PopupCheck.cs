using System;
using System.Collections;
using Illusion.Game;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Net_PopupCheck : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Button btnYes;

	[SerializeField]
	private Button btnNo;

	[SerializeField]
	private Text textMsg;

	private bool? answer;

	public IEnumerator CheckAnswerCor(IObserver<bool> observer, string msg)
	{
		if ((bool)textMsg)
		{
			textMsg.text = msg;
		}
		canvas.gameObject.SetActive(value: true);
		answer = null;
		while (!answer.HasValue)
		{
			yield return null;
		}
		observer.OnNext(answer ?? false);
		observer.OnCompleted();
		canvas.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		if ((bool)btnYes)
		{
			btnYes.OnClickAsObservable().Subscribe(delegate
			{
				answer = true;
				Utils.Sound.Play(SystemSE.ok_s);
			}).AddTo(this);
		}
		if ((bool)btnNo)
		{
			btnNo.OnClickAsObservable().Subscribe(delegate
			{
				answer = false;
				Utils.Sound.Play(SystemSE.cancel);
			}).AddTo(this);
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			Utils.Sound.Play(SystemSE.cancel);
			answer = false;
		}
	}
}
