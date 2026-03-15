using System;
using AIChara;
using UnityEngine;

namespace CharaCustom;

public class BackgroundCtrl : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Camera backCam;

	[SerializeField]
	private int type;

	private FolderAssist dirBG = new FolderAssist();

	private string lastBGName = "";

	private bool m_IsVisible;

	private bool initialize = true;

	public bool isVisible
	{
		get
		{
			return m_IsVisible;
		}
		set
		{
			m_IsVisible = value;
			meshRenderer.enabled = value;
		}
	}

	public bool ChangeBGImage(byte changeNo, bool listUpdate = true)
	{
		if (listUpdate)
		{
			string text = "bg";
			string[] searchPattern = new string[1] { "*.png" };
			dirBG.lstFile.Clear();
			dirBG.CreateFolderInfoEx(DefaultData.Path + text, searchPattern);
			dirBG.CreateFolderInfoEx(UserData.Path + text, searchPattern, clear: false);
			dirBG.SortFullPath();
		}
		int fileCount = dirBG.GetFileCount();
		if (fileCount == 0)
		{
			return false;
		}
		int num = dirBG.GetIndexFromFullPath(lastBGName);
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
		Texture value = PngAssist.LoadTexture(dirBG.lstFile[num].FullPath);
		if ((bool)meshRenderer && (bool)meshRenderer.material)
		{
			Texture texture = meshRenderer.material.GetTexture(ChaShader.MainTex);
			if ((bool)texture)
			{
				UnityEngine.Object.Destroy(texture);
			}
			meshRenderer.material.SetTexture(ChaShader.MainTex, value);
		}
		lastBGName = dirBG.lstFile[num].FullPath;
		return true;
	}

	private void Reflect()
	{
		if (!(null == backCam))
		{
			Vector3[] vertices = meshFilter.mesh.vertices;
			float num = backCam.fieldOfView / 2f;
			float angle = Mathf.Atan(Mathf.Tan((float)Math.PI / 180f * num) * backCam.aspect) * 57.29578f;
			Plane plane = new Plane(Vector3.back, backCam.farClipPlane - 2f);
			Vector3 vector = Raycast(plane, Vector3.forward);
			Vector3 vector2 = Raycast(plane, Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward);
			Vector3 vector3 = Raycast(plane, Quaternion.AngleAxis(num, Vector3.right) * Vector3.forward);
			if (type == 0)
			{
				vertices[0] = new Vector3(vector2.x, 0f - vector3.y, vector.z);
				vertices[1] = new Vector3(0f - vector2.x, vector3.y, vector.z);
				vertices[2] = new Vector3(0f - vector2.x, 0f - vector3.y, vector.z);
				vertices[3] = new Vector3(vector2.x, vector3.y, vector.z);
			}
			else
			{
				float num2 = 63f / 160f;
				float num3 = 0.97777f;
				vertices[0] = new Vector3(vector2.x * num2, (0f - vector3.y) * num3, vector.z - 0.1f);
				vertices[1] = new Vector3((0f - vector2.x) * num2, vector3.y * num3, vector.z - 0.1f);
				vertices[2] = new Vector3((0f - vector2.x) * num2, (0f - vector3.y) * num3, vector.z - 0.1f);
				vertices[3] = new Vector3(vector2.x * num2, vector3.y * num3, vector.z - 0.1f);
			}
			meshFilter.mesh.vertices = vertices;
			meshFilter.mesh.RecalculateBounds();
		}
	}

	private Vector3 Raycast(Plane _plane, Vector3 _dir)
	{
		float enter = 0f;
		_plane.Raycast(new Ray(Vector3.zero, _dir), out enter);
		return _dir * enter;
	}

	private void Start()
	{
		isVisible = true;
	}

	private void LateUpdate()
	{
		if (isVisible && initialize)
		{
			Reflect();
			if (type == 0)
			{
				ChangeBGImage(0);
			}
			initialize = false;
		}
	}
}
