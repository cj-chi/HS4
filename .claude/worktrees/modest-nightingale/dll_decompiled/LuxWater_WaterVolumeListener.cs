using LuxWater;
using UnityEngine;

public class LuxWater_WaterVolumeListener : MonoBehaviour
{
	private void OnEnable()
	{
		LuxWater_WaterVolume.OnEnterWaterVolume += Enter;
		LuxWater_WaterVolume.OnExitWaterVolume += Exit;
	}

	private void OnDisable()
	{
		LuxWater_WaterVolume.OnEnterWaterVolume -= Enter;
		LuxWater_WaterVolume.OnExitWaterVolume -= Exit;
	}

	private void Enter()
	{
	}

	private void Exit()
	{
	}
}
