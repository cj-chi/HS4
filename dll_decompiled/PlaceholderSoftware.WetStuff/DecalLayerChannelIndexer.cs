using System;

namespace PlaceholderSoftware.WetStuff;

public struct DecalLayerChannelIndexer
{
	private readonly DecalLayer _layer;

	public DecalLayerChannel this[int index] => index switch
	{
		1 => _layer.Channel1, 
		2 => _layer.Channel2, 
		3 => _layer.Channel3, 
		4 => _layer.Channel4, 
		_ => throw new IndexOutOfRangeException("Channel index must be 1, 2, 3, or 4"), 
	};

	public DecalLayerChannelIndexer(DecalLayer layer)
	{
		_layer = layer;
	}
}
