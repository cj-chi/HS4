using System.Collections;
using System.IO;
using UnityEngine;

public class ChangeScreenTexture : MonoBehaviour
{
	private FolderAssist bgDirInfo = new FolderAssist();

	private int nowData;

	private bool loadEnd = true;

	private float loadStartTime;

	private IEnumerator coroutine;

	private void Start()
	{
		string text = "bg";
		string[] searchPattern = new string[4] { "*.png", "*.bmp", "*.jpg", "*.jpeg" };
		bgDirInfo.CreateFolderInfoEx(DefaultData.Path + text, searchPattern);
		bgDirInfo.CreateFolderInfoEx(UserData.Path + text, searchPattern, clear: false);
	}

	private IEnumerator LoadTexture(string path, Material material)
	{
		WWW file = new WWW("file://" + path);
		yield return file;
		material.mainTexture = file.texture;
		loadEnd = true;
	}

	private void Update()
	{
		if (!loadEnd)
		{
			if (Time.realtimeSinceStartup - loadStartTime >= 10f)
			{
				StopCoroutine(coroutine);
			}
		}
		else if (!(null == GetComponent<Renderer>()) && !(null == GetComponent<Renderer>().material) && bgDirInfo.lstFile.Count != 0)
		{
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				nowData = (nowData + bgDirInfo.lstFile.Count - 1) % bgDirInfo.lstFile.Count;
				LoadOrder();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				nowData = (nowData + 1) % bgDirInfo.lstFile.Count;
				LoadOrder();
			}
		}
	}

	private void LoadOrder()
	{
		string fullPath = bgDirInfo.lstFile[nowData].FullPath;
		if (File.Exists(fullPath))
		{
			loadStartTime = Time.realtimeSinceStartup;
			loadEnd = false;
			coroutine = LoadTexture(fullPath, GetComponent<Renderer>().material);
			StartCoroutine(coroutine);
		}
	}
}
