using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects;

public struct Uint8x4
{
	public uint PackedData;

	public Uint8x4(Color32 color)
	{
		PackedData = (uint)(color.r | (color.g << 8) | (color.b << 16) | (color.a << 24));
	}

	public static implicit operator Uint8x4(Color32 v)
	{
		return new Uint8x4(v);
	}
}
