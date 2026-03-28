namespace UnityEngine.UI;

public abstract class RepeatIntervalButton : RepeatButton
{
	[SerializeField]
	private float interval = 0.5f;

	private float timer;

	protected bool isOn { get; private set; }

	protected override void Process(bool push)
	{
		if (push)
		{
			isOn = timer == 0f || timer == interval;
			timer += Time.deltaTime;
			timer = Mathf.Min(timer, interval);
		}
		else
		{
			isOn = false;
			timer = 0f;
		}
		if (!base.isSelect)
		{
			isOn = false;
		}
	}
}
