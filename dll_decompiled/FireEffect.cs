using System.Collections.Generic;
using UnityEngine;

public class FireEffect : MonoBehaviour
{
	public enum Axis
	{
		X,
		Y
	}

	private class FireRendererData
	{
		public Vector4 ScrollTex_ST;

		public MaterialPropertyBlock ScrollBlock;

		public float ScrollSpeed;
	}

	public float TilingX = 0.5f;

	public float TilingY = 0.5f;

	public float ScrollSpeedMin = -1f;

	public float ScrollSpeedMax = -0.5f;

	public Axis ScrollAxis = Axis.Y;

	public float TemperatureCoarse = 1f;

	public float TemperatureDetail = 1f;

	public float Opacity = 1f;

	public Renderer[] Renderers;

	private List<FireRendererData> _RendererData;

	private void Initialize()
	{
		if (Renderers == null)
		{
			return;
		}
		_RendererData = new List<FireRendererData>();
		for (int i = 0; i < Renderers.Length; i++)
		{
			FireRendererData fireRendererData = new FireRendererData();
			fireRendererData.ScrollSpeed = Random.Range(ScrollSpeedMin, ScrollSpeedMax);
			float z;
			float w;
			if (ScrollAxis == Axis.Y)
			{
				z = 0.5f - TilingX * 0.5f;
				w = Random.Range(0f, 1f);
			}
			else
			{
				w = 0.5f - TilingY * 0.5f;
				z = Random.Range(0f, 1f);
			}
			fireRendererData.ScrollTex_ST = new Vector4(TilingX, TilingY, z, w);
			fireRendererData.ScrollBlock = new MaterialPropertyBlock();
			fireRendererData.ScrollBlock.SetVector("_ScrollTex_ST", fireRendererData.ScrollTex_ST);
			fireRendererData.ScrollBlock.SetFloat("_OpacityCoarse", Opacity);
			fireRendererData.ScrollBlock.SetFloat("_OpacityDetail", 1f);
			fireRendererData.ScrollBlock.SetFloat("_TemperatureCoarse", TemperatureCoarse);
			fireRendererData.ScrollBlock.SetFloat("_TemperatureDetail", TemperatureDetail);
			_RendererData.Add(fireRendererData);
		}
	}

	private void Reset()
	{
		Renderers = GetComponentsInChildren<Renderer>();
	}

	private void Start()
	{
		Initialize();
	}

	private void OnValidate()
	{
		Initialize();
		Update();
	}

	private void Update()
	{
		if (_RendererData == null)
		{
			return;
		}
		for (int i = 0; i < _RendererData.Count; i++)
		{
			FireRendererData fireRendererData = _RendererData[i];
			Renderer renderer = Renderers[i];
			if (renderer != null)
			{
				if (ScrollAxis == Axis.Y)
				{
					fireRendererData.ScrollTex_ST.w += Time.deltaTime * fireRendererData.ScrollSpeed;
				}
				else
				{
					fireRendererData.ScrollTex_ST.z += Time.deltaTime * fireRendererData.ScrollSpeed;
				}
				fireRendererData.ScrollBlock.SetVector("_ScrollTex_ST", fireRendererData.ScrollTex_ST);
				fireRendererData.ScrollBlock.SetFloat("_OpacityCoarse", Opacity);
				fireRendererData.ScrollBlock.SetFloat("_OpacityDetail", 1f);
				fireRendererData.ScrollBlock.SetFloat("_TemperatureCoarse", TemperatureCoarse);
				fireRendererData.ScrollBlock.SetFloat("_TemperatureDetail", TemperatureDetail);
				renderer.SetPropertyBlock(fireRendererData.ScrollBlock);
			}
		}
	}
}
