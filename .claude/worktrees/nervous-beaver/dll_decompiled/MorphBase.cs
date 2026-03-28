using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MorphBase
{
	public const int morphFilesVersion = 100;

	public MorphCalcInfo[] CalcInfo;

	public int GetMaxPtn()
	{
		if (CalcInfo.Length == 0)
		{
			return 0;
		}
		return CalcInfo[0].UpdateInfo.Length / 2;
	}

	protected bool CreateCalcInfo(GameObject obj)
	{
		if (null == obj)
		{
			return false;
		}
		MorphSetting morphSetting = (MorphSetting)obj.GetComponent("MorphSetting");
		if (null == morphSetting)
		{
			return false;
		}
		CalcInfo = null;
		GC.Collect();
		CalcInfo = new MorphCalcInfo[morphSetting.MorphDataList.Count];
		int num = 0;
		foreach (MorphData morphData in morphSetting.MorphDataList)
		{
			if (null == morphData.TargetObj)
			{
				continue;
			}
			CalcInfo[num] = new MorphCalcInfo();
			CalcInfo[num].TargetObj = morphData.TargetObj;
			MeshFilter meshFilter = new MeshFilter();
			meshFilter = morphData.TargetObj.GetComponent(typeof(MeshFilter)) as MeshFilter;
			if ((bool)meshFilter)
			{
				CalcInfo[num].OriginalMesh = meshFilter.sharedMesh;
				CalcInfo[num].OriginalPos = meshFilter.sharedMesh.vertices;
				CalcInfo[num].OriginalNormal = meshFilter.sharedMesh.normals;
				CalcInfo[num].WeightFlags = false;
			}
			else
			{
				SkinnedMeshRenderer skinnedMeshRenderer = new SkinnedMeshRenderer();
				skinnedMeshRenderer = morphData.TargetObj.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
				CalcInfo[num].OriginalMesh = skinnedMeshRenderer.sharedMesh;
				CalcInfo[num].OriginalPos = skinnedMeshRenderer.sharedMesh.vertices;
				CalcInfo[num].OriginalNormal = skinnedMeshRenderer.sharedMesh.normals;
				CalcInfo[num].WeightFlags = true;
			}
			int num2 = 0;
			if (null == morphData.MorphArea)
			{
				num2 = CalcInfo[num].OriginalMesh.vertices.Length;
				CalcInfo[num].UpdateIndex = new int[num2];
				for (int i = 0; i < num2; i++)
				{
					CalcInfo[num].UpdateIndex[i] = i;
				}
			}
			else if (morphData.MorphArea.colors.Length != 0)
			{
				List<int> list = new List<int>();
				foreach (var item in morphData.MorphArea.colors.Select((Color value, int index) => new { value, index }))
				{
					if (item.value == morphData.AreaColor)
					{
						list.Add(item.index);
					}
				}
				CalcInfo[num].UpdateIndex = new int[list.Count];
				foreach (var item2 in list.Select((int value, int index) => new { value, index }))
				{
					CalcInfo[num].UpdateIndex[item2.index] = item2.value;
				}
				num2 = list.Count;
			}
			else
			{
				num2 = CalcInfo[num].OriginalMesh.vertices.Length;
				CalcInfo[num].UpdateIndex = new int[num2];
				for (int num3 = 0; num3 < num2; num3++)
				{
					CalcInfo[num].UpdateIndex[num3] = num3;
				}
			}
			int num4 = morphData.MorphMesh.Length;
			CalcInfo[num].UpdateInfo = new MorphUpdateInfo[num4];
			for (int num5 = 0; num5 < num4; num5++)
			{
				CalcInfo[num].UpdateInfo[num5] = new MorphUpdateInfo();
				CalcInfo[num].UpdateInfo[num5].Pos = new Vector3[num2];
				CalcInfo[num].UpdateInfo[num5].Normmal = new Vector3[num2];
				if (null == morphData.MorphMesh[num5])
				{
					for (int num6 = 0; num6 < num2; num6++)
					{
						CalcInfo[num].UpdateInfo[num5].Pos[num6] = CalcInfo[num].OriginalMesh.vertices[CalcInfo[num].UpdateIndex[num6]];
						CalcInfo[num].UpdateInfo[num5].Normmal[num6] = CalcInfo[num].OriginalMesh.normals[CalcInfo[num].UpdateIndex[num6]];
					}
				}
				else
				{
					for (int num7 = 0; num7 < num2; num7++)
					{
						CalcInfo[num].UpdateInfo[num5].Pos[num7] = morphData.MorphMesh[num5].vertices[CalcInfo[num].UpdateIndex[num7]];
						CalcInfo[num].UpdateInfo[num5].Normmal[num7] = morphData.MorphMesh[num5].normals[CalcInfo[num].UpdateIndex[num7]];
					}
				}
			}
			num++;
		}
		return true;
	}

	protected bool ChangeRefTargetMesh(List<MorphingTargetInfo> MorphTargetList)
	{
		MorphCalcInfo[] calcInfo = CalcInfo;
		foreach (MorphCalcInfo morphCalcInfo in calcInfo)
		{
			if (null == morphCalcInfo.OriginalMesh)
			{
				continue;
			}
			Mesh mesh = null;
			foreach (MorphingTargetInfo MorphTarget in MorphTargetList)
			{
				if (MorphTarget.TargetObj == morphCalcInfo.TargetObj)
				{
					mesh = MorphTarget.TargetMesh;
					break;
				}
			}
			if ((bool)mesh)
			{
				morphCalcInfo.TargetMesh = mesh;
			}
			else
			{
				MorphCloneMesh.Clone(out morphCalcInfo.TargetMesh, morphCalcInfo.OriginalMesh);
				morphCalcInfo.TargetMesh.name = morphCalcInfo.OriginalMesh.name;
				MorphingTargetInfo morphingTargetInfo = new MorphingTargetInfo();
				morphingTargetInfo.TargetMesh = morphCalcInfo.TargetMesh;
				morphingTargetInfo.TargetObj = morphCalcInfo.TargetObj;
				MorphTargetList.Add(morphingTargetInfo);
			}
			if (morphCalcInfo.WeightFlags)
			{
				new SkinnedMeshRenderer();
				(morphCalcInfo.TargetObj.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer).sharedMesh = morphCalcInfo.TargetMesh;
			}
			else
			{
				new MeshFilter();
				(morphCalcInfo.TargetObj.GetComponent(typeof(MeshFilter)) as MeshFilter).sharedMesh = morphCalcInfo.TargetMesh;
			}
		}
		return true;
	}
}
