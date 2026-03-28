using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIChara;
using AIProject;
using Illusion.CustomAttributes;
using Obi;
using UnityEngine;

namespace Manager;

public class HSceneManager : Singleton<HSceneManager>
{
	public class HSceneTables
	{
		public class HAssetBundle
		{
			public string path;

			public string manifest;

			public HAssetBundle(string _path, string _manifest = "")
			{
				path = _path;
				if (_manifest != "")
				{
					manifest = _manifest;
				}
				else
				{
					manifest = AssetBundleManager.MAIN_MANIFEST_NAME;
				}
			}
		}

		private ExcelData excelData;

		private List<string> row = new List<string>();

		private List<string> pathList = new List<string>();

		private GameObject commonSpace;

		public List<HScene.AnimationListInfo>[] lstAnimInfo = new List<HScene.AnimationListInfo>[7];

		public Dictionary<int, List<string>> lstHitObject = new Dictionary<int, List<string>>();

		public Dictionary<string, List<string>> HitObjAtariName = new Dictionary<string, List<string>>();

		public Dictionary<string, List<HitObjectCtrl.CollisionInfo>> DicLstHitObjInfo = new Dictionary<string, List<HitObjectCtrl.CollisionInfo>>();

		public Dictionary<int, Dictionary<int, Dictionary<string, GameObject>>> DicHitObject = new Dictionary<int, Dictionary<int, Dictionary<string, GameObject>>>();

		public List<Dictionary<int, List<HItemCtrl.ListItem>>>[] lstHItemObjInfo = new List<Dictionary<int, List<HItemCtrl.ListItem>>>[7];

		public List<(string, RuntimeAnimatorController)> lstHItemBase = new List<(string, RuntimeAnimatorController)>();

		private RuntimeAnimatorController tmpHItemRuntimeAnimator;

		private List<string> HitemPathList = new List<string>();

		public Dictionary<int, HPoint.HpointData> loadHPointDatas = new Dictionary<int, HPoint.HpointData>();

		public List<string> HAutoPathList;

		public HAutoCtrl.HAutoInfo HAutoInfo;

		public HAutoCtrl.AutoLeaveItToYou HAutoLeaveItToYou;

		public Dictionary<int, float> autoLeavePersonalityRate = new Dictionary<int, float>();

		public Dictionary<int, float> autoLeaveAttributeRate = new Dictionary<int, float>();

		public Masturbation.MasturbationTimeInfo MBinfo = new Masturbation.MasturbationTimeInfo();

		public string[,] aHsceneBGM;

		public HParticleCtrl hParticle;

		public BaseAnimInfo[,] HBaseRuntimeAnimatorControllers = new BaseAnimInfo[2, 3];

		public Dictionary<int, Dictionary<int, List<YureCtrl.Info>>> DicDicYure = new Dictionary<int, Dictionary<int, List<YureCtrl.Info>>>();

		public Dictionary<int, Dictionary<int, List<YureCtrlMale.Info>>> DicDicYureMale = new Dictionary<int, Dictionary<int, List<YureCtrlMale.Info>>>();

		public Dictionary<int, List<FeelHit.FeelInfo>[]> DicLstHitInfo = new Dictionary<int, List<FeelHit.FeelInfo>[]>();

		public Dictionary<string, List<H_Lookat_dan.MotionLookAtList>> DicLstLookAtDan = new Dictionary<string, List<H_Lookat_dan.MotionLookAtList>>();

		private List<CollisionCtrl.CollisionInfo> LoadCollisionList;

		public Dictionary<string, List<CollisionCtrl.CollisionInfo>> DicLstCollisionInfo = new Dictionary<string, List<CollisionCtrl.CollisionInfo>>();

		public Dictionary<string, Dictionary<string, HLayerCtrl.HLayerInfo>> LayerInfos = new Dictionary<string, Dictionary<string, HLayerCtrl.HLayerInfo>>();

		public bool endHLoad;

		public bool endHLoadObj;

		private StringBuilder sbAssetName = new StringBuilder();

		private StringBuilder sbAbName = new StringBuilder();

		private readonly string[] assetNames = new string[7] { "aibu", "houshi", "sonyu", "tokushu", "les", "3P_F2M1", "3P" };

		private readonly string[,] strAssetAnimatorBase = new string[2, 3]
		{
			{ "animator/h/male/01/aibu.unity3d", "animator/h/male/01/houshi.unity3d", "animator/h/male/01/sonyu.unity3d" },
			{ "animator/h/female/01/aibu.unity3d", "animator/h/female/01/houshi.unity3d", "animator/h/female/01/sonyu.unity3d" }
		};

		private readonly string[,] racBaseNames = new string[2, 3]
		{
			{ "aia_m_base", "aih_m_base", "ais_m_base" },
			{ "aia_f_base", "aih_f_base", "ais_f_base" }
		};

		private const int AnimInfoPromIDdef = 37;

		private const int AnimInfoPromIDMlt = 44;

		public HashSet<HAssetBundle>[] hashUseAssetBundle = new HashSet<HAssetBundle>[2]
		{
			new HashSet<HAssetBundle>(),
			new HashSet<HAssetBundle>()
		};

		public Dictionary<int, StartAnimInfo> StartBase { get; set; } = new Dictionary<int, StartAnimInfo>();

		public Dictionary<int, StartAnimPatternInfo[]> StartPattern { get; set; } = new Dictionary<int, StartAnimPatternInfo[]>();

		public List<int[]> AnimEventJudgePtn { get; set; } = new List<int[]>();

		public List<HParticleCtrl.ParticleInfo> lstHParticleCtrl { get; private set; } = new List<HParticleCtrl.ParticleInfo>();

		public List<HParticleCtrl.ParticleSetInfo> lstHParticleSetInfo { get; private set; } = new List<HParticleCtrl.ParticleSetInfo>();

		public List<HParticleCtrl.ParticleInfoAI> lstHParticleAICtrl { get; private set; } = new List<HParticleCtrl.ParticleInfoAI>();

		public Dictionary<int, ScreenEffect.PresetInfo> ScreenEffectPresetInfos { get; private set; } = new Dictionary<int, ScreenEffect.PresetInfo>();

		public Dictionary<int, AssetBundleInfo> ProbeTexInfos { get; private set; } = new Dictionary<int, AssetBundleInfo>();

		public Dictionary<int, AssetBundleInfo> ColorFilterInfos { get; private set; } = new Dictionary<int, AssetBundleInfo>();

		public Dictionary<int, HResultParam.Param> result { get; private set; } = new Dictionary<int, HResultParam.Param>();

		public Dictionary<int, GameParameterInfo.Param> mind { get; private set; } = new Dictionary<int, GameParameterInfo.Param>();

		public Dictionary<int, GameParameterInfo.Param> trait { get; private set; } = new Dictionary<int, GameParameterInfo.Param>();

		public Dictionary<int, HTaiiParam.Param> taii { get; private set; } = new Dictionary<int, HTaiiParam.Param>();

		public Dictionary<int, Dictionary<int, int[]>> resist { get; private set; } = new Dictionary<int, Dictionary<int, int[]>>();

		public Dictionary<string, Dictionary<string, RootmotionOffset.Info>> DicdicRootmotionOffsets { get; private set; } = new Dictionary<string, Dictionary<string, RootmotionOffset.Info>>();

		public IEnumerator LoadH()
		{
			endHLoad = false;
			endHLoadObj = false;
			hashUseAssetBundle[0] = new HashSet<HAssetBundle>();
			commonSpace = GameObject.Find("CommonSpace");
			yield return LoadAnimationFileName();
			LoadStartBaseList();
			LoadStartAnimPattern();
			LoadHitObject();
			LoadHitObjectAdd();
			LoadMasterbationInfo();
			LoadPresetScreenEffect();
			LoadColorFilterInfo();
			LoadProbeTexInfo();
			LoadHParm();
			yield return LoadHParticleList();
			yield return LoadHParticleSetInfo();
			yield return LoadHParticleSetInfoAI();
			yield return LoadHItemObjInfo();
			yield return LoadHYure();
			yield return LoadHYureMale();
			yield return LoadHPointInfo();
			yield return LoadHItemBaseAnim();
			yield return LoadFeelHit();
			yield return LoadDankonList();
			yield return null;
			foreach (HAssetBundle item in hashUseAssetBundle[0])
			{
				AssetBundleManager.UnloadAssetBundle(item.path, isUnloadForceRefCount: false, item.manifest);
			}
			endHLoad = true;
		}

		public void Release()
		{
			lstHitObject.Clear();
			HitObjAtariName.Clear();
			DicLstHitObjInfo.Clear();
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<string, GameObject>>> item in DicHitObject)
			{
				foreach (KeyValuePair<int, Dictionary<string, GameObject>> item2 in item.Value)
				{
					foreach (KeyValuePair<string, GameObject> item3 in item2.Value)
					{
						UnityEngine.Object.Destroy(item3.Value);
					}
				}
			}
			DicHitObject.Clear();
			for (int i = 0; i < lstHItemObjInfo.Length; i++)
			{
				lstHItemObjInfo[i] = null;
			}
			lstHItemBase.Clear();
			HitemPathList.Clear();
			loadHPointDatas.Clear();
			HAutoPathList.Clear();
			HAutoInfo = null;
			HAutoLeaveItToYou = null;
			autoLeavePersonalityRate.Clear();
			autoLeaveAttributeRate.Clear();
			aHsceneBGM = null;
			lstHParticleCtrl.Clear();
			lstHParticleSetInfo.Clear();
			hParticle.EndProc();
			hParticle = null;
			for (int j = 0; j < HBaseRuntimeAnimatorControllers.GetLength(0); j++)
			{
				for (int k = 0; k < HBaseRuntimeAnimatorControllers.GetLength(1); k++)
				{
					HBaseRuntimeAnimatorControllers[j, k].path = null;
					HBaseRuntimeAnimatorControllers[j, k].name = null;
					HBaseRuntimeAnimatorControllers[j, k].rac = null;
				}
			}
			DicDicYure.Clear();
			DicDicYureMale.Clear();
			DicLstHitInfo.Clear();
			DicLstLookAtDan.Clear();
			DicLstCollisionInfo.Clear();
			LayerInfos.Clear();
			endHLoad = false;
		}

		public IEnumerator LoadHObj()
		{
			hashUseAssetBundle[1] = new HashSet<HAssetBundle>();
			yield return LoadHsceneBaseRAC();
			yield return null;
			endHLoadObj = true;
		}

		private IEnumerator LoadAnimationFileName()
		{
			for (int i = 0; i < lstAnimInfo.Length; i++)
			{
				lstAnimInfo[i] = new List<HScene.AnimationListInfo>();
			}
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetAnimationInfoListFolder);
			pathList.Sort();
			sbAbName.Clear();
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[nLoopCnt]);
				for (int nAssetNameLoopCnt = 0; nAssetNameLoopCnt < assetNames.Length; nAssetNameLoopCnt++)
				{
					sbAssetName.Clear();
					sbAssetName.AppendFormat("{0}_{1}", assetNames[nAssetNameLoopCnt], Path.GetFileNameWithoutExtension(pathList[nLoopCnt]));
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						yield return null;
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						yield return null;
						continue;
					}
					int num = 1;
					while (num < excelData.MaxCell)
					{
						row = excelData.list[num++].list;
						string text = "";
						int num2 = -1;
						if (nAssetNameLoopCnt <= 3)
						{
							text = row.GetElement(37);
						}
						else if (nAssetNameLoopCnt > 3)
						{
							text = row.GetElement(44);
						}
						if (text != "")
						{
							int.TryParse(text, out num2);
						}
						int num3 = 0;
						HScene.AnimationListInfo animationListInfo = new HScene.AnimationListInfo();
						animationListInfo.nameAnimation = row.GetElement(num3++);
						if (!int.TryParse(row.GetElement(num3++), out animationListInfo.id))
						{
							continue;
						}
						animationListInfo.assetpathBaseM = row.GetElement(num3++);
						animationListInfo.assetBaseM = row.GetElement(num3++);
						animationListInfo.assetpathMale = row.GetElement(num3++);
						animationListInfo.fileMale = row.GetElement(num3++);
						animationListInfo.isMaleHitObject = row.GetElement(num3++) == "1";
						animationListInfo.fileMotionNeckMale = row.GetElement(num3++);
						animationListInfo.assetpathBaseF = row.GetElement(num3++);
						animationListInfo.assetBaseF = row.GetElement(num3++);
						animationListInfo.assetpathFemale = row.GetElement(num3++);
						animationListInfo.fileFemale = row.GetElement(num3++);
						animationListInfo.isFemaleHitObject = row.GetElement(num3++) == "1";
						animationListInfo.fileMotionNeckFemale = row.GetElement(num3++);
						if (nAssetNameLoopCnt == 4)
						{
							animationListInfo.fileMotionNeckFemalePlayer = row.GetElement(num3++);
						}
						if (num2 == 0)
						{
							animationListInfo.assetpathBaseM2 = row.GetElement(num3++);
							animationListInfo.assetBaseM2 = row.GetElement(num3++);
							animationListInfo.assetpathMale2 = row.GetElement(num3++);
							animationListInfo.fileMale2 = row.GetElement(num3++);
							animationListInfo.isMaleHitObject2 = row.GetElement(num3++) == "1";
							animationListInfo.fileMotionNeckMale2 = row.GetElement(num3++);
						}
						else if (num2 > 0)
						{
							animationListInfo.assetpathBaseF2 = row.GetElement(num3++);
							animationListInfo.assetBaseF2 = row.GetElement(num3++);
							animationListInfo.assetpathFemale2 = row.GetElement(num3++);
							animationListInfo.fileFemale2 = row.GetElement(num3++);
							animationListInfo.isFemaleHitObject2 = row.GetElement(num3++) == "1";
							animationListInfo.fileMotionNeckFemale2 = row.GetElement(num3++);
						}
						int.TryParse(row.GetElement(num3++), out animationListInfo.ActionCtrl.Item1);
						int.TryParse(row.GetElement(num3++), out animationListInfo.ActionCtrl.Item2);
						string element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string s in array)
							{
								int item = -1;
								if (int.TryParse(s, out item))
								{
									animationListInfo.nPositons.Add(item);
								}
							}
						}
						element = row.GetElement(num3++);
						if (!element.IsNullOrEmpty())
						{
							string[] array = element.Split('/');
							foreach (string item2 in array)
							{
								animationListInfo.lstOffset.Add(item2);
							}
						}
						animationListInfo.isNeedItem = row.GetElement(num3++) == "1";
						int.TryParse(row.GetElement(num3++), out animationListInfo.nDownPtn);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFaintnessLimit);
						element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string s2 in array)
							{
								int item3 = -1;
								if (int.TryParse(s2, out item3))
								{
									animationListInfo.nStatePtns.Add(item3);
								}
							}
						}
						element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string s3 in array)
							{
								int item4 = -1;
								if (int.TryParse(s3, out item4))
								{
									animationListInfo.Achievments.Add(item4);
								}
							}
						}
						int.TryParse(row.GetElement(num3++), out animationListInfo.nInitiativeFemale);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nBackInitiativeID);
						element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string text2 in array)
							{
								if (text2 != "")
								{
									animationListInfo.lstSystem.Add(int.Parse(text2));
								}
							}
						}
						int.TryParse(row.GetElement(num3++), out animationListInfo.nMaleSon);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFemaleUpperCloths[0]);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFemaleLowerCloths[0]);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFemaleUpperCloths[1]);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFemaleLowerCloths[1]);
						int.TryParse(row.GetElement(num3++), out animationListInfo.nFeelHit);
						animationListInfo.nameCamera = row.GetElement(num3++);
						animationListInfo.fileSiruPaste = row.GetElement(num3++);
						if (nAssetNameLoopCnt > 4)
						{
							animationListInfo.fileSiruPasteSecond = row.GetElement(num3++);
						}
						animationListInfo.fileSe = row.GetElement(num3++);
						text = row.GetElement(num3++);
						if (text != "")
						{
							int.TryParse(text, out animationListInfo.nShortBreahtPlay);
						}
						element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string text3 in array)
							{
								if (!(text3 == ""))
								{
									animationListInfo.hasVoiceCategory.Add(int.Parse(text3));
								}
							}
						}
						animationListInfo.nPromiscuity = num2;
						num3++;
						animationListInfo.reverseTaii = row.GetElement(num3++) == "1";
						element = row.GetElement(num3++);
						if (element != "")
						{
							string[] array = element.Split(',');
							foreach (string text4 in array)
							{
								if (text4 == "")
								{
									continue;
								}
								int num4 = -1;
								int num5 = -1;
								string[] array2 = text4.Split('/');
								if (array2.Length != 0 && array2[0] != "")
								{
									if (!int.TryParse(array2[0], out num4))
									{
										num4 = -1;
									}
									if (array2.Length == 2 && array2[1] != "" && !int.TryParse(array2[1], out num5))
									{
										num5 = -1;
									}
								}
								animationListInfo.Event.Add(new int[2] { num4, num5 });
								bool flag = false;
								foreach (int[] item5 in AnimEventJudgePtn)
								{
									if (item5[0] == num4 && item5[1] == num5)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									AnimEventJudgePtn.Add(new int[2] { num4, num5 });
								}
							}
						}
						int.TryParse(row.GetElement(num3++), out animationListInfo.ParmID);
						if (row.Count <= num3)
						{
							animationListInfo.ReleaseEvent = -1;
						}
						else
						{
							text = row.GetElement(num3);
							if (text.IsNullOrEmpty() || !int.TryParse(text, out animationListInfo.ReleaseEvent))
							{
								animationListInfo.ReleaseEvent = -1;
							}
						}
						if (nAssetNameLoopCnt < 6)
						{
							int num6 = LoadInfoCheck(lstAnimInfo[nAssetNameLoopCnt], animationListInfo.id);
							if (num6 < 0)
							{
								lstAnimInfo[nAssetNameLoopCnt].Add(animationListInfo);
							}
							else
							{
								lstAnimInfo[nAssetNameLoopCnt][num6] = animationListInfo;
							}
						}
						else
						{
							int num7 = LoadInfoCheck(lstAnimInfo[animationListInfo.ActionCtrl.Item1], animationListInfo.id);
							if (num7 < 0)
							{
								lstAnimInfo[animationListInfo.ActionCtrl.Item1].Add(animationListInfo);
							}
							else
							{
								lstAnimInfo[animationListInfo.ActionCtrl.Item1][num7] = animationListInfo;
							}
						}
					}
					yield return null;
				}
				yield return null;
			}
		}

		private int LoadInfoCheck(List<HScene.AnimationListInfo> list, int CheckID)
		{
			int num = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].id == CheckID)
				{
					num = i;
				}
			}
			return num;
		}

		private void LoadStartBaseList()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetStartAnimationListFolder);
			pathList.Sort();
			StartBase = new Dictionary<int, StartAnimInfo>();
			excelData = null;
			foreach (string path in pathList)
			{
				if (!GameSystem.IsPathAdd50(path) || !GlobalMethod.AssetFileExist(path, "BaseStartAnim"))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(path, "BaseStartAnim");
				hashUseAssetBundle[0].Add(new HAssetBundle(path));
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					if (row.Count == 0)
					{
						continue;
					}
					int key = -1;
					if (!int.TryParse(row[num2++], out key))
					{
						continue;
					}
					num2++;
					if (!StartBase.ContainsKey(key))
					{
						StartBase.Add(key, new StartAnimInfo());
					}
					StartBase[key].AnimIDs[0] = new StartAnimInfo.StartAnimIDs();
					for (int i = 0; i < 3; i++)
					{
						int num3 = i;
						string text = row[num2++];
						if (!(text != ""))
						{
							continue;
						}
						string[] array = text.Split(',');
						StartBase[key].AnimIDs[0].ID[num3] = new List<int>();
						string[] array2 = array;
						foreach (string s in array2)
						{
							int item = -1;
							if (int.TryParse(s, out item))
							{
								StartBase[key].AnimIDs[0].ID[num3].Add(item);
							}
						}
					}
					StartBase[key].AnimIDs[1] = new StartAnimInfo.StartAnimIDs();
					for (int k = 0; k < 3; k++)
					{
						int num4 = k;
						string text = row[num2++];
						if (!(text != ""))
						{
							continue;
						}
						string[] array3 = text.Split(',');
						StartBase[key].AnimIDs[1].ID[num4] = new List<int>();
						string[] array2 = array3;
						foreach (string s2 in array2)
						{
							int item2 = -1;
							if (int.TryParse(s2, out item2))
							{
								StartBase[key].AnimIDs[1].ID[num4].Add(item2);
							}
						}
					}
				}
			}
		}

		private void LoadStartAnimPattern()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetStartAnimationListFolder);
			pathList.Sort();
			StartPattern = new Dictionary<int, StartAnimPatternInfo[]>();
			excelData = null;
			foreach (string path in pathList)
			{
				if (!GameSystem.IsPathAdd50(path) || !GlobalMethod.AssetFileExist(path, "StartAnimPattern"))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(path, "StartAnimPattern");
				hashUseAssetBundle[0].Add(new HAssetBundle(path));
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					if (row.Count == 0)
					{
						continue;
					}
					int key = -1;
					if (!int.TryParse(row[num2++], out key))
					{
						continue;
					}
					num2++;
					int num3 = -1;
					if (!int.TryParse(row[num2++], out num3))
					{
						continue;
					}
					if (!StartPattern.ContainsKey(key))
					{
						StartPattern.Add(key, new StartAnimPatternInfo[num3]);
					}
					else
					{
						StartPattern[key] = new StartAnimPatternInfo[num3];
					}
					for (int i = 0; i < num3; i++)
					{
						int num4 = i;
						string text = row[num2++];
						if (!(text != ""))
						{
							continue;
						}
						string[] array = text.Split('/');
						int num5 = -1;
						if (!int.TryParse(array[0], out num5))
						{
							num5 = -1;
						}
						StartPattern[key][num4].category = num5;
						if (num5 != -1)
						{
							int id = -1;
							if (!int.TryParse(array[1], out id))
							{
								id = -1;
							}
							StartPattern[key][num4].id = id;
						}
					}
				}
			}
		}

		private IEnumerator LoadHItemObjInfo()
		{
			for (int i = 0; i < lstAnimInfo.Length; i++)
			{
				lstHItemObjInfo[i] = new List<Dictionary<int, List<HItemCtrl.ListItem>>>();
			}
			HitemPathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHItemObjInfoListFolder);
			HitemPathList.Sort();
			sbAbName.Clear();
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < HitemPathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(HitemPathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(HitemPathList[nLoopCnt]);
				for (int nAssetNameLoopCnt = 0; nAssetNameLoopCnt < assetNames.Length; nAssetNameLoopCnt++)
				{
					Dictionary<int, List<HItemCtrl.ListItem>> dictionary = new Dictionary<int, List<HItemCtrl.ListItem>>();
					sbAssetName.Clear();
					sbAssetName.AppendFormat("{00}_{01}", assetNames[nAssetNameLoopCnt], Path.GetFileNameWithoutExtension(HitemPathList[nLoopCnt]));
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						yield return null;
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						yield return null;
						continue;
					}
					int num = 1;
					while (num < excelData.MaxCell)
					{
						row = excelData.list[num++].list;
						int num2 = 1;
						int key = 0;
						if (!int.TryParse(row[num2++], out key))
						{
							continue;
						}
						if (!dictionary.ContainsKey(key))
						{
							dictionary.Add(key, new List<HItemCtrl.ListItem>());
						}
						HItemCtrl.ListItem listItem = new HItemCtrl.ListItem();
						if (!int.TryParse(row.GetElement(num2++), out listItem.itemkind))
						{
							listItem.itemkind = -1;
						}
						if (!int.TryParse(row.GetElement(num2++), out listItem.itemID))
						{
							listItem.itemID = -1;
						}
						listItem.nameManifest = row.GetElement(num2++);
						string element = row.GetElement(num2++);
						if (element == null || element == "")
						{
							break;
						}
						listItem.pathAssetObject = element;
						listItem.nameObject = row.GetElement(num2++);
						listItem.pathAssetAnimatorBase = row.GetElement(num2++);
						listItem.nameAnimatorBase = row.GetElement(num2++);
						listItem.pathAssetAnimator = row.GetElement(num2++);
						listItem.nameAnimator = row.GetElement(num2++);
						do
						{
							HItemCtrl.ParentInfo parentInfo = new HItemCtrl.ParentInfo();
							int num3 = num2;
							element = row.GetElement(num3 + 1);
							if (element == "")
							{
								break;
							}
							parentInfo.isParentMode = row.GetElement(num2++) == "0";
							parentInfo.numToWhomParent = int.Parse(element);
							num2++;
							parentInfo.nameParent = row.GetElement(num2++);
							parentInfo.nameSelf = row.GetElement(num2++);
							parentInfo.isParentScale = row.GetElement(num2++) == "1";
							listItem.lstParent.Add(parentInfo);
						}
						while (num2 < row.Count);
						dictionary[key].Add(listItem);
					}
					lstHItemObjInfo[nAssetNameLoopCnt].Add(dictionary);
				}
			}
			yield return null;
		}

		private IEnumerator LoadHItemBaseAnim()
		{
			sbAbName.Clear();
			List<(string, string)> assetNames = new List<(string, string)>();
			List<Dictionary<int, List<HItemCtrl.ListItem>>>[] array = lstHItemObjInfo;
			(string, string) item = default((string, string));
			foreach (List<Dictionary<int, List<HItemCtrl.ListItem>>> list in array)
			{
				foreach (Dictionary<int, List<HItemCtrl.ListItem>> item2 in list)
				{
					foreach (List<HItemCtrl.ListItem> value in item2.Values)
					{
						foreach (HItemCtrl.ListItem item3 in value)
						{
							item.Item1 = item3.pathAssetAnimatorBase;
							item.Item2 = item3.nameAnimatorBase;
							if (!assetNames.Contains(item))
							{
								assetNames.Add(item);
							}
						}
					}
					yield return null;
				}
				yield return null;
			}
			foreach (var item4 in assetNames)
			{
				if (GlobalMethod.AssetFileExist(item4.Item1, item4.Item2))
				{
					tmpHItemRuntimeAnimator = CommonLib.LoadAsset<RuntimeAnimatorController>(item4.Item1, item4.Item2);
					lstHItemBase.Add((item4.Item2, tmpHItemRuntimeAnimator));
					hashUseAssetBundle[0].Add(new HAssetBundle(item4.Item1));
					yield return null;
				}
			}
		}

		private IEnumerator LoadHPointInfo()
		{
			sbAbName.Clear();
			sbAbName.Append(Singleton<HSceneManager>.Instance.strAssetHpointListFolder);
			sbAssetName.Clear();
			pathList = CommonLib.GetAssetBundleNameListFromPath(sbAbName.ToString());
			pathList.Sort();
			excelData = null;
			int i = 0;
			while (i < pathList.Count)
			{
				if (GameSystem.IsPathAdd50(pathList[i]))
				{
					sbAbName.Clear();
					sbAbName.Append(pathList[i]);
					sbAssetName.Clear();
					sbAssetName.Append(Path.GetFileNameWithoutExtension(pathList[i]));
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						yield return null;
					}
					else
					{
						excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
						if (excelData == null)
						{
							yield return null;
						}
						else
						{
							LoadHpointList(excelData);
							AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
							yield return null;
						}
					}
				}
				int num = i + 1;
				i = num;
			}
		}

		private void LoadHpointList(ExcelData excelData)
		{
			List<int>[] array = new List<int>[6];
			int num = 1;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				int key = -1;
				if (row[num2].IsNullOrEmpty() || !int.TryParse(row[num2++], out key))
				{
					continue;
				}
				if (!loadHPointDatas.ContainsKey(key))
				{
					loadHPointDatas.Add(key, new HPoint.HpointData());
				}
				for (int i = 0; i < 6; i++)
				{
					int num3 = i;
					array[num3] = new List<int>();
					if (num2 >= row.Count)
					{
						continue;
					}
					string[] array2 = row[num2++].Split(',');
					for (int j = 0; j < array2.Length; j++)
					{
						int item = -1;
						if (int.TryParse(array2[j], out item))
						{
							array[num3].Add(item);
						}
					}
				}
				for (int k = 0; k < loadHPointDatas[key].notMotion.Length; k++)
				{
					loadHPointDatas[key].notMotion[k].motionID = new List<int>(array[k]);
				}
			}
		}

		public void HPointInitData(HPointList hPointList, GameObject mapObj)
		{
			if (hPointList == null)
			{
				return;
			}
			foreach (KeyValuePair<int, List<HPoint>> item in hPointList.lst)
			{
				foreach (HPoint item2 in item.Value)
				{
					item2.Init(mapObj);
				}
			}
			HPoint.CheckShowerMasturbation();
		}

		private IEnumerator LoadHParticleList()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHParticleListFolder);
			pathList.Sort();
			sbAbName.Clear();
			excelData = null;
			sbAssetName.Clear();
			sbAssetName.Append("HParticleList");
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[nLoopCnt]);
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					yield return null;
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
				if (excelData == null)
				{
					yield return null;
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int index = 1;
					HParticleCtrl.ParticleInfo particleInfo = new HParticleCtrl.ParticleInfo();
					particleInfo.assetPath = row.GetElement(index++);
					particleInfo.file = row.GetElement(index++);
					particleInfo.manifest = row.GetElement(index);
					lstHParticleCtrl.Add(particleInfo);
				}
			}
		}

		private IEnumerator LoadHParticleSetInfo()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHParticleListFolder);
			pathList.Sort();
			sbAbName.Clear();
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[nLoopCnt]);
				sbAssetName.Clear();
				sbAssetName.Append(Path.GetFileNameWithoutExtension(pathList[nLoopCnt]));
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					yield return null;
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
				if (excelData == null)
				{
					yield return null;
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 1;
					HParticleCtrl.ParticleSetInfo particleSetInfo = new HParticleCtrl.ParticleSetInfo();
					int numParent = 0;
					if (!int.TryParse(row.GetElement(num2++), out numParent))
					{
						continue;
					}
					particleSetInfo.numParent = numParent;
					particleSetInfo.nameParent = row.GetElement(num2++);
					particleSetInfo.pos = new Vector3(float.Parse(row.GetElement(num2++)), float.Parse(row.GetElement(num2++)), float.Parse(row.GetElement(num2++)));
					particleSetInfo.rot = new Vector3(float.Parse(row.GetElement(num2++)), float.Parse(row.GetElement(num2++)), float.Parse(row.GetElement(num2++)));
					particleSetInfo.timings = new List<HParticleCtrl.Timing>();
					while (row.Count > num2)
					{
						HParticleCtrl.Timing timing = new HParticleCtrl.Timing();
						if (float.TryParse(row[num2++], out timing.playStartTime) && int.TryParse(row[num2++], out timing.playID))
						{
							particleSetInfo.timings.Add(timing);
						}
					}
					lstHParticleSetInfo.Add(particleSetInfo);
				}
			}
		}

		private IEnumerator LoadHParticleSetInfoAI()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHParticleListFolder);
			pathList.Sort();
			sbAbName.Clear();
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[nLoopCnt]);
				sbAssetName.Clear();
				sbAssetName.Append(Path.GetFileNameWithoutExtension(pathList[nLoopCnt]));
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					yield return null;
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
				if (excelData == null)
				{
					yield return null;
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int index = 1;
					HParticleCtrl.ParticleInfoAI particleInfoAI = new HParticleCtrl.ParticleInfoAI();
					string element = row.GetElement(index++);
					int num2 = 0;
					if (!int.TryParse(element, out num2))
					{
						particleInfoAI.assetPath = element;
						particleInfoAI.file = row.GetElement(index++);
						particleInfoAI.manifest = row.GetElement(index++);
						particleInfoAI.numParent = int.Parse(row.GetElement(index++));
						particleInfoAI.nameParent = row.GetElement(index++);
						particleInfoAI.pos = new Vector3(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
						particleInfoAI.rot = new Vector3(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index)));
						lstHParticleAICtrl.Add(particleInfoAI);
					}
				}
			}
		}

		private IEnumerator LoadHsceneBaseRAC()
		{
			for (int nSexLoopCnt = 0; nSexLoopCnt < HBaseRuntimeAnimatorControllers.GetLength(0); nSexLoopCnt++)
			{
				for (int nCategoryLoopCnt = 0; nCategoryLoopCnt < HBaseRuntimeAnimatorControllers.GetLength(1); nCategoryLoopCnt++)
				{
					if (!GlobalMethod.AssetFileExist(strAssetAnimatorBase[nSexLoopCnt, nCategoryLoopCnt], racBaseNames[nSexLoopCnt, nCategoryLoopCnt]))
					{
						yield return null;
						continue;
					}
					if (HBaseRuntimeAnimatorControllers[nSexLoopCnt, nCategoryLoopCnt] == null)
					{
						HBaseRuntimeAnimatorControllers[nSexLoopCnt, nCategoryLoopCnt] = new BaseAnimInfo();
					}
					HBaseRuntimeAnimatorControllers[nSexLoopCnt, nCategoryLoopCnt].path = strAssetAnimatorBase[nSexLoopCnt, nCategoryLoopCnt];
					HBaseRuntimeAnimatorControllers[nSexLoopCnt, nCategoryLoopCnt].name = racBaseNames[nSexLoopCnt, nCategoryLoopCnt];
					HBaseRuntimeAnimatorControllers[nSexLoopCnt, nCategoryLoopCnt].rac = CommonLib.LoadAsset<RuntimeAnimatorController>(strAssetAnimatorBase[nSexLoopCnt, nCategoryLoopCnt], racBaseNames[nSexLoopCnt, nCategoryLoopCnt]);
					yield return null;
					AssetBundleManager.UnloadAssetBundle(strAssetAnimatorBase[nSexLoopCnt, nCategoryLoopCnt], isUnloadForceRefCount: false);
				}
			}
		}

		private IEnumerator LoadHAutoInfo()
		{
			sbAssetName.Clear();
			sbAssetName.Append("hAuto");
			HAutoPathList = new List<string>();
			HAutoPathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHAutoListFolder);
			HAutoPathList = HAutoPathList.Where((string l) => GameSystem.IsPathAdd50(l)).ToList();
			if (HAutoPathList == null || HAutoPathList.Count == 0)
			{
				yield break;
			}
			HAutoPathList.Sort();
			excelData = null;
			if (!GlobalMethod.AssetFileExist(HAutoPathList[HAutoPathList.Count - 1], sbAssetName.ToString()))
			{
				yield break;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(HAutoPathList[HAutoPathList.Count - 1], sbAssetName.ToString());
			hashUseAssetBundle[0].Add(new HAssetBundle(HAutoPathList[HAutoPathList.Count - 1]));
			if (!(excelData == null))
			{
				HAutoInfo = new HAutoCtrl.HAutoInfo();
				int excelRowIdx = 2;
				while (excelRowIdx < excelData.MaxCell)
				{
					row = excelData.list[excelRowIdx++].list;
					int index = 0;
					HAutoInfo.start.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoInfo.start.time = UnityEngine.Random.Range(HAutoInfo.start.minmax.x, HAutoInfo.start.minmax.y);
					HAutoInfo.reStart.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoInfo.reStart.time = UnityEngine.Random.Range(HAutoInfo.reStart.minmax.x, HAutoInfo.reStart.minmax.y);
					HAutoInfo.speed.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoInfo.speed.time = UnityEngine.Random.Range(HAutoInfo.speed.minmax.x, HAutoInfo.speed.minmax.y);
					HAutoInfo.lerpTimeSpeed = float.Parse(row.GetElement(index++));
					HAutoInfo.loopChange.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoInfo.loopChange.time = UnityEngine.Random.Range(HAutoInfo.loopChange.minmax.x, HAutoInfo.loopChange.minmax.y);
					HAutoInfo.motionChange.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoInfo.motionChange.time = UnityEngine.Random.Range(HAutoInfo.motionChange.minmax.x, HAutoInfo.motionChange.minmax.y);
					HAutoInfo.rateWeakLoop = int.Parse(row.GetElement(index++));
					HAutoInfo.rateHit = int.Parse(row.GetElement(index++));
					HAutoInfo.rateAddMotionChange = float.Parse(row.GetElement(index++));
					HAutoInfo.rateRestartMotionChange = int.Parse(row.GetElement(index++));
					float num = float.Parse(row.GetElement(index++));
					HAutoInfo.pull.minmax = new Vector2(num, num);
					HAutoInfo.pull.time = UnityEngine.Random.Range(HAutoInfo.motionChange.minmax.x, HAutoInfo.motionChange.minmax.y);
					HAutoInfo.rateInsertPull = float.Parse(row.GetElement(index++));
					HAutoInfo.rateNotInsertPull = float.Parse(row.GetElement(index));
					yield return null;
				}
			}
		}

		private IEnumerator LoadAutoLeaveItToYou()
		{
			HAutoPathList = new List<string>();
			HAutoPathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetLeaveItToYouFolder);
			HAutoPathList.Sort();
			sbAbName.Clear();
			sbAssetName.Clear();
			sbAssetName.Append("LeaveItToYou");
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < HAutoPathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(HAutoPathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(HAutoPathList[nLoopCnt]);
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					yield return null;
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
				if (excelData == null)
				{
					yield return null;
					continue;
				}
				HAutoLeaveItToYou = new HAutoCtrl.AutoLeaveItToYou();
				int num = 2;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int index = 0;
					HAutoLeaveItToYou.time.minmax = new Vector2(float.Parse(row.GetElement(index++)), float.Parse(row.GetElement(index++)));
					HAutoLeaveItToYou.time.Reset();
					HAutoLeaveItToYou.baseTime = HAutoLeaveItToYou.time.minmax;
					HAutoLeaveItToYou.rate = int.Parse(row.GetElement(index));
				}
				yield return null;
			}
		}

		private IEnumerator LoadAutoLeaveItToYouPersonality()
		{
			excelData = null;
			for (int nLoopCnt = 0; nLoopCnt < HAutoPathList.Count; nLoopCnt++)
			{
				sbAbName.Clear();
				sbAbName.Append(HAutoPathList[nLoopCnt]);
				sbAssetName.Clear();
				sbAssetName.AppendFormat("LeaveItToYou_personality_{00}", Path.GetFileNameWithoutExtension(HAutoPathList[nLoopCnt]));
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(sbAbName.ToString());
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int index = 1;
					int key = 0;
					if (int.TryParse(row.GetElement(index++), out key))
					{
						float value = 1f;
						if (!float.TryParse(row.GetElement(index), out value))
						{
							value = 1f;
						}
						if (!autoLeavePersonalityRate.ContainsKey(key))
						{
							autoLeavePersonalityRate.Add(key, value);
						}
						else
						{
							autoLeavePersonalityRate[key] = value;
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator LoadAutoLeaveItToYouAttribute()
		{
			sbAbName.Clear();
			sbAssetName.Clear();
			sbAssetName.Append("LeaveItToYou_attribute");
			excelData = null;
			for (int i = 0; i < HAutoPathList.Count; i++)
			{
				sbAbName.Clear();
				sbAbName.Append(HAutoPathList[i]);
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(sbAbName.ToString());
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int index = 1;
					if (int.TryParse(row.GetElement(index++), out var key))
					{
						float value = 1f;
						if (!float.TryParse(row.GetElement(index), out value))
						{
							value = 1f;
						}
						if (!autoLeaveAttributeRate.ContainsKey(key))
						{
							autoLeaveAttributeRate.Add(key, value);
						}
						else
						{
							autoLeaveAttributeRate[key] = value;
						}
					}
				}
			}
			yield return null;
		}

		public IEnumerator LoadHYure()
		{
			int[] idxBust = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
			pathList.Clear();
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetYureListFolder);
			pathList.Sort();
			yield return LoadHYureAI(idxBust);
			yield return LoadHYureHoney2(idxBust);
		}

		private IEnumerator LoadHYureAI(int[] idxBust)
		{
			excelData = null;
			for (int i = 0; i < 6; i++)
			{
				int key = i;
				Dictionary<int, List<YureCtrl.Info>> dictionary = new Dictionary<int, List<YureCtrl.Info>>();
				for (int j = 0; j < pathList.Count; j++)
				{
					if (!GameSystem.IsPathAdd50(pathList[j]))
					{
						continue;
					}
					sbAbName.Clear();
					sbAssetName.Clear();
					sbAbName.Append(pathList[j]);
					switch (i)
					{
					case 0:
						sbAssetName.AppendFormat("aia_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 1:
						sbAssetName.AppendFormat("aih_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 2:
						sbAssetName.AppendFormat("ais_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 3:
						sbAssetName.AppendFormat("ait_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 4:
						sbAssetName.AppendFormat("ail_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 5:
						sbAssetName.AppendFormat("ai3p_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					}
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						continue;
					}
					int num = 1;
					while (num < excelData.MaxCell)
					{
						row = excelData.list[num++].list;
						int index = 0;
						int key2 = -1;
						if (int.TryParse(row.GetElement(index++), out key2))
						{
							YureCtrl.Info info = new YureCtrl.Info();
							int nFemale = 0;
							if (i > 3 && !int.TryParse(row.GetElement(index++), out nFemale))
							{
								nFemale = 0;
							}
							info.nFemale = nFemale;
							info.nameAnimation = row.GetElement(index++);
							info.aIsActive[0] = row.GetElement(index++) == "1";
							info.aBreastShape[0].MemberInit();
							for (int k = 0; k < idxBust.Length; k++)
							{
								info.aBreastShape[0].breast[k] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[0].nip = row.GetElement(index++) == "1";
							info.aIsActive[1] = row.GetElement(index++) == "1";
							info.aBreastShape[1].MemberInit();
							for (int l = 0; l < idxBust.Length; l++)
							{
								info.aBreastShape[1].breast[l] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[1].nip = row.GetElement(index++) == "1";
							info.aIsActive[2] = row.GetElement(index++) == "1";
							info.aIsActive[3] = row.GetElement(index) == "1";
							if (!dictionary.ContainsKey(key2))
							{
								dictionary.Add(key2, new List<YureCtrl.Info>());
							}
							dictionary[key2].Add(info);
						}
					}
				}
				if (!DicDicYure.ContainsKey(key))
				{
					DicDicYure.Add(key, dictionary);
				}
				DicDicYure[key] = dictionary;
			}
			yield return null;
		}

		private IEnumerator LoadHYureHoney2(int[] idxBust)
		{
			excelData = null;
			for (int i = 0; i < 6; i++)
			{
				int num = i;
				Dictionary<int, List<YureCtrl.Info>> dictionary = new Dictionary<int, List<YureCtrl.Info>>();
				for (int j = 0; j < pathList.Count; j++)
				{
					if (!GameSystem.IsPathAdd50(pathList[j]))
					{
						continue;
					}
					sbAbName.Clear();
					sbAssetName.Clear();
					sbAbName.Append(pathList[j]);
					switch (i)
					{
					case 0:
						sbAssetName.AppendFormat("h2a_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 1:
						sbAssetName.AppendFormat("h2h_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 2:
						sbAssetName.AppendFormat("h2s_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 3:
						sbAssetName.AppendFormat("h2t_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 4:
						sbAssetName.AppendFormat("h2l_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 5:
						sbAssetName.AppendFormat("h23p_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					}
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						continue;
					}
					int num2 = 1;
					while (num2 < excelData.MaxCell)
					{
						row = excelData.list[num2++].list;
						int index = 0;
						int key = -1;
						if (int.TryParse(row.GetElement(index++), out key))
						{
							YureCtrl.Info info = new YureCtrl.Info();
							int nFemale = 0;
							if (i > 3 && !int.TryParse(row.GetElement(index++), out nFemale))
							{
								nFemale = 0;
							}
							info.nFemale = nFemale;
							info.nameAnimation = row.GetElement(index++);
							info.aIsActive[0] = row.GetElement(index++) == "1";
							info.aBreastShape[0].MemberInit();
							for (int k = 0; k < idxBust.Length; k++)
							{
								info.aBreastShape[0].breast[k] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[0].nip = row.GetElement(index++) == "1";
							info.aIsActive[1] = row.GetElement(index++) == "1";
							info.aBreastShape[1].MemberInit();
							for (int l = 0; l < idxBust.Length; l++)
							{
								info.aBreastShape[1].breast[l] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[1].nip = row.GetElement(index++) == "1";
							info.aIsActive[2] = row.GetElement(index++) == "1";
							info.aIsActive[3] = row.GetElement(index) == "1";
							if (!dictionary.ContainsKey(key))
							{
								dictionary.Add(key, new List<YureCtrl.Info>());
							}
							dictionary[key].Add(info);
						}
					}
				}
				if (num > 4)
				{
					foreach (KeyValuePair<int, List<YureCtrl.Info>> item in dictionary)
					{
						bool flag = false;
						for (int m = 5; m < lstAnimInfo.Length; m++)
						{
							foreach (HScene.AnimationListInfo item2 in lstAnimInfo[m])
							{
								if (item2.id == item.Key)
								{
									if (!DicDicYure.ContainsKey(m))
									{
										DicDicYure.Add(m, new Dictionary<int, List<YureCtrl.Info>>());
										DicDicYure[m].Add(item.Key, item.Value);
									}
									else if (DicDicYure[m].ContainsKey(item.Key))
									{
										DicDicYure[m][item.Key] = item.Value;
									}
									else
									{
										DicDicYure[m].Add(item.Key, item.Value);
									}
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
					continue;
				}
				if (!DicDicYure.ContainsKey(num))
				{
					DicDicYure.Add(num, dictionary);
					continue;
				}
				foreach (KeyValuePair<int, List<YureCtrl.Info>> item3 in dictionary)
				{
					if (DicDicYure[num].ContainsKey(item3.Key))
					{
						DicDicYure[num][item3.Key] = item3.Value;
					}
					else
					{
						DicDicYure[num].Add(item3.Key, item3.Value);
					}
				}
			}
			yield return null;
		}

		public IEnumerator LoadHYureMale()
		{
			int[] idxBust = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
			pathList.Clear();
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetYureListFolder);
			pathList.Sort();
			yield return LoadHYureMaleAI(idxBust);
			yield return LoadHYureMaleHoney2(idxBust);
		}

		public IEnumerator LoadHYureMaleAI(int[] idxBust)
		{
			excelData = null;
			for (int i = 0; i < 6; i++)
			{
				int key = i;
				Dictionary<int, List<YureCtrlMale.Info>> dictionary = new Dictionary<int, List<YureCtrlMale.Info>>();
				for (int j = 0; j < pathList.Count; j++)
				{
					if (!GameSystem.IsPathAdd50(pathList[j]) || i == 4)
					{
						continue;
					}
					sbAbName.Clear();
					sbAssetName.Clear();
					sbAbName.Append(pathList[j]);
					switch (i)
					{
					case 0:
						sbAssetName.AppendFormat("aia_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 1:
						sbAssetName.AppendFormat("aih_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 2:
						sbAssetName.AppendFormat("ais_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 3:
						sbAssetName.AppendFormat("ait_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 5:
						sbAssetName.AppendFormat("ai3p_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					}
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						continue;
					}
					int num = 1;
					while (num < excelData.MaxCell)
					{
						row = excelData.list[num++].list;
						int index = 0;
						int key2 = -1;
						if (int.TryParse(row.GetElement(index++), out key2))
						{
							YureCtrlMale.Info info = new YureCtrlMale.Info();
							info.nMale = 0;
							info.nameAnimation = row.GetElement(index++);
							info.aIsActive[0] = row.GetElement(index++) == "1";
							info.aBreastShape[0].MemberInit();
							for (int k = 0; k < idxBust.Length; k++)
							{
								info.aBreastShape[0].breast[k] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[0].nip = row.GetElement(index++) == "1";
							info.aIsActive[1] = row.GetElement(index++) == "1";
							info.aBreastShape[1].MemberInit();
							for (int l = 0; l < idxBust.Length; l++)
							{
								info.aBreastShape[1].breast[l] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[1].nip = row.GetElement(index++) == "1";
							info.aIsActive[2] = row.GetElement(index++) == "1";
							info.aIsActive[3] = row.GetElement(index) == "1";
							if (!dictionary.ContainsKey(key2))
							{
								dictionary.Add(key2, new List<YureCtrlMale.Info>());
							}
							dictionary[key2].Add(info);
						}
					}
				}
				if (!DicDicYure.ContainsKey(key))
				{
					DicDicYureMale.Add(key, dictionary);
				}
				DicDicYureMale[key] = dictionary;
			}
			yield return null;
		}

		public IEnumerator LoadHYureMaleHoney2(int[] idxBust)
		{
			excelData = null;
			for (int i = 0; i < 6; i++)
			{
				int key = i;
				Dictionary<int, List<YureCtrlMale.Info>> dictionary = new Dictionary<int, List<YureCtrlMale.Info>>();
				for (int j = 0; j < pathList.Count; j++)
				{
					if (!GameSystem.IsPathAdd50(pathList[j]) || i == 4)
					{
						continue;
					}
					sbAbName.Clear();
					sbAssetName.Clear();
					sbAbName.Append(pathList[j]);
					switch (i)
					{
					case 0:
						sbAssetName.AppendFormat("h2a_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 1:
						sbAssetName.AppendFormat("h2h_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 2:
						sbAssetName.AppendFormat("h2s_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 3:
						sbAssetName.AppendFormat("h2t_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					case 5:
						sbAssetName.AppendFormat("h23p_m_{0:00}", Path.GetFileNameWithoutExtension(pathList[j]));
						break;
					}
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
					{
						continue;
					}
					excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
					hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
					if (excelData == null)
					{
						continue;
					}
					int num = 1;
					while (num < excelData.MaxCell)
					{
						row = excelData.list[num++].list;
						int index = 0;
						int key2 = -1;
						if (int.TryParse(row.GetElement(index++), out key2))
						{
							YureCtrlMale.Info info = new YureCtrlMale.Info();
							if (i == 5)
							{
								info.nMale = int.Parse(row.GetElement(index++));
							}
							else
							{
								info.nMale = 0;
							}
							info.nameAnimation = row.GetElement(index++);
							info.aIsActive[0] = row.GetElement(index++) == "1";
							info.aBreastShape[0].MemberInit();
							for (int k = 0; k < idxBust.Length; k++)
							{
								info.aBreastShape[0].breast[k] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[0].nip = row.GetElement(index++) == "1";
							info.aIsActive[1] = row.GetElement(index++) == "1";
							info.aBreastShape[1].MemberInit();
							for (int l = 0; l < idxBust.Length; l++)
							{
								info.aBreastShape[1].breast[l] = row.GetElement(index++) == "1";
							}
							info.aBreastShape[1].nip = row.GetElement(index++) == "1";
							info.aIsActive[2] = row.GetElement(index++) == "1";
							info.aIsActive[3] = row.GetElement(index) == "1";
							if (!dictionary.ContainsKey(key2))
							{
								dictionary.Add(key2, new List<YureCtrlMale.Info>());
							}
							dictionary[key2].Add(info);
						}
					}
				}
				if (i != 5)
				{
					if (!DicDicYure.ContainsKey(key))
					{
						DicDicYureMale.Add(key, dictionary);
						continue;
					}
					foreach (KeyValuePair<int, List<YureCtrlMale.Info>> item in dictionary)
					{
						if (DicDicYureMale[key].ContainsKey(item.Key))
						{
							DicDicYureMale[key][item.Key] = item.Value;
						}
						else
						{
							DicDicYureMale[key].Add(item.Key, item.Value);
						}
					}
					continue;
				}
				foreach (KeyValuePair<int, List<YureCtrlMale.Info>> item2 in dictionary)
				{
					bool flag = false;
					for (int m = 5; m < lstAnimInfo.Length; m++)
					{
						foreach (HScene.AnimationListInfo item3 in lstAnimInfo[m])
						{
							if (item3.id == item2.Key)
							{
								if (!DicDicYureMale.ContainsKey(m))
								{
									DicDicYureMale.Add(m, new Dictionary<int, List<YureCtrlMale.Info>>());
									DicDicYureMale[m].Add(item2.Key, item2.Value);
								}
								else if (DicDicYureMale[m].ContainsKey(item2.Key))
								{
									DicDicYureMale[m][item2.Key] = item2.Value;
								}
								else
								{
									DicDicYureMale[m].Add(item2.Key, item2.Value);
								}
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			yield return null;
		}

		private IEnumerator LoadFeelHit()
		{
			List<string> pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetFeelHitListFolder);
			pathList.Sort();
			int[] c = new int[2] { 1, 2 };
			excelData = null;
			List<string>[] rowFeel = new List<string>[2]
			{
				new List<string>(),
				new List<string>()
			};
			string[][] Initloop = new string[4][];
			FeelHit.FeelHitInfo item = default(FeelHit.FeelHitInfo);
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				string text = pathList[nLoopCnt];
				sbAssetName.Clear();
				sbAssetName.Append("FeelHit_");
				sbAssetName.Append(Path.GetFileNameWithoutExtension(pathList[nLoopCnt]));
				if (!GlobalMethod.AssetFileExist(text, sbAssetName.ToString()))
				{
					yield return null;
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(text, sbAssetName.ToString());
				hashUseAssetBundle[0].Add(new HAssetBundle(text));
				if (excelData == null)
				{
					yield return null;
					continue;
				}
				int excelRowIdx = 3;
				while (excelRowIdx < excelData.MaxCell)
				{
					for (int nLoopCntResist = 0; nLoopCntResist < 2; nLoopCntResist++)
					{
						int resist = nLoopCntResist;
						for (int i = 0; i < rowFeel.Length; i++)
						{
							rowFeel[i] = excelData.list[excelRowIdx++].list;
						}
						c[0] = 3;
						c[1] = 4;
						int personality = -1;
						if (!int.TryParse(rowFeel[0].GetElement(c[0]++), out personality))
						{
							continue;
						}
						if (!DicLstHitInfo.ContainsKey(personality))
						{
							DicLstHitInfo.Add(personality, new List<FeelHit.FeelInfo>[2]
							{
								new List<FeelHit.FeelInfo>(),
								new List<FeelHit.FeelInfo>()
							});
						}
						int num = c[0];
						int nX = num + 15;
						FeelHit.FeelInfo feelInfo;
						for (int x = num; x < nX; x += 3)
						{
							feelInfo = new FeelHit.FeelInfo();
							for (int j = 0; j < 3; j++)
							{
								Initloop[j] = rowFeel[0].GetElement(c[0]++).Split('/');
								item.area = new Vector2(float.Parse(Initloop[j][0]), float.Parse(Initloop[j][1]));
								item.rate = float.Parse(rowFeel[1].GetElement(c[1]++));
								feelInfo.lstHitArea.Add(item);
							}
							DicLstHitInfo[personality][resist].Add(feelInfo);
							yield return null;
						}
						feelInfo = new FeelHit.FeelInfo();
						for (int k = 0; k < 4; k++)
						{
							Initloop[k] = rowFeel[0].GetElement(c[0]++).Split('/');
							item.area = new Vector2(float.Parse(Initloop[k][0]), float.Parse(Initloop[k][1]));
							item.rate = float.Parse(rowFeel[1].GetElement(c[1]++));
							feelInfo.lstHitArea.Add(item);
						}
						DicLstHitInfo[personality][resist].Add(feelInfo);
					}
				}
			}
		}

		private IEnumerator LoadDankonList()
		{
			List<string> pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetDankonListFolder);
			pathList.Sort();
			sbAbName.Clear();
			sbAssetName.Clear();
			excelData = null;
			H_Lookat_dan.ShapeInfo shapeInfo = default(H_Lookat_dan.ShapeInfo);
			for (int nLoopCnt = 0; nLoopCnt < pathList.Count; nLoopCnt++)
			{
				if (!GameSystem.IsPathAdd50(pathList[nLoopCnt]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[nLoopCnt]);
				for (int nCategory = 0; nCategory < lstAnimInfo.Length; nCategory++)
				{
					for (int id = 0; id < lstAnimInfo[nCategory].Count; id++)
					{
						if (lstAnimInfo[nCategory][id].fileMale.IsNullOrEmpty())
						{
							continue;
						}
						int num = 0;
						do
						{
							sbAssetName.Clear();
							sbAssetName.Append(lstAnimInfo[nCategory][id].fileMale);
							if (GlobalMethod.StartsWith(lstAnimInfo[nCategory][id].fileMale, "ai"))
							{
								sbAssetName.Replace("_m_", "_");
							}
							else if (GlobalMethod.StartsWith(lstAnimInfo[nCategory][id].fileMale, "h2"))
							{
								if (nCategory == 6)
								{
									sbAssetName.Replace("_m1_", "_");
									sbAssetName.Append($"_{num + 1:00}");
									num++;
								}
								else
								{
									sbAssetName.Replace("_m_", "_");
								}
							}
							if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
							{
								continue;
							}
							excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
							hashUseAssetBundle[0].Add(new HAssetBundle(sbAbName.ToString()));
							if (excelData == null)
							{
								continue;
							}
							if (!DicLstLookAtDan.ContainsKey(sbAssetName.ToString()))
							{
								DicLstLookAtDan.Add(sbAssetName.ToString(), new List<H_Lookat_dan.MotionLookAtList>());
							}
							int num2 = 3;
							while (num2 < excelData.MaxCell)
							{
								row = excelData.list[num2++].list;
								H_Lookat_dan.MotionLookAtList motionLookAtList = new H_Lookat_dan.MotionLookAtList();
								int num3 = 0;
								motionLookAtList.strMotion = row.GetElement(num3++);
								int numFemale = 0;
								if (int.TryParse(row.GetElement(num3++), out numFemale))
								{
									motionLookAtList.numFemale = numFemale;
								}
								motionLookAtList.strLookAtNull = row.GetElement(num3++);
								motionLookAtList.bTopStick = row.GetElement(num3++) == "1";
								motionLookAtList.bManual = row.GetElement(num3++) == "1";
								int num4 = 0;
								for (int i = num3; i < row.Count; i += 10)
								{
									int num5 = 0;
									int shape = 0;
									if (!int.TryParse(row.GetElement(i + num5++), out shape))
									{
										break;
									}
									shapeInfo.shape = shape;
									shapeInfo.minPos = new Vector3(float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5++)));
									shapeInfo.middlePos = new Vector3(float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5++)));
									shapeInfo.maxPos = new Vector3(float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5++)), float.Parse(row.GetElement(i + num5)));
									shapeInfo.bUse = true;
									motionLookAtList.lstShape[num4++] = shapeInfo;
								}
								if (DicLstLookAtDan[sbAssetName.ToString()] == null)
								{
									DicLstLookAtDan[sbAssetName.ToString()] = new List<H_Lookat_dan.MotionLookAtList>();
								}
								DicLstLookAtDan[sbAssetName.ToString()].Add(motionLookAtList);
							}
						}
						while (nCategory == 6 && num < 2);
						yield return null;
					}
				}
			}
		}

		public void LoadHParm()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetParam);
			assetBundleNameListFromPath.Sort();
			result = new Dictionary<int, HResultParam.Param>();
			mind = new Dictionary<int, GameParameterInfo.Param>();
			trait = new Dictionary<int, GameParameterInfo.Param>();
			taii = new Dictionary<int, HTaiiParam.Param>();
			resist = new Dictionary<int, Dictionary<int, int[]>>();
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					for (int j = 0; j < 5; j++)
					{
						LoadHParamTable(assetBundleNameListFromPath[i], j);
					}
				}
			}
		}

		private void LoadHParamTable(string path, int mode)
		{
			sbAssetName.Clear();
			switch (mode)
			{
			default:
				return;
			case 0:
				sbAssetName.Append("HResult_");
				break;
			case 1:
				sbAssetName.Append("Mind_");
				break;
			case 2:
				sbAssetName.Append("Trait_");
				break;
			case 3:
				sbAssetName.Append("Taii_");
				break;
			case 4:
				sbAssetName.Append("Resist_");
				break;
			}
			sbAssetName.Append(Path.GetFileNameWithoutExtension(path));
			if (!GlobalMethod.AssetFileExist(path, sbAssetName.ToString()))
			{
				return;
			}
			switch (mode)
			{
			case 0:
			{
				HResultParam hResultParam = CommonLib.LoadAsset<HResultParam>(path, sbAssetName.ToString());
				if (hResultParam == null || hResultParam.param.Count == 0)
				{
					return;
				}
				foreach (HResultParam.Param item in hResultParam.param)
				{
					if (!result.ContainsKey(item.id))
					{
						result.Add(item.id, item);
					}
					else
					{
						result[item.id] = item;
					}
				}
				break;
			}
			case 1:
			case 2:
			{
				GameParameterInfo gameParameterInfo = CommonLib.LoadAsset<GameParameterInfo>(path, sbAssetName.ToString());
				if (gameParameterInfo == null || gameParameterInfo.param.Count == 0)
				{
					return;
				}
				if (mode == 1)
				{
					foreach (GameParameterInfo.Param item2 in gameParameterInfo.param)
					{
						if (!mind.ContainsKey(item2.id))
						{
							mind.Add(item2.id, item2);
						}
						else
						{
							mind[item2.id] = item2;
						}
					}
					break;
				}
				foreach (GameParameterInfo.Param item3 in gameParameterInfo.param)
				{
					if (!trait.ContainsKey(item3.id))
					{
						trait.Add(item3.id, item3);
					}
					else
					{
						trait[item3.id] = item3;
					}
				}
				break;
			}
			case 3:
			{
				HTaiiParam hTaiiParam = CommonLib.LoadAsset<HTaiiParam>(path, sbAssetName.ToString());
				if (hTaiiParam == null || hTaiiParam.param.Count == 0)
				{
					return;
				}
				foreach (HTaiiParam.Param item4 in hTaiiParam.param)
				{
					if (!taii.ContainsKey(item4.id))
					{
						taii.Add(item4.id, item4);
					}
					else
					{
						taii[item4.id] = item4;
					}
				}
				break;
			}
			case 4:
			{
				excelData = CommonLib.LoadAsset<ExcelData>(path, sbAssetName.ToString());
				if (excelData == null || excelData.MaxCell == 0)
				{
					return;
				}
				int num = 0;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					int key = 0;
					if (!int.TryParse(row[num2++], out key))
					{
						continue;
					}
					int key2 = 0;
					if (int.TryParse(row[num2++], out key2))
					{
						if (!resist.ContainsKey(key))
						{
							resist.Add(key, new Dictionary<int, int[]>());
							resist[key].Add(key2, new int[3]);
						}
						else if (!resist[key].ContainsKey(key2))
						{
							resist[key].Add(key2, new int[3]);
						}
						if (!int.TryParse(row[num2++], out resist[key][key2][0]))
						{
							resist[key][key2][0] = 0;
						}
						if (!int.TryParse(row[num2++], out resist[key][key2][1]))
						{
							resist[key][key2][1] = 0;
						}
						if (!int.TryParse(row[num2++], out resist[key][key2][2]))
						{
							resist[key][key2][2] = 0;
						}
					}
				}
				break;
			}
			}
			hashUseAssetBundle[0].Add(new HAssetBundle(path));
		}

		private bool GetIntMember(string[,] _str, int _y, ref int _line, ref int _member)
		{
			if (_str.GetLength(1) <= _line)
			{
				return false;
			}
			string text = _str[_y, _line++];
			if (!text.IsNullOrEmpty())
			{
				_member = int.Parse(text);
			}
			return true;
		}

		private void LoadHitObject()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHitObjListFolder);
			pathList.Sort();
			sbAbName.Clear();
			sbAssetName.Clear();
			sbAssetName.Append("base");
			excelData = null;
			List<string> list = new List<string>();
			for (int i = 0; i < pathList.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(pathList[i]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(pathList[i]);
				if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
				AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					list = new List<string>();
					int key = -1;
					if (int.TryParse(row.GetElement(num2++), out key))
					{
						while (num2 < row.Count)
						{
							list.Add(row.GetElement(num2++));
						}
						if (!lstHitObject.ContainsKey(key))
						{
							lstHitObject.Add(key, list);
						}
						else
						{
							lstHitObject[key] = list;
						}
					}
				}
			}
		}

		private void LoadHitObjectAdd()
		{
			pathList = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHitObjListFolder);
			pathList.Sort();
			sbAbName.Clear();
			List<string> ret = new List<string>();
			ValueDictionary<string, int, List<string>> Loaded = new ValueDictionary<string, int, List<string>>();
			HitObjectAssetNames(pathList, ref ret);
			for (int i = 0; i < pathList.Count; i++)
			{
				if (GameSystem.IsPathAdd50(pathList[i]))
				{
					LoadHitObjectAdd(pathList[i], ret, ref Loaded);
				}
			}
			foreach (ValueDictionary<int, List<string>> value in Loaded.Values)
			{
				foreach (KeyValuePair<int, List<string>> item in value)
				{
					if (!lstHitObject.ContainsKey(item.Key))
					{
						lstHitObject.Add(item.Key, item.Value);
						continue;
					}
					ret = lstHitObject[item.Key];
					CheckHitObjectListContain(ref ret, item.Value);
				}
			}
		}

		private void LoadHitObjectAdd(string sbAbName, List<string> tmpNameList, ref ValueDictionary<string, int, List<string>> Loaded)
		{
			excelData = null;
			List<string> list = new List<string>();
			for (int i = 0; i < tmpNameList.Count; i++)
			{
				if (!GlobalMethod.AssetFileExist(sbAbName, tmpNameList[i]))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(sbAbName, tmpNameList[i]);
				AssetBundleManager.UnloadAssetBundle(sbAbName, isUnloadForceRefCount: true);
				if (excelData == null)
				{
					continue;
				}
				if (!Loaded.ContainsKey(tmpNameList[i]))
				{
					Loaded[tmpNameList[i]] = Loaded.New();
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					int num2 = 0;
					list = new List<string>();
					int key = -1;
					if (int.TryParse(row.GetElement(num2++), out key))
					{
						while (num2 < row.Count)
						{
							list.Add(row.GetElement(num2++));
						}
						if (!Loaded[tmpNameList[i]].ContainsKey(key))
						{
							Loaded[tmpNameList[i]].Add(key, list);
						}
						else
						{
							Loaded[tmpNameList[i]][key] = list;
						}
					}
				}
			}
		}

		private void HitObjectAssetNames(List<string> pathList, ref List<string> ret)
		{
			foreach (string path in pathList)
			{
				string[] allAssetName = AssetBundleCheck.GetAllAssetName(path, _WithExtension: false);
				foreach (string text in allAssetName)
				{
					if (StartsWith(text, "base_") && !ret.Contains(text))
					{
						ret.Add(text);
					}
				}
			}
		}

		private void CheckHitObjectListContain(ref List<string> check, List<string> values)
		{
			bool flag = false;
			int num = 0;
			while (num < values.Count)
			{
				flag = false;
				for (int i = 0; i < check.Count; i += 3)
				{
					if (!(check[i] != values[num]) && !(check[i + 1] != values[num + 1]) && !(check[i + 2] != values[num + 2]))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					num += 3;
					continue;
				}
				check.Add(values[num++]);
				check.Add(values[num++]);
				check.Add(values[num++]);
			}
		}

		public bool StartsWith(string check, string template)
		{
			int length = check.Length;
			int length2 = template.Length;
			int num = 0;
			int num2 = 0;
			while (num < length && num2 < length2 && check[num] == template[num2])
			{
				num++;
				num2++;
			}
			if (num2 == length2)
			{
				return length >= length2;
			}
			return false;
		}

		private void CollisionLoadExcel()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetCollisionListFolder);
			assetBundleNameListFromPath.Sort();
			CollisionCtrl.CollisionInfo info = default(CollisionCtrl.CollisionInfo);
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(assetBundleNameListFromPath[i]);
				for (int j = 0; j < lstAnimInfo.Length; j++)
				{
					for (int k = 0; k < lstAnimInfo[j].Count; k++)
					{
						CollisionLoadExcel(lstAnimInfo[j][k], 0, ref info);
						CollisionLoadExcel(lstAnimInfo[j][k], 1, ref info);
						CollisionLoadExcel(lstAnimInfo[j][k], 2, ref info);
						CollisionLoadExcel(lstAnimInfo[j][k], 3, ref info);
					}
				}
			}
		}

		private void CollisionLoadExcel(HScene.AnimationListInfo ainfo, int kind, ref CollisionCtrl.CollisionInfo info)
		{
			switch (kind)
			{
			case 0:
				if (ainfo.fileMale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ainfo.fileMale);
				break;
			case 1:
				if (ainfo.fileFemale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ainfo.fileFemale);
				break;
			case 2:
				if (ainfo.fileFemale2.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ainfo.fileFemale2);
				break;
			case 3:
				if (ainfo.fileMale2.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ainfo.fileMale2);
				break;
			}
			if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
			{
				return;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
			AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
			if (excelData == null)
			{
				return;
			}
			if (!DicLstCollisionInfo.ContainsKey(sbAssetName.ToString()))
			{
				DicLstCollisionInfo.Add(sbAssetName.ToString(), new List<CollisionCtrl.CollisionInfo>());
			}
			int num = 1;
			LoadCollisionList = new List<CollisionCtrl.CollisionInfo>();
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				info.nameAnimation = row.GetElement(num2++);
				info.lstIsActive = new List<bool>();
				while (num2 < row.Count)
				{
					info.lstIsActive.Add(row.GetElement(num2++) == "1");
				}
				LoadCollisionList.Add(info);
			}
			DicLstCollisionInfo[sbAssetName.ToString()] = LoadCollisionList;
		}

		private void HitObjLoadExcel()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHitObjListFolder);
			assetBundleNameListFromPath.Sort();
			HitObjectCtrl.CollisionInfo info = default(HitObjectCtrl.CollisionInfo);
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(assetBundleNameListFromPath[i]);
				for (int j = 0; j < lstAnimInfo.Length; j++)
				{
					for (int k = 0; k < lstAnimInfo[j].Count; k++)
					{
						HitObjLoadExcel(lstAnimInfo[j][k], 0, ref info);
						HitObjLoadExcel(lstAnimInfo[j][k], 1, ref info);
						HitObjLoadExcel(lstAnimInfo[j][k], 2, ref info);
						HitObjLoadExcel(lstAnimInfo[j][k], 3, ref info);
					}
				}
			}
		}

		private void HitObjLoadExcel(HScene.AnimationListInfo ai, int kind, ref HitObjectCtrl.CollisionInfo info)
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (kind)
			{
			case 0:
				if (ai.fileMale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileMale);
				break;
			case 1:
				if (ai.fileFemale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileFemale);
				break;
			case 2:
				if (ai.fileFemale2.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileFemale2);
				break;
			case 3:
				if (ai.fileMale2.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileMale2);
				break;
			}
			excelData = null;
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
			AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
			if (excelData == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			num2 = 1;
			if (!HitObjAtariName.ContainsKey(sbAssetName.ToString()))
			{
				HitObjAtariName.Add(sbAssetName.ToString(), new List<string>());
			}
			row = excelData.list[num++].list;
			for (int i = 1; i < row.Count; i++)
			{
				int index = i;
				stringBuilder.Clear();
				stringBuilder.Append(row.GetElement(index));
				if (!HitObjAtariName[sbAssetName.ToString()].Contains(stringBuilder.ToString()))
				{
					HitObjAtariName[sbAssetName.ToString()].Add(stringBuilder.ToString());
				}
			}
			if (!DicLstHitObjInfo.ContainsKey(sbAssetName.ToString()))
			{
				DicLstHitObjInfo.Add(sbAssetName.ToString(), new List<HitObjectCtrl.CollisionInfo>());
			}
			while (num < excelData.MaxCell)
			{
				num2 = 0;
				row = excelData.list[num++].list;
				info.nameAnimation = row.GetElement(num2++);
				info.lstIsActive = new List<bool>();
				for (int j = 1; j < row.Count; j++)
				{
					info.lstIsActive.Add(row.GetElement(num2++) == "1");
				}
				DicLstHitObjInfo[sbAssetName.ToString()].Add(info);
			}
		}

		private void HLayerLoadExcel()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetLayerCtrlListFolder);
			assetBundleNameListFromPath.Sort();
			excelData = null;
			HLayerCtrl.HLayerInfo info = default(HLayerCtrl.HLayerInfo);
			StringBuilder stateName = new StringBuilder();
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				sbAbName.Clear();
				sbAbName.Append(assetBundleNameListFromPath[i]);
				for (int j = 0; j < lstAnimInfo.Length; j++)
				{
					for (int k = 0; k < lstAnimInfo[j].Count; k++)
					{
						HLayerLoadExcel(lstAnimInfo[j][k], 0, stateName, ref info);
						HLayerLoadExcel(lstAnimInfo[j][k], 1, stateName, ref info);
						HLayerLoadExcel(lstAnimInfo[j][k], 2, stateName, ref info);
					}
				}
			}
		}

		private void HLayerLoadExcel(HScene.AnimationListInfo ai, int kind, StringBuilder stateName, ref HLayerCtrl.HLayerInfo info)
		{
			switch (kind)
			{
			case 0:
				if (ai.fileMale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileMale);
				break;
			case 1:
				if (ai.fileFemale.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileFemale);
				break;
			case 2:
				if (ai.fileFemale2.IsNullOrEmpty())
				{
					return;
				}
				sbAssetName.Clear();
				sbAssetName.Append(ai.fileFemale2);
				break;
			}
			if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), sbAssetName.ToString()))
			{
				return;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(sbAbName.ToString(), sbAssetName.ToString());
			AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
			if (excelData == null)
			{
				return;
			}
			if (!LayerInfos.ContainsKey(sbAssetName.ToString()))
			{
				LayerInfos.Add(sbAssetName.ToString(), new Dictionary<string, HLayerCtrl.HLayerInfo>());
			}
			int num = 1;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				stateName.Clear();
				stateName.Append(row[num2++]);
				if (!(stateName.ToString() == ""))
				{
					if (!LayerInfos[sbAssetName.ToString()].ContainsKey(stateName.ToString()))
					{
						LayerInfos[sbAssetName.ToString()].Add(stateName.ToString(), default(HLayerCtrl.HLayerInfo));
					}
					int layerID = 0;
					float weight = 0f;
					if (int.TryParse(row[num2++], out layerID))
					{
						float.TryParse(row[num2++], out weight);
					}
					else
					{
						layerID = 0;
					}
					info.LayerID = layerID;
					info.weight = weight;
					LayerInfos[sbAssetName.ToString()][stateName.ToString()] = info;
				}
			}
		}

		private void LoadMasterbationInfo()
		{
			List<string> list = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetHAutoListFolder);
			string text = "masturbation";
			if (list.Count != 0)
			{
				list = list.Where((string l) => GameSystem.IsPathAdd50(l)).ToList();
			}
			if (list == null || list.Count == 0)
			{
				return;
			}
			int index = list.Count - 1;
			if (!GlobalMethod.AssetFileExist(list[index], text))
			{
				return;
			}
			ExcelData excelData = CommonLib.LoadAsset<ExcelData>(list[index], text);
			hashUseAssetBundle[0].Add(new HAssetBundle(list[index]));
			if (excelData == null)
			{
				return;
			}
			MBinfo = new Masturbation.MasturbationTimeInfo();
			int num = 2;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				if (!float.TryParse(row.GetElement(num2++), out MBinfo.Start[0]))
				{
					MBinfo.Start[0] = 5f;
				}
				if (!float.TryParse(row.GetElement(num2++), out MBinfo.Start[1]))
				{
					MBinfo.Start[1] = 5f;
				}
				if (!float.TryParse(row.GetElement(num2++), out MBinfo.Restart[0]))
				{
					MBinfo.Restart[0] = 5f;
				}
				if (!float.TryParse(row.GetElement(num2++), out MBinfo.Restart[1]))
				{
					MBinfo.Restart[1] = 5f;
				}
			}
		}

		public void HparticleInit(Transform place)
		{
			hParticle = new HParticleCtrl();
			hParticle.Init(place);
		}

		public void HitObjListInit()
		{
			if (lstHitObject == null || !lstHitObject.ContainsKey(1))
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				if (!DicHitObject.ContainsKey(1))
				{
					DicHitObject.Add(1, new Dictionary<int, Dictionary<string, GameObject>>());
				}
				if (!DicHitObject[1].ContainsKey(i))
				{
					DicHitObject[1].Add(i, new Dictionary<string, GameObject>());
				}
				int count = lstHitObject[1].Count;
				for (int j = 0; j < count; j += 3)
				{
					sbAbName.Clear();
					sbAbName.Append(lstHitObject[1][j + 1]);
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), lstHitObject[1][j + 2]))
					{
						continue;
					}
					GameObject gameObject = CommonLib.LoadAsset<GameObject>(sbAbName.ToString(), lstHitObject[1][j + 2], clone: true);
					AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
					if (!(gameObject == null))
					{
						gameObject.SetActive(value: false);
						gameObject.transform.SetParent(HitObjectPlace, worldPositionStays: false);
						ObiCollider[] componentsInChildren = gameObject.GetComponentsInChildren<ObiCollider>(includeInactive: true);
						for (int k = 0; k < componentsInChildren.Length; k++)
						{
							componentsInChildren[k].Phase = 1;
						}
						if (!DicHitObject[1][i].ContainsKey(lstHitObject[1][j + 2]))
						{
							DicHitObject[1][i].Add(lstHitObject[1][j + 2], gameObject);
						}
						else
						{
							DicHitObject[1][i][lstHitObject[1][j + 2]] = gameObject;
						}
					}
				}
			}
			if (!lstHitObject.ContainsKey(0))
			{
				return;
			}
			for (int l = 0; l < 2; l++)
			{
				if (!DicHitObject.ContainsKey(0))
				{
					DicHitObject.Add(0, new Dictionary<int, Dictionary<string, GameObject>>());
				}
				if (!DicHitObject[0].ContainsKey(l))
				{
					DicHitObject[0].Add(l, new Dictionary<string, GameObject>());
				}
				int count2 = lstHitObject[0].Count;
				for (int m = 0; m < count2; m += 3)
				{
					sbAbName.Clear();
					sbAbName.Append(lstHitObject[0][m + 1]);
					if (!GlobalMethod.AssetFileExist(sbAbName.ToString(), lstHitObject[0][m + 2]))
					{
						continue;
					}
					GameObject gameObject2 = CommonLib.LoadAsset<GameObject>(sbAbName.ToString(), lstHitObject[0][m + 2], clone: true);
					AssetBundleManager.UnloadAssetBundle(sbAbName.ToString(), isUnloadForceRefCount: true);
					if (!(gameObject2 == null))
					{
						gameObject2.SetActive(value: false);
						gameObject2.transform.SetParent(HitObjectPlace, worldPositionStays: false);
						ObiCollider[] componentsInChildren = gameObject2.GetComponentsInChildren<ObiCollider>(includeInactive: true);
						for (int k = 0; k < componentsInChildren.Length; k++)
						{
							componentsInChildren[k].Phase = 1;
						}
						if (!DicHitObject[0][l].ContainsKey(lstHitObject[0][m + 2]))
						{
							DicHitObject[0][l].Add(lstHitObject[0][m + 2], gameObject2);
						}
						else
						{
							DicHitObject[0][l][lstHitObject[0][m + 2]] = gameObject2;
						}
					}
				}
			}
			foreach (HAssetBundle item in hashUseAssetBundle[1])
			{
				AssetBundleManager.UnloadAssetBundle(item.path, isUnloadForceRefCount: false, item.manifest);
			}
		}

		public void ReleaceHitObj()
		{
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<string, GameObject>>> item in DicHitObject)
			{
				foreach (KeyValuePair<int, Dictionary<string, GameObject>> item2 in item.Value)
				{
					foreach (KeyValuePair<string, GameObject> item3 in item2.Value)
					{
						UnityEngine.Object.Destroy(item3.Value);
					}
				}
			}
			DicHitObject.Clear();
		}

		private void LoadPresetScreenEffect()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetScreenEffectFolder);
			if (assetBundleNameListFromPath == null || assetBundleNameListFromPath.Count == 0)
			{
				return;
			}
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				int index = i;
				ScreenEffectPresetData[] allAssets = AssetBundleManager.LoadAllAsset(assetBundleNameListFromPath[index], typeof(ScreenEffectPresetData)).GetAllAssets<ScreenEffectPresetData>();
				for (int j = 0; j < allAssets.Length; j++)
				{
					foreach (ScreenEffectPresetData.Param item in allAssets[j].param)
					{
						ScreenEffectPresetInfos[item.No] = new ScreenEffect.PresetInfo(item.No, item.Name, item.AssetBundleName, item.AssetName, item.Manifest);
					}
				}
			}
		}

		private void LoadProbeTexInfo()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetScreenEffectFolder);
			if (assetBundleNameListFromPath == null || assetBundleNameListFromPath.Count == 0)
			{
				return;
			}
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				int index = i;
				string text = "Probe_" + Path.GetFileNameWithoutExtension(assetBundleNameListFromPath[index]);
				if (!GlobalMethod.AssetFileExist(assetBundleNameListFromPath[index], text))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(assetBundleNameListFromPath[index], text);
				hashUseAssetBundle[0].Add(new HAssetBundle(assetBundleNameListFromPath[index]));
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					if (row.Count != 0)
					{
						int num2 = 0;
						if (int.TryParse(row[num2++], out var key))
						{
							ProbeTexInfos[key] = new AssetBundleInfo(row[num2++], row[num2++], row[num2++]);
						}
					}
				}
			}
		}

		private void LoadColorFilterInfo()
		{
			List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(Singleton<HSceneManager>.Instance.strAssetScreenEffectFolder);
			if (assetBundleNameListFromPath == null || assetBundleNameListFromPath.Count == 0)
			{
				return;
			}
			for (int i = 0; i < assetBundleNameListFromPath.Count; i++)
			{
				if (!GameSystem.IsPathAdd50(assetBundleNameListFromPath[i]))
				{
					continue;
				}
				int index = i;
				string text = "Filter_" + Path.GetFileNameWithoutExtension(assetBundleNameListFromPath[index]);
				if (!GlobalMethod.AssetFileExist(assetBundleNameListFromPath[index], text))
				{
					continue;
				}
				excelData = CommonLib.LoadAsset<ExcelData>(assetBundleNameListFromPath[index], text);
				hashUseAssetBundle[0].Add(new HAssetBundle(assetBundleNameListFromPath[index]));
				if (excelData == null)
				{
					continue;
				}
				int num = 1;
				while (num < excelData.MaxCell)
				{
					row = excelData.list[num++].list;
					if (row.Count != 0)
					{
						int num2 = 0;
						if (int.TryParse(row[num2++], out var key))
						{
							ColorFilterInfos[key] = new AssetBundleInfo(row[num2++], row[num2++], row[num2++]);
						}
					}
				}
			}
		}
	}

	public class BaseAnimInfo
	{
		public string path;

		public string name;

		public RuntimeAnimatorController rac;
	}

	public ChaControl player;

	public ChaControl[] females = new ChaControl[2];

	public string[] pngFemales = new string[2];

	public string pngMale;

	public string pngMaleSecond;

	public HashSet<string> hashUseAssetBundle = new HashSet<string>();

	public int mapID = -1;

	public int numFemaleClothCustom;

	private ChaFileDefine.State[] femaleState = new ChaFileDefine.State[2];

	private Dictionary<ChaFileDefine.State, int>[] femaleStateNum = new Dictionary<ChaFileDefine.State, int>[2]
	{
		new Dictionary<ChaFileDefine.State, int>(),
		new Dictionary<ChaFileDefine.State, int>()
	};

	public int[] Personality = new int[2];

	public int hMainKind;

	[SerializeField]
	private Transform hitobjPlace;

	public int height = -1;

	public bool isForce;

	public bool isForceSecond;

	[DisabledGroup("ふたなりボディか:１人目")]
	public bool bFutanari;

	[DisabledGroup("ふたなりボディか:２人目")]
	public bool bFutanariSecond;

	public int UrineType;

	public HScene Hscene;

	[HideInInspector]
	public readonly string strAssetCameraList = "list/h/camera/";

	[HideInInspector]
	public readonly string strAssetAnimationInfoListFolder = "list/h/animationinfo/";

	[HideInInspector]
	public readonly string strAssetStartAnimationListFolder = "list/h/startanimation/";

	[HideInInspector]
	public readonly string strAssetMoveOffsetListFolder = "list/h/move/";

	[HideInInspector]
	public readonly string strAssetNeckCtrlListFolder = "list/h/neckcontrol/";

	[HideInInspector]
	public readonly string strAssetYureListFolder = "list/h/yure/";

	[HideInInspector]
	public readonly string strAssetLayerCtrlListFolder = "list/h/layer/";

	[HideInInspector]
	public readonly string strAssetDankonListFolder = "list/h/lookatdan/";

	[HideInInspector]
	public readonly string strAssetDynamicBoneListFolder = "list/h/dynamicbone/";

	[HideInInspector]
	public readonly string strAssetHAutoListFolder = "list/h/hauto/hauto/";

	[HideInInspector]
	public readonly string strAssetLeaveItToYouFolder = "list/h/hauto/leaveittoyou/";

	[HideInInspector]
	public readonly string strAssetHParticleListFolder = "list/h/hparticle/";

	[HideInInspector]
	public readonly string strAssetSiruPasteListFolder = "list/h/sirupaste/";

	[HideInInspector]
	public readonly string strAssetObiListFolder = "list/h/siruobi/";

	[HideInInspector]
	public readonly string strAssetHpointListFolder = "list/h/hpointinfo/";

	[HideInInspector]
	public readonly string strAssetFeelHitListFolder = "list/h/feelhit/";

	[HideInInspector]
	public readonly string strAssetHItemInfoListFolder = "list/h/hitem/";

	[HideInInspector]
	public readonly string strAssetHItemObjInfoListFolder = "list/h/hitemobj/";

	[HideInInspector]
	public readonly string strAssetHitObjListFolder = "list/h/hit/hitobject/";

	[HideInInspector]
	public readonly string strAssetCollisionListFolder = "list/h/hit/collision/";

	[HideInInspector]
	public readonly string strAssetSEListFolder = "list/h/sound/se/";

	[HideInInspector]
	public readonly string strAssetBGMListFolder = "list/h/sound/bgm/";

	[HideInInspector]
	public readonly string strAssetVoiceListFolder = "list/h/sound/voice/";

	[HideInInspector]
	public readonly string strAssetVoiceFaceListFolder = "list/h/sound/face/";

	[HideInInspector]
	public readonly string strAssetBreathListFolder = "list/h/sound/breath/";

	[HideInInspector]
	public readonly string strAssetSE = "sound/data/se/h";

	[HideInInspector]
	public readonly string strAssetParam = "list/h/param/";

	[HideInInspector]
	public readonly string strAssetIKListFolder = "list/h/ikinfo/";

	[HideInInspector]
	public readonly string strAssetMobFolder = "list/h/mob/";

	[HideInInspector]
	public readonly string strAssetScreenEffectFolder = "list/h/screeneffect/";

	[HideInInspector]
	public readonly string strAssetRootMotionFolder = "list/h/rootmotionoffset/";

	public int maleFinish;

	public int femaleFinish;

	public bool isCtrl;

	public byte endStatus;

	public IDisposable choiceDisposable;

	private bool _isH;

	private HSceneTables hResourceTables = new HSceneTables();

	public bool SecondSitori;

	public ChaFileDefine.State[] FemaleState => femaleState;

	public Dictionary<ChaFileDefine.State, int>[] FemaleStateNum => femaleStateNum;

	public static Transform HitObjectPlace
	{
		get
		{
			if (!Singleton<HSceneManager>.IsInstance())
			{
				return null;
			}
			return Singleton<HSceneManager>.Instance.hitobjPlace;
		}
	}

	public static bool isHScene
	{
		get
		{
			if (!Singleton<HSceneManager>.IsInstance())
			{
				return false;
			}
			return Singleton<HSceneManager>.Instance._isH;
		}
	}

	public static HSceneTables HResourceTables
	{
		get
		{
			if (!Singleton<HSceneManager>.IsInstance())
			{
				return null;
			}
			return Singleton<HSceneManager>.Instance.hResourceTables;
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<AssetBundleManager>.IsInstance());
		if (hResourceTables == null)
		{
			hResourceTables = new HSceneTables();
		}
		yield return hResourceTables.LoadH();
	}

	public void LoadHScene()
	{
		Scene.LoadReserve(new Scene.Data
		{
			levelName = "HScene",
			isAdd = true,
			fadeType = FadeCanvas.Fade.In
		}, isLoadingImageDraw: false);
	}

	public void EndHScene()
	{
		_isH = false;
	}

	public void SetHFlag()
	{
		_isH = true;
	}

	public HScreenEffectEnable GetHScreenEffect()
	{
		HScreenEffectEnable result = default(HScreenEffectEnable);
		if (Hscene != null)
		{
			ScreenEffectUI screenEffectUI = Hscene.screenEffectUI;
			if (Hscene.screenEffect.ProcessVolumeDef == null)
			{
				screenEffectUI.SetEnableDef();
			}
			return screenEffectUI.Enables;
		}
		return result;
	}

	public void SetFemaleState(ChaControl[] female)
	{
		for (int i = 0; i < female.Length; i++)
		{
			if (!(female[i] == null))
			{
				femaleState[i] = female[i].fileGameInfo2.nowDrawState;
				FemaleStateNum[i].Clear();
				FemaleStateNum[i].Add(ChaFileDefine.State.Favor, female[i].fileGameInfo2.Favor);
				FemaleStateNum[i].Add(ChaFileDefine.State.Enjoyment, female[i].fileGameInfo2.Enjoyment);
				FemaleStateNum[i].Add(ChaFileDefine.State.Slavery, female[i].fileGameInfo2.Slavery);
				FemaleStateNum[i].Add(ChaFileDefine.State.Aversion, female[i].fileGameInfo2.Aversion);
			}
		}
	}
}
