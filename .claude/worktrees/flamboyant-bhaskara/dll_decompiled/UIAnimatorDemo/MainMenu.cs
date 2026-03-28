using UnityEngine;

namespace UIAnimatorDemo;

public class MainMenu : BasePanel
{
	[SerializeField]
	private MenuButtons m_menuButtons;

	public override bool IsDefaultPanel => true;

	public override string PanelName => "MAIN_MENU";

	public override void Init()
	{
		m_menuButtons.Init(m_uiManager);
	}
}
