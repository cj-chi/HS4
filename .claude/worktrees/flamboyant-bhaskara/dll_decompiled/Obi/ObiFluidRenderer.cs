using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Obi;

public class ObiFluidRenderer : ObiBaseFluidRenderer
{
	[Range(0f, 0.1f)]
	public float blurRadius = 0.02f;

	[Range(0.01f, 2f)]
	public float thicknessCutoff = 1.2f;

	private Material depth_BlurMaterial;

	private Material normal_ReconstructMaterial;

	private Material thickness_Material;

	private Color thicknessBufferClear = new Color(1f, 1f, 1f, 0f);

	public Material colorMaterial;

	public Material fluidMaterial;

	protected override void Setup()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		if (depth_BlurMaterial == null)
		{
			depth_BlurMaterial = CreateMaterial(Shader.Find("Hidden/ScreenSpaceCurvatureFlow"));
		}
		if (normal_ReconstructMaterial == null)
		{
			normal_ReconstructMaterial = CreateMaterial(Shader.Find("Hidden/NormalReconstruction"));
		}
		if (thickness_Material == null)
		{
			thickness_Material = CreateMaterial(Shader.Find("Hidden/FluidThickness"));
		}
		if (!depth_BlurMaterial || !normal_ReconstructMaterial || !thickness_Material || !SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			base.enabled = false;
			return;
		}
		Shader.SetGlobalMatrix("_Camera_to_World", currentCam.cameraToWorldMatrix);
		Shader.SetGlobalMatrix("_World_to_Camera", currentCam.worldToCameraMatrix);
		Shader.SetGlobalMatrix("_InvProj", currentCam.projectionMatrix.inverse);
		float fieldOfView = currentCam.fieldOfView;
		float farClipPlane = currentCam.farClipPlane;
		float num = (currentCam.orthographic ? (2f * currentCam.orthographicSize) : (2f * Mathf.Tan(fieldOfView * ((float)Math.PI / 180f) * 0.5f) * farClipPlane));
		float x = num * currentCam.aspect;
		Shader.SetGlobalVector("_FarCorner", new Vector3(x, num, farClipPlane));
		depth_BlurMaterial.SetFloat("_BlurScale", currentCam.orthographic ? 1f : ((float)currentCam.pixelWidth / currentCam.aspect * (1f / Mathf.Tan(fieldOfView * ((float)Math.PI / 180f) * 0.5f))));
		depth_BlurMaterial.SetFloat("_BlurRadiusWorldspace", blurRadius);
		if (fluidMaterial != null)
		{
			fluidMaterial.SetFloat("_ThicknessCutoff", thicknessCutoff);
		}
	}

	protected override void Cleanup()
	{
		if (depth_BlurMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(depth_BlurMaterial);
		}
		if (normal_ReconstructMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(normal_ReconstructMaterial);
		}
		if (thickness_Material != null)
		{
			UnityEngine.Object.DestroyImmediate(thickness_Material);
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
		int num2 = Shader.PropertyToID("_Foam");
		int num3 = Shader.PropertyToID("_FluidDepthTexture");
		int num4 = Shader.PropertyToID("_FluidThickness1");
		int num5 = Shader.PropertyToID("_FluidThickness2");
		int num6 = Shader.PropertyToID("_FluidSurface");
		int num7 = Shader.PropertyToID("_FluidNormals");
		renderFluid.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear);
		renderFluid.GetTemporaryRT(num2, -1, -1, 0, FilterMode.Bilinear);
		renderFluid.GetTemporaryRT(num3, -1, -1, 24, FilterMode.Point, RenderTextureFormat.Depth);
		renderFluid.GetTemporaryRT(num4, -2, -2, 16, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
		renderFluid.GetTemporaryRT(num5, -2, -2, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
		renderFluid.GetTemporaryRT(num6, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
		renderFluid.GetTemporaryRT(num7, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
		renderFluid.Blit(BuiltinRenderTextureType.CurrentActive, num);
		renderFluid.SetRenderTarget(num3);
		renderFluid.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		ObiParticleRenderer[] array = particleRenderers;
		foreach (ObiParticleRenderer obiParticleRenderer in array)
		{
			if (!(obiParticleRenderer != null))
			{
				continue;
			}
			foreach (Mesh particleMesh in obiParticleRenderer.ParticleMeshes)
			{
				if (obiParticleRenderer.ParticleMaterial != null)
				{
					renderFluid.DrawMesh(particleMesh, Matrix4x4.identity, obiParticleRenderer.ParticleMaterial, 0, 0);
				}
			}
		}
		renderFluid.SetRenderTarget(num4);
		renderFluid.ClearRenderTarget(clearDepth: true, clearColor: true, thicknessBufferClear);
		array = particleRenderers;
		foreach (ObiParticleRenderer obiParticleRenderer2 in array)
		{
			if (!(obiParticleRenderer2 != null))
			{
				continue;
			}
			renderFluid.SetGlobalColor("_ParticleColor", obiParticleRenderer2.particleColor);
			renderFluid.SetGlobalFloat("_RadiusScale", obiParticleRenderer2.radiusScale);
			foreach (Mesh particleMesh2 in obiParticleRenderer2.ParticleMeshes)
			{
				renderFluid.DrawMesh(particleMesh2, Matrix4x4.identity, thickness_Material, 0, 0);
				renderFluid.DrawMesh(particleMesh2, Matrix4x4.identity, colorMaterial, 0, 0);
			}
		}
		renderFluid.Blit(num4, num5, thickness_Material, 1);
		renderFluid.Blit(num5, num4, thickness_Material, 2);
		renderFluid.SetRenderTarget(num2);
		renderFluid.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		array = particleRenderers;
		foreach (ObiParticleRenderer obiParticleRenderer3 in array)
		{
			if (!(obiParticleRenderer3 != null))
			{
				continue;
			}
			ObiFoamGenerator component = obiParticleRenderer3.GetComponent<ObiFoamGenerator>();
			if (component != null && component.advector != null && component.advector.Particles != null)
			{
				ParticleSystemRenderer component2 = component.advector.Particles.GetComponent<ParticleSystemRenderer>();
				if (component2 != null)
				{
					renderFluid.DrawRenderer(component2, component2.material);
				}
			}
		}
		renderFluid.Blit(num3, num6, depth_BlurMaterial);
		renderFluid.Blit(num6, num7, normal_ReconstructMaterial);
		renderFluid.SetGlobalTexture("_FluidDepth", num3);
		renderFluid.SetGlobalTexture("_Foam", num2);
		renderFluid.SetGlobalTexture("_Refraction", num);
		renderFluid.SetGlobalTexture("_Thickness", num4);
		renderFluid.SetGlobalTexture("_Normals", num7);
		renderFluid.Blit(num6, BuiltinRenderTextureType.CameraTarget, fluidMaterial);
		renderFluid.ReleaseTemporaryRT(num3);
	}
}
