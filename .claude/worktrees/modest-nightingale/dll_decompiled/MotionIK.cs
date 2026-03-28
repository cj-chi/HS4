using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion;
using Illusion.Component.Correct;
using Illusion.Component.Correct.Process;
using Illusion.Extensions;
using IllusionUtility.GetUtility;
using RootMotion.FinalIK;
using UnityEngine;

public class MotionIK
{
	public class IKTargetPair
	{
		public enum IKTarget
		{
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot
		}

		public IKEffector effector { get; }

		public IKConstraintBend bend { get; }

		public static int IKTargetLength { get; } = Utils.Enum<IKTarget>.Length * 2;

		public static IKTargetPair[] GetPairs(IKSolverFullBodyBiped solver)
		{
			return (from index in Enumerable.Range(0, Utils.Enum<IKTarget>.Length)
				select new IKTargetPair((IKTarget)index, solver)).ToArray();
		}

		public IKTargetPair(IKTarget target, IKSolverFullBodyBiped solver)
		{
			switch (target)
			{
			case IKTarget.LeftHand:
				effector = solver.leftHandEffector;
				bend = solver.leftArmChain.bendConstraint;
				break;
			case IKTarget.RightHand:
				effector = solver.rightHandEffector;
				bend = solver.rightArmChain.bendConstraint;
				break;
			case IKTarget.LeftFoot:
				effector = solver.leftFootEffector;
				bend = solver.leftLegChain.bendConstraint;
				break;
			case IKTarget.RightFoot:
				effector = solver.rightFootEffector;
				bend = solver.rightLegChain.bendConstraint;
				break;
			}
		}
	}

	public MotionIKDataBinder binder { get; }

	public ChaControl info { get; private set; }

	public List<GameObject> items { get; } = new List<GameObject>();

	public FullBodyBipedIK ik { get; private set; }

	public MotionIK[] partners { get; private set; }

	public IKTargetPair[] ikTargetPairs
	{
		get
		{
			if (!(ik == null))
			{
				return IKTargetPair.GetPairs(ik.solver);
			}
			return null;
		}
	}

	public MotionIKData data { get; private set; }

	public FrameCorrect frameCorrect { get; private set; }

	public IKCorrect ikCorrect { get; private set; }

	public bool enabled
	{
		get
		{
			if (ik != null)
			{
				return ik.enabled;
			}
			return false;
		}
		set
		{
			if (!(ik == null))
			{
				ik.enabled = value;
				ikCorrect.isEnabled = value;
			}
		}
	}

	public string[] stateNames
	{
		get
		{
			if (data.states != null)
			{
				return data.states.Select((MotionIKData.State p) => p.name).ToArray();
			}
			return new string[0];
		}
	}

	public bool isBlend => data.isBlend;

	private Transform[] DefBone { get; }

	private Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>[] calcBlend { get; } = new Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>[4]
	{
		new Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>(),
		new Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>(),
		new Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>(),
		new Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>>()
	};

	private Dictionary<int, List<MotionIKData.BlendWeightInfo>>[] calcBlendBend { get; } = new Dictionary<int, List<MotionIKData.BlendWeightInfo>>[4]
	{
		new Dictionary<int, List<MotionIKData.BlendWeightInfo>>(),
		new Dictionary<int, List<MotionIKData.BlendWeightInfo>>(),
		new Dictionary<int, List<MotionIKData.BlendWeightInfo>>(),
		new Dictionary<int, List<MotionIKData.BlendWeightInfo>>()
	};

	public static List<MotionIK> Setup(List<ChaControl> infos)
	{
		List<MotionIK> ret = (from i in Enumerable.Range(0, infos.Count)
			select new MotionIK(infos[i])).ToList();
		ret.ForEach(delegate(MotionIK p)
		{
			p.SetPartners(ret);
		});
		return ret;
	}

	public static Vector3 GetShapeLerpPositionValue(float shape, Vector3 min, Vector3 med, Vector3 max)
	{
		if (!(shape >= 0.5f))
		{
			return Vector3.Lerp(min, med, Mathf.InverseLerp(0f, 0.5f, shape));
		}
		return Vector3.Lerp(med, max, Mathf.InverseLerp(0.5f, 1f, shape));
	}

	public static Vector3 GetShapeLerpPositionValue(float shape, Vector3 min, Vector3 max)
	{
		if (!(shape >= 0.5f))
		{
			return Vector3.Lerp(min, Vector3.zero, Mathf.InverseLerp(0f, 0.5f, shape));
		}
		return Vector3.Lerp(Vector3.zero, max, Mathf.InverseLerp(0.5f, 1f, shape));
	}

	public static Vector3 GetShapeLerpAngleValue(float shape, Vector3 min, Vector3 med, Vector3 max)
	{
		Vector3 zero = Vector3.zero;
		if (shape >= 0.5f)
		{
			float t = Mathf.InverseLerp(0.5f, 1f, shape);
			for (int i = 0; i < 3; i++)
			{
				zero[i] = Mathf.LerpAngle(med[i], max[i], t);
			}
		}
		else
		{
			float t2 = Mathf.InverseLerp(0f, 0.5f, shape);
			for (int j = 0; j < 3; j++)
			{
				zero[j] = Mathf.LerpAngle(min[j], med[j], t2);
			}
		}
		return zero;
	}

	public static Vector3 GetShapeLerpAngleValue(float shape, Vector3 min, Vector3 max)
	{
		Vector3 zero = Vector3.zero;
		if (shape >= 0.5f)
		{
			float t = Mathf.InverseLerp(0.5f, 1f, shape);
			for (int i = 0; i < 3; i++)
			{
				zero[i] = Mathf.LerpAngle(0f, max[i], t);
			}
		}
		else
		{
			float t2 = Mathf.InverseLerp(0f, 0.5f, shape);
			for (int j = 0; j < 3; j++)
			{
				zero[j] = Mathf.LerpAngle(min[j], 0f, t2);
			}
		}
		return zero;
	}

	public MotionIK(ChaControl info, MotionIKData data = null)
	{
		this.info = info;
		binder = info.GetOrAddComponent<MotionIKDataBinder>();
		binder.motionIK = this;
		if (data != null)
		{
			binder.data = data;
		}
		else
		{
			MotionIKDataBinder motionIKDataBinder = binder;
			MotionIKData obj = binder.data ?? new MotionIKData();
			data = obj;
			motionIKDataBinder.data = obj;
		}
		this.data = data;
		Animator animBody = info.animBody;
		ik = animBody.GetComponent<FullBodyBipedIK>();
		if (ik != null)
		{
			frameCorrect = animBody.GetComponent<FrameCorrect>();
			ikCorrect = animBody.GetComponent<IKCorrect>();
		}
		SetPartners();
		Reset();
		List<Transform> list = new List<Transform>();
		animBody.transform.FindLoopPrefix(list, "f_pv_");
		DefBone = new Transform[8]
		{
			list.Find((Transform item) => item.name == "f_pv_arm_L"),
			list.Find((Transform item) => item.name == "f_pv_elbo_L"),
			list.Find((Transform item) => item.name == "f_pv_arm_R"),
			list.Find((Transform item) => item.name == "f_pv_elbo_R"),
			list.Find((Transform item) => item.name == "f_pv_leg_L"),
			list.Find((Transform item) => item.name == "f_pv_knee_L"),
			list.Find((Transform item) => item.name == "f_pv_leg_R"),
			list.Find((Transform item) => item.name == "f_pv_knee_R")
		};
	}

	public void SetPartners(params MotionIK[] partners)
	{
		this.partners = new MotionIK[1] { this }.Concat(partners.Where((MotionIK p) => p != this)).ToArray();
	}

	public void SetPartners(IEnumerable<MotionIK> partners)
	{
		SetPartners(partners?.ToArray());
	}

	public void SetPartners(IReadOnlyCollection<(int, int, MotionIK)> partners)
	{
		SetPartners(partners?.Select(((int, int, MotionIK) s) => s.Item3));
	}

	public void SetItems(GameObject[] items)
	{
		MotionIK[] array = partners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].items.AddRange(items);
		}
	}

	public void Reset()
	{
		InitFrameCalc();
		enabled = false;
	}

	public void Release()
	{
		data.Release();
	}

	public bool LoadData(TextAsset ta)
	{
		return data.Read(ta);
	}

	public bool LoadData(string path)
	{
		return data.Read(path);
	}

	public void SetData(MotionIKData data)
	{
		MotionIKDataBinder motionIKDataBinder = binder;
		MotionIKData obj = data ?? new MotionIKData();
		MotionIKData motionIKData = obj;
		this.data = obj;
		motionIKDataBinder.data = motionIKData;
	}

	public void InitFrameCalc()
	{
		if (frameCorrect != null)
		{
			foreach (BaseCorrect.Info item in frameCorrect.list)
			{
				item.enabled = false;
				item.pos = Vector3.zero;
				item.ang = Vector3.zero;
			}
		}
		if (!(ikCorrect != null))
		{
			return;
		}
		foreach (BaseCorrect.Info item2 in ikCorrect.list)
		{
			item2.enabled = false;
			item2.pos = Vector3.zero;
			item2.ang = Vector3.zero;
			item2.bone = null;
		}
	}

	public MotionIKData.State InitState(string stateName)
	{
		return data.InitState(stateName);
	}

	public MotionIKData.State GetNowState(string stateName)
	{
		return GetNowState(Animator.StringToHash(stateName));
	}

	public MotionIKData.State GetNowState(int stateNameHash)
	{
		int num = data?.states?.Check((MotionIKData.State state) => state.nameHash == stateNameHash) ?? (-1);
		if (num != -1)
		{
			return data.states[num];
		}
		return null;
	}

	public MotionIKData.Frame[] GetNowFrames(string stateName)
	{
		return GetNowState(stateName)?.frames;
	}

	public void Calc(string stateName)
	{
		Calc(Animator.StringToHash(stateName));
	}

	public void Calc(int hashName)
	{
		if (frameCorrect == null)
		{
			return;
		}
		InitFrameCalc();
		MotionIKData.State nowState = GetNowState(hashName);
		binder.state = nowState;
		if (nowState != null)
		{
			int iKTargetLength = IKTargetPair.IKTargetLength;
			MotionIKData.Frame[] frames = nowState.frames;
			foreach (MotionIKData.Frame frame in frames)
			{
				int num = frame.frameNo - iKTargetLength;
				if (num >= 0)
				{
					BaseCorrect.Info obj = frameCorrect.list[num];
					obj.enabled = true;
					Vector3[] correctShapeValues = GetCorrectShapeValues(isBlend, partners[frame.editNo].info, frame.shapes);
					obj.pos = correctShapeValues[0];
					obj.ang = correctShapeValues[1];
				}
			}
		}
		enabled = nowState != null || isBlend;
		if (nowState != null)
		{
			foreach (var item in ikTargetPairs.Select((IKTargetPair target, int index) => new { target, index }))
			{
				LinkIK(item.index, nowState, item.target);
			}
			return;
		}
		if (!isBlend)
		{
			return;
		}
		foreach (var item2 in ikTargetPairs.Select((IKTargetPair target, int index) => new { target, index }))
		{
			BaseData component = item2.target.effector.target.GetComponent<BaseData>();
			if (component.bone == null)
			{
				component.bone = DefBone[item2.index * 2];
				component.GetComponent<BaseProcess>().enabled = true;
				item2.target.effector.positionWeight = 1f;
				item2.target.effector.rotationWeight = 1f;
			}
			BaseData component2 = item2.target.bend.bendGoal.GetComponent<BaseData>();
			if (component2.bone == null)
			{
				component2.bone = DefBone[item2.index * 2 + 1];
				component2.GetComponent<BaseProcess>().enabled = true;
				item2.target.bend.weight = 1f;
			}
		}
	}

	private static Vector3[] GetCorrectShapeValues(bool isBlend, ChaControl chara, MotionIKData.Shape[] shapes)
	{
		Vector3[] array = new Vector3[2]
		{
			Vector3.zero,
			Vector3.zero
		};
		if (isBlend)
		{
			MotionIKData.Shape[] array2 = shapes;
			foreach (MotionIKData.Shape shape in array2)
			{
				float shapeBodyValue = chara.GetShapeBodyValue(shape.shapeNo);
				for (int j = 0; j < array.Length; j++)
				{
					if (j == 0)
					{
						array[j] += GetShapeLerpPositionValue(shapeBodyValue, shape.small[j], shape.large[j]);
					}
					else
					{
						array[j] += GetShapeLerpAngleValue(shapeBodyValue, shape.small[j], shape.large[j]);
					}
				}
			}
		}
		else
		{
			MotionIKData.Shape[] array2 = shapes;
			foreach (MotionIKData.Shape shape2 in array2)
			{
				float shapeBodyValue2 = chara.GetShapeBodyValue(shape2.shapeNo);
				for (int k = 0; k < array.Length; k++)
				{
					if (k == 0)
					{
						array[k] += GetShapeLerpPositionValue(shapeBodyValue2, shape2.small[k], shape2.mediam[k], shape2.large[k]);
					}
					else
					{
						array[k] += GetShapeLerpAngleValue(shapeBodyValue2, shape2.small[k], shape2.mediam[k], shape2.large[k]);
					}
				}
			}
		}
		return array;
	}

	private void LinkIK(int index, MotionIKData.State state, IKTargetPair pair)
	{
		MotionIKData.Parts parts = state?[index];
		MotionIKData.Param2 param = parts?.param2;
		IKEffector effector = pair.effector;
		SetDataParam(param, effector.target.GetComponent<BaseData>(), out var boneTarget);
		if (boneTarget == null || param == null)
		{
			effector.positionWeight = 0f;
			effector.rotationWeight = 0f;
		}
		else
		{
			effector.positionWeight = param.weightPos;
			effector.rotationWeight = param.weightAng;
		}
		MotionIKData.Param3 param2 = parts?.param3;
		IKConstraintBend bend = pair.bend;
		SetDataParam3(param2, bend.bendGoal.GetComponent<BaseData>(), out var boneTarget2);
		if (boneTarget2 == null || param2 == null)
		{
			bend.weight = 0f;
		}
		else
		{
			bend.weight = param2.weight;
		}
		MotionIKData.Frame FindFrame(int no)
		{
			return state?.frames?.FirstOrDefault((MotionIKData.Frame p) => p.frameNo == no);
		}
		Transform GetTarget(int sex, string frameName)
		{
			if (frameName.IsNullOrEmpty())
			{
				return null;
			}
			return ((sex < partners.Length) ? partners[sex].info.GetComponentsInChildren<Transform>() : partners[0].items[sex - partners.Length].GetComponentsInChildren<Transform>()).FirstOrDefault((Transform p) => p.name == frameName);
		}
		void SetDataFrame(MotionIKData.Frame frame, BaseData data)
		{
			if (frame == null)
			{
				data.pos = Vector3.zero;
				data.rot = Quaternion.identity;
			}
			else
			{
				Set(data, GetCorrectShapeValues(isBlend: false, partners[frame.editNo].info, frame.shapes));
			}
		}
		void SetDataParam(MotionIKData.Param2 param3, BaseData data, out Transform reference)
		{
			reference = GetTarget(param3?.sex ?? 0, param3?.target);
			if (!(data == null))
			{
				data.bone = reference;
				BaseProcess component = data.GetComponent<BaseProcess>();
				if (component != null)
				{
					component.enabled = reference != null;
				}
				if (!isBlend)
				{
					SetDataFrame(FindFrame(index * 2), data);
				}
				else
				{
					Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>> dictionary = calcBlend[index];
					for (int i = 0; i < parts.param2.blendInfos.Length; i++)
					{
						if (!dictionary.TryGetValue(i, out var value))
						{
							value = (dictionary[i] = new Dictionary<int, List<MotionIKData.BlendWeightInfo>>());
						}
						CalcBlend(value, parts.param2.blendInfos[i]);
					}
					Set(data, GetCorrectShapeValues(partners[0].info, 0f, dictionary));
				}
			}
		}
		void SetDataParam3(MotionIKData.Param3 param3, BaseData data, out Transform reference)
		{
			reference = GetTarget(0, param3?.chein);
			if (!(data == null))
			{
				data.bone = reference;
				BaseProcess component = data.GetComponent<BaseProcess>();
				if (component != null)
				{
					component.enabled = reference != null;
				}
				if (!isBlend)
				{
					SetDataFrame(FindFrame(index * 2 + 1), data);
				}
				else
				{
					Dictionary<int, List<MotionIKData.BlendWeightInfo>> target = calcBlendBend[index];
					CalcBlend(target, parts.param3.blendInfos);
					Set(data, GetCorrectShapeValues(partners[0].info, 0f, target));
				}
			}
		}
	}

	public void ChangeWeight(int nameHash, float normalizedTime)
	{
		if (data?.states?.Any((MotionIKData.State item) => item.nameHash == nameHash) ?? false)
		{
			ChaControl chara = partners[0].info;
			for (int num = 0; num < 4; num++)
			{
				Set(ikTargetPairs[num].effector.target.GetComponent<BaseData>(), GetCorrectShapeValues(chara, normalizedTime, calcBlend[num]));
				Set(ikTargetPairs[num].bend.bendGoal.GetComponent<BaseData>(), GetCorrectShapeValues(chara, normalizedTime, calcBlendBend[num]));
			}
		}
	}

	private void Set(BaseData data, Vector3[] dirs)
	{
		if (!(data == null))
		{
			data.pos = dirs[0];
			data.rot = Quaternion.Euler(dirs[1]);
		}
	}

	private static Vector3[] GetCorrectShapeValues(ChaControl chara, float keyFrame, Dictionary<int, Dictionary<int, List<MotionIKData.BlendWeightInfo>>> calcBlend)
	{
		Vector3[] array = new Vector3[2]
		{
			Vector3.zero,
			Vector3.zero
		};
		for (int i = 0; i < 2; i++)
		{
			foreach (List<MotionIKData.BlendWeightInfo> value in calcBlend[i].Values)
			{
				MotionIKData.BlendWeightInfo blendWeightInfo = value.Find((MotionIKData.BlendWeightInfo x) => x.StartKey <= keyFrame) ?? value[0];
				int shapeNo = blendWeightInfo.shape.shapeNo;
				if (shapeNo >= 0)
				{
					float shapeBodyValue = chara.GetShapeBodyValue(shapeNo);
					float t = Mathf.InverseLerp(blendWeightInfo.StartKey, blendWeightInfo.EndKey, keyFrame);
					if (i == 0)
					{
						Vector3 tmp = GetShapeLerpPositionValue(shapeBodyValue, blendWeightInfo.shape.small[i], blendWeightInfo.shape.mediam[i], blendWeightInfo.shape.large[i]);
						Lerp(ref tmp, blendWeightInfo.pattern, t);
						array[i] += tmp;
					}
					else
					{
						Vector3 tmp2 = GetShapeLerpAngleValue(shapeBodyValue, blendWeightInfo.shape.small[i], blendWeightInfo.shape.mediam[i], blendWeightInfo.shape.large[i]);
						Lerp(ref tmp2, blendWeightInfo.pattern, t);
						array[i] += tmp2;
					}
				}
			}
		}
		return array;
	}

	private static Vector3[] GetCorrectShapeValues(ChaControl chara, float keyFrame, Dictionary<int, List<MotionIKData.BlendWeightInfo>> calcBlend)
	{
		Vector3[] array = new Vector3[2]
		{
			Vector3.zero,
			Vector3.zero
		};
		foreach (List<MotionIKData.BlendWeightInfo> value in calcBlend.Values)
		{
			MotionIKData.BlendWeightInfo blendWeightInfo = value.Find((MotionIKData.BlendWeightInfo x) => x.StartKey <= keyFrame) ?? value[0];
			int shapeNo = blendWeightInfo.shape.shapeNo;
			if (shapeNo >= 0)
			{
				float shapeBodyValue = chara.GetShapeBodyValue(shapeNo);
				float t = Mathf.InverseLerp(blendWeightInfo.StartKey, blendWeightInfo.EndKey, keyFrame);
				Vector3 tmp = GetShapeLerpPositionValue(shapeBodyValue, blendWeightInfo.shape.small[0], blendWeightInfo.shape.mediam[0], blendWeightInfo.shape.large[0]);
				Lerp(ref tmp, blendWeightInfo.pattern, t);
				array[0] += tmp;
				Vector3 tmp2 = GetShapeLerpAngleValue(shapeBodyValue, blendWeightInfo.shape.small[1], blendWeightInfo.shape.mediam[1], blendWeightInfo.shape.large[1]);
				Lerp(ref tmp2, blendWeightInfo.pattern, t);
				array[1] += tmp2;
			}
		}
		return array;
	}

	private static void Lerp(ref Vector3 tmp, int pattern, float t)
	{
		switch (pattern)
		{
		case 1:
			tmp = Vector3.Lerp(Vector3.zero, tmp, t);
			break;
		case 2:
			tmp = Vector3.Lerp(tmp, Vector3.zero, t);
			break;
		}
	}

	private static void CalcBlend(Dictionary<int, List<MotionIKData.BlendWeightInfo>> target, IReadOnlyCollection<MotionIKData.BlendWeightInfo> binfo)
	{
		target.Clear();
		foreach (MotionIKData.BlendWeightInfo item in binfo)
		{
			int shapeNo = item.shape.shapeNo;
			if (!target.TryGetValue(shapeNo, out var value))
			{
				value = (target[shapeNo] = new List<MotionIKData.BlendWeightInfo>());
			}
			value.Add(item);
		}
		foreach (List<MotionIKData.BlendWeightInfo> value2 in target.Values)
		{
			value2.Sort((MotionIKData.BlendWeightInfo a, MotionIKData.BlendWeightInfo b) => Compare(a, b));
		}
		static int Compare(MotionIKData.BlendWeightInfo a, MotionIKData.BlendWeightInfo b)
		{
			float num = a.StartKey - b.StartKey;
			if (num > 0f)
			{
				return 1;
			}
			if (num < 0f)
			{
				return -1;
			}
			return 0;
		}
	}
}
