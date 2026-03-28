using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class CameraSetting : BaseSetting
{
	[Header("注視点の表示")]
	[SerializeField]
	private Toggle[] lookToggles;

	[Header("カメラ移動Xの反転")]
	[SerializeField]
	private Toggle[] invertMoveXToggles;

	[Header("カメラ移動Yの反転")]
	[SerializeField]
	private Toggle[] invertMoveYToggles;

	public override void Init()
	{
		CameraSystem cameraData = Manager.Config.CameraData;
		LinkToggleArray(lookToggles, delegate(int i)
		{
			cameraData.Look = i == 0;
		});
		LinkToggleArray(invertMoveXToggles, delegate(int i)
		{
			cameraData.InvertMoveY = i == 1;
		});
		LinkToggleArray(invertMoveYToggles, delegate(int i)
		{
			cameraData.InvertMoveX = i == 1;
		});
	}

	protected override void ValueToUI()
	{
		CameraSystem cameraData = Manager.Config.CameraData;
		SetToggleUIArray(lookToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 0) ? cameraData.Look : (!cameraData.Look));
		});
		SetToggleUIArray(invertMoveXToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 1) ? cameraData.InvertMoveY : (!cameraData.InvertMoveY));
		});
		SetToggleUIArray(invertMoveYToggles, delegate(Toggle tgl, int index)
		{
			tgl.isOn = ((index == 1) ? cameraData.InvertMoveX : (!cameraData.InvertMoveX));
		});
	}
}
