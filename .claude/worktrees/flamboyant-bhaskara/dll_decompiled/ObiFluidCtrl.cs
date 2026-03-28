using System.Collections.Generic;
using System.Linq;
using Obi;
using UnityEngine;

public class ObiFluidCtrl
{
	private (ObiEmitterCtrl ctrl, ObiFluidRenderer renderer)[] values;

	public ObiEmitterCtrl[] ObiEmitterCtrls { get; private set; }

	public ObiFluidCtrl((ObiEmitterCtrl ctrl, ObiFluidRenderer renderer)[] _values)
	{
		ObiEmitterCtrls = _values.Select(((ObiEmitterCtrl ctrl, ObiFluidRenderer renderer) v) => v.ctrl).ToArray();
		values = _values;
	}

	public void Release()
	{
		if (((IReadOnlyCollection<(ObiEmitterCtrl, ObiFluidRenderer)>)(object)values).IsNullOrEmpty())
		{
			return;
		}
		foreach (ObiFluidRenderer r in new HashSet<ObiFluidRenderer>(values.Select(((ObiEmitterCtrl ctrl, ObiFluidRenderer renderer) v) => v.renderer)).Where((ObiFluidRenderer v) => v != null))
		{
			List<ObiParticleRenderer> list = new List<ObiParticleRenderer>(r.particleRenderers);
			foreach (var item in values.Where(((ObiEmitterCtrl ctrl, ObiFluidRenderer renderer) v) => v.renderer == r))
			{
				list.Remove(item.ctrl.ObiParticleRenderer);
			}
			r.particleRenderers = list.ToArray();
		}
		ObiEmitterCtrl[] obiEmitterCtrls = ObiEmitterCtrls;
		for (int num = 0; num < obiEmitterCtrls.Length; num++)
		{
			Object.DestroyImmediate(obiEmitterCtrls[num].gameObject);
		}
		ObiEmitterCtrls = null;
		values = null;
	}
}
