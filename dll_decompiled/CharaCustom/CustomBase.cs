using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using MyLocalize;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomBase : Singleton<CustomBase>
{
	public class CustomSettingSave
	{
		public class HairCtrlSetting
		{
			public bool drawController;

			public int controllerType;

			public float controllerSpeed = 0.3f;

			public float controllerScale = 0.4f;
		}

		public class AcsCtrlSetting
		{
			public class CorrectSetting
			{
				public int posRate;

				public int rotRate;

				public int sclRate;

				public bool draw;

				public int type;

				public float speed = 0.3f;

				public float scale = 1.5f;
			}

			public CorrectSetting[] correctSetting = new CorrectSetting[2];

			public AcsCtrlSetting()
			{
				for (int i = 0; i < correctSetting.Length; i++)
				{
					correctSetting[i] = new CorrectSetting();
				}
			}
		}

		public Version version = CharaCustomDefine.CustomSettingVersion;

		public Color backColor = Color.gray;

		public bool bgmOn = true;

		public bool sliderWheel = true;

		public bool centerDraw = true;

		public float bgmVol = 0.3f;

		public float seVol = 0.5f;

		public HairCtrlSetting hairCtrlSetting = new HairCtrlSetting();

		public AcsCtrlSetting acsCtrlSetting = new AcsCtrlSetting();

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
			string path = UserData.Path + "custom/customscene.dat";
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(CharaCustomDefine.CustomSettingVersion.ToString());
			binaryWriter.Write(backColor.r);
			binaryWriter.Write(backColor.g);
			binaryWriter.Write(backColor.b);
			binaryWriter.Write(bgmOn);
			binaryWriter.Write(hairCtrlSetting.drawController);
			binaryWriter.Write(hairCtrlSetting.controllerType);
			for (int i = 0; i < 2; i++)
			{
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].posRate);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].rotRate);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].sclRate);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].draw);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].type);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].speed);
				binaryWriter.Write(acsCtrlSetting.correctSetting[i].scale);
			}
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
			binaryWriter.Write(hairCtrlSetting.controllerSpeed);
			binaryWriter.Write(hairCtrlSetting.controllerScale);
		}

		public void Load()
		{
			string path = UserData.Path + "custom/customscene.dat";
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
			hairCtrlSetting.drawController = binaryReader.ReadBoolean();
			hairCtrlSetting.controllerType = binaryReader.ReadInt32();
			for (int i = 0; i < 2; i++)
			{
				acsCtrlSetting.correctSetting[i].posRate = binaryReader.ReadInt32();
				acsCtrlSetting.correctSetting[i].rotRate = binaryReader.ReadInt32();
				acsCtrlSetting.correctSetting[i].sclRate = binaryReader.ReadInt32();
				acsCtrlSetting.correctSetting[i].draw = binaryReader.ReadBoolean();
				acsCtrlSetting.correctSetting[i].type = binaryReader.ReadInt32();
				acsCtrlSetting.correctSetting[i].speed = binaryReader.ReadSingle();
				acsCtrlSetting.correctSetting[i].scale = binaryReader.ReadSingle();
			}
			if (version < new Version("0.0.1"))
			{
				return;
			}
			sliderWheel = binaryReader.ReadBoolean();
			centerDraw = binaryReader.ReadBoolean();
			bgmVol = binaryReader.ReadSingle();
			seVol = binaryReader.ReadSingle();
			if (!(version < new Version("0.0.2")))
			{
				winSubLayout.x = binaryReader.ReadSingle();
				winSubLayout.y = binaryReader.ReadSingle();
				winDrawLayout.x = binaryReader.ReadSingle();
				winDrawLayout.y = binaryReader.ReadSingle();
				winColorLayout.x = binaryReader.ReadSingle();
				winColorLayout.y = binaryReader.ReadSingle();
				winPatternLayout.x = binaryReader.ReadSingle();
				winPatternLayout.y = binaryReader.ReadSingle();
				if (!(version < new Version("0.0.3")))
				{
					hairCtrlSetting.controllerSpeed = binaryReader.ReadSingle();
					hairCtrlSetting.controllerScale = binaryReader.ReadSingle();
				}
			}
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

	[Serializable]
	public class HairUICondition
	{
		public GameObject objTopColorSet;

		public GameObject objUnderColorSet;

		public GameObject objGlossColorSet;
	}

	[Header("-----------------------------------------")]
	private BoolReactiveProperty _sliderControlWheel = new BoolReactiveProperty(initialValue: true);

	public CustomColorCtrl customColorCtrl;

	public CvsCaptureMenu cvsCapMenu;

	public CustomDrawMenu drawMenu;

	public SaveFrameAssist saveFrameAssist;

	public CustomCultureControl cultureControl;

	public Light lightCustom;

	public GameObject objAcs01ControllerTop;

	public GameObject objAcs02ControllerTop;

	public GameObject objHairControllerTop;

	[SerializeField]
	private Toggle tglEyesSameSetting;

	[SerializeField]
	private Toggle tglHairSameSetting;

	[SerializeField]
	private Toggle tglHairAutoSetting;

	[SerializeField]
	private Toggle tglHairControlTogether;

	[SerializeField]
	private Toggle tglSliderWheel;

	[SerializeField]
	private Toggle tglCenterDraw;

	[SerializeField]
	private Slider sldBGMVol;

	[SerializeField]
	private Slider sldSEVol;

	[SerializeField]
	private UI_ButtonEx subMenuBot;

	[SerializeField]
	private UI_ButtonEx subMenuInnerDown;

	public PlayVoiceBackup playVoiceBackup = new PlayVoiceBackup();

	[Header("髪の毛関連の表示 ------------------------")]
	[SerializeField]
	private HairUICondition hairUICondition;

	[Header("アクセサリのスロット名 ------------------")]
	[SerializeField]
	private Text[] acsSlotText;

	public CustomSettingSave customSettingSave = new CustomSettingSave();

	public ChaFileControl defChaCtrl = new ChaFileControl();

	public ChaFileControl editBackChaCtrl = new ChaFileControl();

	private string animeStateName = "";

	private List<bool> lstShow = new List<bool>();

	public bool[] showAcsController = new bool[2];

	public CustomControl customCtrl;

	public Dictionary<int, string> dictPersonality = new Dictionary<int, string>();

	public List<InputField> lstInputField = new List<InputField>();

	private BoolReactiveProperty _centerDraw = new BoolReactiveProperty(initialValue: true);

	private FloatReactiveProperty _bgmVol = new FloatReactiveProperty(0.3f);

	private FloatReactiveProperty _seVol = new FloatReactiveProperty(0.5f);

	private BoolReactiveProperty _drawSaveFrameTop = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _forceBackFrameHide = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawSaveFrameBack = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawSaveFrameFront = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _changeCharaName = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawTopHairColor = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _drawUnderHairColor = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _autoHairColor = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _playPoseAnime = new BoolReactiveProperty(initialValue: true);

	private BoolReactiveProperty _cursorDraw = new BoolReactiveProperty(initialValue: true);

	private BoolReactiveProperty _accessoryDraw = new BoolReactiveProperty(initialValue: true);

	public int backPoseNo = -1;

	private IntReactiveProperty _poseNo = new IntReactiveProperty(-1);

	public float animationPos = -1f;

	private IntReactiveProperty _eyelook = new IntReactiveProperty(0);

	private IntReactiveProperty _necklook = new IntReactiveProperty(1);

	private BoolReactiveProperty _updateCvsFaceType = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeWhole = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeChin = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeCheek = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeEyebrow = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeEyes = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeNose = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeMouth = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFaceShapeEar = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsMole = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyeLR = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyeEtc = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyeHL = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyebrow = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyelashes = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsEyeshadow = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsCheek = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsLip = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFacePaint = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBeard = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeWhole = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeBreast = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeUpper = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeLower = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeArm = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyShapeLeg = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodySkinType = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsSunburn = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsNip = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsUnderhair = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsNail = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsBodyPaint = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFutanari = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsHair = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsClothes = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsClothesSaveDelete = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsClothesLoad = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsAccessory = new BoolReactiveProperty(initialValue: false);

	public bool forceUpdateAcsList;

	private BoolReactiveProperty _updateCvsAcsCopy = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsChara = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsType = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsStatus = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsCharaSaveDelete = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsCharaLoad = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty _updateCvsFusion = new BoolReactiveProperty(initialValue: false);

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

	public string editSaveFileName { get; set; } = "";

	public bool modeNew { get; set; } = true;

	public byte modeSex { get; set; } = 1;

	public ChaControl chaCtrl { get; set; }

	public MotionIK customMotionIK { get; set; }

	public bool autoClothesState { get; set; } = true;

	public int autoClothesStateNo { get; set; }

	public int clothesStateNo { get; set; }

	public bool showAcsControllerAll { get; set; }

	public bool showHairController { get; set; }

	public int eyebrowPtn { get; set; }

	public int eyePtn { get; set; }

	public int mouthPtn { get; set; }

	public bool centerDraw
	{
		get
		{
			return _centerDraw.Value;
		}
		set
		{
			_centerDraw.Value = value;
		}
	}

	public float bgmVol
	{
		get
		{
			return _bgmVol.Value;
		}
		set
		{
			_bgmVol.Value = value;
		}
	}

	public float seVol
	{
		get
		{
			return _seVol.Value;
		}
		set
		{
			_seVol.Value = value;
		}
	}

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

	public bool changeCharaName
	{
		get
		{
			return _changeCharaName.Value;
		}
		set
		{
			_changeCharaName.Value = value;
		}
	}

	public bool drawTopHairColor
	{
		get
		{
			return _drawTopHairColor.Value;
		}
		set
		{
			_drawTopHairColor.Value = value;
		}
	}

	public bool drawUnderHairColor
	{
		get
		{
			return _drawUnderHairColor.Value;
		}
		set
		{
			_drawUnderHairColor.Value = value;
		}
	}

	public bool autoHairColor
	{
		get
		{
			return _autoHairColor.Value;
		}
		set
		{
			_autoHairColor.Value = value;
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

	public bool updateCvsFaceType
	{
		get
		{
			return _updateCvsFaceType.Value;
		}
		set
		{
			_updateCvsFaceType.Value = value;
		}
	}

	public bool updateCvsFaceShapeWhole
	{
		get
		{
			return _updateCvsFaceShapeWhole.Value;
		}
		set
		{
			_updateCvsFaceShapeWhole.Value = value;
		}
	}

	public bool updateCvsFaceShapeChin
	{
		get
		{
			return _updateCvsFaceShapeChin.Value;
		}
		set
		{
			_updateCvsFaceShapeChin.Value = value;
		}
	}

	public bool updateCvsFaceShapeCheek
	{
		get
		{
			return _updateCvsFaceShapeCheek.Value;
		}
		set
		{
			_updateCvsFaceShapeCheek.Value = value;
		}
	}

	public bool updateCvsFaceShapeEyebrow
	{
		get
		{
			return _updateCvsFaceShapeEyebrow.Value;
		}
		set
		{
			_updateCvsFaceShapeEyebrow.Value = value;
		}
	}

	public bool updateCvsFaceShapeEyes
	{
		get
		{
			return _updateCvsFaceShapeEyes.Value;
		}
		set
		{
			_updateCvsFaceShapeEyes.Value = value;
		}
	}

	public bool updateCvsFaceShapeNose
	{
		get
		{
			return _updateCvsFaceShapeNose.Value;
		}
		set
		{
			_updateCvsFaceShapeNose.Value = value;
		}
	}

	public bool updateCvsFaceShapeMouth
	{
		get
		{
			return _updateCvsFaceShapeMouth.Value;
		}
		set
		{
			_updateCvsFaceShapeMouth.Value = value;
		}
	}

	public bool updateCvsFaceShapeEar
	{
		get
		{
			return _updateCvsFaceShapeEar.Value;
		}
		set
		{
			_updateCvsFaceShapeEar.Value = value;
		}
	}

	public bool updateCvsMole
	{
		get
		{
			return _updateCvsMole.Value;
		}
		set
		{
			_updateCvsMole.Value = value;
		}
	}

	public bool updateCvsEyeLR
	{
		get
		{
			return _updateCvsEyeLR.Value;
		}
		set
		{
			_updateCvsEyeLR.Value = value;
		}
	}

	public bool updateCvsEyeEtc
	{
		get
		{
			return _updateCvsEyeEtc.Value;
		}
		set
		{
			_updateCvsEyeEtc.Value = value;
		}
	}

	public bool updateCvsEyeHL
	{
		get
		{
			return _updateCvsEyeHL.Value;
		}
		set
		{
			_updateCvsEyeHL.Value = value;
		}
	}

	public bool updateCvsEyebrow
	{
		get
		{
			return _updateCvsEyebrow.Value;
		}
		set
		{
			_updateCvsEyebrow.Value = value;
		}
	}

	public bool updateCvsEyelashes
	{
		get
		{
			return _updateCvsEyelashes.Value;
		}
		set
		{
			_updateCvsEyelashes.Value = value;
		}
	}

	public bool updateCvsEyeshadow
	{
		get
		{
			return _updateCvsEyeshadow.Value;
		}
		set
		{
			_updateCvsEyeshadow.Value = value;
		}
	}

	public bool updateCvsCheek
	{
		get
		{
			return _updateCvsCheek.Value;
		}
		set
		{
			_updateCvsCheek.Value = value;
		}
	}

	public bool updateCvsLip
	{
		get
		{
			return _updateCvsLip.Value;
		}
		set
		{
			_updateCvsLip.Value = value;
		}
	}

	public bool updateCvsFacePaint
	{
		get
		{
			return _updateCvsFacePaint.Value;
		}
		set
		{
			_updateCvsFacePaint.Value = value;
		}
	}

	public bool updateCvsBeard
	{
		get
		{
			return _updateCvsBeard.Value;
		}
		set
		{
			_updateCvsBeard.Value = value;
		}
	}

	public bool updateCvsBodyShapeWhole
	{
		get
		{
			return _updateCvsBodyShapeWhole.Value;
		}
		set
		{
			_updateCvsBodyShapeWhole.Value = value;
		}
	}

	public bool updateCvsBodyShapeBreast
	{
		get
		{
			return _updateCvsBodyShapeBreast.Value;
		}
		set
		{
			_updateCvsBodyShapeBreast.Value = value;
		}
	}

	public bool updateCvsBodyShapeUpper
	{
		get
		{
			return _updateCvsBodyShapeUpper.Value;
		}
		set
		{
			_updateCvsBodyShapeUpper.Value = value;
		}
	}

	public bool updateCvsBodyShapeLower
	{
		get
		{
			return _updateCvsBodyShapeLower.Value;
		}
		set
		{
			_updateCvsBodyShapeLower.Value = value;
		}
	}

	public bool updateCvsBodyShapeArm
	{
		get
		{
			return _updateCvsBodyShapeArm.Value;
		}
		set
		{
			_updateCvsBodyShapeArm.Value = value;
		}
	}

	public bool updateCvsBodyShapeLeg
	{
		get
		{
			return _updateCvsBodyShapeLeg.Value;
		}
		set
		{
			_updateCvsBodyShapeLeg.Value = value;
		}
	}

	public bool updateCvsBodySkinType
	{
		get
		{
			return _updateCvsBodySkinType.Value;
		}
		set
		{
			_updateCvsBodySkinType.Value = value;
		}
	}

	public bool updateCvsSunburn
	{
		get
		{
			return _updateCvsSunburn.Value;
		}
		set
		{
			_updateCvsSunburn.Value = value;
		}
	}

	public bool updateCvsNip
	{
		get
		{
			return _updateCvsNip.Value;
		}
		set
		{
			_updateCvsNip.Value = value;
		}
	}

	public bool updateCvsUnderhair
	{
		get
		{
			return _updateCvsUnderhair.Value;
		}
		set
		{
			_updateCvsUnderhair.Value = value;
		}
	}

	public bool updateCvsNail
	{
		get
		{
			return _updateCvsNail.Value;
		}
		set
		{
			_updateCvsNail.Value = value;
		}
	}

	public bool updateCvsBodyPaint
	{
		get
		{
			return _updateCvsBodyPaint.Value;
		}
		set
		{
			_updateCvsBodyPaint.Value = value;
		}
	}

	public bool updateCvsFutanari
	{
		get
		{
			return _updateCvsFutanari.Value;
		}
		set
		{
			_updateCvsFutanari.Value = value;
		}
	}

	public bool updateCvsHair
	{
		get
		{
			return _updateCvsHair.Value;
		}
		set
		{
			_updateCvsHair.Value = value;
		}
	}

	public bool updateCvsClothes
	{
		get
		{
			return _updateCvsClothes.Value;
		}
		set
		{
			_updateCvsClothes.Value = value;
		}
	}

	public bool updateCvsClothesSaveDelete
	{
		get
		{
			return _updateCvsClothesSaveDelete.Value;
		}
		set
		{
			_updateCvsClothesSaveDelete.Value = value;
		}
	}

	public bool updateCvsClothesLoad
	{
		get
		{
			return _updateCvsClothesLoad.Value;
		}
		set
		{
			_updateCvsClothesLoad.Value = value;
		}
	}

	public bool updateCvsAccessory
	{
		get
		{
			return _updateCvsAccessory.Value;
		}
		set
		{
			_updateCvsAccessory.Value = value;
		}
	}

	public bool updateCvsAcsCopy
	{
		get
		{
			return _updateCvsAcsCopy.Value;
		}
		set
		{
			_updateCvsAcsCopy.Value = value;
		}
	}

	public bool updateCvsChara
	{
		get
		{
			return _updateCvsChara.Value;
		}
		set
		{
			_updateCvsChara.Value = value;
		}
	}

	public bool updateCvsType
	{
		get
		{
			return _updateCvsType.Value;
		}
		set
		{
			_updateCvsType.Value = value;
		}
	}

	public bool updateCvsStatus
	{
		get
		{
			return _updateCvsStatus.Value;
		}
		set
		{
			_updateCvsStatus.Value = value;
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

	public bool updateCvsFusion
	{
		get
		{
			return _updateCvsFusion.Value;
		}
		set
		{
			_updateCvsFusion.Value = value;
		}
	}

	public event Action actUpdateCvsFaceType;

	public event Action actUpdateCvsFaceShapeWhole;

	public event Action actUpdateCvsFaceShapeChin;

	public event Action actUpdateCvsFaceShapeCheek;

	public event Action actUpdateCvsFaceShapeEyebrow;

	public event Action actUpdateCvsFaceShapeEyes;

	public event Action actUpdateCvsFaceShapeNose;

	public event Action actUpdateCvsFaceShapeMouth;

	public event Action actUpdateCvsFaceShapeEar;

	public event Action actUpdateCvsMole;

	public event Action actUpdateCvsEyeLR;

	public event Action actUpdateCvsEyeEtc;

	public event Action actUpdateCvsEyeHL;

	public event Action actUpdateCvsEyebrow;

	public event Action actUpdateCvsEyelashes;

	public event Action actUpdateCvsEyeshadow;

	public event Action actUpdateCvsCheek;

	public event Action actUpdateCvsLip;

	public event Action actUpdateCvsFacePaint;

	public event Action actUpdateCvsBeard;

	public event Action actUpdateCvsBodyShapeWhole;

	public event Action actUpdateCvsBodyShapeBreast;

	public event Action actUpdateCvsBodyShapeUpper;

	public event Action actUpdateCvsBodyShapeLower;

	public event Action actUpdateCvsBodyShapeArm;

	public event Action actUpdateCvsBodyShapeLeg;

	public event Action actUpdateCvsBodySkinType;

	public event Action actUpdateCvsSunburn;

	public event Action actUpdateCvsNip;

	public event Action actUpdateCvsUnderhair;

	public event Action actUpdateCvsNail;

	public event Action actUpdateCvsBodyPaint;

	public event Action actUpdateCvsFutanari;

	public event Action actUpdateCvsHair;

	public event Action actUpdateCvsClothes;

	public event Action actUpdateCvsClothesSaveDelete;

	public event Action actUpdateCvsClothesLoad;

	public event Action actUpdateCvsAccessory;

	public event Action actUpdateCvsAcsCopy;

	public event Action actUpdateCvsChara;

	public event Action actUpdateCvsType;

	public event Action actUpdateCvsStatus;

	public event Action actUpdateCvsCharaSaveDelete;

	public event Action actUpdateCvsCharaLoad;

	public event Action actUpdateCvsFusion;

	public void ChangeCharaData()
	{
		RestrictSubMenu();
		ChangeAcsSlotName();
	}

	public void RestrictSubMenu()
	{
		if (!(null == chaCtrl) && chaCtrl.cmpClothes != null)
		{
			bool interactable = true;
			bool interactable2 = true;
			ListInfoBase listInfoBase = chaCtrl.infoClothes[0];
			if (listInfoBase != null)
			{
				interactable = "0" == listInfoBase.GetInfo(ChaListDefine.KeyType.Coordinate);
			}
			listInfoBase = chaCtrl.infoClothes[2];
			if (listInfoBase != null)
			{
				interactable2 = "0" == listInfoBase.GetInfo(ChaListDefine.KeyType.Coordinate);
			}
			if ((bool)subMenuBot)
			{
				subMenuBot.interactable = interactable;
			}
			if ((bool)subMenuInnerDown)
			{
				subMenuInnerDown.interactable = interactable2;
			}
		}
	}

	public void ChangeAcsSlotName(int slotNo = -1)
	{
		for (int i = 0; i < acsSlotText.Length; i++)
		{
			if ((-1 == slotNo || i == slotNo) && !(null == acsSlotText[i]))
			{
				int type = chaCtrl.nowCoordinate.accessory.parts[i].type;
				if (350 == type)
				{
					acsSlotText[i].text = (i + 1).ToString("00");
					continue;
				}
				ListInfoBase listInfo = chaCtrl.lstCtrl.GetListInfo((ChaListDefine.CategoryNo)type, chaCtrl.nowCoordinate.accessory.parts[i].id);
				acsSlotText[i].text = $"{i + 1:00} {listInfo.Name}";
			}
		}
	}

	public void ChangeAcsSlotColor(int slotNo)
	{
		for (int i = 0; i < acsSlotText.Length; i++)
		{
			if (!(null == acsSlotText[i]))
			{
				if (i != slotNo)
				{
					acsSlotText[i].color = new Color32(235, 226, 215, byte.MaxValue);
				}
				else
				{
					acsSlotText[i].color = new Color32(204, 197, 59, byte.MaxValue);
				}
			}
		}
	}

	public void ChangeClothesStateAuto(int stateNo)
	{
		autoClothesStateNo = (byte)stateNo;
		if (autoClothesState)
		{
			ChangeClothesState(0);
		}
	}

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

	public static string ConvertTextFromRate(int min, int max, float value)
	{
		return Mathf.RoundToInt(Mathf.Lerp(min, max, value)).ToString();
	}

	public static float ConvertRateFromText(int min, int max, string buf)
	{
		if (buf.IsNullOrEmpty())
		{
			return 0f;
		}
		if (!int.TryParse(buf, out var result))
		{
			return 0f;
		}
		return Mathf.InverseLerp(min, max, result);
	}

	public static float ConvertValueFromTextLimit(float min, float max, int digit, string buf)
	{
		if (buf.IsNullOrEmpty())
		{
			return 0f;
		}
		if (!MathfEx.RangeEqualOn(0, digit, 4))
		{
			return 0f;
		}
		float result = 0f;
		float.TryParse(buf, out result);
		string[] array = new string[5] { "f0", "f1", "f2", "f3", "f4" };
		result = float.Parse(result.ToString(array[digit]));
		return Mathf.Clamp(result, min, max);
	}

	public void SetUpdateToggleSetting()
	{
		if ((bool)tglEyesSameSetting)
		{
			tglEyesSameSetting.SetIsOnWithoutCallback(chaCtrl.fileFace.pupilSameSetting);
		}
		if ((bool)tglHairSameSetting)
		{
			tglHairSameSetting.SetIsOnWithoutCallback(chaCtrl.fileHair.sameSetting);
		}
		if ((bool)tglHairAutoSetting)
		{
			tglHairAutoSetting.SetIsOnWithoutCallback(chaCtrl.fileHair.autoSetting);
		}
		autoHairColor = chaCtrl.fileHair.autoSetting;
		if ((bool)tglHairControlTogether)
		{
			tglHairControlTogether.SetIsOnWithoutCallback(chaCtrl.fileHair.ctrlTogether);
		}
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
		tglSliderWheel.SetIsOnWithoutCallback(customSettingSave.sliderWheel);
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
		SetUpdateToggleSetting();
		if ((bool)tglEyesSameSetting)
		{
			tglEyesSameSetting.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				if ((bool)chaCtrl)
				{
					chaCtrl.fileFace.pupilSameSetting = isOn;
				}
			});
		}
		if ((bool)tglHairSameSetting)
		{
			tglHairSameSetting.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				if ((bool)chaCtrl)
				{
					chaCtrl.fileHair.sameSetting = isOn;
				}
			});
		}
		if ((bool)tglHairAutoSetting)
		{
			tglHairAutoSetting.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				if ((bool)chaCtrl)
				{
					chaCtrl.fileHair.autoSetting = isOn;
				}
				autoHairColor = isOn;
			});
		}
		if ((bool)tglHairControlTogether)
		{
			tglHairControlTogether.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				if ((bool)chaCtrl)
				{
					chaCtrl.fileHair.ctrlTogether = isOn;
				}
			});
		}
		_changeCharaName.Where((bool f) => f).Subscribe(delegate
		{
			customCtrl.UpdateCharaNameText();
			changeCharaName = false;
		});
		_updateCvsFaceType.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceType?.Invoke();
			updateCvsFaceType = false;
		});
		_updateCvsFaceShapeWhole.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeWhole?.Invoke();
			updateCvsFaceShapeWhole = false;
		});
		_updateCvsFaceShapeChin.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeChin?.Invoke();
			updateCvsFaceShapeChin = false;
		});
		_updateCvsFaceShapeCheek.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeCheek?.Invoke();
			updateCvsFaceShapeCheek = false;
		});
		_updateCvsFaceShapeEyebrow.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeEyebrow?.Invoke();
			updateCvsFaceShapeEyebrow = false;
		});
		_updateCvsFaceShapeEyes.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeEyes?.Invoke();
			updateCvsFaceShapeEyes = false;
		});
		_updateCvsFaceShapeNose.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeNose?.Invoke();
			updateCvsFaceShapeNose = false;
		});
		_updateCvsFaceShapeMouth.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeMouth?.Invoke();
			updateCvsFaceShapeMouth = false;
		});
		_updateCvsFaceShapeEar.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFaceShapeEar?.Invoke();
			updateCvsFaceShapeEar = false;
		});
		_updateCvsMole.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsMole?.Invoke();
			updateCvsMole = false;
		});
		_updateCvsEyeLR.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyeLR?.Invoke();
			updateCvsEyeLR = false;
		});
		_updateCvsEyeEtc.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyeEtc?.Invoke();
			updateCvsEyeEtc = false;
		});
		_updateCvsEyeHL.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyeHL?.Invoke();
			updateCvsEyeHL = false;
		});
		_updateCvsEyebrow.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyebrow?.Invoke();
			updateCvsEyebrow = false;
		});
		_updateCvsEyelashes.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyelashes?.Invoke();
			updateCvsEyelashes = false;
		});
		_updateCvsEyeshadow.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsEyeshadow?.Invoke();
			updateCvsEyeshadow = false;
		});
		_updateCvsCheek.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsCheek?.Invoke();
			updateCvsCheek = false;
		});
		_updateCvsLip.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsLip?.Invoke();
			updateCvsLip = false;
		});
		_updateCvsFacePaint.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFacePaint?.Invoke();
			updateCvsFacePaint = false;
		});
		_updateCvsBeard.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBeard?.Invoke();
			updateCvsBeard = false;
		});
		_updateCvsBodyShapeWhole.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeWhole?.Invoke();
			updateCvsBodyShapeWhole = false;
		});
		_updateCvsBodyShapeBreast.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeBreast?.Invoke();
			updateCvsBodyShapeBreast = false;
		});
		_updateCvsBodyShapeUpper.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeUpper?.Invoke();
			updateCvsBodyShapeUpper = false;
		});
		_updateCvsBodyShapeLower.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeLower?.Invoke();
			updateCvsBodyShapeLower = false;
		});
		_updateCvsBodyShapeArm.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeArm?.Invoke();
			updateCvsBodyShapeArm = false;
		});
		_updateCvsBodyShapeLeg.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyShapeLeg?.Invoke();
			updateCvsBodyShapeLeg = false;
		});
		_updateCvsBodySkinType.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodySkinType?.Invoke();
			updateCvsBodySkinType = false;
		});
		_updateCvsSunburn.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsSunburn?.Invoke();
			updateCvsSunburn = false;
		});
		_updateCvsNip.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsNip?.Invoke();
			updateCvsNip = false;
		});
		_updateCvsUnderhair.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsUnderhair?.Invoke();
			updateCvsUnderhair = false;
		});
		_updateCvsNail.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsNail?.Invoke();
			updateCvsNail = false;
		});
		_updateCvsBodyPaint.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsBodyPaint?.Invoke();
			updateCvsBodyPaint = false;
		});
		_updateCvsFutanari.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsFutanari?.Invoke();
			updateCvsFutanari = false;
		});
		_updateCvsHair.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsHair?.Invoke();
			updateCvsHair = false;
		});
		_updateCvsClothes.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsClothes?.Invoke();
			updateCvsClothes = false;
		});
		_updateCvsClothesSaveDelete.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsClothesSaveDelete?.Invoke();
			updateCvsClothesSaveDelete = false;
		});
		_updateCvsClothesLoad.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsClothesLoad?.Invoke();
			updateCvsClothesLoad = false;
		});
		_updateCvsAccessory.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsAccessory?.Invoke();
			updateCvsAccessory = false;
		});
		_updateCvsAcsCopy.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsAcsCopy?.Invoke();
			updateCvsAcsCopy = false;
		});
		_updateCvsChara.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsChara?.Invoke();
			updateCvsChara = false;
		});
		_updateCvsType.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsType?.Invoke();
			updateCvsType = false;
		});
		_updateCvsStatus.Where((bool f) => f).Subscribe(delegate
		{
			this.actUpdateCvsStatus?.Invoke();
			updateCvsStatus = false;
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
		_sliderControlWheel.Subscribe(delegate(bool f)
		{
			if (customCtrl.sliderScrollRaycast != null)
			{
				customCtrl.sliderScrollRaycast.ChangeActiveRaycast(!f);
			}
		});
		tglSliderWheel.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
		{
			customSettingSave.sliderWheel = isOn;
			sliderControlWheel = isOn;
		});
		_centerDraw.Subscribe(delegate(bool f)
		{
			customCtrl.camCtrl.isOutsideTargetTex = f;
		});
		_autoHairColor.Subscribe(delegate(bool f)
		{
			if ((bool)hairUICondition.objTopColorSet)
			{
				hairUICondition.objTopColorSet.SetActiveIfDifferent(!f && drawTopHairColor);
			}
			if ((bool)hairUICondition.objUnderColorSet)
			{
				hairUICondition.objUnderColorSet.SetActiveIfDifferent(!f && drawUnderHairColor);
			}
			if ((bool)hairUICondition.objGlossColorSet)
			{
				hairUICondition.objGlossColorSet.SetActiveIfDifferent(!f);
			}
		});
		_drawTopHairColor.Subscribe(delegate(bool f)
		{
			if ((bool)hairUICondition.objTopColorSet)
			{
				hairUICondition.objTopColorSet.SetActiveIfDifferent(!autoHairColor && f);
			}
		});
		_drawUnderHairColor.Subscribe(delegate(bool f)
		{
			if ((bool)hairUICondition.objUnderColorSet)
			{
				hairUICondition.objUnderColorSet.SetActiveIfDifferent(!autoHairColor && f);
			}
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

	public void Update()
	{
		bool flag = true;
		lstShow.Clear();
		lstShow.Add(showAcsControllerAll);
		lstShow.Add(showAcsController[0]);
		lstShow.Add(customSettingSave.acsCtrlSetting.correctSetting[0].draw);
		flag = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)objAcs01ControllerTop)
		{
			objAcs01ControllerTop.SetActiveIfDifferent(flag);
		}
		lstShow.Clear();
		lstShow.Add(showAcsControllerAll);
		lstShow.Add(showAcsController[1]);
		lstShow.Add(customSettingSave.acsCtrlSetting.correctSetting[1].draw);
		flag = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)objAcs02ControllerTop)
		{
			objAcs02ControllerTop.SetActiveIfDifferent(flag);
		}
		lstShow.Clear();
		lstShow.Add(showHairController);
		lstShow.Add(customSettingSave.hairCtrlSetting.drawController);
		flag = YS_Assist.CheckFlagsList(lstShow);
		if ((bool)objHairControllerTop)
		{
			objHairControllerTop.SetActiveIfDifferent(flag);
		}
	}

	private void OnDestroy()
	{
		customSettingSave.Save();
	}
}
