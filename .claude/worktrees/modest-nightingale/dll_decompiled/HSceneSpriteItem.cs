using System.Collections.Generic;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HSceneSpriteItem : MonoBehaviour
{
	public List<GameObject> lstObj;

	public List<Slider> lstSlider;

	public List<Toggle> lstUseSliderToggle;

	public Button recoverbt;

	public PointerDownAction[] downActions;

	public void SetRecoverEnable(bool value)
	{
		if (!(recoverbt == null) && recoverbt.interactable != value)
		{
			recoverbt.interactable = value;
		}
	}
}
