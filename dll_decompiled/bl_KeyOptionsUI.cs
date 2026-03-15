using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_KeyOptionsUI : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private bool DetectIfKeyIsUse = true;

	[Header("References")]
	[SerializeField]
	private GameObject KeyOptionPrefab;

	[SerializeField]
	private Transform KeyOptionPanel;

	[SerializeField]
	private GameObject WaitKeyWindowUI;

	[SerializeField]
	private Text WaitKeyText;

	private bool WaitForKey;

	private bl_KeyInfo WaitFunctionKey;

	private List<bl_KeyInfoUI> cacheKeysInfoUI = new List<bl_KeyInfoUI>();

	private bool m_InterectableKey = true;

	public bool InteractableKey
	{
		get
		{
			return m_InterectableKey;
		}
		set
		{
			m_InterectableKey = value;
		}
	}

	private void Start()
	{
		InstanceKeysUI();
		WaitKeyWindowUI.SetActive(value: false);
	}

	private void InstanceKeysUI()
	{
		List<bl_KeyInfo> list = new List<bl_KeyInfo>();
		list = bl_Input.Instance.AllKeys;
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(KeyOptionPrefab);
			gameObject.GetComponent<bl_KeyInfoUI>().Init(list[i], this);
			gameObject.transform.SetParent(KeyOptionPanel, worldPositionStays: false);
			gameObject.gameObject.name = list[i].Function;
			cacheKeysInfoUI.Add(gameObject.GetComponent<bl_KeyInfoUI>());
		}
	}

	private void ClearList()
	{
		foreach (bl_KeyInfoUI item in cacheKeysInfoUI)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		cacheKeysInfoUI.Clear();
	}

	private void Update()
	{
		if (WaitForKey && m_InterectableKey)
		{
			DetectKey();
		}
	}

	private void DetectKey()
	{
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKey(value))
			{
				if (DetectIfKeyIsUse && bl_Input.Instance.isKeyUsed(value) && value != WaitFunctionKey.Key)
				{
					WaitKeyText.text = $"KEY <b>'{value.ToString().ToUpper()}'</b> IS ALREADY USE, \n PLEASE PRESS ANOTHER KEY FOR REPLACE <b>{WaitFunctionKey.Description.ToUpper()}</b>";
					continue;
				}
				KeyDetected(value);
				WaitForKey = false;
			}
		}
	}

	public void SetWaitKeyProcess(bl_KeyInfo info)
	{
		if (!WaitForKey)
		{
			WaitFunctionKey = info;
			WaitForKey = true;
			WaitKeyText.text = $"PRESS A KEY FOR REPLACE <b>{info.Description.ToUpper()}</b>";
			WaitKeyWindowUI.SetActive(value: true);
		}
	}

	private void KeyDetected(KeyCode KeyPressed)
	{
		if (WaitFunctionKey != null && bl_Input.Instance.SetKey(WaitFunctionKey.Function, KeyPressed))
		{
			ClearList();
			InstanceKeysUI();
			WaitFunctionKey = null;
			WaitKeyWindowUI.SetActive(value: false);
		}
	}

	public void CancelWait()
	{
		WaitForKey = false;
		WaitFunctionKey = null;
		WaitKeyWindowUI.SetActive(value: false);
		InteractableKey = true;
	}
}
