using System;
using UnityEngine;

namespace LuxWater;

public class LuxWater_InfiniteOcean : MonoBehaviour
{
	[Space(6f)]
	[LuxWater_HelpBtn("h.c1utuz9up55r")]
	public Camera MainCam;

	public float GridSize = 10f;

	private Transform trans;

	private Transform camTrans;

	private void OnEnable()
	{
		trans = GetComponent<Transform>();
	}

	private void LateUpdate()
	{
		if (MainCam == null)
		{
			Camera main = Camera.main;
			if (main == null)
			{
				return;
			}
			MainCam = main;
		}
		if (camTrans == null)
		{
			camTrans = MainCam.transform;
		}
		Vector3 position = camTrans.position;
		Vector3 position2 = trans.position;
		Vector3 lossyScale = trans.lossyScale;
		Vector2 vector = new Vector2(GridSize * lossyScale.x, GridSize * lossyScale.z);
		float num = (float)Math.Round(position.x / vector.x);
		float num2 = vector.x * num;
		num = (float)Math.Round(position.z / vector.y);
		float num3 = vector.y * num;
		position2.x = num2 + position2.x % vector.x;
		position2.z = num3 + position2.z % vector.y;
		trans.position = position2;
	}
}
