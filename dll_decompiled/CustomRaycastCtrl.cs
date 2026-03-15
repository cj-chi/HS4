using System.Linq;
using Illusion.CustomAttributes;
using UnityEngine;

public class CustomRaycastCtrl : MonoBehaviour
{
	[Button("GetRaycastCtrlComponents", "取得", new object[] { })]
	public int getRaycastCtrlComponents;

	[Button("UpdateRaycastCtrl", "全更新", new object[] { })]
	public int updateRaycastCtrl;

	[SerializeField]
	private UI_RaycastCtrl[] raycastCtrl;

	private void GetRaycastCtrlComponents()
	{
		UI_RaycastCtrl[] componentsInChildren = GetComponentsInChildren<UI_RaycastCtrl>(includeInactive: true);
		raycastCtrl = componentsInChildren.ToArray();
	}

	private void UpdateRaycastCtrl()
	{
		UI_RaycastCtrl[] array = raycastCtrl;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
	}

	private void Reset()
	{
		GetRaycastCtrlComponents();
	}
}
