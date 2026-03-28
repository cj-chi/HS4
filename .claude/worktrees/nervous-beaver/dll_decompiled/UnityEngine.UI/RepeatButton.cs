using Illusion.Component.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.UI;

public abstract class RepeatButton : SelectUI, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler
{
	private bool push;

	protected abstract void Process(bool push);

	public void OnPointerDown(PointerEventData eventData)
	{
		push = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		push = false;
	}

	protected virtual void Awake()
	{
		push = false;
	}

	private void Update()
	{
		Process(push);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		push = false;
	}
}
