using UnityEngine;

namespace UIAnimatorDemo;

public class FlashBangResults : BasePanel
{
	[SerializeField]
	private MenuButtons m_menuButtons;

	public override string PanelName => "FLASH_BANG_RESULTS";

	public override void Init()
	{
		m_menuButtons.Init(m_uiManager);
	}
}
