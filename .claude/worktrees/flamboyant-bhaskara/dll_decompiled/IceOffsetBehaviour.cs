using UnityEngine;

public class IceOffsetBehaviour : MonoBehaviour
{
	private void Start()
	{
		FadeInOutShaderFloat component = GetComponent<FadeInOutShaderFloat>();
		if (component == null)
		{
			return;
		}
		Transform parent = base.transform.parent;
		SkinnedMeshRenderer component2 = parent.GetComponent<SkinnedMeshRenderer>();
		Mesh sharedMesh;
		if (component2 != null)
		{
			sharedMesh = component2.sharedMesh;
		}
		else
		{
			MeshFilter component3 = parent.GetComponent<MeshFilter>();
			if (component3 == null)
			{
				return;
			}
			sharedMesh = component3.sharedMesh;
		}
		if (!sharedMesh.isReadable)
		{
			component.MaxFloat = 0f;
			return;
		}
		int num = sharedMesh.triangles.Length;
		if (num < 1000)
		{
			if (num > 500)
			{
				component.MaxFloat = (float)num / 1000f - 0.5f;
			}
			else
			{
				component.MaxFloat = 0f;
			}
		}
	}

	private void Update()
	{
	}
}
