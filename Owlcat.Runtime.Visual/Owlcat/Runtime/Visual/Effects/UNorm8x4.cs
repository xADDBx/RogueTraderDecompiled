using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects;

public struct UNorm8x4
{
	public uint PackedData;

	public UNorm8x4(Color32 color)
	{
		PackedData = (uint)(color.r | (color.g << 8) | (color.b << 16) | (color.a << 24));
	}

	public static implicit operator UNorm8x4(Color32 v)
	{
		return new UNorm8x4(v);
	}
}
