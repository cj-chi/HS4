using IllusionUtility.GetUtility;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HAnimationInfoComponent : MonoBehaviour
{
	public HScene.AnimationListInfo info;

	public Text text;

	public PointerAction ptrAction;

	private void OnEnable()
	{
		Transform transform = null;
		if (text == null)
		{
			transform = base.transform.FindLoop("Label");
			text = transform.GetComponent<Text>();
		}
		if (ptrAction == null)
		{
			ptrAction = GetComponent<PointerAction>();
		}
	}
}
