using System;
using System.Collections;
using System.Threading;
using Illusion.Extensions;
using Illusion.Game;
using UniRx;
using UnityEngine;

public class EmocreScreenShot : MonoBehaviour
{
	[SerializeField]
	private GameObject[] objChangeDraw;

	[SerializeField]
	private GameObject objCapMark;

	private bool[] backupActiveFlags;

	public void ScreenShot()
	{
		backupActiveFlags = new bool[objChangeDraw.Length];
		for (int i = 0; i < objChangeDraw.Length; i++)
		{
			backupActiveFlags[i] = objChangeDraw[i].activeSelf;
			objChangeDraw[i].SetActiveIfDifferent(active: false);
		}
		if ((bool)objCapMark)
		{
			objCapMark.SetActiveIfDifferent(active: true);
		}
		Observable.FromCoroutine((CancellationToken res) => Capture()).Subscribe(delegate
		{
			Utils.Sound.Play(SystemSE.photo);
			for (int j = 0; j < objChangeDraw.Length; j++)
			{
				objChangeDraw[j].SetActiveIfDifferent(backupActiveFlags[j]);
			}
			if ((bool)objCapMark)
			{
				objCapMark.SetActiveIfDifferent(active: false);
			}
		}, delegate
		{
			for (int j = 0; j < objChangeDraw.Length; j++)
			{
				objChangeDraw[j].SetActiveIfDifferent(backupActiveFlags[j]);
			}
			if ((bool)objCapMark)
			{
				objCapMark.SetActiveIfDifferent(active: false);
			}
		}, delegate
		{
		});
	}

	public IEnumerator Capture()
	{
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(UserData.Create("cap") + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png", 1);
	}
}
