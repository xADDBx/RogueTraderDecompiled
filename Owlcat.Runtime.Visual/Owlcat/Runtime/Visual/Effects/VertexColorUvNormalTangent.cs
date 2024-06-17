using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Effects;

public struct VertexColorUvNormalTangent
{
	public float3 Pos;

	public half4 Normal;

	public half4 Tangent;

	public UNorm8x4 Color;

	public float2 Uv;
}
