using System;
using System.Collections.Generic;
using AIChara;
using UnityEngine;

public class EliminateScale : MonoBehaviour
{
	public enum EliminateScaleKind
	{
		ALL,
		X,
		Y,
		Z,
		XY,
		XZ,
		YZ,
		NONE
	}

	[Serializable]
	public class ShapeMove
	{
		public int numShape;

		public Vector3 posMax;

		public Vector3 posMid;

		public Vector3 posMin;

		public Vector3 rotMax;

		public Vector3 rotMid;

		public Vector3 rotMin;
	}

	[Tooltip("省きたいスケール軸")]
	public EliminateScaleKind kind;

	public List<ShapeMove> lstShape = new List<ShapeMove>();

	[Header("Debug表示")]
	public Vector3 defScale = Vector3.one;

	public Vector3 defPositon = Vector3.zero;

	public Quaternion defRotation = Quaternion.identity;

	public ChaControl custom;

	private void Start()
	{
		defScale = base.transform.lossyScale;
		defPositon = base.transform.localPosition;
		defRotation = base.transform.localRotation;
	}

	private void LateUpdate()
	{
		Vector3 lossyScale = base.transform.lossyScale;
		Vector3 localScale = base.transform.localScale;
		base.transform.localScale = new Vector3((kind == EliminateScaleKind.ALL || kind == EliminateScaleKind.X || kind == EliminateScaleKind.XY || kind == EliminateScaleKind.XZ) ? (localScale.x / lossyScale.x * defScale.x) : localScale.x, (kind == EliminateScaleKind.ALL || kind == EliminateScaleKind.Y || kind == EliminateScaleKind.XY || kind == EliminateScaleKind.YZ) ? (localScale.y / lossyScale.y * defScale.y) : localScale.y, (kind == EliminateScaleKind.ALL || kind == EliminateScaleKind.Z || kind == EliminateScaleKind.XZ || kind == EliminateScaleKind.YZ) ? (localScale.z / lossyScale.z * defScale.z) : localScale.z);
		if (custom != null)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			for (int i = 0; i < lstShape.Count; i++)
			{
				float shapeBodyValue = custom.GetShapeBodyValue(Mathf.Max(lstShape[i].numShape, 0));
				zero += ((shapeBodyValue >= 0.5f) ? Vector3.Lerp(lstShape[i].posMid, lstShape[i].posMax, Mathf.InverseLerp(0.5f, 1f, shapeBodyValue)) : Vector3.Lerp(lstShape[i].posMin, lstShape[i].posMid, Mathf.InverseLerp(0f, 0.5f, shapeBodyValue)));
				zero2 += ((shapeBodyValue >= 0.5f) ? Vector3.Lerp(lstShape[i].rotMid, lstShape[i].rotMax, Mathf.InverseLerp(0.5f, 1f, shapeBodyValue)) : Vector3.Lerp(lstShape[i].rotMin, lstShape[i].rotMid, Mathf.InverseLerp(0f, 0.5f, shapeBodyValue)));
			}
			base.transform.localPosition = defPositon + zero;
			base.transform.localRotation = defRotation * Quaternion.Euler(zero2);
		}
	}
}
