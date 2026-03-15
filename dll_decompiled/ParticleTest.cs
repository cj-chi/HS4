using System.Collections;
using AIChara;
using Manager;
using UnityEngine;

public class ParticleTest : MonoBehaviour
{
	public RuntimeAnimatorController animatorController;

	public StepWorld[] stepWorlds;

	private IEnumerator Start()
	{
		yield return new WaitWhile(() => !Singleton<Character>.IsInstance());
		ChaControl chara = Singleton<Character>.Instance.CreateChara(1, null, 0);
		chara.chaFile.LoadCharaFile(UserData.Path + "chara/female/HS2ChaF_20200723160123925.png");
		chara.ChangeNowCoordinate();
		chara.Load(reflectStatus: true);
		chara.ChangeLookNeckPtn(1);
		chara.animBody.runtimeAnimatorController = animatorController;
		yield return null;
		stepWorlds = chara.objTop.GetComponentsInChildren<StepWorld>(includeInactive: true);
	}
}
