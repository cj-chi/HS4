using UIAnimatorCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIAnimatorDemo;

public class AudioSetting : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Image m_audioEnabledState;

	public Image m_audioMutedState;

	private bool m_isAudioCurrentlyEnabled = true;

	private const string c_audioEnabledStatePlayerPrefKey = "UIAnimatorDemo_AudioEnabled";

	private void Awake()
	{
		SetAudioPlayingState(PlayerPrefs.GetInt("UIAnimatorDemo_AudioEnabled", 1) == 1);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SetAudioPlayingState(!m_isAudioCurrentlyEnabled);
	}

	private void SetAudioPlayingState(bool a_isAudioEnabled)
	{
		m_isAudioCurrentlyEnabled = a_isAudioEnabled;
		m_audioEnabledState.gameObject.SetActive(m_isAudioCurrentlyEnabled);
		m_audioMutedState.gameObject.SetActive(!m_isAudioCurrentlyEnabled);
		UIAnimator.SetUIAudioState(m_isAudioCurrentlyEnabled);
		PlayerPrefs.SetInt("UIAnimatorDemo_AudioEnabled", m_isAudioCurrentlyEnabled ? 1 : 0);
	}
}
