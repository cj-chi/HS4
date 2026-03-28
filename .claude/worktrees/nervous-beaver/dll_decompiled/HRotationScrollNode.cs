using SceneAssist;
using UI;
using UnityEngine;

public class HRotationScrollNode : ScrollCylinderNode
{
	public int id = -1;

	private UIObjectScaleOnCursor uIObjectScale;

	private PointerEnterExitAction pointerEnterExit;

	public Vector3 addSclOnCursor = Vector3.zero;

	protected override void Start()
	{
		base.Start();
		uIObjectScale = GetComponent<UIObjectScaleOnCursor>();
		pointerEnterExit = GetComponent<PointerEnterExitAction>();
	}

	protected override void Update()
	{
		tmpColor = BG.color;
		float currentVelocity = 0f;
		float deltaTime = Time.deltaTime;
		tmpColor.a = Mathf.SmoothDamp(tmpColor.a, endA, ref currentVelocity, smoothTime, float.PositiveInfinity, deltaTime);
		BG.color = tmpColor;
		if (text != null)
		{
			text.color = tmpColor;
		}
		tmpScl = BG.transform.localScale;
		Vector3 currentVelocity2 = Vector3.zero;
		if ((prephaseScale == 0 && phaseScale == 1) || (prephaseScale == 1 && phaseScale == 0))
		{
			tmpScl = Vector3.SmoothDamp(tmpScl, endScl, ref currentVelocity2, smoothTime, float.PositiveInfinity, deltaTime);
		}
		else
		{
			tmpScl = Vector3.SmoothDamp(tmpScl, endScl, ref currentVelocity2, smoothTimeV2, float.PositiveInfinity, deltaTime);
		}
		BG.transform.localScale = tmpScl;
		if (text != null)
		{
			text.transform.localScale = tmpScl;
		}
	}

	public void ChangeScale(int id, bool onEnter)
	{
		prephaseScale = phaseScale;
		if (uIObjectScale != null && pointerEnterExit != null)
		{
			if (onEnter && id == 0)
			{
				endScl = new Vector3(scale[id], scale[id], scale[id]) + addSclOnCursor;
			}
			else
			{
				endScl = new Vector3(scale[id], scale[id], scale[id]);
			}
		}
		else
		{
			endScl = new Vector3(scale[id], scale[id], scale[id]);
		}
		phaseScale = id;
	}
}
