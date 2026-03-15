using System.Collections.Generic;
using SceneAssist;
using UnityEngine;
using UnityEngine.UI;

public class HSceneSpriteLightCategory : MonoBehaviour
{
	public enum CtrlLightMode
	{
		COLOR,
		DIRPOWER
	}

	public List<Slider> lstColSlider;

	public List<Slider> lstDirPowerSlider;

	public List<Button> lstButton;

	private List<Slider> lstSlider;

	[SerializeField]
	private Toggle tglSubLifgt;

	public PointerDownAction[] downActions;

	public void SetValue(CtrlLightMode mode, float _value, int _array = -1)
	{
		switch (mode)
		{
		case CtrlLightMode.COLOR:
			lstSlider = lstColSlider;
			break;
		case CtrlLightMode.DIRPOWER:
			lstSlider = lstDirPowerSlider;
			break;
		}
		if (_array < 0)
		{
			for (int i = 0; i < lstSlider.Count; i++)
			{
				lstSlider[i].value = _value;
			}
		}
		else if (lstSlider.Count > _array)
		{
			lstSlider[_array].value = _value;
		}
	}

	public float GetValue(CtrlLightMode mode, int _array)
	{
		switch (mode)
		{
		case CtrlLightMode.COLOR:
			lstSlider = lstColSlider;
			break;
		case CtrlLightMode.DIRPOWER:
			lstSlider = lstDirPowerSlider;
			break;
		}
		if (lstSlider.Count <= _array)
		{
			return 0f;
		}
		return lstSlider[_array].value;
	}

	public void SetValueToggle(bool val)
	{
		if (!(tglSubLifgt == null))
		{
			tglSubLifgt.isOn = val;
		}
	}

	public Toggle GetToggle()
	{
		if (tglSubLifgt == null)
		{
			return null;
		}
		return tglSubLifgt;
	}

	public void SetEnable(bool _enable, int _array = -1)
	{
		if (_array < 0)
		{
			for (int i = 0; i < lstButton.Count; i++)
			{
				if (lstButton[i].interactable != _enable)
				{
					lstButton[i].interactable = _enable;
				}
			}
		}
		else if (lstButton.Count > _array && lstButton[_array].interactable != _enable)
		{
			lstButton[_array].interactable = _enable;
		}
	}

	public void SetActive(bool _active, int _array = -1)
	{
		if (_array < 0)
		{
			for (int i = 0; i < lstButton.Count; i++)
			{
				if (lstButton[i].isActiveAndEnabled != _active)
				{
					lstButton[i].gameObject.SetActive(_active);
				}
			}
		}
		else if (lstButton.Count > _array && lstButton[_array].isActiveAndEnabled != _active)
		{
			lstButton[_array].gameObject.SetActive(_active);
		}
	}
}
