using Config;
using Manager;
using PlaceholderSoftware.WetStuff;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace CameraEffector;

[DisallowMultipleComponent]
public class ConfigEffectorWet : MonoBehaviour
{
	public WetStuff Wetstuff;

	public DepthOfField dof;

	public GlobalFog gf;

	public SunShafts ss;

	private void Refresh()
	{
		GraphicSystem graphicData = Manager.Config.GraphicData;
		SunLightInfo sunLightInfo = BaseMap.sunLightInfo;
		bool selfShadow = graphicData.SelfShadow;
		int qualityLevel = QualitySettings.GetQualityLevel();
		byte graphicQuality = graphicData.GraphicQuality;
		if (QualitySettings.masterTextureLimit != graphicQuality)
		{
			QualitySettings.masterTextureLimit = graphicQuality;
		}
		if (!HSceneManager.isHScene)
		{
			int num = qualityLevel / 2 * 2 + ((!selfShadow) ? 1 : 0);
			if (num != qualityLevel)
			{
				QualitySettings.SetQualityLevel(num);
			}
			if (sunLightInfo != null)
			{
				if ((bool)dof && sunLightInfo.info.dofUse && dof.enabled != graphicData.DepthOfField)
				{
					dof.enabled = graphicData.DepthOfField;
				}
				if ((bool)gf && sunLightInfo.info.fogUse && gf.enabled != graphicData.Fog)
				{
					gf.enabled = graphicData.Fog;
				}
				if ((bool)ss && sunLightInfo.info.sunShaftsUse && ss.enabled != graphicData.SunShaft)
				{
					ss.enabled = graphicData.SunShaft;
				}
			}
			if ((bool)Wetstuff && Wetstuff.enabled != graphicData.Rain)
			{
				Wetstuff.enabled = graphicData.Rain;
			}
		}
		else
		{
			HScreenEffectEnable hScreenEffect = Singleton<HSceneManager>.Instance.GetHScreenEffect();
			int num2 = qualityLevel / 2 * 2 + ((!selfShadow || !hScreenEffect.SelfShadow) ? 1 : 0);
			if (num2 != qualityLevel)
			{
				QualitySettings.SetQualityLevel(num2);
			}
			if ((bool)dof && dof.enabled != (graphicData.DepthOfField && hScreenEffect.DOF))
			{
				dof.enabled = graphicData.DepthOfField && hScreenEffect.DOF;
			}
			if ((bool)gf && gf.enabled != (graphicData.Fog && hScreenEffect.Fog))
			{
				gf.enabled = graphicData.Fog && hScreenEffect.Fog;
			}
			if ((bool)ss && ss.enabled != (graphicData.SunShaft && hScreenEffect.SunShaft))
			{
				ss.enabled = graphicData.SunShaft && hScreenEffect.SunShaft;
			}
			if ((bool)Wetstuff && Wetstuff.enabled != graphicData.Rain)
			{
				Wetstuff.enabled = graphicData.Rain;
			}
		}
	}

	private void Reset()
	{
		Wetstuff = GetComponent<WetStuff>();
		dof = GetComponent<DepthOfField>();
		gf = GetComponent<GlobalFog>();
		ss = GetComponent<SunShafts>();
	}

	private void Start()
	{
		Refresh();
	}

	private void Update()
	{
		Refresh();
	}
}
