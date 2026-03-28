using AIChara;
using Illusion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SaveFrameAssist : MonoBehaviour
{
	private FolderAssist dirFrameBack = new FolderAssist();

	private FolderAssist dirFrameFront = new FolderAssist();

	private string lastFrameBackName = "";

	private string lastFrameFrontName = "";

	[SerializeField]
	private GameObject objSaveFrameTop;

	[SerializeField]
	private GameObject objSaveBack;

	[SerializeField]
	private GameObject objSaveFront;

	[SerializeField]
	private Renderer rendBack;

	[SerializeField]
	private RawImage riFront;

	public Camera backFrameCam;

	public Camera frontFrameCam;

	public BoolReactiveProperty _forceBackFrameHide = new BoolReactiveProperty(initialValue: false);

	public BoolReactiveProperty _backFrameDraw = new BoolReactiveProperty(initialValue: false);

	public BoolReactiveProperty _frontFrameDraw = new BoolReactiveProperty(initialValue: false);

	public bool forceBackFrameHide
	{
		get
		{
			return _forceBackFrameHide.Value;
		}
		set
		{
			_forceBackFrameHide.Value = value;
		}
	}

	public bool backFrameDraw
	{
		get
		{
			return _backFrameDraw.Value;
		}
		set
		{
			_backFrameDraw.Value = value;
		}
	}

	public bool frontFrameDraw
	{
		get
		{
			return _frontFrameDraw.Value;
		}
		set
		{
			_frontFrameDraw.Value = value;
		}
	}

	public void ForgetLastName()
	{
		lastFrameBackName = "";
		lastFrameFrontName = "";
	}

	public bool Initialize()
	{
		ChangeSaveFrameBack(1);
		ChangeSaveFrameFront(1);
		return true;
	}

	public bool ChangeSaveFrameBack(byte changeNo, bool listUpdate = true)
	{
		if (listUpdate)
		{
			string text = "cardframe/Back";
			string[] searchPattern = new string[1] { "*.png" };
			dirFrameBack.lstFile.Clear();
			dirFrameBack.CreateFolderInfoEx(DefaultData.Path + text, searchPattern);
			dirFrameBack.CreateFolderInfoEx(UserData.Path + text, searchPattern, clear: false);
			dirFrameBack.SortFullPath();
		}
		int fileCount = dirFrameBack.GetFileCount();
		if (fileCount == 0)
		{
			return false;
		}
		int num = dirFrameBack.GetIndexFromFullPath(lastFrameBackName);
		if (-1 == num)
		{
			num = 0;
		}
		else if (changeNo == 0)
		{
			num = (num + 1) % fileCount;
		}
		else if (1 == changeNo)
		{
			num = (num + fileCount - 1) % fileCount;
		}
		Texture value = PngAssist.LoadTexture(dirFrameBack.lstFile[num].FullPath);
		if ((bool)rendBack && (bool)rendBack.material)
		{
			Texture texture = rendBack.material.GetTexture(ChaShader.MainTex);
			if ((bool)texture)
			{
				Object.Destroy(texture);
			}
			rendBack.material.SetTexture(ChaShader.MainTex, value);
		}
		lastFrameBackName = dirFrameBack.lstFile[num].FullPath;
		return true;
	}

	public bool ChangeSaveFrameFront(byte changeNo, bool listUpdate = true)
	{
		if (listUpdate)
		{
			string text = "cardframe/Front";
			string[] searchPattern = new string[1] { "*.png" };
			dirFrameFront.lstFile.Clear();
			dirFrameFront.CreateFolderInfoEx(DefaultData.Path + text, searchPattern);
			dirFrameFront.CreateFolderInfoEx(UserData.Path + text, searchPattern, clear: false);
			dirFrameFront.SortFullPath();
		}
		int fileCount = dirFrameFront.GetFileCount();
		if (fileCount == 0)
		{
			return false;
		}
		int num = dirFrameFront.GetIndexFromFullPath(lastFrameFrontName);
		if (-1 == num)
		{
			num = 0;
		}
		else if (changeNo == 0)
		{
			num = (num + 1) % fileCount;
		}
		else if (1 == changeNo)
		{
			num = (num + fileCount - 1) % fileCount;
		}
		Texture texture = PngAssist.LoadTexture(dirFrameFront.lstFile[num].FullPath);
		if ((bool)riFront.texture)
		{
			Object.Destroy(riFront.texture);
		}
		riFront.texture = texture;
		lastFrameFrontName = dirFrameFront.lstFile[num].FullPath;
		return true;
	}

	public string GetNowPositionStringBack()
	{
		int fileCount = dirFrameBack.GetFileCount();
		if (fileCount == 0)
		{
			return "ファイルがありません";
		}
		int indexFromFullPath = dirFrameBack.GetIndexFromFullPath(lastFrameBackName);
		return $"{indexFromFullPath + 1:000} / {fileCount:000}";
	}

	public string GetNowPositionStringFront()
	{
		int fileCount = dirFrameFront.GetFileCount();
		if (fileCount == 0)
		{
			return "ファイルがありません";
		}
		int indexFromFullPath = dirFrameFront.GetIndexFromFullPath(lastFrameFrontName);
		return $"{indexFromFullPath + 1:000} / {fileCount:000}";
	}

	public bool SetActiveSaveFrameTop(bool active)
	{
		if (null == objSaveFrameTop)
		{
			return false;
		}
		objSaveFrameTop.SetActiveIfDifferent(active);
		return true;
	}

	public bool ChangeSaveFrameTexture(int bf, Texture tex)
	{
		if (null == objSaveFrameTop)
		{
			return false;
		}
		if (bf == 0)
		{
			if ((bool)rendBack && (bool)rendBack.material)
			{
				Texture texture = rendBack.material.GetTexture(ChaShader.MainTex);
				if ((bool)texture)
				{
					Object.Destroy(texture);
				}
				rendBack.material.SetTexture(ChaShader.MainTex, tex);
			}
		}
		else
		{
			if ((bool)riFront.texture)
			{
				Object.Destroy(riFront.texture);
			}
			riFront.texture = tex;
		}
		return true;
	}

	private void Start()
	{
		_forceBackFrameHide.Subscribe(delegate(bool hide)
		{
			if (hide)
			{
				if (null != objSaveBack)
				{
					objSaveBack.SetActiveIfDifferent(active: false);
				}
				backFrameCam.enabled = false;
			}
			else
			{
				if (null != objSaveBack)
				{
					objSaveBack.SetActiveIfDifferent(backFrameDraw);
				}
				backFrameCam.enabled = backFrameDraw;
			}
		});
		_backFrameDraw.Subscribe(delegate(bool visible)
		{
			bool active = !forceBackFrameHide && visible;
			if (null != objSaveBack)
			{
				objSaveBack.SetActiveIfDifferent(active);
			}
			backFrameCam.enabled = active;
		});
		_frontFrameDraw.Subscribe(delegate(bool visible)
		{
			if (null != objSaveFront)
			{
				objSaveFront.SetActiveIfDifferent(visible);
			}
			frontFrameCam.enabled = visible;
		});
	}
}
