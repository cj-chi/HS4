using AIChara;
using Illusion.CustomAttributes;
using UnityEngine;

namespace ADV;

public class HMotionShake : MonoBehaviour
{
	[Header("---< モーション関連 >---")]
	[SerializeField]
	[RangeLabel("モーション揺らぎ", 0f, 1f)]
	private float motion;

	[SerializeField]
	[Label("揺らぎモーション発動までの最小時間")]
	private float timeAutoMotionMin = 3f;

	[SerializeField]
	[Label("揺らぎモーション発動までの最大時間")]
	private float timeAutoMotionMax = 5f;

	[SerializeField]
	[Label("揺らぎモーションを変更している最小時間")]
	private float timeMotionMin = 2f;

	[SerializeField]
	[Label("揺らぎモーションを変更している最大時間")]
	private float timeMotionMax = 3f;

	[SerializeField]
	[Label("揺らぎモーションを最低でもこんだけ進める")]
	private float rateMotionMin = 0.1f;

	[SerializeField]
	[Label("揺らぎモーション変更しているときのリープアニメーション")]
	private AnimationCurve curveMotion = new AnimationCurve(default(Keyframe), new Keyframe
	{
		time = 1f,
		value = 1f
	});

	[Header("---< デバッグ表示 >---")]
	[NotEditable]
	[SerializeField]
	private bool allowMotion;

	[NotEditable]
	[SerializeField]
	private bool enableMotion;

	[NotEditable]
	[SerializeField]
	private float timeMotionCalc;

	[NotEditable]
	[SerializeField]
	private float timeMotion;

	[NotEditable]
	[SerializeField]
	private float timeAutoMotionCalc;

	[NotEditable]
	[SerializeField]
	private float timeAutoMotion;

	[NotEditable]
	[SerializeField]
	private Vector2 lerpMotion;

	private ChaControl[] chaCtrls;

	public void SetCharas(params ChaControl[] ctrls)
	{
		chaCtrls = ctrls;
	}

	private void Update()
	{
		if (chaCtrls == null)
		{
			return;
		}
		if (enableMotion)
		{
			timeMotionCalc = Mathf.Clamp(timeMotionCalc + Time.deltaTime, 0f, timeMotion);
			float num = curveMotion.Evaluate(Mathf.Clamp01(timeMotionCalc / timeMotion));
			motion = Mathf.Lerp(lerpMotion.x, lerpMotion.y, num);
			if (num >= 1f)
			{
				enableMotion = false;
			}
		}
		else
		{
			timeAutoMotionCalc = Mathf.Min(timeAutoMotionCalc + Time.deltaTime, timeAutoMotion);
			if (timeAutoMotion > timeAutoMotionCalc)
			{
				return;
			}
			timeAutoMotion = Random.Range(timeAutoMotionMin, timeAutoMotionMax);
			timeAutoMotionCalc = 0f;
			timeMotionCalc = 0f;
			float num2 = ((!allowMotion) ? motion : (1f - motion));
			if (num2 <= rateMotionMin)
			{
				num2 = (allowMotion ? 1 : 0);
			}
			else
			{
				float num3 = Random.Range(rateMotionMin, num2);
				num2 = motion + ((!allowMotion) ? (0f - num3) : num3);
			}
			if (num2 >= 1f)
			{
				allowMotion = false;
			}
			else if (num2 <= 0f)
			{
				allowMotion = true;
			}
			lerpMotion = new Vector2(motion, num2);
			timeMotion = Random.Range(timeMotionMin, timeMotionMax);
			enableMotion = true;
		}
		ChaControl[] array = chaCtrls;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].setAnimatorParamFloat("motion", motion);
		}
	}
}
