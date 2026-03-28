using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi;

[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Instanced Particle Renderer")]
[RequireComponent(typeof(ObiActor))]
public class ObiInstancedParticleRenderer : MonoBehaviour
{
	public bool render = true;

	public Mesh mesh;

	public Material material;

	public Vector3 instanceScale = Vector3.one;

	private ObiActor actor;

	private List<Matrix4x4> matrices = new List<Matrix4x4>();

	private List<Vector4> colors = new List<Vector4>();

	private MaterialPropertyBlock mpb;

	private int meshesPerBatch;

	private int batchCount;

	public void Awake()
	{
		actor = GetComponent<ObiActor>();
	}

	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(ScenePreCull));
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(ScenePreCull));
	}

	private void ScenePreCull(Camera cam)
	{
		if (mesh == null || material == null || !render || !base.isActiveAndEnabled || !actor.isActiveAndEnabled || !actor.Initialized)
		{
			return;
		}
		_ = actor.Solver;
		meshesPerBatch = 1023;
		batchCount = actor.positions.Length / meshesPerBatch + 1;
		meshesPerBatch = Mathf.Min(meshesPerBatch, actor.positions.Length);
		for (int i = 0; i < batchCount; i++)
		{
			matrices.Clear();
			colors.Clear();
			mpb = new MaterialPropertyBlock();
			int num = Mathf.Min((i + 1) * meshesPerBatch, actor.active.Length);
			for (int j = i * meshesPerBatch; j < num; j++)
			{
				if (actor.active[j])
				{
					actor.GetParticleAnisotropy(j, out var b, out var b2, out var b3);
					matrices.Add(Matrix4x4.TRS(actor.GetParticlePosition(j), actor.GetParticleOrientation(j), Vector3.Scale(new Vector3(b[3], b2[3], b3[3]), instanceScale)));
					colors.Add((actor.colors != null && j < actor.colors.Length) ? actor.colors[j] : Color.white);
				}
			}
			if (colors.Count > 0)
			{
				mpb.SetVectorArray("_Color", colors);
			}
			Graphics.DrawMeshInstanced(mesh, 0, material, matrices, mpb);
		}
	}
}
