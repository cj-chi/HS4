using System;
using System.Collections.Generic;
using AIChara;
using IllusionUtility.GetUtility;
using Manager;
using Obi;
using UniRx;
using UnityEngine;

[Serializable]
public class ObiCtrl
{
	public class SiruObiInfo
	{
		private SiruObiParamInfo[] info = new SiruObiParamInfo[2];

		public string ParentName;

		public Vector3[] OffsetPos = new Vector3[2];

		public Vector3[] OffsetRot = new Vector3[2];

		public Vector3[,] LoadOffsetPos = new Vector3[2, 3];

		public Vector3[,] LoadOffsetRot = new Vector3[2, 3];

		public List<ShootInfo> ShootInfo = new List<ShootInfo>();

		public int PlayParticleID;

		public int nowState;

		public SiruObiParamInfo[] Info => info;

		public void SetInfo(int id, SiruObiParamInfo val, ChaControl shapeFemale)
		{
			info[id] = val;
			info[id].ConvertSetupInfo(shapeFemale);
			if (shapeFemale != null)
			{
				float shapeBodyValue = shapeFemale.GetShapeBodyValue(0);
				float t = ((shapeBodyValue >= 0.5f) ? Mathf.InverseLerp(0.5f, 1f, shapeBodyValue) : Mathf.InverseLerp(0f, 0.5f, shapeBodyValue));
				bool flag = shapeBodyValue >= 0.5f;
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						OffsetPos[i][j] = (flag ? Mathf.Lerp(LoadOffsetPos[i, 1][j], LoadOffsetPos[i, 2][j], t) : Mathf.Lerp(LoadOffsetPos[i, 0][j], LoadOffsetPos[i, 1][j], t));
						OffsetRot[i][j] = (flag ? Mathf.Lerp(LoadOffsetRot[i, 1][j], LoadOffsetRot[i, 2][j], t) : Mathf.Lerp(LoadOffsetRot[i, 0][j], LoadOffsetRot[i, 1][j], t));
					}
				}
				return;
			}
			for (int k = 0; k < 2; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					OffsetPos[k][l] = LoadOffsetPos[k, 1][l];
					OffsetRot[k][l] = LoadOffsetRot[k, 1][l];
				}
			}
		}
	}

	public class SiruObiParamInfo
	{
		public AssetBundleInfo EmitterPtn;

		public AssetBundleInfo FluidParam;

		public AssetBundleInfo ColParam;

		public float UseSpeed;

		public float UseLifeSpan;

		public float UseRandVel;

		public int UseNumParticle;

		public float UseShapeRadius;

		public float UseParticleRadius;

		public float[] Speed = new float[3];

		public float[] LifeSpan = new float[3];

		public float[] RandVel = new float[3];

		public int[] NumParticle = new int[3];

		public float[] ShapeRadius = new float[3];

		public float[] ParticleRadius = new float[3];

		public ObiEmitterCtrl.SetupInfo SetupInfo;

		public void ConvertSetupInfo(ChaControl shapeFemale)
		{
			if (shapeFemale == null)
			{
				UseSpeed = Speed[1];
				UseLifeSpan = LifeSpan[1];
				UseRandVel = RandVel[1];
				UseNumParticle = NumParticle[1];
				UseShapeRadius = ShapeRadius[1];
				UseParticleRadius = ParticleRadius[1];
			}
			else
			{
				float shapeBodyValue = shapeFemale.GetShapeBodyValue(0);
				float t = ((shapeBodyValue >= 0.5f) ? Mathf.InverseLerp(0.5f, 1f, shapeBodyValue) : Mathf.InverseLerp(0f, 0.5f, shapeBodyValue));
				bool flag = shapeBodyValue >= 0.5f;
				UseSpeed = (flag ? Mathf.Lerp(Speed[1], Speed[2], t) : Mathf.Lerp(Speed[0], Speed[1], t));
				UseLifeSpan = (flag ? Mathf.Lerp(LifeSpan[1], LifeSpan[2], t) : Mathf.Lerp(LifeSpan[0], LifeSpan[1], t));
				UseRandVel = (flag ? Mathf.Lerp(RandVel[1], RandVel[2], t) : Mathf.Lerp(RandVel[0], RandVel[1], t));
				UseNumParticle = Mathf.RoundToInt(flag ? Mathf.Lerp(NumParticle[1], NumParticle[2], t) : Mathf.Lerp(NumParticle[0], NumParticle[1], t));
				UseShapeRadius = (flag ? Mathf.Lerp(ShapeRadius[1], ShapeRadius[2], t) : Mathf.Lerp(ShapeRadius[0], ShapeRadius[1], t));
				UseParticleRadius = (flag ? Mathf.Lerp(ParticleRadius[1], ParticleRadius[2], t) : Mathf.Lerp(ParticleRadius[0], ParticleRadius[1], t));
			}
			SetupInfo = new ObiEmitterCtrl.SetupInfo(FluidParam.assetbundle, FluidParam.asset, FluidParam.manifest, ColParam.assetbundle, ColParam.asset, ColParam.manifest, UseSpeed, UseLifeSpan, UseRandVel, UseNumParticle, UseShapeRadius, UseParticleRadius);
		}
	}

	public struct ShootInfo
	{
		public bool Inside;

		public string Anim;

		public float NormalizeTime;

		public float CalcOldTime;
	}

	private ObiFluidManager obiManager;

	private HSceneManager hManager;

	private ObiSolver solver;

	private bool checkWait;

	private ObiFluidCtrl[] obiFluidCtrlMale = new ObiFluidCtrl[2];

	private ObiFluidCtrl[] obiFluidCtrlFemale = new ObiFluidCtrl[2];

	private ObiFluidCtrl obiFluidCtrlObj;

	private ExcelData excelData;

	private List<string> row;

	private ObiEmitterCtrl.SetupInfo LoadInfo;

	private List<ObiFluidManager.AddTargetParam> TargetParams;

	private HParticleCtrl particle;

	private Dictionary<int, Dictionary<int, SiruObiInfo>> siruInfos = new Dictionary<int, Dictionary<int, SiruObiInfo>>();

	private ObiFluidManager.AddTargetParam[] siruInfoObjs;

	private ChaControl ShapeFemale;

	private bool[] CanUrinePloc = new bool[2];

	private HSceneFlagCtrl Hflag;

	private IDisposable Disposable;

	public ObiCtrl(HSceneManager _hManager, HSceneFlagCtrl flag)
	{
		if (Singleton<ObiFluidManager>.IsInstance())
		{
			if (obiManager == null)
			{
				obiManager = Singleton<ObiFluidManager>.Instance;
				solver = obiManager.ObiSolver;
			}
			hManager = _hManager;
			Hflag = flag;
			checkWait = false;
			if (Disposable != null)
			{
				Disposable.Dispose();
				Disposable = null;
			}
			Disposable = Observable.EveryUpdate().Subscribe(delegate
			{
				SolverActiveChangeProc();
			});
		}
	}

	public void SetChara(ChaControl[] male, ChaControl[] female, bool SecondChaSet)
	{
		if (male[0].objBody != null && obiFluidCtrlMale[0] == null)
		{
			obiFluidCtrlMale[0] = obiManager.Setup(male[0], 0);
		}
		if (female[0].objBody != null && obiFluidCtrlFemale[0] == null)
		{
			obiFluidCtrlFemale[0] = obiManager.Setup(female[0], 1);
			ShapeFemale = female[0];
		}
		if (SecondChaSet)
		{
			if (male[1] != null && male[1].objBody != null && male[1].visibleAll && obiFluidCtrlMale[1] == null)
			{
				obiFluidCtrlMale[1] = obiManager.Setup(male[1], 0);
			}
			if (female[1] != null && female[1].objBody != null && female[1].visibleAll && obiFluidCtrlFemale[1] == null)
			{
				obiFluidCtrlFemale[1] = obiManager.Setup(female[1], 1);
			}
		}
		AssetBundleManager.UnloadAssetBundle("list/h/siruobi/30.unity3d", isUnloadForceRefCount: true);
	}

	public void InitSetting(string lstName, GameObject item)
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(hManager.strAssetObiListFolder);
		string ret = "";
		GetFileName(lstName, ref ret);
		siruInfos = new Dictionary<int, Dictionary<int, SiruObiInfo>>();
		siruInfoObjs = null;
		if (ret == "")
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
			if (!GlobalMethod.AssetFileExist(assetBundleNameListFromPath[index], ret))
			{
				continue;
			}
			excelData = CommonLib.LoadAsset<ExcelData>(assetBundleNameListFromPath[index], ret);
			AssetBundleManager.UnloadAssetBundle(assetBundleNameListFromPath[index], isUnloadForceRefCount: true);
			if (excelData == null)
			{
				continue;
			}
			int num = 3;
			while (num < excelData.MaxCell)
			{
				row = excelData.list[num++].list;
				int num2 = 0;
				string text = row[num2++];
				if (text.IsNullOrEmpty())
				{
					continue;
				}
				int num3 = -1;
				num3 = int.Parse(text);
				int num4 = 0;
				num4 = int.Parse(row[num2++]);
				int playParticleID = -1;
				text = row[num2++];
				if (!text.IsNullOrEmpty())
				{
					playParticleID = int.Parse(text);
				}
				string parentName = "";
				Vector3[,] array = new Vector3[2, 3]
				{
					{
						Vector3.zero,
						Vector3.zero,
						Vector3.zero
					},
					{
						Vector3.zero,
						Vector3.zero,
						Vector3.zero
					}
				};
				Vector3[,] array2 = new Vector3[2, 3]
				{
					{
						Vector3.zero,
						Vector3.zero,
						Vector3.zero
					},
					{
						Vector3.zero,
						Vector3.zero,
						Vector3.zero
					}
				};
				if (num3 < 0)
				{
					parentName = row[num2++];
				}
				else
				{
					num2++;
				}
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						if (!float.TryParse(row[num2++], out array[j, k].x))
						{
							array[j, k].x = 0f;
						}
						if (!float.TryParse(row[num2++], out array[j, k].y))
						{
							array[j, k].y = 0f;
						}
						if (!float.TryParse(row[num2++], out array[j, k].z))
						{
							array[j, k].z = 0f;
						}
						if (!float.TryParse(row[num2++], out array2[j, k].x))
						{
							array2[j, k].x = 0f;
						}
						if (!float.TryParse(row[num2++], out array2[j, k].y))
						{
							array2[j, k].y = 0f;
						}
						if (!float.TryParse(row[num2++], out array2[j, k].z))
						{
							array2[j, k].z = 0f;
						}
					}
				}
				SiruObiParamInfo[] array3 = new SiruObiParamInfo[2]
				{
					new SiruObiParamInfo(),
					new SiruObiParamInfo()
				};
				for (int l = 0; l < 2; l++)
				{
					array3[l].EmitterPtn.assetbundle = row[num2++];
					array3[l].EmitterPtn.asset = row[num2++];
					array3[l].EmitterPtn.manifest = row[num2++];
				}
				for (int m = 0; m < 2; m++)
				{
					array3[m].FluidParam.assetbundle = row[num2++];
					array3[m].FluidParam.asset = row[num2++];
					array3[m].FluidParam.manifest = row[num2++];
				}
				for (int n = 0; n < 2; n++)
				{
					array3[n].ColParam.assetbundle = row[num2++];
					array3[n].ColParam.asset = row[num2++];
					array3[n].ColParam.manifest = row[num2++];
				}
				for (int num5 = 0; num5 < 2; num5++)
				{
					for (int num6 = 0; num6 < 3; num6++)
					{
						if (!float.TryParse(row[num2++], out array3[num5].Speed[num6]))
						{
							array3[num5].Speed[num6] = -1f;
						}
						if (!float.TryParse(row[num2++], out array3[num5].LifeSpan[num6]))
						{
							array3[num5].LifeSpan[num6] = -1f;
						}
						if (!float.TryParse(row[num2++], out array3[num5].RandVel[num6]))
						{
							array3[num5].RandVel[num6] = -1f;
						}
						if (!int.TryParse(row[num2++], out array3[num5].NumParticle[num6]))
						{
							array3[num5].NumParticle[num6] = -1;
						}
						if (!float.TryParse(row[num2++], out array3[num5].ShapeRadius[num6]))
						{
							array3[num5].ShapeRadius[num6] = -1f;
						}
						if (!float.TryParse(row[num2++], out array3[num5].ParticleRadius[num6]))
						{
							array3[num5].ParticleRadius[num6] = -1f;
						}
					}
				}
				if (!siruInfos.ContainsKey(num3))
				{
					siruInfos.Add(num3, new Dictionary<int, SiruObiInfo>());
				}
				if (!siruInfos[num3].ContainsKey(num4))
				{
					siruInfos[num3].Add(num4, new SiruObiInfo());
				}
				siruInfos[num3][num4].PlayParticleID = playParticleID;
				siruInfos[num3][num4].ParentName = parentName;
				siruInfos[num3][num4].LoadOffsetPos = array;
				siruInfos[num3][num4].LoadOffsetRot = array2;
				siruInfos[num3][num4].SetInfo(0, array3[0], ShapeFemale);
				siruInfos[num3][num4].SetInfo(1, array3[1], ShapeFemale);
				while (row.Count > num2)
				{
					text = row[num2++];
					if (text == "")
					{
						break;
					}
					ShootInfo item2 = new ShootInfo
					{
						Inside = (text == "1"),
						Anim = row[num2++],
						NormalizeTime = float.Parse(row[num2++]),
						CalcOldTime = 0f
					};
					siruInfos[num3][num4].ShootInfo.Add(item2);
				}
			}
		}
		ObiEmitterCtrl obiEmitterCtrl = null;
		foreach (KeyValuePair<int, Dictionary<int, SiruObiInfo>> siruInfo in siruInfos)
		{
			if (siruInfo.Key == -1)
			{
				continue;
			}
			if (siruInfo.Key % 2 == 0)
			{
				if (obiFluidCtrlMale[siruInfo.Key / 2] == null)
				{
					continue;
				}
				foreach (KeyValuePair<int, SiruObiInfo> item3 in siruInfo.Value)
				{
					obiEmitterCtrl = obiFluidCtrlMale[siruInfo.Key / 2].ObiEmitterCtrls[item3.Key];
					obiEmitterCtrl.Setup(item3.Value.Info[0].SetupInfo);
					if (!(item3.Value.Info[0].EmitterPtn.assetbundle.IsNullOrEmpty() | item3.Value.Info[0].EmitterPtn.asset.IsNullOrEmpty()))
					{
						obiEmitterCtrl.LoadFile(item3.Value.Info[0].EmitterPtn.assetbundle, item3.Value.Info[0].EmitterPtn.asset, item3.Value.Info[0].EmitterPtn.manifest);
						SetOffset(obiEmitterCtrl, item3.Value.OffsetPos[0], item3.Value.OffsetRot[0]);
					}
				}
			}
			else
			{
				if (obiFluidCtrlFemale[siruInfo.Key / 2] == null)
				{
					continue;
				}
				foreach (KeyValuePair<int, SiruObiInfo> item4 in siruInfo.Value)
				{
					obiEmitterCtrl = obiFluidCtrlFemale[siruInfo.Key / 2].ObiEmitterCtrls[item4.Key];
					obiEmitterCtrl.Setup(item4.Value.Info[0].SetupInfo);
					if (!(item4.Value.Info[0].EmitterPtn.assetbundle.IsNullOrEmpty() | item4.Value.Info[0].EmitterPtn.asset.IsNullOrEmpty()))
					{
						obiEmitterCtrl.LoadFile(item4.Value.Info[0].EmitterPtn.assetbundle, item4.Value.Info[0].EmitterPtn.asset, item4.Value.Info[0].EmitterPtn.manifest);
						SetOffset(obiEmitterCtrl, item4.Value.OffsetPos[0], item4.Value.OffsetRot[0]);
					}
				}
			}
		}
		if (siruInfos.ContainsKey(-1) && siruInfos[-1].Count > 0)
		{
			ObjSet(siruInfos[-1], item);
		}
		excelData = null;
		row = null;
	}

	public void ChangeSetupInfo(int state)
	{
		ObiEmitterCtrl obiEmitterCtrl = null;
		foreach (KeyValuePair<int, Dictionary<int, SiruObiInfo>> siruInfo in siruInfos)
		{
			if (siruInfo.Key >= 0)
			{
				if (siruInfo.Key % 2 == 0)
				{
					if (obiFluidCtrlMale[siruInfo.Key / 2] == null)
					{
						continue;
					}
					foreach (KeyValuePair<int, SiruObiInfo> item in siruInfo.Value)
					{
						if (!(item.Value.Info[state].EmitterPtn.assetbundle.IsNullOrEmpty() | item.Value.Info[state].EmitterPtn.asset.IsNullOrEmpty() | item.Value.Info[state].EmitterPtn.manifest.IsNullOrEmpty()))
						{
							obiEmitterCtrl = obiFluidCtrlMale[siruInfo.Key / 2].ObiEmitterCtrls[item.Key];
							obiEmitterCtrl.Setup(item.Value.Info[state].SetupInfo);
							obiEmitterCtrl.LoadFile(item.Value.Info[state].EmitterPtn.assetbundle, item.Value.Info[state].EmitterPtn.asset, item.Value.Info[state].EmitterPtn.manifest);
							item.Value.nowState = state;
							SetOffset(obiEmitterCtrl, item.Value.OffsetPos[state], item.Value.OffsetRot[state]);
						}
					}
				}
				else
				{
					if (siruInfo.Key % 2 != 1 || obiFluidCtrlFemale[siruInfo.Key / 2] == null)
					{
						continue;
					}
					foreach (KeyValuePair<int, SiruObiInfo> item2 in siruInfo.Value)
					{
						if (!(item2.Value.Info[state].EmitterPtn.assetbundle.IsNullOrEmpty() | item2.Value.Info[state].EmitterPtn.asset.IsNullOrEmpty() | item2.Value.Info[state].EmitterPtn.manifest.IsNullOrEmpty()))
						{
							obiEmitterCtrl = obiFluidCtrlFemale[siruInfo.Key / 2].ObiEmitterCtrls[item2.Key];
							obiEmitterCtrl.Setup(item2.Value.Info[state].SetupInfo);
							obiEmitterCtrl.LoadFile(item2.Value.Info[state].EmitterPtn.assetbundle, item2.Value.Info[state].EmitterPtn.asset, item2.Value.Info[state].EmitterPtn.manifest);
							item2.Value.nowState = state;
							SetOffset(obiEmitterCtrl, item2.Value.OffsetPos[state], item2.Value.OffsetRot[state]);
						}
					}
				}
				continue;
			}
			foreach (KeyValuePair<int, SiruObiInfo> item3 in siruInfo.Value)
			{
				if (!(item3.Value.Info[state].EmitterPtn.assetbundle.IsNullOrEmpty() | item3.Value.Info[state].EmitterPtn.asset.IsNullOrEmpty() | item3.Value.Info[state].EmitterPtn.manifest.IsNullOrEmpty()))
				{
					obiEmitterCtrl = obiFluidCtrlObj.ObiEmitterCtrls[item3.Key];
					obiEmitterCtrl.Setup(item3.Value.Info[state].SetupInfo);
					obiEmitterCtrl.LoadFile(item3.Value.Info[state].EmitterPtn.assetbundle, item3.Value.Info[state].EmitterPtn.asset, item3.Value.Info[state].EmitterPtn.manifest);
					item3.Value.nowState = state;
					SetOffset(obiEmitterCtrl, item3.Value.OffsetPos[state], item3.Value.OffsetRot[state]);
				}
			}
		}
	}

	private void ObjSet(Dictionary<int, SiruObiInfo> infos, GameObject item)
	{
		siruInfoObjs = new ObiFluidManager.AddTargetParam[infos.Count];
		for (int i = 0; i < siruInfoObjs.Length; i++)
		{
			int num = i;
			Transform transform = item.transform.FindLoop(infos[num].ParentName);
			if (!(transform == null))
			{
				siruInfoObjs[num] = new ObiFluidManager.AddTargetParam(transform, infos[num].OffsetPos[0], infos[num].OffsetRot[0], infos[num].Info[0].SetupInfo);
			}
		}
		obiFluidCtrlObj = obiManager.Add(siruInfoObjs);
		for (int j = 0; j < obiFluidCtrlObj.ObiEmitterCtrls.Length; j++)
		{
			int num2 = j;
			obiFluidCtrlObj.ObiEmitterCtrls[num2].LoadFile(infos[num2].Info[0].EmitterPtn.assetbundle, infos[num2].Info[0].EmitterPtn.asset, infos[num2].Info[0].EmitterPtn.manifest);
		}
	}

	public void Proc(AnimatorStateInfo _stateInfo, bool _isInside = false)
	{
		if (Singleton<HSceneFlagCtrl>.Instance.semenType == 2 || siruInfos == null || ShapeFemale == null)
		{
			return;
		}
		float shapeBodyValue = ShapeFemale.GetShapeBodyValue(0);
		for (int i = 0; i < obiFluidCtrlMale.Length; i++)
		{
			if (obiFluidCtrlMale[i] == null)
			{
				continue;
			}
			int key = 0;
			if (i != 0)
			{
				key = 2;
			}
			if (!siruInfos.ContainsKey(key))
			{
				continue;
			}
			for (int j = 0; j < obiFluidCtrlMale[i].ObiEmitterCtrls.Length; j++)
			{
				int num = j;
				if (!siruInfos[key].ContainsKey(num))
				{
					continue;
				}
				int playParticleID = siruInfos[key][num].PlayParticleID;
				if ((Singleton<HSceneFlagCtrl>.Instance.semenType == 1 && playParticleID < 0) || !IsAnimation(_isInside, _stateInfo, siruInfos[key][num]))
				{
					continue;
				}
				if (Singleton<HSceneFlagCtrl>.Instance.semenType == 1)
				{
					if (particle != null)
					{
						particle.SiruPlay(playParticleID);
					}
				}
				else if (obiFluidCtrlMale[i].ObiEmitterCtrls[num] != null)
				{
					if (!solver.gameObject.activeSelf)
					{
						solver.gameObject.SetActive(value: true);
						checkWait = true;
					}
					obiFluidCtrlMale[i].ObiEmitterCtrls[num].Play(-1, shapeBodyValue);
				}
			}
		}
		for (int k = 0; k < obiFluidCtrlFemale.Length; k++)
		{
			if (obiFluidCtrlFemale[k] == null)
			{
				continue;
			}
			int key2 = 1;
			if (k != 0)
			{
				key2 = 3;
			}
			if (!siruInfos.ContainsKey(key2))
			{
				continue;
			}
			for (int l = 0; l < obiFluidCtrlFemale[k].ObiEmitterCtrls.Length; l++)
			{
				int num2 = l;
				if (Hflag.UrineIDs.Contains(num2) || !siruInfos[key2].ContainsKey(num2))
				{
					continue;
				}
				int playParticleID2 = siruInfos[key2][num2].PlayParticleID;
				if ((Singleton<HSceneFlagCtrl>.Instance.semenType == 1 && playParticleID2 < 0) || !IsAnimation(_isInside, _stateInfo, siruInfos[key2][num2]))
				{
					continue;
				}
				if (Singleton<HSceneFlagCtrl>.Instance.semenType == 1)
				{
					if (particle != null)
					{
						particle.SiruPlay(playParticleID2);
					}
				}
				else if (obiFluidCtrlFemale[k].ObiEmitterCtrls[num2] != null)
				{
					if (!solver.gameObject.activeSelf)
					{
						solver.gameObject.SetActive(value: true);
						checkWait = true;
					}
					obiFluidCtrlFemale[k].ObiEmitterCtrls[num2].Play(-1, shapeBodyValue);
				}
			}
		}
		if (!siruInfos.ContainsKey(-1))
		{
			return;
		}
		for (int m = 0; m < obiFluidCtrlObj.ObiEmitterCtrls.Length; m++)
		{
			int num3 = m;
			int key3 = -1;
			if (!siruInfos[key3].ContainsKey(num3))
			{
				continue;
			}
			int playParticleID3 = siruInfos[key3][num3].PlayParticleID;
			if ((Singleton<HSceneFlagCtrl>.Instance.semenType == 1 && playParticleID3 < 0) || !IsAnimation(_isInside, _stateInfo, siruInfos[key3][num3]))
			{
				continue;
			}
			if (Singleton<HSceneFlagCtrl>.Instance.semenType == 1)
			{
				if (particle != null)
				{
					particle.SiruPlay(playParticleID3);
				}
			}
			else if (obiFluidCtrlObj.ObiEmitterCtrls[num3] != null)
			{
				if (!solver.gameObject.activeSelf)
				{
					solver.gameObject.SetActive(value: true);
					checkWait = true;
				}
				obiFluidCtrlObj.ObiEmitterCtrls[num3].Play(-1, shapeBodyValue);
			}
		}
	}

	public void PlayUrine(bool use, int num = 0)
	{
		if (!use)
		{
			CanUrinePloc[0] = use;
			CanUrinePloc[1] = use;
		}
		else
		{
			CanUrinePloc[num] = use;
		}
	}

	public void PlayUrine(AnimatorStateInfo _stateInfo, int num = 0)
	{
		if (!CanUrinePloc[num])
		{
			return;
		}
		float shapeBodyValue = ShapeFemale.GetShapeBodyValue(0);
		if (obiFluidCtrlFemale[num] == null)
		{
			return;
		}
		int key = 1;
		if (num != 0)
		{
			key = 3;
		}
		if (!siruInfos.ContainsKey(key))
		{
			return;
		}
		for (int i = 0; i < Hflag.UrineIDs.Count; i++)
		{
			int num2 = Hflag.UrineIDs[i];
			if (siruInfos[key].ContainsKey(num2) && IsAnimation(_isInside: false, _stateInfo, siruInfos[key][num2]) && obiFluidCtrlFemale[num].ObiEmitterCtrls[num2] != null)
			{
				if (!solver.gameObject.activeSelf)
				{
					solver.gameObject.SetActive(value: true);
					checkWait = true;
				}
				obiFluidCtrlFemale[num].ObiEmitterCtrls[num2].Play(-1, shapeBodyValue);
			}
		}
	}

	private bool IsAnimation(bool _isInside, AnimatorStateInfo _stateInfo, SiruObiInfo info)
	{
		for (int i = 0; i < info.ShootInfo.Count; i++)
		{
			ShootInfo value = info.ShootInfo[i];
			if (!_stateInfo.IsName(value.Anim))
			{
				value.CalcOldTime = 0f;
				info.ShootInfo[i] = value;
			}
			else if (!value.Inside || _isInside)
			{
				if (!(value.CalcOldTime > value.NormalizeTime) && !(value.NormalizeTime >= _stateInfo.normalizedTime))
				{
					value.CalcOldTime = _stateInfo.normalizedTime;
					info.ShootInfo[i] = value;
					return true;
				}
				value.CalcOldTime = _stateInfo.normalizedTime;
				info.ShootInfo[i] = value;
			}
		}
		return false;
	}

	private void GetFileName(string fileFemale, ref string ret)
	{
		if (GlobalMethod.StartsWith(fileFemale, "ai"))
		{
			if (GlobalMethod.StartsWith(fileFemale, "ai3p"))
			{
				ret = fileFemale.Remove(4, 3);
			}
			else if (fileFemale.ContainsAny("_f1_", "_f2_"))
			{
				ret = fileFemale.Remove(3, 3);
			}
			else
			{
				ret = fileFemale.Remove(3, 2);
			}
		}
		else
		{
			if (!GlobalMethod.StartsWith(fileFemale, "h2"))
			{
				return;
			}
			if (GlobalMethod.StartsWith(fileFemale, "h2_"))
			{
				if (fileFemale.ContainsAny("_f1_", "_f2_"))
				{
					ret = fileFemale.Remove(6, 3);
				}
				else
				{
					ret = fileFemale.Remove(6, 2);
				}
			}
			else
			{
				ret = fileFemale.Remove(3, 2);
			}
		}
	}

	public void SetParticle(HParticleCtrl _particle)
	{
		particle = _particle;
	}

	public static void RemoveObiCollider(GameObject HitTop)
	{
		if (HitTop == null)
		{
			return;
		}
		ObiCollider[] componentsInChildren = HitTop.GetComponentsInChildren<ObiCollider>(includeInactive: true);
		if (!((IReadOnlyCollection<ObiCollider>)(object)componentsInChildren).IsNullOrEmpty())
		{
			ObiCollider[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RemoveCollider();
			}
		}
	}

	private void SetOffset(ObiEmitterCtrl emitter, Vector3 pos, Vector3 rot)
	{
		emitter.transform.localPosition = pos;
		emitter.transform.localRotation = Quaternion.Euler(rot);
	}

	private void SolverActiveChangeProc()
	{
		if (solver == null || !solver.gameObject.activeSelf)
		{
			return;
		}
		if (checkWait)
		{
			checkWait = false;
			return;
		}
		foreach (ObiActor actor in solver.actors)
		{
			ObiEmitter component = actor.GetComponent<ObiEmitter>();
			if (component.playMode != ObiEmitter.PlayMode.Stop || component.ActiveParticles != 0)
			{
				return;
			}
		}
		solver.gameObject.SetActive(value: false);
	}

	public void EndPloc()
	{
		obiManager = null;
		solver = null;
		hManager = null;
		Hflag = null;
	}

	public void EndPlocSolver()
	{
		obiFluidCtrlMale[0] = null;
		obiFluidCtrlMale[1] = null;
		obiFluidCtrlFemale[0] = null;
		obiFluidCtrlFemale[1] = null;
		if (Disposable != null)
		{
			Disposable.Dispose();
			Disposable = null;
		}
		checkWait = false;
		if (!solver.gameObject.activeSelf)
		{
			solver.gameObject.SetActive(value: true);
		}
	}
}
