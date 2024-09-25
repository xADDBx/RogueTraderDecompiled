using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct LineVertex
{
	public static readonly VertexAttributeDescriptor[] kAttributes = new VertexAttributeDescriptor[4]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 2),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float16, 2)
	};

	public float3 position;

	public half2 spatialUv;

	public half2 segmentedUv;

	public half2 segmentedLengthSpatialLength;

	public override string ToString()
	{
		return position.ToString();
	}
}
