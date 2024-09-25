using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[BurstCompile]
public struct LightCookieDescriptor
{
	public int textureId;

	public int2 textureSize;

	public uint textureVersion;

	public TextureDimension textureDimension;

	public float2 uvSize;

	public float2 uvOffset;

	public override string ToString()
	{
		return $"(id:{textureId}, size:{textureSize}, version:{textureVersion}, dimension:{textureDimension}, uv_size:{uvSize}, uv_offset:{uvOffset})";
	}
}
