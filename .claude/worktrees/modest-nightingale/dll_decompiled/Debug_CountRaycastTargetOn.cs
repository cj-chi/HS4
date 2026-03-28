using Illusion.CustomAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Debug_CountRaycastTargetOn : MonoBehaviour
{
	[Button("CountRaycastTargetOn", "RaycastTargetOn数取得", new object[] { })]
	public int countRaycastTargetOn;

	public int raycastTargetOnNum;

	public void CountRaycastTargetOn()
	{
		raycastTargetOnNum = 0;
		Image[] componentsInChildren = GetComponentsInChildren<Image>(includeInactive: true);
		foreach (Image image in componentsInChildren)
		{
			if (null != image && image.raycastTarget)
			{
				raycastTargetOnNum++;
			}
		}
	}
}
