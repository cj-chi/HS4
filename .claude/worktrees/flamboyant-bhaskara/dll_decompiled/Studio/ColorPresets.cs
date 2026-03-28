using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illusion.Component.UI;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class ColorPresets : MonoBehaviour
{
	public class ColorInfo
	{
		public int select = 5;

		public List<Color> lstColor01 = new List<Color>();

		public List<Color> lstColor02 = new List<Color>();

		public List<Color> lstColor03 = new List<Color>();

		public List<Color> lstColor04 = new List<Color>();

		public List<Color> lstColor05 = new List<Color>();

		public List<Color> lstColorSample = new List<Color>();

		public void SetList(int idx, List<Color> lst)
		{
			switch (idx)
			{
			case 0:
				lstColor01 = lst;
				break;
			case 1:
				lstColor02 = lst;
				break;
			case 2:
				lstColor03 = lst;
				break;
			case 3:
				lstColor04 = lst;
				break;
			case 4:
				lstColor05 = lst;
				break;
			case 5:
				lstColorSample = lst;
				break;
			}
		}

		public List<Color> GetList(int idx)
		{
			return idx switch
			{
				0 => lstColor01, 
				1 => lstColor02, 
				2 => lstColor03, 
				3 => lstColor04, 
				4 => lstColor05, 
				5 => lstColorSample, 
				_ => null, 
			};
		}

		public void DeleteList(int idx)
		{
			switch (idx)
			{
			case 0:
				lstColor01.Clear();
				break;
			case 1:
				lstColor02.Clear();
				break;
			case 2:
				lstColor03.Clear();
				break;
			case 3:
				lstColor04.Clear();
				break;
			case 4:
				lstColor05.Clear();
				break;
			case 5:
				lstColorSample.Clear();
				break;
			}
		}
	}

	private ColorInfo colorInfo = new ColorInfo();

	private string saveDir = "";

	private readonly string saveFile = "ColorPresets.json";

	private const int presetMax = 90;

	[SerializeField]
	private Toggle[] tglFile;

	[SerializeField]
	private GameObject objTemplate;

	[SerializeField]
	private GameObject objNew;

	private Image imgNew;

	private Transform trfParent;

	[SerializeField]
	private Button btnDelete;

	private List<UI_ColorPresetsInfo> lstInfo = new List<UI_ColorPresetsInfo>();

	private Color _color = Color.white;

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			SetColor(_color);
		}
	}

	public event Action<Color> updateColorAction;

	public event Action clickAction;

	private void Awake()
	{
		if (null == objNew)
		{
			objNew = base.transform.FindLoop("New")?.gameObject;
		}
		if ((bool)objNew)
		{
			Transform transform = objNew.transform.Find("imgColor");
			if ((bool)transform)
			{
				imgNew = transform.GetComponent<Image>();
			}
			trfParent = objNew.transform.parent;
		}
		if (null == objTemplate)
		{
			objTemplate = base.transform.FindLoop("TemplateColor")?.gameObject;
		}
		lstInfo.Clear();
	}

	private void Start()
	{
		saveDir = UserData.Path + "Custom/";
		LoadPresets();
		for (int i = 0; i < tglFile.Length; i++)
		{
			tglFile[i].isOn = false;
		}
		tglFile[colorInfo.select].isOn = true;
		SetPreset();
		if ((bool)objNew)
		{
			trfParent = objNew.transform.parent;
			Button component = objNew.GetComponent<Button>();
			if ((bool)component)
			{
				component.OnClickAsObservable().Subscribe(delegate
				{
					AddNewPreset(color);
					SavePresets();
				});
			}
			objNew.SetActiveIfDifferent(5 != colorInfo.select);
		}
		if (null != btnDelete)
		{
			btnDelete.OnClickAsObservable().Subscribe(delegate
			{
				SetPreset(delete: true);
			});
			btnDelete.gameObject.SetActiveIfDifferent(5 != colorInfo.select);
		}
		for (int num = 0; num < tglFile.Length; num++)
		{
			int no = num;
			tglFile[num].onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					colorInfo.select = no;
					SetPreset();
					SavePresets();
					btnDelete.gameObject.SetActiveIfDifferent(5 != no);
					objNew.SetActiveIfDifferent(5 != no);
				}
			});
		}
	}

	public int GetSelectIndex()
	{
		for (int i = 0; i < tglFile.Length; i++)
		{
			if (tglFile[i].isOn)
			{
				return i;
			}
		}
		return 0;
	}

	public void SetColor(Color c)
	{
		if ((bool)objNew && (bool)imgNew)
		{
			imgNew.color = c;
		}
	}

	public void AddNewPreset(Color addColor, bool load = false)
	{
		GameObject addObj = UnityEngine.Object.Instantiate(objTemplate, trfParent);
		if (!(null != addObj))
		{
			return;
		}
		int idx = GetSelectIndex();
		addObj.name = $"PresetColor";
		addObj.transform.SetSiblingIndex(lstInfo.Count);
		objNew.transform.SetSiblingIndex(90);
		UI_ColorPresetsInfo cpi = addObj.GetComponent<UI_ColorPresetsInfo>();
		cpi.color = addColor;
		if ((bool)cpi.image)
		{
			cpi.image.color = addColor;
		}
		lstInfo.Add(cpi);
		MouseButtonCheck mouseButtonCheck = addObj.AddComponent<MouseButtonCheck>();
		mouseButtonCheck.buttonType = MouseButtonCheck.ButtonType.Right;
		mouseButtonCheck.onPointerUp.AddListener(delegate
		{
			if (5 != colorInfo.select)
			{
				lstInfo.Remove(cpi);
				colorInfo.SetList(idx, lstInfo.Select((UI_ColorPresetsInfo info) => info.color).ToList());
				SavePresets();
				UnityEngine.Object.Destroy(addObj);
			}
		});
		if (5 == colorInfo.select)
		{
			UI_OnMouseOverMessage component = addObj.GetComponent<UI_OnMouseOverMessage>();
			if ((bool)component)
			{
				component.comment = "左クリックで適用";
			}
		}
		MouseButtonCheck mouseButtonCheck2 = addObj.AddComponent<MouseButtonCheck>();
		mouseButtonCheck2.buttonType = MouseButtonCheck.ButtonType.Left;
		mouseButtonCheck2.onPointerUp.AddListener(delegate
		{
			SetColor(cpi.color);
			this.updateColorAction?.Invoke(cpi.color);
			this.clickAction?.Invoke();
		});
		if (!load)
		{
			colorInfo.SetList(idx, lstInfo.Select((UI_ColorPresetsInfo info) => info.color).ToList());
		}
		addObj.SetActiveIfDifferent(active: true);
		if (90 <= lstInfo.Count)
		{
			objNew.SetActiveIfDifferent(active: false);
		}
	}

	public void SetPreset(bool delete = false)
	{
		int count = lstInfo.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(lstInfo[num].gameObject);
		}
		lstInfo.Clear();
		int idx = colorInfo.select;
		if (delete)
		{
			colorInfo.DeleteList(idx);
		}
		List<Color> list = colorInfo.GetList(idx);
		count = list.Count;
		for (int i = 0; i < count; i++)
		{
			AddNewPreset(list[i], load: true);
		}
	}

	public void SavePresets()
	{
		string path = saveDir + saveFile;
		if (!Directory.Exists(saveDir))
		{
			Directory.CreateDirectory(saveDir);
		}
		string contents = JsonUtility.ToJson(colorInfo);
		File.WriteAllText(path, contents);
	}

	public void LoadPresets()
	{
		string path = saveDir + saveFile;
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			colorInfo = JsonUtility.FromJson<ColorInfo>(json);
		}
	}
}
