using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class LuxWater_CameraDepthMode : MonoBehaviour
{
	public bool GrabDepthTexture;

	private Camera cam;

	private Material CopyDepthMat;

	private RenderTextureFormat format;

	private Dictionary<Camera, CommandBuffer> m_cmdBuffer = new Dictionary<Camera, CommandBuffer>();

	private bool CamCallBackAdded;

	[HideInInspector]
	public bool ShowShaderWarning = true;

	private void OnEnable()
	{
		cam = GetComponent<Camera>();
		cam.depthTextureMode |= DepthTextureMode.Depth;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnPrecull));
			CamCallBackAdded = true;
			CopyDepthMat = new Material(Shader.Find("Hidden/Lux Water/CopyDepth"));
			format = RenderTextureFormat.RFloat;
			if (!SystemInfo.SupportsRenderTextureFormat(format))
			{
				format = RenderTextureFormat.RHalf;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(format))
			{
				format = RenderTextureFormat.ARGBHalf;
			}
		}
	}

	private void OnDisable()
	{
		if (CamCallBackAdded)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnPrecull));
			foreach (KeyValuePair<Camera, CommandBuffer> item in m_cmdBuffer)
			{
				if (item.Key != null)
				{
					item.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, item.Value);
				}
			}
			m_cmdBuffer.Clear();
		}
		ShowShaderWarning = true;
	}

	private void OnPrecull(Camera camera)
	{
		if (!GrabDepthTexture)
		{
			return;
		}
		if (cam.actualRenderingPath == RenderingPath.DeferredShading && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			if (!m_cmdBuffer.TryGetValue(camera, out var value))
			{
				value = new CommandBuffer();
				value.name = "Lux Water Grab Depth";
				camera.AddCommandBuffer(CameraEvent.BeforeSkybox, value);
				m_cmdBuffer[camera] = value;
			}
			value.Clear();
			int pixelWidth = camera.pixelWidth;
			int pixelHeight = camera.pixelHeight;
			int num = Shader.PropertyToID("_Lux_GrabbedDepth");
			value.GetTemporaryRT(num, pixelWidth, pixelHeight, 0, FilterMode.Point, format, RenderTextureReadWrite.Linear);
			value.Blit(BuiltinRenderTextureType.CurrentActive, num, CopyDepthMat, 0);
			value.ReleaseTemporaryRT(num);
			return;
		}
		GrabDepthTexture = false;
		foreach (KeyValuePair<Camera, CommandBuffer> item in m_cmdBuffer)
		{
			if (item.Key != null)
			{
				item.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, item.Value);
			}
		}
		m_cmdBuffer.Clear();
		ShowShaderWarning = true;
	}
}
