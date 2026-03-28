using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Processing : MonoBehaviour
{
	[SerializeField]
	private Image[] img;

	private Color[] color = new Color[12];

	public bool update = true;

	private void Start()
	{
		for (int i = 0; i < 12; i++)
		{
			color[i] = Color.HSVToRGB(0f, 0f, 1f - (float)i * 0.02f);
		}
		int index = 0;
		Observable.Interval(TimeSpan.FromMilliseconds(50.0)).Subscribe(delegate
		{
			if (update)
			{
				index = (index + 11) % 12;
				for (int j = 0; j < 12; j++)
				{
					int num = (index + j) % 12;
					img[j].color = color[num];
				}
			}
			for (int k = 0; k < 12; k++)
			{
				img[k].enabled = update;
			}
		}).AddTo(this);
	}

	private void Update()
	{
	}
}
