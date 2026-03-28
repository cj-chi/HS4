using System;
using UnityEngine;

public class ResetPositionOnDiactivated : MonoBehaviour
{
	public EffectSettings EffectSettings;

	private void Start()
	{
		EffectSettings.EffectDeactivated += EffectSettings_EffectDeactivated;
	}

	private void EffectSettings_EffectDeactivated(object sender, EventArgs e)
	{
		base.transform.localPosition = Vector3.zero;
	}
}
