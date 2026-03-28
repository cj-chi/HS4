using System;
using System.Collections.Generic;
using System.Linq;
using CharaUtils;
using Illusion.Extensions;
using RootMotion.FinalIK;
using UnityEngine;

namespace AIChara;

public class ChaInfo : ChaReference
{
	private GameObject[] _objHair;

	private GameObject[] _objClothes;

	private GameObject[] _objAccessory;

	private Transform[,] _trfAcsMove;

	public Dictionary<int, Transform> dictAccessoryParent;

	private GameObject[] _objExtraAccessory;

	private Transform[,] _trfExtraAcsMove;

	private ListInfoBase[] _infoHair;

	private ListInfoBase[] _infoClothes;

	private ListInfoBase[] _infoAccessory;

	public ChaFileControl chaFile { get; protected set; }

	public ChaFileCustom fileCustom => chaFile.custom;

	public ChaFileBody fileBody => chaFile.custom.body;

	public ChaFileFace fileFace => chaFile.custom.face;

	public ChaFileHair fileHair => chaFile.custom.hair;

	public ChaFileParameter fileParam => chaFile.parameter;

	public ChaFileGameInfo fileGameInfo => chaFile.gameinfo;

	public ChaFileParameter2 fileParam2 => chaFile.parameter2;

	public ChaFileGameInfo2 fileGameInfo2 => chaFile.gameinfo2;

	public ChaFileStatus fileStatus => chaFile.status;

	public ChaListControl lstCtrl { get; protected set; }

	public EyeLookController eyeLookCtrl { get; protected set; }

	public NeckLookControllerVer2 neckLookCtrl { get; protected set; }

	public FaceBlendShape fbsCtrl { get; protected set; }

	public FBSCtrlEyebrow eyebrowCtrl { get; protected set; }

	public FBSCtrlEyes eyesCtrl { get; protected set; }

	public FBSCtrlMouth mouthCtrl { get; protected set; }

	public Expression expression { get; protected set; }

	public CmpBoneHead cmpBoneHead { get; protected set; }

	public CmpBoneBody cmpBoneBody { get; protected set; }

	public CmpFace cmpFace { get; protected set; }

	public CmpBody cmpBody { get; protected set; }

	public CmpBody cmpSimpleBody { get; protected set; }

	public CmpHair[] cmpHair { get; protected set; }

	public CmpClothes[] cmpClothes { get; protected set; }

	public CmpAccessory[] cmpAccessory { get; protected set; }

	public CmpAccessory[] cmpExtraAccessory { get; protected set; }

	public FullBodyBipedIK fullBodyIK { get; protected set; }

	public int chaID { get; protected set; }

	public int loadNo { get; protected set; }

	public byte sex => chaFile.parameter.sex;

	public bool isPlayer { get; set; }

	public bool hideMoz { get; set; }

	public bool loadEnd { get; protected set; }

	public bool visibleAll { get; set; }

	public bool visibleBody
	{
		get
		{
			return fileStatus.visibleBodyAlways;
		}
		set
		{
			fileStatus.visibleBodyAlways = value;
		}
	}

	public bool visibleSon
	{
		get
		{
			if (sex == 0)
			{
				return fileStatus.visibleSon;
			}
			if (!fileParam.futanari)
			{
				return false;
			}
			return fileStatus.visibleSonAlways;
		}
		set
		{
			if (sex == 0)
			{
				fileStatus.visibleSonAlways = true;
				fileStatus.visibleSon = value;
			}
			else
			{
				fileStatus.visibleSonAlways = fileParam.futanari && value;
				fileStatus.visibleSon = true;
			}
		}
	}

	public bool updateShapeFace { get; set; }

	public bool updateShapeBody { get; set; }

	public bool updateShape
	{
		set
		{
			updateShapeFace = value;
			updateShapeBody = value;
		}
	}

	public bool updateWet { get; set; }

	public bool resetDynamicBoneAll { get; set; }

	public bool reSetupDynamicBoneBust { get; set; }

	protected bool[] enableDynamicBoneBustAndHip { get; set; }

	public bool updateBustSize { get; set; }

	public bool releaseCustomInputTexture { get; set; }

	public bool loadWithDefaultColorAndPtn { get; set; }

	protected bool[] showExtraAccessory { get; set; }

	public bool hideHairForThumbnailCapture { get; set; }

	public Renderer[] rendBra { get; protected set; }

	public Renderer rendInnerTB { get; protected set; }

	public Renderer rendInnerB { get; protected set; }

	public Renderer rendPanst { get; protected set; }

	public CustomTextureControl customTexCtrlFace { get; protected set; }

	public CustomTextureControl customTexCtrlBody { get; protected set; }

	public Material customMatFace
	{
		get
		{
			if (customTexCtrlFace != null)
			{
				return customTexCtrlFace.matDraw;
			}
			return null;
		}
	}

	public Material customMatBody
	{
		get
		{
			if (customTexCtrlBody != null)
			{
				return customTexCtrlBody.matDraw;
			}
			return null;
		}
	}

	public CustomTextureCreate[,] ctCreateClothes { get; protected set; }

	public CustomTextureCreate[,] ctCreateClothesGloss { get; protected set; }

	public GameObject objRoot { get; protected set; }

	public GameObject objTop { get; protected set; }

	public GameObject objAnim { get; protected set; }

	public GameObject objBodyBone { get; protected set; }

	public GameObject objBody { get; protected set; }

	public GameObject objSimpleBody { get; protected set; }

	public GameObject objHeadBone { get; protected set; }

	public GameObject objHead { get; protected set; }

	public GameObject[] objHair
	{
		get
		{
			return _objHair;
		}
		protected set
		{
			_objHair = value;
		}
	}

	public GameObject[] objClothes
	{
		get
		{
			return _objClothes;
		}
		protected set
		{
			_objClothes = value;
		}
	}

	public GameObject[] objAccessory
	{
		get
		{
			return _objAccessory;
		}
		protected set
		{
			_objAccessory = value;
		}
	}

	public Transform[,] trfAcsMove
	{
		get
		{
			return _trfAcsMove;
		}
		protected set
		{
			_trfAcsMove = value;
		}
	}

	public GameObject objHitBody { get; protected set; }

	public GameObject objHitHead { get; protected set; }

	public Animator animBody { get; protected set; }

	public GameObject objEyesLookTargetP { get; protected set; }

	public GameObject objEyesLookTarget { get; protected set; }

	public GameObject objNeckLookTargetP { get; protected set; }

	public GameObject objNeckLookTarget { get; protected set; }

	public GameObject[] objExtraAccessory
	{
		get
		{
			return _objExtraAccessory;
		}
		protected set
		{
			_objExtraAccessory = value;
		}
	}

	public ListInfoBase infoHead { get; protected set; }

	public ListInfoBase[] infoHair
	{
		get
		{
			return _infoHair;
		}
		protected set
		{
			_infoHair = value;
		}
	}

	public ListInfoBase[] infoClothes
	{
		get
		{
			return _infoClothes;
		}
		protected set
		{
			_infoClothes = value;
		}
	}

	public ListInfoBase[] infoAccessory
	{
		get
		{
			return _infoAccessory;
		}
		protected set
		{
			_infoAccessory = value;
		}
	}

	public bool enableExpression
	{
		get
		{
			if (!(null == expression))
			{
				return expression.enable;
			}
			return false;
		}
		set
		{
			if (null != expression)
			{
				expression.enable = value;
			}
		}
	}

	public CmpHair GetCustomHairComponent(int parts)
	{
		if (this.cmpHair == null)
		{
			return null;
		}
		if (parts >= this.cmpHair.Length)
		{
			return null;
		}
		CmpHair cmpHair = this.cmpHair[parts];
		if (null == cmpHair)
		{
			return null;
		}
		return cmpHair;
	}

	public CmpClothes GetCustomClothesComponent(int parts)
	{
		if (this.cmpClothes == null)
		{
			return null;
		}
		if (parts >= this.cmpClothes.Length)
		{
			return null;
		}
		CmpClothes cmpClothes = this.cmpClothes[parts];
		if (null == cmpClothes)
		{
			return null;
		}
		return cmpClothes;
	}

	public CmpAccessory GetAccessoryComponent(int parts)
	{
		if (this.cmpAccessory == null)
		{
			return null;
		}
		if (parts >= this.cmpAccessory.Length)
		{
			return null;
		}
		CmpAccessory cmpAccessory = this.cmpAccessory[parts];
		if (null == cmpAccessory)
		{
			return null;
		}
		return cmpAccessory;
	}

	public CmpAccessory GetExtraAccessoryComponent(int parts)
	{
		if (cmpExtraAccessory == null)
		{
			return null;
		}
		if (parts >= cmpExtraAccessory.Length)
		{
			return null;
		}
		CmpAccessory cmpAccessory = cmpExtraAccessory[parts];
		if (null == cmpAccessory)
		{
			return null;
		}
		return cmpAccessory;
	}

	public void EnableExpressionIndex(int indexNo, bool enable)
	{
		if (null != expression)
		{
			expression.EnableIndex(indexNo, enable);
		}
	}

	public void EnableExpressionCategory(int categoryNo, bool enable)
	{
		if (null != expression)
		{
			expression.EnableCategory(categoryNo, enable);
		}
	}

	public int GetBustSizeKind()
	{
		return fileCustom.GetBustSizeKind();
	}

	public int GetHeightKind()
	{
		return fileCustom.GetHeightKind();
	}

	public int GetHairType()
	{
		return fileHair.kind;
	}

	protected void MemberInitializeAll()
	{
		chaFile = null;
		lstCtrl = null;
		chaID = 0;
		loadNo = -1;
		hideMoz = false;
		releaseCustomInputTexture = true;
		loadWithDefaultColorAndPtn = false;
		hideHairForThumbnailCapture = false;
		objRoot = null;
		customTexCtrlBody = null;
		MemberInitializeObject();
	}

	protected void MemberInitializeObject()
	{
		eyeLookCtrl = null;
		neckLookCtrl = null;
		fbsCtrl = null;
		eyebrowCtrl = null;
		eyesCtrl = null;
		mouthCtrl = null;
		expression = null;
		cmpFace = null;
		cmpBody = null;
		cmpHair = new CmpHair[Enum.GetNames(typeof(ChaFileDefine.HairKind)).Length];
		cmpClothes = new CmpClothes[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length];
		cmpAccessory = new CmpAccessory[20];
		cmpExtraAccessory = new CmpAccessory[Enum.GetNames(typeof(ChaControlDefine.ExtraAccessoryParts)).Length];
		customTexCtrlFace = null;
		ctCreateClothes = new CustomTextureCreate[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length, 3];
		ctCreateClothesGloss = new CustomTextureCreate[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length, 3];
		loadEnd = false;
		visibleAll = true;
		updateShapeFace = false;
		updateShapeBody = false;
		resetDynamicBoneAll = false;
		reSetupDynamicBoneBust = false;
		enableDynamicBoneBustAndHip = new bool[4] { true, true, true, true };
		updateBustSize = false;
		showExtraAccessory = new bool[Enum.GetNames(typeof(ChaControlDefine.ExtraAccessoryParts)).Length];
		for (int i = 0; i < showExtraAccessory.Length; i++)
		{
			showExtraAccessory[i] = false;
		}
		rendBra = new Renderer[2];
		rendInnerTB = null;
		rendInnerB = null;
		rendPanst = null;
		objTop = null;
		objAnim = null;
		objBodyBone = null;
		objBody = null;
		objSimpleBody = null;
		objHeadBone = null;
		objHead = null;
		objHair = new GameObject[Enum.GetNames(typeof(ChaFileDefine.HairKind)).Length];
		objClothes = new GameObject[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length];
		objAccessory = new GameObject[20];
		trfAcsMove = new Transform[20, 2];
		objHitHead = null;
		objHitBody = null;
		animBody = null;
		objEyesLookTargetP = null;
		objEyesLookTarget = null;
		objNeckLookTargetP = null;
		objNeckLookTarget = null;
		dictAccessoryParent = null;
		objExtraAccessory = new GameObject[Enum.GetNames(typeof(ChaControlDefine.ExtraAccessoryParts)).Length];
		infoHead = null;
		infoHair = new ListInfoBase[Enum.GetNames(typeof(ChaFileDefine.HairKind)).Length];
		infoClothes = new ListInfoBase[Enum.GetNames(typeof(ChaFileDefine.ClothesKind)).Length];
		infoAccessory = new ListInfoBase[20];
	}

	protected void ReleaseInfoAll()
	{
		ReleaseInfoObject(init: false);
		if (customTexCtrlBody != null)
		{
			customTexCtrlBody.Release();
		}
		Resources.UnloadUnusedAssets();
	}

	protected void ReleaseInfoObject(bool init = true)
	{
		if (customTexCtrlFace != null)
		{
			customTexCtrlFace.Release();
		}
		if (ctCreateClothes != null)
		{
			for (int i = 0; i < ctCreateClothes.GetLength(0); i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (ctCreateClothes[i, j] != null)
					{
						ctCreateClothes[i, j].Release();
						ctCreateClothes[i, j] = null;
					}
				}
			}
		}
		if (ctCreateClothesGloss != null)
		{
			for (int k = 0; k < ctCreateClothesGloss.GetLength(0); k++)
			{
				for (int l = 0; l < 3; l++)
				{
					if (ctCreateClothesGloss[k, l] != null)
					{
						ctCreateClothesGloss[k, l].Release();
						ctCreateClothesGloss[k, l] = null;
					}
				}
			}
		}
		if (false)
		{
			if (null != objTop)
			{
				objTop.SetActiveIfDifferent(active: false);
				objTop.name = "Delete_Reserve";
				UnityEngine.Object.Destroy(objTop);
			}
		}
		else
		{
			SafeDestroy(objTop);
		}
		objTop = null;
		ReleaseRefAll();
		if (init)
		{
			MemberInitializeObject();
		}
	}

	public void SafeDestroy(GameObject obj)
	{
		if (null != obj)
		{
			obj.SetActiveIfDifferent(active: false);
			obj.transform.SetParent(null);
			obj.name = "Delete_Reserve";
			UnityEngine.Object.Destroy(obj);
		}
	}

	public DynamicBone[] GetDynamicBoneHairAll()
	{
		if (cmpHair == null)
		{
			return null;
		}
		List<DynamicBone> list = new List<DynamicBone>();
		for (int i = 0; i < cmpHair.Length; i++)
		{
			if (null == cmpHair[i] || cmpHair[i].boneInfo == null)
			{
				continue;
			}
			foreach (DynamicBone[] item in from x in cmpHair[i].boneInfo
				where x != null && x.dynamicBone != null
				select x.dynamicBone)
			{
				list.AddRange(item);
			}
		}
		return list.ToArray();
	}

	public DynamicBone[] GetDynamicBoneHair(int parts)
	{
		if (cmpHair == null)
		{
			return null;
		}
		if (parts >= cmpHair.Length)
		{
			return null;
		}
		if (cmpHair[parts].boneInfo == null)
		{
			return null;
		}
		List<DynamicBone> list = new List<DynamicBone>();
		foreach (DynamicBone[] item in from x in cmpHair[parts].boneInfo
			where x != null && x.dynamicBone != null
			select x.dynamicBone)
		{
			list.AddRange(item);
		}
		return list.ToArray();
	}

	public void InitializeAccessoryParent()
	{
		dictAccessoryParent = new Dictionary<int, Transform>();
		if (null != cmpBoneHead)
		{
			string[] array = new string[14]
			{
				"N_Hair_pony", "N_Hair_twin_L", "N_Hair_twin_R", "N_Hair_pin_L", "N_Hair_pin_R", "N_Head_top", "N_Head", "N_Hitai", "N_Face", "N_Megane",
				"N_Earring_L", "N_Earring_R", "N_Nose", "N_Mouth"
			};
			Transform[] array2 = new Transform[14]
			{
				cmpBoneHead.targetAccessory.acs_Hair_pony,
				cmpBoneHead.targetAccessory.acs_Hair_twin_L,
				cmpBoneHead.targetAccessory.acs_Hair_twin_R,
				cmpBoneHead.targetAccessory.acs_Hair_pin_L,
				cmpBoneHead.targetAccessory.acs_Hair_pin_R,
				cmpBoneHead.targetAccessory.acs_Head_top,
				cmpBoneHead.targetAccessory.acs_Head,
				cmpBoneHead.targetAccessory.acs_Hitai,
				cmpBoneHead.targetAccessory.acs_Face,
				cmpBoneHead.targetAccessory.acs_Megane,
				cmpBoneHead.targetAccessory.acs_Earring_L,
				cmpBoneHead.targetAccessory.acs_Earring_R,
				cmpBoneHead.targetAccessory.acs_Nose,
				cmpBoneHead.targetAccessory.acs_Mouth
			};
			for (int i = 0; i < array.Length; i++)
			{
				int accessoryParentInt = ChaAccessoryDefine.GetAccessoryParentInt(array[i]);
				dictAccessoryParent[accessoryParentInt] = array2[i];
			}
		}
		if (null != cmpBoneBody)
		{
			string[] array3 = new string[40]
			{
				"N_Neck", "N_Chest_f", "N_Chest", "N_Tikubi_L", "N_Tikubi_R", "N_Back", "N_Back_L", "N_Back_R", "N_Waist", "N_Waist_f",
				"N_Waist_b", "N_Waist_L", "N_Waist_R", "N_Leg_L", "N_Leg_R", "N_Knee_L", "N_Knee_R", "N_Ankle_L", "N_Ankle_R", "N_Foot_L",
				"N_Foot_R", "N_Shoulder_L", "N_Shoulder_R", "N_Elbo_L", "N_Elbo_R", "N_Arm_L", "N_Arm_R", "N_Wrist_L", "N_Wrist_R", "N_Hand_L",
				"N_Hand_R", "N_Index_L", "N_Index_R", "N_Middle_L", "N_Middle_R", "N_Ring_L", "N_Ring_R", "N_Dan", "N_Kokan", "N_Ana"
			};
			Transform[] array4 = new Transform[40]
			{
				cmpBoneBody.targetAccessory.acs_Neck,
				cmpBoneBody.targetAccessory.acs_Chest_f,
				cmpBoneBody.targetAccessory.acs_Chest,
				cmpBoneBody.targetAccessory.acs_Tikubi_L,
				cmpBoneBody.targetAccessory.acs_Tikubi_R,
				cmpBoneBody.targetAccessory.acs_Back,
				cmpBoneBody.targetAccessory.acs_Back_L,
				cmpBoneBody.targetAccessory.acs_Back_R,
				cmpBoneBody.targetAccessory.acs_Waist,
				cmpBoneBody.targetAccessory.acs_Waist_f,
				cmpBoneBody.targetAccessory.acs_Waist_b,
				cmpBoneBody.targetAccessory.acs_Waist_L,
				cmpBoneBody.targetAccessory.acs_Waist_R,
				cmpBoneBody.targetAccessory.acs_Leg_L,
				cmpBoneBody.targetAccessory.acs_Leg_R,
				cmpBoneBody.targetAccessory.acs_Knee_L,
				cmpBoneBody.targetAccessory.acs_Knee_R,
				cmpBoneBody.targetAccessory.acs_Ankle_L,
				cmpBoneBody.targetAccessory.acs_Ankle_R,
				cmpBoneBody.targetAccessory.acs_Foot_L,
				cmpBoneBody.targetAccessory.acs_Foot_R,
				cmpBoneBody.targetAccessory.acs_Shoulder_L,
				cmpBoneBody.targetAccessory.acs_Shoulder_R,
				cmpBoneBody.targetAccessory.acs_Elbo_L,
				cmpBoneBody.targetAccessory.acs_Elbo_R,
				cmpBoneBody.targetAccessory.acs_Arm_L,
				cmpBoneBody.targetAccessory.acs_Arm_R,
				cmpBoneBody.targetAccessory.acs_Wrist_L,
				cmpBoneBody.targetAccessory.acs_Wrist_R,
				cmpBoneBody.targetAccessory.acs_Hand_L,
				cmpBoneBody.targetAccessory.acs_Hand_R,
				cmpBoneBody.targetAccessory.acs_Index_L,
				cmpBoneBody.targetAccessory.acs_Index_R,
				cmpBoneBody.targetAccessory.acs_Middle_L,
				cmpBoneBody.targetAccessory.acs_Middle_R,
				cmpBoneBody.targetAccessory.acs_Ring_L,
				cmpBoneBody.targetAccessory.acs_Ring_R,
				cmpBoneBody.targetAccessory.acs_Dan,
				cmpBoneBody.targetAccessory.acs_Kokan,
				cmpBoneBody.targetAccessory.acs_Ana
			};
			for (int j = 0; j < array3.Length; j++)
			{
				int accessoryParentInt2 = ChaAccessoryDefine.GetAccessoryParentInt(array3[j]);
				dictAccessoryParent[accessoryParentInt2] = array4[j];
			}
		}
	}

	public Transform GetAccessoryParentTransform(string key)
	{
		int accessoryParentInt = ChaAccessoryDefine.GetAccessoryParentInt(key);
		if (dictAccessoryParent.TryGetValue(accessoryParentInt, out var value))
		{
			return value;
		}
		return null;
	}

	public Transform GetAccessoryParentTransform(int index)
	{
		if (dictAccessoryParent.TryGetValue(index, out var value))
		{
			return value;
		}
		return null;
	}
}
