using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater;

public class LuxWater_Projector : MonoBehaviour
{
	public enum ProjectorType
	{
		FoamProjector,
		NormalProjector
	}

	[Space(8f)]
	public ProjectorType Type;

	[NonSerialized]
	public static List<LuxWater_Projector> FoamProjectors = new List<LuxWater_Projector>();

	[NonSerialized]
	public static List<LuxWater_Projector> NormalProjectors = new List<LuxWater_Projector>();

	[NonSerialized]
	public Renderer m_Rend;

	[NonSerialized]
	public Material m_Mat;

	private bool added;

	private Vector3 origPos;

	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y = origPos.y;
	}

	private void OnEnable()
	{
		origPos = base.transform.position;
		if (GetComponent<Renderer>() != null)
		{
			m_Rend = GetComponent<Renderer>();
			m_Mat = m_Rend.sharedMaterials[0];
			m_Rend.enabled = false;
			if (Type == ProjectorType.FoamProjector)
			{
				FoamProjectors.Add(this);
			}
			else
			{
				NormalProjectors.Add(this);
			}
			added = true;
		}
	}

	private void OnDisable()
	{
		if (added)
		{
			if (Type == ProjectorType.FoamProjector)
			{
				FoamProjectors.Remove(this);
			}
			else
			{
				NormalProjectors.Remove(this);
			}
			m_Rend.enabled = true;
		}
	}
}
