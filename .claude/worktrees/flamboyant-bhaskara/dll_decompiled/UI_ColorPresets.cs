using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CharaCustom;
using Illusion.Component.UI;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using Manager;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UI_ColorPresets : MonoBehaviour
{
	public class ColorInfo
	{
		public int select = 3;

		public List<Color> lstColor01 = new List<Color>();

		public List<Color> lstColor02 = new List<Color>();

		public List<Color> lstColor03 = new List<Color>();

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
				3 => lstColorSample, 
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
				lstColorSample.Clear();
				break;
			}
		}
	}

	private const string colorPresetsFile = "ColorPresets.json";

	private const string sampleAssetBundle = "custom/colorsample.unity3d";

	private const string sampleAsset = "ColorPresets";

	private ColorInfo colorInfo = new ColorInfo();

	private string saveDir = "";

	private const int presetMax = 77;

	[SerializeField]
	private UI_ToggleEx[] tglFile;

	[SerializeField]
	private GameObject objTemplate;

	[SerializeField]
	private GameObject objNew;

	private Image imgNew;

	private Transform trfParent;

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

	private void Awake()
	{
		if (objNew == null)
		{
			Transform transform = base.transform.FindLoop("New");
			if (transform != null)
			{
				objNew = transform.gameObject;
			}
		}
		if (null != objNew)
		{
			Transform transform2 = objNew.transform.Find("imgColor");
			if (null != transform2)
			{
				imgNew = transform2.GetComponent<Image>();
			}
			trfParent = objNew.transform.parent;
		}
		if (objTemplate == null)
		{
			Transform transform3 = base.transform.FindLoop("TemplateColor");
			if (transform3 != null)
			{
				objTemplate = transform3.gameObject;
			}
		}
		lstInfo.Clear();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		saveDir = UserData.Path + "Custom/";
		LoadPresets();
		for (int num = 0; num < tglFile.Length; num++)
		{
			tglFile[num].isOn = false;
		}
		if (!tglFile.SafeProc(colorInfo.select, delegate(UI_ToggleEx _t)
		{
			_t.isOn = true;
		}))
		{
			colorInfo.select = Mathf.Clamp(colorInfo.select, 0, tglFile.Length - 1);
			tglFile[colorInfo.select].isOn = true;
		}
		SetPreset();
		if (null != objNew)
		{
			trfParent = objNew.transform.parent;
			Button component = objNew.GetComponent<Button>();
			if (null != component)
			{
				component.OnClickAsObservable().Subscribe(delegate
				{
					AddNewPreset(color);
					SavePresets();
				});
			}
			UI_OnMouseOverMessageEx component2 = objNew.GetComponent<UI_OnMouseOverMessageEx>();
			if (null != component2)
			{
				component2.ChangeMessage(CharaCustomDefine.ColorPresetNewMessage[Singleton<GameSystem>.Instance.languageInt]);
			}
			objNew.SetActiveIfDifferent(3 != colorInfo.select);
			if (77 <= lstInfo.Count)
			{
				objNew.SetActiveIfDifferent(active: false);
			}
		}
		for (int num2 = 0; num2 < tglFile.Length; num2++)
		{
			int no = num2;
			tglFile[num2].onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					colorInfo.select = no;
					SetPreset();
					SavePresets();
					objNew.SetActiveIfDifferent(3 != no);
					if (77 <= lstInfo.Count)
					{
						objNew.SetActiveIfDifferent(active: false);
					}
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
		if (null != objNew && null != imgNew)
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
		objNew.transform.SetSiblingIndex(77);
		UI_ColorPresetsInfo cpi = addObj.GetComponent<UI_ColorPresetsInfo>();
		cpi.color = addColor;
		if (null != cpi.image)
		{
			cpi.image.color = addColor;
		}
		lstInfo.Add(cpi);
		MouseButtonCheck mouseButtonCheck = addObj.AddComponent<MouseButtonCheck>();
		mouseButtonCheck.isLeft = false;
		mouseButtonCheck.isCenter = false;
		mouseButtonCheck.onPointerUp.AddListener(delegate
		{
			if (3 != colorInfo.select)
			{
				lstInfo.Remove(cpi);
				colorInfo.SetList(idx, lstInfo.Select((UI_ColorPresetsInfo info) => info.color).ToList());
				SavePresets();
				UnityEngine.Object.Destroy(addObj);
				objNew.SetActiveIfDifferent(active: true);
			}
		});
		UI_OnMouseOverMessageEx component = addObj.GetComponent<UI_OnMouseOverMessageEx>();
		if (null != component)
		{
			component.ChangeMessage(CharaCustomDefine.ColorPresetMessage[Singleton<GameSystem>.Instance.languageInt, 0], CharaCustomDefine.ColorPresetMessage[Singleton<GameSystem>.Instance.languageInt, 1]);
			component.showMsgNo = ((3 == colorInfo.select) ? 1 : 0);
		}
		MouseButtonCheck mouseButtonCheck2 = addObj.AddComponent<MouseButtonCheck>();
		mouseButtonCheck2.isRight = false;
		mouseButtonCheck2.isCenter = false;
		mouseButtonCheck2.onPointerUp.AddListener(delegate
		{
			SetColor(cpi.color);
			color = cpi.color;
			this.updateColorAction?.Invoke(cpi.color);
		});
		if (!load)
		{
			colorInfo.SetList(idx, lstInfo.Select((UI_ColorPresetsInfo info) => info.color).ToList());
		}
		addObj.SetActiveIfDifferent(active: true);
		if (77 <= lstInfo.Count)
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
		string path = saveDir + "ColorPresets.json";
		if (!Directory.Exists(saveDir))
		{
			Directory.CreateDirectory(saveDir);
		}
		string contents = JsonUtility.ToJson(colorInfo);
		File.WriteAllText(path, contents);
	}

	public void LoadPresets()
	{
		string path = saveDir + "ColorPresets.json";
		if (!File.Exists(path))
		{
			TextAsset textAsset = CommonLib.LoadAsset<TextAsset>("custom/colorsample.unity3d", "ColorPresets");
			if (null != textAsset)
			{
				this.colorInfo = JsonUtility.FromJson<ColorInfo>(textAsset.text);
				AssetBundleManager.UnloadAssetBundle("custom/colorsample.unity3d", isUnloadForceRefCount: true);
				SavePresets();
			}
			return;
		}
		string json = File.ReadAllText(path);
		this.colorInfo = JsonUtility.FromJson<ColorInfo>(json);
		if (this.colorInfo.lstColorSample.Count == 0)
		{
			TextAsset textAsset2 = CommonLib.LoadAsset<TextAsset>("custom/colorsample.unity3d", "ColorPresets");
			if (null != textAsset2)
			{
				ColorInfo colorInfo = JsonUtility.FromJson<ColorInfo>(textAsset2.text);
				AssetBundleManager.UnloadAssetBundle("custom/colorsample.unity3d", isUnloadForceRefCount: true);
				this.colorInfo.lstColorSample = new List<Color>(colorInfo.lstColorSample);
				SavePresets();
			}
		}
	}
}
