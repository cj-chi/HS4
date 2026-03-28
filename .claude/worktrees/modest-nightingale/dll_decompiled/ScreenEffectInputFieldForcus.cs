using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenEffectInputFieldForcus : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ISubmitHandler
{
	[SerializeField]
	private InputField inputField;

	[SerializeField]
	private HSceneFlagCtrl hFlag;

	private void Start()
	{
		if (hFlag == null)
		{
			hFlag = Singleton<HSceneFlagCtrl>.Instance;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		hFlag.DeselectInputField(inputField);
	}

	public void OnSelect(BaseEventData eventData)
	{
		hFlag.SelectInputField(inputField);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		hFlag.SelectInputField(inputField);
	}
}
