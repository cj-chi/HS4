using Manager;
using Studio.SceneAssist;
using UnityEngine;

namespace Studio;

public class ExitScene : MonoBehaviour
{
	[SerializeField]
	private VoiceNode yes;

	[SerializeField]
	private VoiceNode no;

	private float timeScale = 1f;

	private void Awake()
	{
		timeScale = Time.timeScale;
		Time.timeScale = 0f;
	}

	private void Start()
	{
		yes.addOnClick = delegate
		{
			ExitDialog.GameEnd(isCheck: false);
		};
		yes.addOnClick = delegate
		{
			Assist.PlayDecisionSE();
		};
		no.addOnClick = delegate
		{
			Scene.Unload();
		};
		no.addOnClick = delegate
		{
			Manager.Sound.Play(new Manager.Sound.Loader
			{
				type = Manager.Sound.Type.SystemSE,
				bundle = Assist.AssetBundleSystemSE,
				asset = "sse_00_03",
				fadeTime = 0f
			});
		};
	}

	private void OnDestroy()
	{
		Time.timeScale = timeScale;
	}
}
