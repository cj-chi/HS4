using UnityEngine;

namespace UIAnimatorDemo;

public class HighscoreResults : BasePanel
{
	[SerializeField]
	private MenuButtons m_menuButtons;

	public override string PanelName => "HIGHSCORE_RESULTS";

	public override void Init()
	{
		m_menuButtons.Init(m_uiManager);
	}
}
