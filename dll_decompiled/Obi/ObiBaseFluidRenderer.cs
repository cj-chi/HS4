using UnityEngine;
using UnityEngine.Rendering;

namespace Obi;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public abstract class ObiBaseFluidRenderer : MonoBehaviour
{
	public ObiParticleRenderer[] particleRenderers;

	public bool autoupdate = true;

	protected CommandBuffer renderFluid;

	protected Camera currentCam;

	private void Awake()
	{
		currentCam = GetComponent<Camera>();
	}

	public void OnEnable()
	{
		GetComponent<Camera>().forceIntoRenderTexture = true;
		DestroyCommandBuffer();
		Cleanup();
	}

	public void OnDisable()
	{
		DestroyCommandBuffer();
		Cleanup();
	}

	protected Material CreateMaterial(Shader shader)
	{
		if (!shader || !shader.isSupported)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
	}

	protected virtual void Setup()
	{
	}

	protected virtual void Cleanup()
	{
	}

	public abstract void UpdateFluidRenderingCommandBuffer();

	private void DestroyCommandBuffer()
	{
		if (renderFluid != null)
		{
			GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, renderFluid);
			renderFluid = null;
		}
	}

	private void OnPreRender()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled || particleRenderers == null || particleRenderers.Length == 0)
		{
			DestroyCommandBuffer();
			Cleanup();
			return;
		}
		Setup();
		if (renderFluid == null)
		{
			renderFluid = new CommandBuffer();
			renderFluid.name = "Render fluid";
			UpdateFluidRenderingCommandBuffer();
			currentCam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, renderFluid);
		}
		else if (autoupdate)
		{
			UpdateFluidRenderingCommandBuffer();
		}
	}
}
