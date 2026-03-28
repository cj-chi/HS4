using System.Collections;
using Manager;
using UnityEngine;

public class obisampletest : MonoBehaviour
{
	private ObiFluidCtrl obiFluidCtrl;

	[SerializeField]
	private float speed = 10f;

	[SerializeField]
	private float life = 10f;

	[SerializeField]
	private float random = 0.1f;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => Singleton<ObiFluidManager>.IsInstance());
		yield return new WaitUntil(() => Singleton<GameSystem>.IsInstance());
		ObiFluidManager.AddTargetParam param = new ObiFluidManager.AddTargetParam(base.transform, _setupInfo: new ObiEmitterCtrl.SetupInfo("obifluidtest.unity3d", "Honey 2", "abdata", "obifluidtest.unity3d", "VerySticky 2", "abdata", speed, life, random), _pos: new Vector3(0f, 10f, 0f), _rot: Vector3.zero);
		obiFluidCtrl = Singleton<ObiFluidManager>.Instance.Add(param);
		obiFluidCtrl.ObiEmitterCtrls[0].LoadFile("obifluidtest.unity3d", "timing", "abdata");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			obiFluidCtrl.ObiEmitterCtrls[0].Play();
		}
	}
}
