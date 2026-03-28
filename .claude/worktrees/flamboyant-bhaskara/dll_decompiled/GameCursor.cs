using System;
using System.Runtime.InteropServices;
using UniRx;
using UnityEngine;

public class GameCursor : Singleton<GameCursor>
{
	public enum CursorKind
	{
		None = -1,
		Spanking
	}

	private struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	public struct POINT
	{
		public int X;

		public int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	public static Vector3 pos = Vector3.zero;

	public static float speed = 2000f;

	public static bool isLock = false;

	private POINT lockPos;

	private bool GUICheckFlag;

	private static bool mouseLocked = false;

	private const int numTex = 1;

	private Texture2D[] atexChange = new Texture2D[1];

	private readonly string[] anameTex = new string[1] { "spanking" };

	private const int MOUSEEVENTF_LEFTDOWN = 2;

	private const int MOUSEEVENTF_LEFTUP = 4;

	private IntPtr windowPtr = GetForegroundWindow();

	private RECT winRect;

	public static bool isDraw
	{
		get
		{
			return Cursor.visible;
		}
		set
		{
			Cursor.visible = value;
		}
	}

	public static bool MouseLocked
	{
		get
		{
			return mouseLocked;
		}
		set
		{
			mouseLocked = value;
			Cursor.visible = !value;
			Cursor.lockState = ((!Cursor.visible) ? CursorLockMode.Locked : CursorLockMode.None);
		}
	}

	public CursorKind kindCursor { get; private set; }

	private void Start()
	{
		pos = Input.mousePosition;
		GetCursorPos(out lockPos);
		windowPtr = GetForegroundWindow();
		(from _ in Observable.EveryUpdate().TakeUntilDestroy(base.gameObject)
			where base.isActiveAndEnabled
			select _).Subscribe(delegate
		{
			if (isLock)
			{
				SetCursorPos(lockPos.X, lockPos.Y);
			}
			else
			{
				pos = Input.mousePosition;
				GetCursorPos(out lockPos);
			}
		});
	}

	public bool setCursor(CursorKind _eKind, Vector2 _vHotSpot)
	{
		Texture2D texture = null;
		if (_eKind == CursorKind.Spanking)
		{
			texture = atexChange[(int)_eKind];
		}
		Cursor.SetCursor(texture, _vHotSpot, CursorMode.ForceSoftware);
		kindCursor = _eKind;
		return true;
	}

	[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern void SetCursorPos(int X, int Y);

	[DllImport("USER32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("USER32.dll")]
	private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

	[DllImport("USER32.dll")]
	private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

	[DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
	private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	[DllImport("user32.dll")]
	private static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);

	[DllImport("user32.dll")]
	private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string className, string windowName);

	[DllImport("user32.dll")]
	public static extern bool SetWindowText(IntPtr hwnd, string lpString);

	public void SetCoursorPosition(Vector3 mousePos)
	{
		POINT lpPoint = new POINT(0, 0);
		ClientToScreen(windowPtr, ref lpPoint);
		GetWindowRect(windowPtr, ref winRect);
		POINT pOINT = new POINT(lpPoint.X - winRect.left, lpPoint.Y - winRect.top);
		lpPoint.X = (int)mousePos.x;
		lpPoint.Y = Screen.height - (int)mousePos.y;
		ClientToScreen(windowPtr, ref lpPoint);
		SetCursorPos(lpPoint.X + pOINT.X, lpPoint.Y + pOINT.Y);
	}

	public void SetCursorLock(bool setLockFlag)
	{
		if (setLockFlag)
		{
			if (!isLock)
			{
				GetCursorPos(out lockPos);
				isLock = true;
				Cursor.visible = false;
			}
		}
		else if (isLock)
		{
			SetCursorPos(lockPos.X, lockPos.Y);
			isLock = false;
			Cursor.visible = true;
		}
	}

	public void UnLockCursor()
	{
		if (isLock)
		{
			isLock = false;
			Cursor.visible = true;
		}
	}

	public void UpdateCursorLock()
	{
		if (isLock)
		{
			SetCursorPos(lockPos.X, lockPos.Y);
		}
	}
}
