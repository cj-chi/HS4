using UnityEngine;
using UnityEngine.Rendering;

namespace Obi;

public class ObiSimpleFluidRenderer : ObiBaseFluidRenderer
{
	[Range(0.01f, 2f)]
	public float thicknessCutoff = 1.2f;

	private Material thickness_Material;

	public Material fluidMaterial;

	protected override void Setup()
	{
		if (thickness_Material == null)
		{
			thickness_Material = CreateMaterial(Shader.Find("Hidden/FluidThickness"));
		}
		if (!thickness_Material || !SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (fluidMaterial != null)
		{
			fluidMaterial.SetFloat("_ThicknessCutoff", thicknessCutoff);
		}
	}

	protected override void Cleanup()
	{
		if (thickness_Material != null)
		{
			Object.DestroyImmediate(thickness_Material);
		}
	}

	public override void UpdateFluidRenderingCommandBuffer()
	{
		renderFluid.Clear();
		if (particleRenderers == null || fluidMaterial == null)
		{
			return;
		}
		int num = Shader.PropertyToID("_Refraction");
		int num2 = Shader.PropertyToID("_Thickness");
		renderFluid.GetTemporaryRT(num, -2, -2, 0, FilterMode.Bilinear);
		renderFluid.GetTemporaryRT(num2, -2, -2, 0, FilterMode.Bilinear);
		renderFluid.Blit(BuiltinRenderTextureType.CurrentActive, num);
		renderFluid.SetRenderTarget(num2);
		renderFluid.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		ObiParticleRenderer[] array = particleRenderers;
		foreach (ObiParticleRenderer obiParticleRenderer in array)
		{
			if (!(obiParticleRenderer != null))
			{
				continue;
			}
			renderFluid.SetGlobalColor("_ParticleColor", obiParticleRenderer.particleColor);
			renderFluid.SetGlobalFloat("_RadiusScale", obiParticleRenderer.radiusScale);
			foreach (Mesh particleMesh in obiParticleRenderer.ParticleMeshes)
			{
				renderFluid.DrawMesh(particleMesh, Matrix4x4.identity, thickness_Material, 0, 0);
			}
		}
		renderFluid.Blit(num, BuiltinRenderTextureType.CameraTarget, fluidMaterial);
	}
}
