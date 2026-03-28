using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater;

public class LuxWater_WaterVolume : MonoBehaviour
{
	public delegate void TriggerEnter();

	public delegate void TriggerExit();

	[Space(6f)]
	[LuxWater_HelpBtn("h.86taxuhovssb")]
	public Mesh WaterVolumeMesh;

	[Space(8f)]
	public bool SlidingVolume;

	public float GridSize = 10f;

	private LuxWater_UnderWaterRendering waterrendermanager;

	private bool readyToGo;

	private int ID;

	public static event TriggerEnter OnEnterWaterVolume;

	public static event TriggerExit OnExitWaterVolume;

	private void OnEnable()
	{
		if (!(WaterVolumeMesh == null))
		{
			ID = GetInstanceID();
			Invoke("Register", 0f);
			Renderer component = GetComponent<Renderer>();
			component.shadowCastingMode = ShadowCastingMode.Off;
			Material sharedMaterial = component.sharedMaterial;
			sharedMaterial.EnableKeyword("USINGWATERVOLUME");
			sharedMaterial.SetFloat("_WaterSurfaceYPos", base.transform.position.y);
		}
	}

	private void OnDisable()
	{
		if ((bool)waterrendermanager)
		{
			waterrendermanager.DeRegisterWaterVolume(this, ID);
		}
		readyToGo = false;
		GetComponent<Renderer>().sharedMaterial.DisableKeyword("USINGWATERVOLUME");
	}

	private void Register()
	{
		if (LuxWater_UnderWaterRendering.instance != null)
		{
			waterrendermanager = LuxWater_UnderWaterRendering.instance;
			bool isVisible = GetComponent<Renderer>().isVisible;
			waterrendermanager.RegisterWaterVolume(this, ID, isVisible, SlidingVolume);
			readyToGo = true;
		}
		else
		{
			Invoke("Register", 0f);
		}
	}

	private void OnBecameVisible()
	{
		if (readyToGo)
		{
			waterrendermanager.SetWaterVisible(ID);
		}
	}

	private void OnBecameInvisible()
	{
		if (readyToGo)
		{
			waterrendermanager.SetWaterInvisible(ID);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		LuxWater_WaterVolumeTrigger component = other.GetComponent<LuxWater_WaterVolumeTrigger>();
		if (component != null && waterrendermanager != null && readyToGo && component.active)
		{
			waterrendermanager.EnteredWaterVolume(this, ID, component.cam, GridSize);
			if (LuxWater_WaterVolume.OnEnterWaterVolume != null)
			{
				LuxWater_WaterVolume.OnEnterWaterVolume();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		LuxWater_WaterVolumeTrigger component = other.GetComponent<LuxWater_WaterVolumeTrigger>();
		if (component != null && waterrendermanager != null && readyToGo && component.active)
		{
			waterrendermanager.EnteredWaterVolume(this, ID, component.cam, GridSize);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		LuxWater_WaterVolumeTrigger component = other.GetComponent<LuxWater_WaterVolumeTrigger>();
		if (component != null && waterrendermanager != null && readyToGo && component.active)
		{
			waterrendermanager.LeftWaterVolume(this, ID, component.cam);
			if (LuxWater_WaterVolume.OnExitWaterVolume != null)
			{
				LuxWater_WaterVolume.OnExitWaterVolume();
			}
		}
	}
}
