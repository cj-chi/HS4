using UnityEngine;

namespace UIAnimatorDemo;

public class DemoUIManager : MonoBehaviour
{
	[SerializeField]
	private PanelManager m_panelManager;

	[SerializeField]
	private PopupManager m_popupManager;

	public PanelManager PanelManager => m_panelManager;

	public PopupManager PopupManager => m_popupManager;

	private void Awake()
	{
		m_panelManager.Init(this);
		m_popupManager.Init(this);
	}
}
