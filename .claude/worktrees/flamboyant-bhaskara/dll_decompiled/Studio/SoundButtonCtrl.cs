using System;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class SoundButtonCtrl : MonoBehaviour
{
	[Serializable]
	private class CommonInfo
	{
		public GameObject obj;

		public Button button;

		public bool active
		{
			set
			{
				if ((bool)obj && obj.activeSelf != value)
				{
					obj.SetActive(value);
					button.image.color = (value ? Color.green : Color.white);
				}
			}
		}
	}

	[SerializeField]
	private CommonInfo[] ciRoot;

	private int select = -1;

	public void OnClickButton(int _idx)
	{
		select = ((select == _idx) ? (-1) : _idx);
		for (int i = 0; i < ciRoot.Length; i++)
		{
			ciRoot[i].active = i == select;
		}
	}

	private void Start()
	{
		select = -1;
	}
}
