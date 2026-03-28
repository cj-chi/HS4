using System;
using UIAnimatorCore;
using UnityEngine;

namespace UIAnimatorDemo;

public class PopupManager : MonoBehaviour
{
	[SerializeField]
	private UIAnimator m_popupUIAnimator;

	[SerializeField]
	private PopupButtons m_popupButtons;

	private DemoUIManager m_uiManager;

	public void Init(DemoUIManager a_uiManager)
	{
		m_uiManager = a_uiManager;
		m_popupButtons.Init(a_uiManager);
		m_popupUIAnimator.gameObject.SetActive(value: false);
	}

	public void ShowPopup()
	{
		m_popupUIAnimator.gameObject.SetActive(value: true);
		m_popupUIAnimator.PlayAnimation(AnimSetupType.Intro);
		if (m_uiManager.PanelManager.CurrentPanel != null)
		{
			m_uiManager.PanelManager.CurrentPanel.UIAnimator.Paused = true;
		}
	}

	public void ClosePopup(Action a_onClosed = null)
	{
		m_popupUIAnimator.PlayAnimation(AnimSetupType.Outro, delegate
		{
			m_popupUIAnimator.gameObject.SetActive(value: false);
			if (m_uiManager.PanelManager.CurrentPanel != null)
			{
				m_uiManager.PanelManager.CurrentPanel.UIAnimator.Paused = false;
			}
			if (a_onClosed != null)
			{
				a_onClosed();
			}
		});
	}
}
