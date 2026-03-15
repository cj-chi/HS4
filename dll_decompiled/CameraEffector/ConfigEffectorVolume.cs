using Config;
using Illusion.Extensions;
using Manager;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CameraEffector;

[DisallowMultipleComponent]
public class ConfigEffectorVolume : MonoBehaviour
{
	public PostProcessVolume postProcessVolume;

	private Bloom bloom;

	private AmbientOcclusion ao;

	private ScreenSpaceReflections ssr;

	private Vignette vignette;

	public bool isBloom = true;

	public bool isAo = true;

	public bool isSsr = true;

	public bool isVignette = true;

	public ReflectionProbe rp;

	public bool isHSceneSettingUse = true;

	public ParticleSystem particle;

	private void Refresh()
	{
		GraphicSystem graphicData = Manager.Config.GraphicData;
		if (!HSceneManager.isHScene || !isHSceneSettingUse)
		{
			if (isBloom && (bool)bloom && bloom.active != graphicData.Bloom)
			{
				bloom.active = graphicData.Bloom;
			}
			if (isAo && (bool)ao && ao.active != graphicData.SSAO)
			{
				ao.active = graphicData.SSAO;
			}
			if (isSsr && (bool)ssr && ssr.active != graphicData.SSR)
			{
				ssr.active = graphicData.SSR;
			}
			if (isVignette && (bool)vignette && vignette.active != graphicData.Vignette)
			{
				vignette.active = graphicData.Vignette;
			}
			if ((bool)rp && rp.enabled != graphicData.RP)
			{
				rp.enabled = graphicData.RP;
			}
			if ((bool)particle && particle.gameObject.activeSelf != graphicData.Rain)
			{
				particle.gameObject.SetActiveIfDifferent(graphicData.Rain);
			}
			return;
		}
		HScreenEffectEnable hScreenEffect = Singleton<HSceneManager>.Instance.GetHScreenEffect();
		if (isBloom && (bool)bloom && bloom.active != (graphicData.Bloom && hScreenEffect.Bloom))
		{
			bloom.active = graphicData.Bloom && hScreenEffect.Bloom;
		}
		if (isAo && (bool)ao && ao.active != (graphicData.SSAO && hScreenEffect.AO))
		{
			ao.active = graphicData.SSAO && hScreenEffect.AO;
		}
		if (isSsr && (bool)ssr && ssr.active != (graphicData.SSR && hScreenEffect.SSR))
		{
			ssr.active = graphicData.SSR && hScreenEffect.SSR;
		}
		if (isVignette && (bool)vignette && vignette.active != (graphicData.Vignette && hScreenEffect.Vignette))
		{
			vignette.active = graphicData.Vignette && hScreenEffect.Vignette;
		}
		if ((bool)rp && rp.enabled != (graphicData.RP && hScreenEffect.RP))
		{
			rp.enabled = graphicData.RP && hScreenEffect.RP;
		}
		if ((bool)particle && particle.gameObject.activeSelf != graphicData.Rain)
		{
			particle.gameObject.SetActiveIfDifferent(graphicData.Rain);
		}
	}

	private void Reset()
	{
		postProcessVolume = GetComponent<PostProcessVolume>();
	}

	private async void Start()
	{
		await UniTask.WaitUntil(() => Manager.Config.initialized);
		if ((bool)postProcessVolume)
		{
			bloom = postProcessVolume.profile.GetSetting<Bloom>();
			ao = postProcessVolume.profile.GetSetting<AmbientOcclusion>();
			ssr = postProcessVolume.profile.GetSetting<ScreenSpaceReflections>();
			vignette = postProcessVolume.profile.GetSetting<Vignette>();
		}
		if (!rp)
		{
			rp = GetComponent<ReflectionProbe>();
		}
		if (!particle)
		{
			GameObject gameObject = GameObject.Find("Particle_rain");
			if ((bool)gameObject)
			{
				particle = gameObject.GetComponent<ParticleSystem>();
			}
		}
		Refresh();
	}

	private void Update()
	{
		Refresh();
	}
}
