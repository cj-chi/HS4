using System.Collections.Generic;
using AIChara;

public class DeliveryMember
{
	public HSceneFlagCtrl ctrlFlag;

	public ChaControl[] chaFemales;

	public ChaControl[] chaMales;

	public CrossFade fade;

	public ObiCtrl ctrlObi;

	public HSceneSprite sprite;

	public HItemCtrl item;

	public FeelHit feelHit;

	public HAutoCtrl auto;

	public HVoiceCtrl voice;

	public HParticleCtrl particle;

	public HSeCtrl se;

	public List<(int, int, MotionIK)> lstMotionIK;

	public YureCtrl[] ctrlYures;

	public RootmotionOffset[] rootmotionOffsetsF;

	public RootmotionOffset[] rootmotionOffsetsM;

	public int eventNo = -1;

	public int peepkind = -1;
}
