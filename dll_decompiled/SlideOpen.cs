using System;
using UniRx;
using UnityEngine;

public class SlideOpen : MonoBehaviour
{
	[Serializable]
	public class OpenTarget
	{
		[SerializeField]
		private GameObject TargetObj;

		[SerializeField]
		private RectTransform TargetRt;

		[SerializeField]
		private float OpenSizeX;

		[SerializeField]
		private float OpenSizeY;

		[SerializeField]
		private int OpenMode;

		[SerializeField]
		private SlideOpenScrollRect rect;

		private bool NowOpen;

		private bool NowClose;

		private float nowTime;

		private float openTime;

		private float closeTime;

		private IDisposable[] OpenDisposables = new IDisposable[2];

		private IDisposable[] CloseDisposables = new IDisposable[2];

		public void Open(float OpenTime)
		{
			if (NowOpen)
			{
				return;
			}
			if (rect != null)
			{
				rect.SetMode(0);
				rect.SetViewSize(OpenSizeY);
			}
			NowOpen = true;
			nowTime = 0f;
			openTime = OpenTime;
			for (int i = 0; i < CloseDisposables.Length; i++)
			{
				if (CloseDisposables[i] != null)
				{
					CloseDisposables[i].Dispose();
					CloseDisposables[i] = null;
				}
			}
			if (OpenMode == 0)
			{
				TargetRt.sizeDelta = new Vector2(OpenSizeX, 0f);
				TargetObj.SetActive(value: true);
				OpenDisposables[0] = Observable.EveryUpdate().TakeWhile((long _) => NowOpen).Finally(delegate
				{
					NowOpen = false;
					if (rect != null)
					{
						rect.ResetMode();
					}
				})
					.Subscribe(delegate
					{
						float num = Mathf.InverseLerp(0f, openTime, nowTime);
						float y = Mathf.Lerp(0f, OpenSizeY, num);
						TargetRt.sizeDelta = new Vector2(OpenSizeX, y);
						nowTime += Time.deltaTime;
						if (num >= 1f)
						{
							NowOpen = false;
						}
					});
			}
			else
			{
				if (OpenMode != 1)
				{
					return;
				}
				TargetRt.sizeDelta = new Vector2(0f, OpenSizeY);
				TargetObj.SetActive(value: true);
				OpenDisposables[1] = Observable.EveryUpdate().TakeWhile((long _) => NowOpen).Finally(delegate
				{
					NowOpen = false;
				})
					.Subscribe(delegate
					{
						float num = Mathf.InverseLerp(0f, openTime, nowTime);
						float x = Mathf.Lerp(0f, OpenSizeX, num);
						TargetRt.sizeDelta = new Vector2(x, OpenSizeY);
						nowTime += Time.deltaTime;
						if (num >= 1f)
						{
							NowOpen = false;
						}
					});
			}
		}

		public void Close(float CloseTime)
		{
			if (NowClose)
			{
				return;
			}
			if (rect != null)
			{
				rect.SetMode(1);
				rect.SetViewSize(OpenSizeY);
			}
			NowClose = true;
			nowTime = 0f;
			closeTime = CloseTime;
			for (int i = 0; i < OpenDisposables.Length; i++)
			{
				if (OpenDisposables[i] != null)
				{
					OpenDisposables[i].Dispose();
					OpenDisposables[i] = null;
				}
			}
			if (OpenMode == 0)
			{
				CloseDisposables[0] = Observable.EveryUpdate().TakeWhile((long _) => NowClose).Finally(delegate
				{
					TargetRt.sizeDelta = new Vector2(OpenSizeX, 0f);
					TargetObj.SetActive(value: false);
					NowClose = false;
					if (rect != null)
					{
						rect.ResetMode();
					}
				})
					.Subscribe(delegate
					{
						float num = Mathf.InverseLerp(0f, closeTime, nowTime);
						float y = Mathf.Lerp(OpenSizeY, 0f, num);
						TargetRt.sizeDelta = new Vector2(OpenSizeX, y);
						nowTime += Time.deltaTime;
						if (num >= 1f)
						{
							NowClose = false;
						}
					});
			}
			else
			{
				if (OpenMode != 1)
				{
					return;
				}
				CloseDisposables[1] = Observable.EveryUpdate().TakeWhile((long _) => NowClose).Finally(delegate
				{
					TargetRt.sizeDelta = new Vector2(0f, OpenSizeY);
					TargetObj.SetActive(value: false);
					NowClose = false;
				})
					.Subscribe(delegate
					{
						float num = Mathf.InverseLerp(0f, closeTime, nowTime);
						float x = Mathf.Lerp(OpenSizeX, 0f, num);
						TargetRt.sizeDelta = new Vector2(x, OpenSizeY);
						nowTime += Time.deltaTime;
						if (num >= 1f)
						{
							NowClose = false;
						}
					});
			}
		}

		public void SetView()
		{
			if (!(rect == null))
			{
				rect.SetViewSize(OpenSizeY);
			}
		}

		public void Release()
		{
			for (int i = 0; i < CloseDisposables.Length; i++)
			{
				if (CloseDisposables[i] != null)
				{
					CloseDisposables[i].Dispose();
					CloseDisposables[i] = null;
				}
			}
			for (int j = 0; j < OpenDisposables.Length; j++)
			{
				if (OpenDisposables[j] != null)
				{
					OpenDisposables[j].Dispose();
					OpenDisposables[j] = null;
				}
			}
		}
	}

	[SerializeField]
	private OpenTarget[] Targets;

	[SerializeField]
	private float OpenTime;

	[SerializeField]
	private float CloseTime;

	public void Open()
	{
		OpenTarget[] targets = Targets;
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].Open(OpenTime);
		}
	}

	public void Close()
	{
		OpenTarget[] targets = Targets;
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].Close(CloseTime);
		}
	}

	public void SetViewSize()
	{
		OpenTarget[] targets = Targets;
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].SetView();
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < Targets.Length; i++)
		{
			Targets[i].Release();
		}
	}
}
