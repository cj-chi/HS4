using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering;

internal struct MaterialPermutation : IEquatable<MaterialPermutation>
{
	public readonly WetDecalMode Mode;

	public readonly LayerMode LayerMode;

	public readonly ProjectionMode LayerProjectionMode;

	public readonly DecalShape Shape;

	public readonly bool EnableJitter;

	public int RenderOrder
	{
		get
		{
			if (Mode != WetDecalMode.Wet)
			{
				return 1;
			}
			return 0;
		}
	}

	public MaterialPermutation(WetDecalMode mode, LayerMode layerMode, ProjectionMode layerProjectionMode, DecalShape shape, bool enableJitter)
	{
		Mode = mode;
		LayerMode = layerMode;
		LayerProjectionMode = layerProjectionMode;
		Shape = shape;
		EnableJitter = enableJitter;
	}

	public Shader SelectShader(Shader wet, Shader dry)
	{
		if (Mode != WetDecalMode.Wet)
		{
			return dry;
		}
		return wet;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MaterialPermutation))
		{
			return false;
		}
		return Equals((MaterialPermutation)obj);
	}

	public bool Equals(MaterialPermutation other)
	{
		if (EqualityComparer<WetDecalMode>.Default.Equals(Mode, other.Mode) && EqualityComparer<LayerMode>.Default.Equals(LayerMode, other.LayerMode) && EqualityComparer<ProjectionMode>.Default.Equals(LayerProjectionMode, other.LayerProjectionMode) && EqualityComparer<DecalShape>.Default.Equals(Shape, other.Shape))
		{
			return EnableJitter == other.EnableJitter;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((409612459 * -1521134295 + Mode.GetHashCode()) * -1521134295 + LayerMode.GetHashCode()) * -1521134295 + LayerProjectionMode.GetHashCode()) * -1521134295 + Shape.GetHashCode()) * -1521134295 + EnableJitter.GetHashCode();
	}

	public static bool operator ==(MaterialPermutation permutation1, MaterialPermutation permutation2)
	{
		return permutation1.Equals(permutation2);
	}

	public static bool operator !=(MaterialPermutation permutation1, MaterialPermutation permutation2)
	{
		return !(permutation1 == permutation2);
	}
}
