using System.Collections.Generic;
using UIAnimatorCore;
using UnityEngine;

namespace UIAnimatorDemo;

public class PanelManager : MonoBehaviour
{
	private Dictionary<string, BasePanel> m_panelsLookup;

	private BasePanel m_currentPanel;

	public BasePanel CurrentPanel => m_currentPanel;

	public void Init(DemoUIManager a_uiManager)
	{
		BasePanel[] componentsInChildren = GetComponentsInChildren<BasePanel>(includeInactive: true);
		m_panelsLookup = new Dictionary<string, BasePanel>();
		BasePanel basePanel = null;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			m_panelsLookup.Add(componentsInChildren[i].PanelName, componentsInChildren[i]);
			componentsInChildren[i].InitBase(a_uiManager);
			componentsInChildren[i].gameObject.SetActive(value: false);
			if (componentsInChildren[i].IsDefaultPanel)
			{
				basePanel = componentsInChildren[i];
			}
		}
		if (basePanel != null)
		{
			OpenPanel(basePanel.PanelName);
		}
	}

	public void OpenPanel(string a_panelName, bool a_playOutro = true)
	{
		if (m_currentPanel == null)
		{
			OpenPanelWithName(a_panelName);
		}
		else if (a_playOutro && m_currentPanel.UIAnimator != null)
		{
			m_currentPanel.UIAnimator.PlayAnimation(AnimSetupType.Outro, delegate
			{
				m_currentPanel.Close();
				OpenPanelWithName(a_panelName);
			});
		}
		else
		{
			m_currentPanel.Close();
			OpenPanelWithName(a_panelName);
		}
	}

	private void OpenPanelWithName(string a_panelName)
	{
		if (m_panelsLookup.ContainsKey(a_panelName))
		{
			m_currentPanel = m_panelsLookup[a_panelName];
			m_currentPanel.Open();
			m_currentPanel.UIAnimator.PlayAnimation(AnimSetupType.Intro);
		}
	}
}
