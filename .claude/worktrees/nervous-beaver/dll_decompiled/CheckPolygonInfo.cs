using Illusion.CustomAttributes;
using UnityEngine;

public class CheckPolygonInfo : MonoBehaviour
{
	public int SkinnedMeshRendererFig;

	public int MeshFilterFig;

	public int polygonFig;

	[Button("GetPolygonInfo", "取得", new object[] { })]
	public int getPolygonInfo;

	public void GetPolygonInfo()
	{
		SkinnedMeshRendererFig = 0;
		MeshFilterFig = 0;
		polygonFig = 0;
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren != null)
		{
			SkinnedMeshRendererFig = componentsInChildren.Length;
			SkinnedMeshRenderer[] array = componentsInChildren;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				polygonFig += skinnedMeshRenderer.sharedMesh.triangles.Length / 3;
			}
		}
		MeshFilter[] componentsInChildren2 = GetComponentsInChildren<MeshFilter>();
		if (componentsInChildren2 != null)
		{
			MeshFilterFig = componentsInChildren2.Length;
			MeshFilter[] array2 = componentsInChildren2;
			foreach (MeshFilter meshFilter in array2)
			{
				polygonFig += meshFilter.sharedMesh.triangles.Length / 3;
			}
		}
	}
}
