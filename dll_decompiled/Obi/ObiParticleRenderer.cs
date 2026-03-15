using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi;

[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Particle Renderer")]
[RequireComponent(typeof(ObiActor))]
public class ObiParticleRenderer : MonoBehaviour
{
	public bool render = true;

	public Shader shader;

	public Color particleColor = Color.white;

	public float radiusScale = 1f;

	private ObiActor actor;

	private List<Mesh> meshes = new List<Mesh>();

	private Material material;

	private List<Vector3> vertices = new List<Vector3>(4000);

	private List<Vector3> normals = new List<Vector3>(4000);

	private List<Color> colors = new List<Color>(4000);

	private List<int> triangles = new List<int>(6000);

	private List<Vector4> anisotropy1 = new List<Vector4>(4000);

	private List<Vector4> anisotropy2 = new List<Vector4>(4000);

	private List<Vector4> anisotropy3 = new List<Vector4>(4000);

	private int particlesPerDrawcall;

	private int drawcallCount;

	private Vector3 particleOffset0 = new Vector3(1f, 1f, 0f);

	private Vector3 particleOffset1 = new Vector3(-1f, 1f, 0f);

	private Vector3 particleOffset2 = new Vector3(-1f, -1f, 0f);

	private Vector3 particleOffset3 = new Vector3(1f, -1f, 0f);

	public IEnumerable<Mesh> ParticleMeshes => meshes;

	public Material ParticleMaterial => material;

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
		ClearMeshes();
		UnityEngine.Object.DestroyImmediate(material);
	}

	private void CreateMaterialIfNeeded()
	{
		if (shader != null)
		{
			_ = shader.isSupported;
			if (material == null || material.shader != shader)
			{
				UnityEngine.Object.DestroyImmediate(material);
				material = new Material(shader);
				material.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}

	private void ScenePreCull(Camera cam)
	{
		if (!base.isActiveAndEnabled || !actor.isActiveAndEnabled || !actor.Initialized)
		{
			ClearMeshes();
			return;
		}
		CreateMaterialIfNeeded();
		_ = actor.Solver;
		particlesPerDrawcall = 16250;
		drawcallCount = actor.positions.Length / particlesPerDrawcall + 1;
		particlesPerDrawcall = Mathf.Min(particlesPerDrawcall, actor.positions.Length);
		if (drawcallCount != meshes.Count)
		{
			ClearMeshes();
			for (int i = 0; i < drawcallCount; i++)
			{
				Mesh mesh = new Mesh();
				mesh.name = "Particle imposters";
				mesh.hideFlags = HideFlags.HideAndDontSave;
				mesh.MarkDynamic();
				meshes.Add(mesh);
			}
		}
		for (int j = 0; j < drawcallCount; j++)
		{
			vertices.Clear();
			normals.Clear();
			colors.Clear();
			triangles.Clear();
			anisotropy1.Clear();
			anisotropy2.Clear();
			anisotropy3.Clear();
			int num = 0;
			int num2 = Mathf.Min((j + 1) * particlesPerDrawcall, actor.active.Length);
			for (int k = j * particlesPerDrawcall; k < num2; k++)
			{
				if (actor.active[k])
				{
					Vector3 particlePosition = actor.GetParticlePosition(k);
					actor.GetParticleAnisotropy(k, out var b, out var b2, out var b3);
					Color item = ((actor.colors != null && k < actor.colors.Length) ? actor.colors[k] : Color.white);
					vertices.Add(particlePosition);
					vertices.Add(particlePosition);
					vertices.Add(particlePosition);
					vertices.Add(particlePosition);
					normals.Add(particleOffset0);
					normals.Add(particleOffset1);
					normals.Add(particleOffset2);
					normals.Add(particleOffset3);
					colors.Add(item);
					colors.Add(item);
					colors.Add(item);
					colors.Add(item);
					anisotropy1.Add(b);
					anisotropy1.Add(b);
					anisotropy1.Add(b);
					anisotropy1.Add(b);
					anisotropy2.Add(b2);
					anisotropy2.Add(b2);
					anisotropy2.Add(b2);
					anisotropy2.Add(b2);
					anisotropy3.Add(b3);
					anisotropy3.Add(b3);
					anisotropy3.Add(b3);
					anisotropy3.Add(b3);
					triangles.Add(num + 2);
					triangles.Add(num + 1);
					triangles.Add(num);
					triangles.Add(num + 3);
					triangles.Add(num + 2);
					triangles.Add(num);
					num += 4;
				}
			}
			Apply(meshes[j]);
		}
		DrawParticles();
	}

	private void DrawParticles()
	{
		if (!(material != null))
		{
			return;
		}
		material.SetFloat("_RadiusScale", radiusScale);
		material.SetColor("_Color", particleColor);
		if (!render)
		{
			return;
		}
		foreach (Mesh mesh in meshes)
		{
			Graphics.DrawMesh(mesh, Matrix4x4.identity, material, base.gameObject.layer);
		}
	}

	private void Apply(Mesh mesh)
	{
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetColors(colors);
		mesh.SetUVs(0, anisotropy1);
		mesh.SetUVs(1, anisotropy2);
		mesh.SetUVs(2, anisotropy3);
		mesh.SetTriangles(triangles, 0, calculateBounds: true);
	}

	private void ClearMeshes()
	{
		foreach (Mesh mesh in meshes)
		{
			UnityEngine.Object.DestroyImmediate(mesh);
		}
		meshes.Clear();
	}
}
