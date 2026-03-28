using System;
using PlaceholderSoftware.WetStuff.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff;

internal class WetAttributeModifier : IDisposable
{
	private readonly Camera _camera;

	private readonly Material _gbufferMaterial;

	private readonly Material _normalSmoothingMaterial;

	private readonly Material _blit;

	private Mesh _fullScreenQuad;

	private bool _destroyed;

	public float AmbientDarkenStrength
	{
		get
		{
			return _gbufferMaterial.GetFloat("_AmbientDarken");
		}
		set
		{
			_gbufferMaterial.SetFloat("_AmbientDarken", value);
		}
	}

	public WetAttributeModifier([NotNull] Camera camera)
	{
		if (!camera)
		{
			throw new ArgumentNullException("camera");
		}
		_gbufferMaterial = new Material(Shader.Find("Hidden/WetSurfaceModifier"))
		{
			hideFlags = HideFlags.DontSave
		};
		Shader shader = Shader.Find("Hidden/WS_BlurNormals");
		_normalSmoothingMaterial = new Material(shader)
		{
			hideFlags = HideFlags.DontSave
		};
		_blit = new Material(Shader.Find("Hidden/StereoBlit"))
		{
			hideFlags = HideFlags.DontSave
		};
		_camera = camera;
	}

	public void Dispose()
	{
		UnityEngine.Object.DestroyImmediate(_gbufferMaterial);
		UnityEngine.Object.DestroyImmediate(_blit);
		UnityEngine.Object.DestroyImmediate(_normalSmoothingMaterial);
		UnityEngine.Object.DestroyImmediate(_fullScreenQuad);
		_destroyed = true;
	}

	public void RecordCommandBuffer([NotNull] CommandBuffer cmd)
	{
		if (_destroyed)
		{
			return;
		}
		if (!_fullScreenQuad)
		{
			_fullScreenQuad = Primitives.CreateFullscreenQuad();
		}
		if (PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableNormalSmoothing)
		{
			int num = Shader.PropertyToID("_NormalsItermediateBlur");
			int nameID = Shader.PropertyToID("_Source");
			if (XRSettings.enabled && _camera.stereoEnabled)
			{
				RenderTextureDescriptor eyeTextureDesc = XRSettings.eyeTextureDesc;
				eyeTextureDesc.colorFormat = RenderTextureFormat.ARGB2101010;
				cmd.GetTemporaryRT(num, eyeTextureDesc, FilterMode.Bilinear);
			}
			else
			{
				cmd.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB2101010);
			}
			cmd.SetRenderTarget(num, BuiltinRenderTextureType.CameraTarget);
			cmd.SetGlobalTexture(nameID, BuiltinRenderTextureType.GBuffer2);
			cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _normalSmoothingMaterial, 0, PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableStencil ? 2 : 0);
			cmd.SetRenderTarget(BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget);
			cmd.SetGlobalTexture(nameID, num);
			cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _normalSmoothingMaterial, 0, (!PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableStencil) ? 1 : 3);
			cmd.ReleaseTemporaryRT(num);
		}
		int nameID2 = CopyGBufferTexture(cmd, "_GBufferSpecularCopy", BuiltinRenderTextureType.GBuffer1, _camera.allowHDR);
		cmd.SetRenderTarget(new RenderTargetIdentifier[3]
		{
			BuiltinRenderTextureType.GBuffer0,
			BuiltinRenderTextureType.GBuffer1,
			BuiltinRenderTextureType.CameraTarget
		}, BuiltinRenderTextureType.CameraTarget);
		cmd.DrawMesh(_fullScreenQuad, Matrix4x4.identity, _gbufferMaterial, 0, PlaceholderSoftware.WetStuff.Rendering.RenderSettings.Instance.EnableStencil ? 1 : 0);
		cmd.ReleaseTemporaryRT(nameID2);
	}

	private int CopyGBufferTexture([NotNull] CommandBuffer cmd, [NotNull] string shaderParameterName, BuiltinRenderTextureType texture, bool hdr)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		int num = Shader.PropertyToID(shaderParameterName);
		if (XRSettings.enabled && _camera.stereoEnabled)
		{
			RenderTextureDescriptor eyeTextureDesc = XRSettings.eyeTextureDesc;
			eyeTextureDesc.colorFormat = GBufferFormat(texture, hdr);
			eyeTextureDesc.depthBufferBits = 0;
			cmd.GetTemporaryRT(num, eyeTextureDesc, FilterMode.Point);
		}
		else
		{
			cmd.GetTemporaryRT(num, -1, -1, 0, FilterMode.Point, GBufferFormat(texture, hdr));
		}
		cmd.Blit(texture, num, _blit);
		return num;
	}

	private static RenderTextureFormat GBufferFormat(BuiltinRenderTextureType gbuffer, bool hdr)
	{
		switch (gbuffer)
		{
		case BuiltinRenderTextureType.GBuffer0:
			return RenderTextureFormat.ARGB32;
		case BuiltinRenderTextureType.GBuffer1:
			return RenderTextureFormat.ARGB32;
		case BuiltinRenderTextureType.GBuffer2:
			return RenderTextureFormat.ARGB2101010;
		case BuiltinRenderTextureType.GBuffer3:
			if (!hdr)
			{
				return RenderTextureFormat.ARGB2101010;
			}
			return RenderTextureFormat.ARGBHalf;
		default:
			throw new ArgumentException("Provided render texture type is not a GBuffer", "gbuffer");
		}
	}
}
