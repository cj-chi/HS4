using UnityEngine;

namespace SkinnedMetaball;

public class StaticMetaballSeed : MetaballSeedBase
{
	public MeshFilter meshFilter;

	private MetaballCellCluster _cellCluster;

	public override Mesh Mesh
	{
		get
		{
			return meshFilter.sharedMesh;
		}
		set
		{
			meshFilter.sharedMesh = value;
		}
	}

	public override bool IsTreeShape => false;

	private void ConstructCellCluster(MetaballCellCluster cluster, Transform parentNode, Matrix4x4 toLocalMtx, Transform meshTrans)
	{
		for (int i = 0; i < parentNode.childCount; i++)
		{
			Transform child = parentNode.GetChild(i);
			MetaballNode component = child.GetComponent<MetaballNode>();
			if (component != null)
			{
				_cellCluster.AddCell(meshTrans.InverseTransformPoint(child.position), 0f, component.Radius, child.gameObject.name).density = component.Density;
			}
			ConstructCellCluster(cluster, child, toLocalMtx, meshTrans);
		}
	}

	private void WorldPositionBounds(Transform parentNode, ref Bounds bounds)
	{
		for (int i = 0; i < parentNode.childCount; i++)
		{
			Transform child = parentNode.GetChild(i);
			MetaballNode component = child.GetComponent<MetaballNode>();
			if (component != null)
			{
				for (int j = 0; j < 3; j++)
				{
					if (child.transform.position[j] - component.Radius < bounds.min[j])
					{
						Vector3 min = bounds.min;
						min[j] = child.transform.position[j] - component.Radius;
						bounds.min = min;
					}
					if (child.transform.position[j] + component.Radius > bounds.max[j])
					{
						Vector3 max = bounds.max;
						max[j] = child.transform.position[j] + component.Radius;
						bounds.max = max;
					}
				}
			}
			WorldPositionBounds(child, ref bounds);
		}
	}

	private bool WorldPositionBoundsFirst(Transform parentNode, ref Bounds bounds)
	{
		for (int i = 0; i < parentNode.childCount; i++)
		{
			Transform child = parentNode.GetChild(i);
			MetaballNode component = child.GetComponent<MetaballNode>();
			if (component != null)
			{
				for (int j = 0; j < 3; j++)
				{
					float value = child.transform.position[j] - component.Radius;
					Vector3 min = bounds.min;
					min[j] = value;
					bounds.min = min;
					min = bounds.max;
					min[j] = value;
					bounds.max = min;
				}
				return true;
			}
			if (WorldPositionBoundsFirst(child, ref bounds))
			{
				return true;
			}
		}
		return false;
	}

	[ContextMenu("CreateMesh")]
	public override void CreateMesh()
	{
		CleanupBoneRoot();
		_cellCluster = new MetaballCellCluster();
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		WorldPositionBoundsFirst(sourceRoot.transform, ref bounds);
		WorldPositionBounds(sourceRoot.transform, ref bounds);
		meshFilter.transform.position = bounds.center;
		Matrix4x4 worldToLocalMatrix = meshFilter.transform.worldToLocalMatrix;
		ConstructCellCluster(_cellCluster, sourceRoot.transform, worldToLocalMatrix, meshFilter.transform);
		GetUVBaseVector(out var uDir, out var vDir, out var offset);
		Bounds? bounds2 = null;
		if (bUseFixedBounds)
		{
			bounds2 = fixedBounds;
		}
		_errorMsg = MetaballBuilder.Instance.CreateMesh(_cellCluster, boneRoot.transform, powerThreshold, base.GridSize, uDir, vDir, offset, out var out_mesh, cellObjPrefab, bReverse, bounds2, bAutoGridSize, autoGridQuarity);
		if (string.IsNullOrEmpty(_errorMsg))
		{
			out_mesh.RecalculateBounds();
			meshFilter.sharedMesh = out_mesh;
			EnumBoneNodes();
		}
	}
}
