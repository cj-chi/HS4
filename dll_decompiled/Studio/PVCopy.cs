using System.Linq;
using IllusionUtility.GetUtility;
using IllusionUtility.SetUtility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Studio;

public class PVCopy : MonoBehaviour
{
	[SerializeField]
	private GameObject[] pv;

	[SerializeField]
	private GameObject[] bone;

	private bool[] _enable = new bool[8] { true, true, true, true, true, true, true, true };

	private bool enable => _enable.Any();

	public bool this[int _idx]
	{
		get
		{
			return _enable.SafeGet(_idx);
		}
		set
		{
			if (MathfEx.RangeEqualOn(0, _idx, _enable.Length - 1))
			{
				_enable[_idx] = value;
			}
		}
	}

	private void Start()
	{
		(from _ in this.LateUpdateAsObservable()
			where enable
			select _).Subscribe(delegate
		{
			for (int i = 0; i < pv.Length; i++)
			{
				if (_enable[i])
				{
					bone[i].transform.CopyPosRotScl(pv[i].transform);
				}
			}
		});
	}

	private void Reset()
	{
		string[] array = new string[8] { "f_pv_arm_L", "f_pv_elbo_L", "f_pv_arm_R", "f_pv_elbo_R", "f_pv_leg_L", "f_pv_knee_L", "f_pv_leg_R", "f_pv_knee_R" };
		pv = new GameObject[8];
		for (int i = 0; i < array.Length; i++)
		{
			pv[i] = base.transform.FindLoop(array[i])?.gameObject;
		}
		string[] array2 = new string[8] { "f_t_arm_L", "f_t_elbo_L", "f_t_arm_R", "f_t_elbo_R", "f_t_leg_L", "f_t_knee_L", "f_t_leg_R", "f_t_knee_R" };
		bone = new GameObject[8];
		for (int j = 0; j < array2.Length; j++)
		{
			bone[j] = base.transform.FindLoop(array2[j])?.gameObject;
		}
	}
}
