using UnityEngine;

namespace LuxWater;

[ExecuteInEditMode]
public class LuxWater_PlanarWaterTile : MonoBehaviour
{
	[Space(6f)]
	[LuxWater_HelpBtn("h.nu6w5ylbucb7")]
	public LuxWater_PlanarReflection reflection;

	public void OnEnable()
	{
		AcquireComponents();
	}

	private void AcquireComponents()
	{
		if (!reflection)
		{
			if ((bool)base.transform.parent)
			{
				reflection = base.transform.parent.GetComponent<LuxWater_PlanarReflection>();
			}
			else
			{
				reflection = base.transform.GetComponent<LuxWater_PlanarReflection>();
			}
		}
	}

	public void OnWillRenderObject()
	{
		if ((bool)reflection)
		{
			reflection.WaterTileBeingRendered(base.transform, Camera.current);
		}
	}
}
