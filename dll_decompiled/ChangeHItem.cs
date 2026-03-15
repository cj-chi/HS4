using UnityEngine;

public class ChangeHItem : MonoBehaviour
{
	[Tooltip("体位アイテムと入れ替わって表示を消すオブジェクト")]
	public GameObject VisibleObj;

	public void ChangeActive(bool val)
	{
		if (!(VisibleObj == null) && VisibleObj.activeSelf != val)
		{
			VisibleObj.SetActive(val);
		}
	}
}
