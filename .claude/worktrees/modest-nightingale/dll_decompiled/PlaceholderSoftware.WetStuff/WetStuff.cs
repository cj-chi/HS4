using System;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlaceholderSoftware.WetStuff;

[ImageEffectAllowedInSceneView]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/WetStuff/")]
public class WetStuff : MonoBehaviour
{
	public static readonly SemanticVersion Version = new SemanticVersion(2, 0, 3);

	private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(WetStuff).Name);

	[SerializeField]
	[Range(0f, 1f)]
	private float _ambientModificationFactor = 0.35f;

	private bool _appliedEditorRestartHack;

	private Camera _camera;

	private CommandBuffer _cmd;

	private WetDecalRenderer _decalRenderer;

	private WetAttributeModifier _gbufferModifier;

	private bool _initialized;

	public event Action<CommandBuffer> AfterDecalRender;

	private void Startup()
	{
		if (!_initialized)
		{
			_initialized = true;
			_camera = GetComponent<Camera>();
			if (_camera.name != "SceneCamera" && _camera.actualRenderingPath != RenderingPath.DeferredShading)
			{
				Log.Error("Camera rendering path is '{0}', expected 'DeferredShading'", _camera.actualRenderingPath);
			}
			_cmd = new CommandBuffer
			{
				name = "Wet Surface Decals"
			};
			_camera.AddCommandBuffer(CameraEvent.BeforeReflections, _cmd);
			if (_camera.commandBufferCount < 1)
			{
				Log.Error("Failed to attach CommandBuffer");
			}
			_decalRenderer = new WetDecalRenderer(_camera);
			_gbufferModifier = new WetAttributeModifier(_camera)
			{
				AmbientDarkenStrength = _ambientModificationFactor
			};
		}
	}

	private void Teardown()
	{
		if (_initialized)
		{
			_initialized = false;
			_decalRenderer.Dispose();
			_gbufferModifier.Dispose();
			_camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, _cmd);
			_cmd.Dispose();
			_cmd = null;
			_decalRenderer = null;
			_gbufferModifier = null;
		}
	}

	protected virtual void OnEnable()
	{
		Startup();
	}

	protected virtual void OnDisable()
	{
		Teardown();
	}

	protected virtual void OnDestroy()
	{
		Teardown();
	}

	protected virtual void Update()
	{
		Logs.WriteMultithreadedLogs();
	}

	protected virtual void LateUpdate()
	{
		_decalRenderer.Update();
	}

	protected virtual void OnPreRender()
	{
		if (_initialized)
		{
			_cmd.Clear();
			_decalRenderer.RecordCommandBuffer(_cmd);
			OnAfterDecalRender();
			_gbufferModifier.RecordCommandBuffer(_cmd);
		}
	}

	protected virtual void OnValidate()
	{
		if (_initialized)
		{
			_gbufferModifier.AmbientDarkenStrength = _ambientModificationFactor;
		}
	}

	private void OnAfterDecalRender()
	{
		this.AfterDecalRender?.Invoke(_cmd);
	}
}
