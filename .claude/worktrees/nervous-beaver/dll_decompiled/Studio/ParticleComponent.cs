using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Studio;

public class ParticleComponent : MonoBehaviour
{
	[Header("カラー１反映対象")]
	public ParticleSystem[] particleColor1;

	public Color defColor01 = Color.white;

	[Header("読み込まれたタイミングで再生する")]
	public bool playOnLoad;

	public ParticleSystem[] particlesPlay;

	public bool IsPlay
	{
		get
		{
			if (playOnLoad)
			{
				return !((IReadOnlyCollection<ParticleSystem>)(object)particlesPlay).IsNullOrEmpty();
			}
			return false;
		}
	}

	public bool[] UseColor => new bool[3] { UseColor1, false, false };

	public bool UseColor1 => !((IReadOnlyCollection<ParticleSystem>)(object)particleColor1).IsNullOrEmpty();

	public bool check => !((IReadOnlyCollection<ParticleSystem>)(object)particleColor1).IsNullOrEmpty();

	public void UpdateColor(OIItemInfo _info)
	{
		if (!((IReadOnlyCollection<ParticleSystem>)(object)particleColor1).IsNullOrEmpty())
		{
			ParticleSystem[] array = particleColor1;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.MainModule main = array[i].main;
				main.startColor = _info.colors[0].mainColor;
			}
		}
	}

	public void PlayOnLoad()
	{
		if (!playOnLoad || ((IReadOnlyCollection<ParticleSystem>)(object)particlesPlay).IsNullOrEmpty())
		{
			return;
		}
		foreach (ParticleSystem item in particlesPlay.Where((ParticleSystem v) => v != null))
		{
			item.Play();
		}
	}

	public void SetColor()
	{
		if (!((IReadOnlyCollection<ParticleSystem>)(object)particleColor1).IsNullOrEmpty())
		{
			defColor01 = particleColor1[0].main.startColor.color;
		}
	}
}
