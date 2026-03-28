using UIAnimatorCore;
using UnityEngine;

namespace UIAnimatorDemo;

public abstract class BasePanel : MonoBehaviour
{
	[SerializeField]
	protected UIAnimator m_uiAnimator;

	protected DemoUIManager m_uiManager;

	public abstract string PanelName { get; }

	public virtual bool IsDefaultPanel => false;

	public UIAnimator UIAnimator => m_uiAnimator;

	public void InitBase(DemoUIManager a_uiManager)
	{
		m_uiManager = a_uiManager;
		Init();
	}

	public virtual void Init()
	{
	}

	public virtual void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void Open()
	{
		base.gameObject.SetActive(value: true);
	}
}
