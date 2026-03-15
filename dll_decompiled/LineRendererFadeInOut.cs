using UnityEngine;

public class LineRendererFadeInOut : MonoBehaviour
{
	public EffectSettings EffectSettings;

	public float FadeInSpeed;

	public float FadeOutSpeed;

	public float Length = 2f;

	public float StartWidth = 1f;

	public float EndWidth = 1f;

	private FadeInOutStatus fadeInOutStatus;

	private LineRenderer lineRenderer;

	private float currentStartWidth;

	private float currentEndWidth;

	private float currentLength;

	private bool isInit;

	private bool canUpdate = true;

	private void Start()
	{
		if (EffectSettings != null)
		{
			EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
		}
		lineRenderer = GetComponent<LineRenderer>();
		fadeInOutStatus = FadeInOutStatus.In;
		lineRenderer.SetPosition(1, new Vector3(0f, 0f, 0f));
		lineRenderer.SetWidth(0f, 0f);
		lineRenderer.enabled = true;
		isInit = true;
	}

	private void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
	{
		fadeInOutStatus = FadeInOutStatus.Out;
		canUpdate = true;
	}

	private void OnEnable()
	{
		if (isInit)
		{
			fadeInOutStatus = FadeInOutStatus.In;
			canUpdate = true;
			lineRenderer.enabled = true;
		}
	}

	private void Update()
	{
		switch (fadeInOutStatus)
		{
		case FadeInOutStatus.In:
			if (canUpdate)
			{
				currentStartWidth += Time.deltaTime * (StartWidth / FadeInSpeed);
				currentEndWidth += Time.deltaTime * (EndWidth / FadeInSpeed);
				currentLength += Time.deltaTime * (Length / FadeInSpeed);
				if (currentStartWidth >= StartWidth)
				{
					canUpdate = false;
					currentStartWidth = StartWidth;
					currentEndWidth = EndWidth;
					currentLength = Length;
				}
				lineRenderer.SetPosition(1, new Vector3(0f, 0f, currentLength));
				lineRenderer.SetWidth(currentStartWidth, currentEndWidth);
			}
			break;
		case FadeInOutStatus.Out:
			if (canUpdate)
			{
				currentStartWidth -= Time.deltaTime * (StartWidth / FadeOutSpeed);
				currentEndWidth -= Time.deltaTime * (EndWidth / FadeOutSpeed);
				currentLength -= Time.deltaTime * (Length / FadeOutSpeed);
				if (currentStartWidth <= 0f)
				{
					canUpdate = false;
					currentStartWidth = 0f;
					currentEndWidth = 0f;
					currentLength = 0f;
				}
				lineRenderer.SetPosition(1, new Vector3(0f, 0f, currentLength));
				lineRenderer.SetWidth(currentStartWidth, currentEndWidth);
				if (!canUpdate)
				{
					lineRenderer.enabled = false;
				}
			}
			break;
		}
	}
}
