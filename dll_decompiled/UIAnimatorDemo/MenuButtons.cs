using UnityEngine;

namespace UIAnimatorDemo;

public class MenuButtons : MonoBehaviour
{
	private DemoUIManager m_uiManager;

	public void Init(DemoUIManager a_uiManager)
	{
		m_uiManager = a_uiManager;
	}

	public void OnButton1Pressed()
	{
		m_uiManager.PanelManager.OpenPanel("MAIN_MENU");
	}

	public void OnButton2Pressed()
	{
		m_uiManager.PanelManager.OpenPanel("FLASH_BANG_RESULTS", a_playOutro: false);
	}

	public void OnButton3Pressed()
	{
		m_uiManager.PanelManager.OpenPanel("HIGHSCORE_RESULTS");
	}

	public void OnButton4Pressed()
	{
		m_uiManager.PopupManager.ShowPopup();
	}
}
