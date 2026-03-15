using UnityEngine.UI;

public class SlideOpenScrollRect : ScrollRect
{
	private float viewsize;

	public int moveMode = -1;

	private int hidemode = -1;

	protected override void Awake()
	{
		base.Awake();
	}

	public void SetViewSize(float size)
	{
		viewsize = size;
		if (base.content.sizeDelta.y <= viewsize)
		{
			hidemode = 0;
		}
		else
		{
			hidemode = 1;
		}
	}

	public void SetMode(int mode)
	{
		moveMode = mode;
	}

	public void ResetMode()
	{
		moveMode = -1;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (base.verticalScrollbar == null)
		{
			return;
		}
		if (moveMode == -1)
		{
			if (base.content.sizeDelta.y <= viewsize)
			{
				if (base.verticalScrollbar.gameObject.activeSelf)
				{
					base.verticalScrollbar.gameObject.SetActive(value: false);
				}
				hidemode = 0;
			}
			else
			{
				if (!base.verticalScrollbar.gameObject.activeSelf)
				{
					base.verticalScrollbar.gameObject.SetActive(value: true);
				}
				hidemode = 1;
			}
			return;
		}
		if (hidemode != 0 && base.content.sizeDelta.y <= viewsize)
		{
			hidemode = 0;
		}
		else if (hidemode != 1 && base.content.sizeDelta.y > viewsize)
		{
			hidemode = 1;
		}
		if (hidemode == 0)
		{
			if (base.verticalScrollbar.gameObject.activeSelf)
			{
				base.verticalScrollbar.gameObject.SetActive(value: false);
			}
		}
		else if (hidemode == 1 && !base.verticalScrollbar.gameObject.activeSelf)
		{
			base.verticalScrollbar.gameObject.SetActive(value: true);
		}
	}
}
