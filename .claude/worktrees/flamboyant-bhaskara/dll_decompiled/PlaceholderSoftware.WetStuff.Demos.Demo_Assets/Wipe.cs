using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets;

[RequireComponent(typeof(WetStuff))]
public class Wipe : MonoBehaviour
{
	private Mesh _mesh;

	private Material _material;

	private WetStuff _decals;

	private Camera _camera;

	[Range(0f, 1f)]
	public float Progress;

	private void Start()
	{
		Shader shader = Shader.Find("Demo/ExcludeWetness");
		_material = new Material(shader);
		_mesh = CreateFullscreenQuad();
		_decals = GetComponent<WetStuff>();
		_camera = GetComponent<Camera>();
		_decals.AfterDecalRender += RecordCommandBuffer;
	}

	private static Mesh CreateFullscreenQuad()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, -1f, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		Mesh mesh2 = mesh;
		mesh2.SetIndices(new int[6] { 0, 1, 2, 2, 3, 0 }, MeshTopology.Triangles, 0);
		return mesh2;
	}

	private void OnDestroy()
	{
		if (_decals != null)
		{
			_decals.AfterDecalRender -= RecordCommandBuffer;
		}
	}

	private void RecordCommandBuffer(CommandBuffer cmd)
	{
		if (_camera.enabled && !_camera.stereoEnabled)
		{
			float x = Mathf.Lerp(-2f, 2f, Progress);
			cmd.SetRenderTarget(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget);
			cmd.DrawMesh(_mesh, Matrix4x4.Translate(new Vector3(x, 0f, 0f)), _material);
		}
	}

	public void OnGUI()
	{
		if (!_camera.enabled)
		{
			return;
		}
		if (XRSettings.enabled && _camera.stereoEnabled)
		{
			if (GUI.Button(new Rect(20f, 20f, 200f, 30f), "Toggle Decals"))
			{
				_decals.enabled = !_decals.enabled;
			}
		}
		else
		{
			float width = GUILayoutUtility.GetRect(float.MaxValue, 1f).width;
			Rect position = new Rect(170f, 20f, width - 190f, 30f);
			Progress = GUI.HorizontalSlider(position, Progress, 0f, 0.5f);
			GUI.Label(new Rect(20f, 15f, 160f, 30f), "Remove Wetness Effect:");
		}
	}
}
