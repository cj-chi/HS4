using System.Collections.Generic;
using UnityEngine;

public class bl_Input : ScriptableObject
{
	public List<bl_KeyInfo> AllKeys = new List<bl_KeyInfo>();

	private static bl_Input m_Instance;

	public static bl_Input Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = Resources.Load("InputManager", typeof(bl_Input)) as bl_Input;
			}
			return m_Instance;
		}
	}

	public static float VerticalAxis
	{
		get
		{
			if (GetKey("Up") && !GetKey("Down"))
			{
				return 1f;
			}
			if (!GetKey("Up") && GetKey("Down"))
			{
				return -1f;
			}
			if (GetKey("Up") && GetKey("Down"))
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	public static float HorizontalAxis
	{
		get
		{
			if (GetKey("Right") && !GetKey("Left"))
			{
				return 1f;
			}
			if (!GetKey("Right") && GetKey("Left"))
			{
				return -1f;
			}
			if (GetKey("Right") && GetKey("Left"))
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	public void InitInput()
	{
		for (int i = 0; i < AllKeys.Count; i++)
		{
			string key = $"Key.{AllKeys[i].Function}";
			AllKeys[i].Key = (KeyCode)PlayerPrefs.GetInt(key, (int)AllKeys[i].Key);
		}
	}

	public static bool GetKeyDown(string function)
	{
		return Input.GetKeyDown(Instance.GetKeyCode(function));
	}

	public static bool GetKey(string function)
	{
		return Input.GetKey(Instance.GetKeyCode(function));
	}

	public static bool GetKeyUp(string function)
	{
		return Input.GetKeyUp(Instance.GetKeyCode(function));
	}

	public bool SetKey(string function, KeyCode newKey)
	{
		for (int i = 0; i < AllKeys.Count; i++)
		{
			if (AllKeys[i].Function == function)
			{
				AllKeys[i].Key = newKey;
				PlayerPrefs.SetInt($"Key.{function}", (int)newKey);
				return true;
			}
		}
		return false;
	}

	public KeyCode GetKeyCode(string function)
	{
		for (int i = 0; i < AllKeys.Count; i++)
		{
			if (AllKeys[i].Function == function)
			{
				return AllKeys[i].Key;
			}
		}
		return KeyCode.None;
	}

	public bool isKeyUsed(KeyCode newKey)
	{
		for (int i = 0; i < AllKeys.Count; i++)
		{
			if (AllKeys[i].Key == newKey)
			{
				return true;
			}
		}
		return false;
	}
}
