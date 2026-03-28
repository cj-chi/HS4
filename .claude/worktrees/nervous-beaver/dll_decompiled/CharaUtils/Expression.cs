using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CharaUtils;

public class Expression : MonoBehaviour
{
	[Serializable]
	public class LookAt
	{
		public enum AxisType
		{
			X,
			Y,
			Z,
			RevX,
			RevY,
			RevZ,
			None
		}

		public enum RotationOrder
		{
			XYZ,
			XZY,
			YXZ,
			YZX,
			ZXY,
			ZYX
		}

		public string lookAtName = "";

		public string targetName = "";

		public AxisType targetAxisType = AxisType.Z;

		public string upAxisName = "";

		public AxisType upAxisType = AxisType.Y;

		public AxisType sourceAxisType = AxisType.Y;

		public AxisType limitAxisType = AxisType.None;

		public RotationOrder rotOrder = RotationOrder.ZXY;

		[Range(-180f, 180f)]
		public float limitMin;

		[Range(-180f, 180f)]
		public float limitMax;

		public Transform trfLookAt { get; private set; }

		public Transform trfTarget { get; private set; }

		public Transform trfUpAxis { get; private set; }

		public LookAt()
		{
			trfLookAt = null;
			trfTarget = null;
			trfUpAxis = null;
		}

		public void SetLookAtTransform(Transform trf)
		{
			trfLookAt = trf;
		}

		public void SetTargetTransform(Transform trf)
		{
			trfTarget = trf;
		}

		public void SetUpAxisTransform(Transform trf)
		{
			trfUpAxis = trf;
		}

		public void Update()
		{
			if (null == trfTarget || null == trfLookAt)
			{
				return;
			}
			Vector3 upVector = GetUpVector();
			Vector3 vector = Vector3.Normalize(trfTarget.position - trfLookAt.position);
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(upVector, vector));
			Vector3 vector3 = Vector3.Cross(vector, vector2);
			if (targetAxisType == AxisType.RevX || targetAxisType == AxisType.RevY || targetAxisType == AxisType.RevZ)
			{
				vector = -vector;
				vector2 = -vector2;
			}
			Vector3 xvec = Vector3.zero;
			Vector3 yvec = Vector3.zero;
			Vector3 zvec = Vector3.zero;
			switch (targetAxisType)
			{
			case AxisType.X:
			case AxisType.RevX:
				xvec = vector;
				if (sourceAxisType == AxisType.Y)
				{
					yvec = vector3;
					zvec = -vector2;
				}
				else if (sourceAxisType == AxisType.RevY)
				{
					yvec = -vector3;
					zvec = vector2;
				}
				else if (sourceAxisType == AxisType.Z)
				{
					yvec = vector2;
					zvec = vector3;
				}
				else if (sourceAxisType == AxisType.RevZ)
				{
					yvec = -vector2;
					zvec = -vector3;
				}
				break;
			case AxisType.Y:
			case AxisType.RevY:
				yvec = vector;
				if (sourceAxisType == AxisType.X)
				{
					xvec = vector3;
					zvec = vector2;
				}
				else if (sourceAxisType == AxisType.RevX)
				{
					xvec = -vector3;
					zvec = -vector2;
				}
				else if (sourceAxisType == AxisType.Z)
				{
					xvec = -vector2;
					zvec = vector3;
				}
				else if (sourceAxisType == AxisType.RevZ)
				{
					xvec = vector2;
					zvec = -vector3;
				}
				break;
			case AxisType.Z:
			case AxisType.RevZ:
				zvec = vector;
				if (sourceAxisType == AxisType.X)
				{
					xvec = vector3;
					yvec = -vector2;
				}
				else if (sourceAxisType == AxisType.RevX)
				{
					xvec = -vector3;
					yvec = vector2;
				}
				else if (sourceAxisType == AxisType.Y)
				{
					xvec = vector2;
					yvec = vector3;
				}
				else if (sourceAxisType == AxisType.RevY)
				{
					xvec = -vector2;
					yvec = -vector3;
				}
				break;
			}
			if (limitAxisType == AxisType.None)
			{
				trfLookAt.rotation = LookAtQuat(xvec, yvec, zvec);
				return;
			}
			trfLookAt.rotation = LookAtQuat(xvec, yvec, zvec);
			ConvertRotation.RotationOrder order = (ConvertRotation.RotationOrder)rotOrder;
			Quaternion localRotation = trfLookAt.localRotation;
			Vector3 vector4 = ConvertRotation.ConvertDegreeFromQuaternion(order, localRotation);
			Quaternion q = Quaternion.Slerp(localRotation, Quaternion.identity, 0.5f);
			Vector3 vector5 = ConvertRotation.ConvertDegreeFromQuaternion(order, q);
			if (limitAxisType == AxisType.X)
			{
				if ((vector4.x < 0f && vector5.x > 0f) || (vector4.x > 0f && vector5.x < 0f))
				{
					vector4.x *= -1f;
				}
				vector4.x = Mathf.Clamp(vector4.x, limitMin, limitMax);
			}
			else if (limitAxisType == AxisType.Y)
			{
				if ((vector4.y < 0f && vector5.y > 0f) || (vector4.y > 0f && vector5.y < 0f))
				{
					vector4.y *= -1f;
				}
				vector4.y = Mathf.Clamp(vector4.y, limitMin, limitMax);
			}
			else if (limitAxisType == AxisType.Z)
			{
				if ((vector4.z < 0f && vector5.z > 0f) || (vector4.z > 0f && vector5.z < 0f))
				{
					vector4.z *= -1f;
				}
				vector4.z = Mathf.Clamp(vector4.z, limitMin, limitMax);
			}
			trfLookAt.localRotation = ConvertRotation.ConvertDegreeToQuaternion(order, vector4.x, vector4.y, vector4.z);
		}

		private Vector3 GetUpVector()
		{
			Vector3 result = Vector3.up;
			if (null != trfUpAxis)
			{
				switch (upAxisType)
				{
				case AxisType.X:
					result = trfUpAxis.right;
					break;
				case AxisType.Y:
					result = trfUpAxis.up;
					break;
				case AxisType.Z:
					result = trfUpAxis.forward;
					break;
				}
			}
			return result;
		}

		private Quaternion LookAtQuat(Vector3 xvec, Vector3 yvec, Vector3 zvec)
		{
			float num = 1f + xvec.x + yvec.y + zvec.z;
			if (num == 0f)
			{
				return Quaternion.identity;
			}
			float num2 = Mathf.Sqrt(num) / 2f;
			if (float.IsNaN(num2))
			{
				return Quaternion.identity;
			}
			float num3 = 4f * num2;
			if (num3 == 0f)
			{
				return Quaternion.identity;
			}
			float x = (yvec.z - zvec.y) / num3;
			float y = (zvec.x - xvec.z) / num3;
			float z = (xvec.y - yvec.x) / num3;
			return new Quaternion(x, y, z, num2);
		}
	}

	[Serializable]
	public class Correct
	{
		public enum CalcType
		{
			Euler,
			Quaternion
		}

		public enum RotationOrder
		{
			XYZ,
			XZY,
			YXZ,
			YZX,
			ZXY,
			ZYX
		}

		public string correctName = "";

		public string referenceName = "";

		public CalcType calcType;

		public RotationOrder rotOrder = RotationOrder.ZXY;

		[Range(0f, 1f)]
		public float charmRate;

		public bool useRX;

		[Range(-1f, 1f)]
		public float valRXMin;

		[Range(-1f, 1f)]
		public float valRXMax;

		public bool useRY;

		[Range(-1f, 1f)]
		public float valRYMin;

		[Range(-1f, 1f)]
		public float valRYMax;

		public bool useRZ;

		[Range(-1f, 1f)]
		public float valRZMin;

		[Range(-1f, 1f)]
		public float valRZMax;

		public Transform trfCorrect { get; private set; }

		public Transform trfReference { get; private set; }

		public Correct()
		{
			trfCorrect = null;
			trfReference = null;
		}

		public void SetCorrectTransform(Transform trf)
		{
			trfCorrect = trf;
		}

		public void SetReferenceTransform(Transform trf)
		{
			trfReference = trf;
		}

		public void Update()
		{
			if (null == trfCorrect || null == trfReference)
			{
				return;
			}
			if (calcType == CalcType.Euler)
			{
				ConvertRotation.RotationOrder order = (ConvertRotation.RotationOrder)rotOrder;
				Vector3 vector = ConvertRotation.ConvertDegreeFromQuaternion(order, trfCorrect.localRotation);
				Vector3 vector2 = ConvertRotation.ConvertDegreeFromQuaternion(order, trfReference.localRotation);
				float num = 1f;
				Quaternion identity = Quaternion.identity;
				Vector3 vector3 = Vector3.zero;
				if (0f != charmRate)
				{
					identity = Quaternion.Slerp(trfReference.localRotation, Quaternion.identity, charmRate);
					vector3 = ConvertRotation.ConvertDegreeFromQuaternion(order, identity);
				}
				if (useRX)
				{
					num = Mathf.InverseLerp(0f, 90f, Mathf.Clamp(Mathf.Abs(vector2.x), 0f, 90f));
					num = Mathf.Lerp(valRXMin, valRXMax, num);
					vector.x = vector2.x * num;
					if (0f != charmRate && ((vector2.x < 0f && vector3.x > 0f) || (vector2.x > 0f && vector3.x < 0f)))
					{
						vector.x *= -1f;
					}
				}
				if (useRY)
				{
					num = Mathf.InverseLerp(0f, 90f, Mathf.Clamp(Mathf.Abs(vector2.y), 0f, 90f));
					num = Mathf.Lerp(valRYMin, valRYMax, num);
					vector.y = vector2.y * num;
					if (0f != charmRate && ((vector2.y < 0f && vector3.y > 0f) || (vector2.y > 0f && vector3.y < 0f)))
					{
						vector.y *= -1f;
					}
				}
				if (useRZ)
				{
					num = Mathf.InverseLerp(0f, 90f, Mathf.Clamp(Mathf.Abs(vector2.z), 0f, 90f));
					num = Mathf.Lerp(valRZMin, valRZMax, num);
					vector.z = vector2.z * num;
					if (0f != charmRate && ((vector2.z < 0f && vector3.z > 0f) || (vector2.z > 0f && vector3.z < 0f)))
					{
						vector.z *= -1f;
					}
				}
				trfCorrect.localRotation = ConvertRotation.ConvertDegreeToQuaternion(order, vector.x, vector.y, vector.z);
			}
			else if (CalcType.Quaternion == calcType)
			{
				Quaternion localRotation = trfCorrect.localRotation;
				if (useRX)
				{
					localRotation.x = trfReference.localRotation.x * (valRXMin + valRXMax) * 0.5f;
				}
				if (useRY)
				{
					localRotation.y = trfReference.localRotation.y * (valRYMin + valRYMax) * 0.5f;
				}
				if (useRZ)
				{
					localRotation.z = trfReference.localRotation.z * (valRZMin + valRZMax) * 0.5f;
				}
				trfCorrect.localRotation = localRotation;
			}
		}
	}

	[Serializable]
	public class ScriptInfo
	{
		public string elementName = "";

		public bool enable = true;

		public bool enableLookAt;

		public LookAt lookAt = new LookAt();

		public bool enableCorrect;

		public Correct correct = new Correct();

		public int index;

		public int categoryNo;

		public void Update()
		{
			if (enable)
			{
				if (enableLookAt && lookAt != null)
				{
					lookAt.Update();
				}
				if (enableCorrect && correct != null)
				{
					correct.Update();
				}
			}
		}

		public void UpdateArrow()
		{
		}

		public void Destroy()
		{
		}

		public void DestroyArrow()
		{
		}
	}

	public Transform trfChara;

	public ScriptInfo[] info;

	public bool enable = true;

	private Dictionary<string, Transform> dictTrf = new Dictionary<string, Transform>();

	public void SetCharaTransform(Transform trf)
	{
		trfChara = trf;
	}

	public void Initialize()
	{
		if (null == trfChara)
		{
			return;
		}
		FindObjectAll(dictTrf, trfChara);
		Transform value = null;
		ScriptInfo[] array = info;
		foreach (ScriptInfo scriptInfo in array)
		{
			if (scriptInfo.enableLookAt && scriptInfo.lookAt != null && dictTrf.TryGetValue(scriptInfo.lookAt.lookAtName, out value))
			{
				scriptInfo.lookAt.SetLookAtTransform(value);
				if (dictTrf.TryGetValue(scriptInfo.lookAt.targetName, out value))
				{
					scriptInfo.lookAt.SetTargetTransform(value);
					dictTrf.TryGetValue(scriptInfo.lookAt.upAxisName, out value);
					scriptInfo.lookAt.SetUpAxisTransform(value);
				}
			}
			if (scriptInfo.enableCorrect && scriptInfo.correct != null && dictTrf.TryGetValue(scriptInfo.correct.correctName, out value))
			{
				scriptInfo.correct.SetCorrectTransform(value);
				dictTrf.TryGetValue(scriptInfo.correct.referenceName, out value);
				scriptInfo.correct.SetReferenceTransform(value);
			}
		}
	}

	public void FindObjectAll(Dictionary<string, Transform> _dictTrf, Transform _trf)
	{
		if (!_dictTrf.ContainsKey(_trf.name))
		{
			_dictTrf[_trf.name] = _trf;
		}
		for (int i = 0; i < _trf.childCount; i++)
		{
			FindObjectAll(_dictTrf, _trf.GetChild(i));
		}
	}

	public void EnableCategory(int categoryNo, bool _enable)
	{
		for (int i = 0; i < info.Length; i++)
		{
			if (info[i].categoryNo == categoryNo)
			{
				info[i].enable = _enable;
			}
		}
	}

	public void EnableIndex(int indexNo, bool _enable)
	{
		if (0 <= indexNo && indexNo < info.Length)
		{
			info[indexNo].enable = _enable;
		}
	}

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (info != null && enable)
		{
			ScriptInfo[] array = info;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update();
			}
		}
	}

	private void OnDestroy()
	{
		if (info != null)
		{
			ScriptInfo[] array = info;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Destroy();
			}
		}
	}

	public bool LoadSetting(string assetBundleName, string assetName)
	{
		TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, assetName);
		if (null == textAsset)
		{
			return false;
		}
		string[] collection = textAsset.text.Replace("\r", "").Split('\n');
		List<string> list = new List<string>();
		list.AddRange(collection);
		AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: true, null, unloadAllLoadedObjects: true);
		return LoadSettingSub(list);
	}

	public bool LoadSetting(string path)
	{
		List<string> list = new List<string>();
		using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			while (streamReader.Peek() > -1)
			{
				list.Add(streamReader.ReadLine());
			}
		}
		return LoadSettingSub(list);
	}

	public bool LoadSettingSub(List<string> slist)
	{
		if (slist.Count == 0)
		{
			return false;
		}
		string[] array = null;
		array = slist[0].Split('\t');
		int num = int.Parse(array[0]);
		if (num > slist.Count - 1)
		{
			return false;
		}
		info = new ScriptInfo[num];
		for (int i = 0; i < num; i++)
		{
			array = slist[i + 1].Split('\t');
			info[i] = new ScriptInfo();
			info[i].index = i;
			int num2 = 0;
			info[i].categoryNo = int.Parse(array[num2++]);
			info[i].enableLookAt = array[num2++] == "○";
			if (info[i].enableLookAt)
			{
				info[i].lookAt.lookAtName = array[num2++];
				if ("0" == info[i].lookAt.lookAtName)
				{
					info[i].lookAt.lookAtName = "";
				}
				else
				{
					info[i].elementName = info[i].lookAt.lookAtName;
				}
				info[i].lookAt.targetName = array[num2++];
				if ("0" == info[i].lookAt.targetName)
				{
					info[i].lookAt.targetName = "";
				}
				info[i].lookAt.targetAxisType = (LookAt.AxisType)Enum.Parse(typeof(LookAt.AxisType), array[num2++]);
				info[i].lookAt.upAxisName = array[num2++];
				if ("0" == info[i].lookAt.upAxisName)
				{
					info[i].lookAt.upAxisName = "";
				}
				info[i].lookAt.upAxisType = (LookAt.AxisType)Enum.Parse(typeof(LookAt.AxisType), array[num2++]);
				info[i].lookAt.sourceAxisType = (LookAt.AxisType)Enum.Parse(typeof(LookAt.AxisType), array[num2++]);
				info[i].lookAt.limitAxisType = (LookAt.AxisType)Enum.Parse(typeof(LookAt.AxisType), array[num2++]);
				info[i].lookAt.rotOrder = (LookAt.RotationOrder)Enum.Parse(typeof(LookAt.RotationOrder), array[num2++]);
				info[i].lookAt.limitMin = float.Parse(array[num2++]);
				info[i].lookAt.limitMax = float.Parse(array[num2++]);
			}
			else
			{
				num2 += 10;
			}
			info[i].enableCorrect = array[num2++] == "○";
			if (info[i].enableCorrect)
			{
				info[i].correct.correctName = array[num2++];
				if ("0" == info[i].correct.correctName)
				{
					info[i].correct.correctName = "";
				}
				else
				{
					info[i].elementName = info[i].correct.correctName;
				}
				info[i].correct.referenceName = array[num2++];
				if ("0" == info[i].correct.referenceName)
				{
					info[i].correct.referenceName = "";
				}
				info[i].correct.calcType = (Correct.CalcType)Enum.Parse(typeof(Correct.CalcType), array[num2++]);
				info[i].correct.rotOrder = (Correct.RotationOrder)Enum.Parse(typeof(Correct.RotationOrder), array[num2++]);
				info[i].correct.charmRate = float.Parse(array[num2++]);
				info[i].correct.useRX = array[num2++] == "○";
				info[i].correct.valRXMin = float.Parse(array[num2++]);
				info[i].correct.valRXMax = float.Parse(array[num2++]);
				info[i].correct.useRY = array[num2++] == "○";
				info[i].correct.valRYMin = float.Parse(array[num2++]);
				info[i].correct.valRYMax = float.Parse(array[num2++]);
				info[i].correct.useRZ = array[num2++] == "○";
				info[i].correct.valRZMin = float.Parse(array[num2++]);
				info[i].correct.valRZMax = float.Parse(array[num2++]);
			}
		}
		return true;
	}
}
