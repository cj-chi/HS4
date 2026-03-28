using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Manager;
using MyLocalize;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class SearchBase : Singleton<SearchBase>
{
	public class CustomSettingSave
	{
		public Version version = CharaSearchDefine.SettingVersion;

		public Color backColor = Color.gray;

		public bool bgmOn = true;

		public bool sliderWheel = true;

		public bool centerDraw = true;

		public float bgmVol = 0.3f;

		public float seVol = 0.5f;

		public Vector2 winSubLayout = new Vector2(1444f, -8f);

		public Vector2 winDrawLayout = new Vector2(1536f, -568f);

		public Vector2 winColorLayout = new Vector2(1536f, -768f);

		public Vector2 winPatternLayout = new Vector2(1176f, -8f);

		public void ResetWinLayout()
		{
			winSubLayout = new Vector2(1444f, -8f);
			winDrawLayout = new Vector2(1536f, -568f);
			winColorLayout = new Vector2(1536f, -768f);
			winPatternLayout = new Vector2(1176f, -8f);
		}

		public void Save()
		{
			string path = UserData.Path + "custom/searchscene.dat";
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(CharaSearchDefine.SettingVersion.ToString());
			binaryWriter.Write(backColor.r);
			binaryWriter.Write(backColor.g);
			binaryWriter.Write(backColor.b);
			binaryWriter.Write(bgmOn);
			binaryWriter.Write(sliderWheel);
			binaryWriter.Write(centerDraw);
			binaryWriter.Write(bgmVol);
			binaryWriter.Write(seVol);
			binaryWriter.Write(winSubLayout.x);
			binaryWriter.Write(winSubLayout.y);
			binaryWriter.Write(winDrawLayout.x);
			binaryWriter.Write(winDrawLayout.y);
			binaryWriter.Write(winColorLayout.x);
			binaryWriter.Write(winColorLayout.y);
			binaryWriter.Write(winPatternLayout.x);
			binaryWriter.Write(winPatternLayout.y);
		}

		public void Load()
		{
			string path = UserData.Path + "custom/searchscene.dat";
			if (!File.Exists(path))
			{
				return;
			}
			using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(input);
			version = new Version(binaryReader.ReadString());
			backColor.r = binaryReader.ReadSingle();
			backColor.g = binaryReader.ReadSingle();
			backColor.b = binaryReader.ReadSingle();
			bgmOn = binaryReader.ReadBoolean();
			sliderWheel = binaryReader.ReadBoolean();
			centerDraw = binaryReader.ReadBoolean();
			bgmVol = binaryReader.ReadSingle();
			seVol = binaryReader.ReadSingle();
			winSubLayout.x = binaryReader.ReadSingle();
			winSubLayout.y = binaryReader.ReadSingle();
			winDrawLayout.x = binaryReader.ReadSingle();
			winDrawLayout.y = binaryReader.ReadSingle();
			winColorLayout.x = binaryReader.ReadSingle();
			winColorLayout.y = binaryReader.ReadSingle();
			winPatternLayout.x = binaryReader.ReadSingle();
			winPatternLayout.y = binaryReader.ReadSingle();
		}
	}

	public class PlayVoiceBackup
	{
		public bool playSampleVoice;

		public int backEyebrowPtn;

		public int backEyesPtn;

		public bool backBlink = true;

		public float backEyesOpen = 1f;

		public int backMouthPtn;

		public bool backMouthFix = true;

		public float backMouthOpen;
	}

	[Header("-----------------------------------------")]
	private BoolReactiveProperty _sliderControlWheel = new BoolReactiveProperty(initialValue: true);

	public SearchColorCtrl searchColorCtrl;

	public SvsCaptureMenu svsCapMenu;

	public SearchDrawMenu drawMenu;

	public SaveFrameAssist saveFrameAssist;

	public CustomCultureControl cultureControl;

	public ChaFemaleRandom chaRandom;

	public Light lightCustom;

	public PlayVoiceBackup playVoiceBackup = new PlayVoiceBackup();

	public CustomSettingSave customSettingSave = new CustomSettingSave();

	public ChaFileControl defChaCtrl = new ChaFileControl();

	private string animeStateName = "";

	public SearchControl searchCtrl;

	public FoundFemaleWindow foundFemaleWindow;

	public Dictionary<int, string> dictPersonality = new Dictionary<int, string>();

	public List<InputField> lstInputField = new List<InputField>();

	private BoolReactiveProperty _drawSaveFrameTop = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _forceBackFrameHide = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawSaveFrameBack = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawSaveFrameFront = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _playPoseAnime = new BoolReactiveProperty(initialValue: true);

	private BoolReactiveProperty _cursorDraw = new BoolReactiveProperty(initialValue: true);

	private BoolReactiveProperty _accessoryDraw = new BoolReactiveProperty(initialValue: true);

	public int backPoseNo = -1;

	private IntReactiveProperty _poseNo = new IntReactiveProperty(-1);

	public float animationPos = -1f;

	private IntReactiveProperty _eyelook = new IntReactiveProperty(0);

	private IntReactiveProperty _necklook = new IntReactiveProperty(1);

	private BoolReactiveProperty _updateCvsCharaSaveDelete = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsCharaLoad = new BoolReactiveProperty(initialValue: false);

	public bool sliderControlWheel
	{
		get
		{
			return _sliderControlWheel.Value;
		}
		set
		{
			_sliderControlWheel.Value = value;
		}
	}

	public string nextSceneName { get; set; } = "";

	public byte modeSex { get; set; } = 1;

	public ChaControl chaCtrl { get; set; }

	public MotionIK customMotionIK { get; set; }

	public bool autoClothesState { get; set; } = true;

	public int autoClothesStateNo { get; set; }

	public int clothesStateNo { get; set; }

	public int eyebrowPtn { get; set; }

	public int eyePtn { get; set; }

	public int mouthPtn { get; set; }

	public bool drawSaveFrameTop
	{
		get
		{
			return _drawSaveFrameTop.Value;
		}
		set
		{
			_drawSaveFrameTop.Value = value;
		}
	}

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

	public bool drawSaveFrameBack
	{
		get
		{
			return _drawSaveFrameBack.Value;
		}
		set
		{
			_drawSaveFrameBack.Value = value;
		}
	}

	public bool drawSaveFrameFront
	{
		get
		{
			return _drawSaveFrameFront.Value;
		}
		set
		{
			_drawSaveFrameFront.Value = value;
		}
	}

	public bool playPoseAnime
	{
		get
		{
			return _playPoseAnime.Value;
		}
		set
		{
			_playPoseAnime.Value = value;
		}
	}

	public bool cursorDraw
	{
		get
		{
			return _cursorDraw.Value;
		}
		set
		{
			_cursorDraw.Value = value;
		}
	}

	public bool updateCustomUI { get; set; }

	public bool accessoryDraw
	{
		get
		{
			return _accessoryDraw.Value;
		}
		set
		{
			_accessoryDraw.Value = value;
		}
	}

	public int poseNo
	{
		get
		{
			return _poseNo.Value;
		}
		set
		{
			_poseNo.Value = value;
		}
	}

	public int eyelook
	{
		get
		{
			return _eyelook.Value;
		}
		set
		{
			_eyelook.Value = value;
		}
	}

	public int necklook
	{
		get
		{
			return _necklook.Value;
		}
		set
		{
			_necklook.Value = value;
		}
	}

	public bool updateCvsCharaSaveDelete
	{
		get
		{
			return _updateCvsCharaSaveDelete.Value;
		}
		set
		{
			_updateCvsCharaSaveDelete.Value = value;
		}
	}

	public bool updateCvsCharaLoad
	{
		get
		{
			return _updateCvsCharaLoad.Value;
		}
		set
		{
			_updateCvsCharaLoad.Value = value;
		}
	}

	public event Action actUpdateCvsCharaSaveDelete;

	public event Action actUpdateCvsCharaLoad;

	public void ChangeClothesState(int stateNo)
	{
		byte[,] array = new byte[3, 8]
		{
			{ 0, 0, 0, 0, 0, 0, 0, 0 },
			{ 2, 2, 0, 0, 0, 0, 0, 2 },
			{ 2, 2, 2, 2, 2, 2, 2, 2 }
		};
		if (-1 != stateNo)
		{
			if (stateNo == 0)
			{
				autoClothesState = true;
				clothesStateNo = autoClothesStateNo;
			}
			else
			{
				autoClothesState = false;
				clothesStateNo = stateNo - 1;
			}
		}
		if ((bool)chaCtrl)
		{
			int num = Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length;
			for (int i = 0; i < num; i++)
			{
				chaCtrl.SetClothesState(i, array[clothesStateNo, i]);
			}
		}
	}

	public void ChangeAnimationNext(int next)
	{
		if (null == chaCtrl)
		{
			return;
		}
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_pose_m : ChaListDefine.CategoryNo.custom_pose_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		if (next == 0)
		{
			int num = (poseNo + array.Length - 1) % array.Length;
			if (num == 0)
			{
				num = array.Length - 1;
			}
			poseNo = num;
		}
		else
		{
			int num2 = (poseNo + 1) % array.Length;
			if (num2 == 0)
			{
				num2++;
			}
			poseNo = num2;
		}
	}

	public void ChangeAnimationNo(int no, bool mannequin = false)
	{
		if (!(null == chaCtrl))
		{
			ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_pose_m : ChaListDefine.CategoryNo.custom_pose_f);
			int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
			if (!mannequin && no < 1)
			{
				no = 1;
			}
			if (no >= array.Length)
			{
				no = array.Length - 1;
			}
			poseNo = no;
		}
	}

	public bool ChangeAnimation()
	{
		if (null == chaCtrl)
		{
			return false;
		}
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_pose_m : ChaListDefine.CategoryNo.custom_pose_f);
		Dictionary<int, ListInfoBase> categoryInfo = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type);
		int[] array = categoryInfo.Keys.ToArray();
		if (poseNo >= array.Length || poseNo < 0)
		{
			return false;
		}
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		string text6 = "";
		if (categoryInfo.TryGetValue(array[poseNo], out var value))
		{
			text = value.GetInfo(ChaListDefine.KeyType.MainManifest);
			text2 = value.GetInfo(ChaListDefine.KeyType.MainAB);
			text3 = value.GetInfo(ChaListDefine.KeyType.MainData);
			text4 = value.GetInfo(ChaListDefine.KeyType.Clip);
			text5 = value.GetInfo(ChaListDefine.KeyType.IKAB);
			text6 = value.GetInfo(ChaListDefine.KeyType.IKData);
			bool flag = true;
			if (0 <= backPoseNo && categoryInfo.TryGetValue(array[backPoseNo], out var value2) && value2.GetInfo(ChaListDefine.KeyType.MainManifest) == text && value2.GetInfo(ChaListDefine.KeyType.MainAB) == text2 && value2.GetInfo(ChaListDefine.KeyType.MainData) == text3)
			{
				flag = false;
			}
			if (flag)
			{
				chaCtrl.LoadAnimation(text2, text3, text);
			}
			if (customMotionIK != null)
			{
				TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(text5, text6);
				if ((bool)textAsset)
				{
					customMotionIK.LoadData(textAsset);
				}
			}
			if (0f > animationPos)
			{
				chaCtrl.AnimPlay(text4);
			}
			else
			{
				chaCtrl.syncPlay(text4, 0, animationPos);
			}
			animationPos = -1f;
			if (customMotionIK != null)
			{
				customMotionIK.Calc(text4);
			}
			chaCtrl.resetDynamicBoneAll = true;
			animeStateName = text4;
			backPoseNo = poseNo;
			return true;
		}
		return false;
	}

	public void ChangeEyebrowPtnNext(int next)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_eyebrow_m : ChaListDefine.CategoryNo.custom_eyebrow_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		if (-1 == next)
		{
			eyebrowPtn = 0;
		}
		else if (next == 0)
		{
			eyebrowPtn = (eyebrowPtn + array.Length - 1) % array.Length;
		}
		else
		{
			eyebrowPtn = (eyebrowPtn + 1) % array.Length;
		}
		chaCtrl.ChangeEyebrowPtn(array[eyebrowPtn]);
	}

	public void ChangeEyebrowPtnNo(int no)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_eyebrow_m : ChaListDefine.CategoryNo.custom_eyebrow_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		eyebrowPtn = Mathf.Clamp(no - 1, 0, array.Length - 1);
		chaCtrl.ChangeEyebrowPtn(array[eyebrowPtn]);
	}

	public void ChangeEyePtnNext(int next)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_eye_m : ChaListDefine.CategoryNo.custom_eye_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		if (-1 == next)
		{
			eyePtn = 0;
		}
		else if (next == 0)
		{
			eyePtn = (eyePtn + array.Length - 1) % array.Length;
		}
		else
		{
			eyePtn = (eyePtn + 1) % array.Length;
		}
		chaCtrl.ChangeEyesPtn(array[eyePtn]);
	}

	public void ChangeEyePtnNo(int no)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_eye_m : ChaListDefine.CategoryNo.custom_eye_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		eyePtn = Mathf.Clamp(no - 1, 0, array.Length - 1);
		chaCtrl.ChangeEyesPtn(array[eyePtn]);
	}

	public void ChangeMouthPtnNext(int next)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_mouth_m : ChaListDefine.CategoryNo.custom_mouth_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		if (-1 == next)
		{
			mouthPtn = 0;
		}
		else if (next == 0)
		{
			mouthPtn = (mouthPtn + array.Length - 1) % array.Length;
		}
		else
		{
			mouthPtn = (mouthPtn + 1) % array.Length;
		}
		chaCtrl.ChangeMouthPtn(array[mouthPtn]);
	}

	public void ChangeMouthPtnNo(int no)
	{
		ChaListDefine.CategoryNo type = ((chaCtrl.sex == 0) ? ChaListDefine.CategoryNo.custom_mouth_m : ChaListDefine.CategoryNo.custom_mouth_f);
		int[] array = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(type).Keys.ToArray();
		mouthPtn = Mathf.Clamp(no - 1, 0, array.Length - 1);
		chaCtrl.ChangeMouthPtn(array[mouthPtn]);
	}

	public void UpdateIKCalc()
	{
		if (customMotionIK != null)
		{
			customMotionIK.Calc(animeStateName);
		}
	}

	public bool IsInputFocused()
	{
		foreach (InputField item in lstInputField)
		{
			if (!(null == item) && item.isFocused)
			{
				return true;
			}
		}
		return false;
	}

	public void ResetLightSetting()
	{
		lightCustom.transform.localEulerAngles = new Vector3(8f, -20f, 0f);
		lightCustom.color = new Color(0.951f, 0.906f, 0.876f);
		lightCustom.intensity = 1f;
	}

	protected override void Awake()
	{
		lstInputField.Clear();
		customSettingSave.Load();
	}

	private IEnumerator Start()
	{
		base.enabled = false;
		yield return new WaitUntil(() => Singleton<Character>.IsInstance());
		sliderControlWheel = customSettingSave.sliderWheel;
		_drawSaveFrameTop.Subscribe(delegate(bool draw)
		{
			if (null != saveFrameAssist)
			{
				saveFrameAssist.SetActiveSaveFrameTop(draw);
			}
		});
		_forceBackFrameHide.Subscribe(delegate(bool hide)
		{
			if (null != saveFrameAssist)
			{
				saveFrameAssist.forceBackFrameHide = hide;
			}
		});
		_drawSaveFrameBack.Subscribe(delegate(bool draw)
		{
			if (null != saveFrameAssist)
			{
				saveFrameAssist.backFrameDraw = draw;
			}
		});
		_drawSaveFrameFront.Subscribe(delegate(bool draw)
		{
			if (null != saveFrameAssist)
			{
				saveFrameAssist.frontFrameDraw = draw;
			}
		});
		_updateCvsCharaSaveDelete.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsCharaSaveDelete?.Invoke();
			updateCvsCharaSaveDelete = false;
		});
		_updateCvsCharaLoad.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsCharaLoad?.Invoke();
			updateCvsCharaLoad = false;
		});
		_accessoryDraw.Subscribe(delegate(bool f)
		{
			chaCtrl.SetAccessoryStateAll(f);
		});
		_poseNo.Subscribe(delegate
		{
			ChangeAnimation();
		});
		_eyelook.Subscribe(delegate(int v)
		{
			chaCtrl.ChangeLookEyesPtn((v == 0) ? 1 : 0);
		});
		_necklook.Subscribe(delegate(int v)
		{
			chaCtrl.ChangeLookNeckPtn((v == 0) ? 1 : 3);
		});
		_cursorDraw.Subscribe(delegate(bool f)
		{
			Cursor.visible = f;
		});
		_playPoseAnime.Subscribe(delegate(bool f)
		{
			chaCtrl.animBody.speed = (f ? 1f : 0f);
		});
		base.enabled = true;
	}

	private void OnDestroy()
	{
		customSettingSave.Save();
	}
}
