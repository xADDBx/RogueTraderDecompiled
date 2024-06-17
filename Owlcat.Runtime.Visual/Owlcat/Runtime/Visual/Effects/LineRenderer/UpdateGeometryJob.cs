using Owlcat.Runtime.Core.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

[BurstCompile]
public struct UpdateGeometryJob : IJobParallelFor
{
	[ReadOnly]
	internal NativeArray<GeometryLineDescriptor> GeometryLineDescriptors;

	[ReadOnly]
	internal NativeArray<LineDescriptor> LineDescriptors;

	[ReadOnly]
	public NativeArray<Point> Points;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<VertexColorUv> VerticesUnlit;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<VertexColorUvNormalTangent> VerticesLit;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> Indices;

	[WriteOnly]
	public NativeArray<Bounds> LineBounds;

	public JobGradient ColorOverLength;

	public JobAnimationCurve WidthCurve;

	public float WidthScale;

	public Space Space;

	public float4x4 InvWorld;

	public float3 Up;

	public float3 CameraPosition;

	public LineTextureMode TextureMode;

	public LineAlignment Alignment;

	public bool GenerateLightingData;

	public bool MeshIndicesIsDirty;

	public void Execute(int index)
	{
		GeometryLineDescriptor geometryLineDescriptor = GeometryLineDescriptors[index];
		LineDescriptor lineDescriptor = LineDescriptors[index];
		int positionCount = lineDescriptor.PositionCount;
		float num = 0f;
		Bounds value = default(Bounds);
		if (positionCount < 2)
		{
			LineBounds[index] = value;
			return;
		}
		for (int i = 0; i < positionCount - 1; i++)
		{
			int num2 = lineDescriptor.PositionsOffset + i;
			num += math.distance(Points[num2].Position, Points[num2 + 1].Position);
		}
		float3 @float = 0;
		float num3 = 0f;
		for (int j = 0; j < positionCount; j++)
		{
			int num4 = lineDescriptor.PositionsOffset + j;
			Point point = Points[num4];
			float3 float2 = ((j + 1 >= positionCount) ? @float : math.normalize(Points[num4 + 1].Position - point.Position));
			if (j == 0)
			{
				@float = float2;
			}
			if (float.IsNaN(float2.x) || float.IsNaN(float2.y) || float.IsNaN(float2.z))
			{
				float2 = new float3(0f, 1f, 0f);
			}
			if (j > 0)
			{
				num3 += math.distance(point.Position, Points[num4 - 1].Position);
			}
			float num5 = num3 / num;
			float3 float3 = Up;
			if (Alignment == LineAlignment.Camera)
			{
				float3 = math.normalize(CameraPosition - point.Position);
				float3 = math.cross(math.cross(float2, float3), float2);
			}
			float3 x = math.normalize(float2 + @float);
			float3 float4 = math.normalize(math.cross(float2, float3));
			float3 float5;
			if (math.any(math.isnan(float4)))
			{
				float5 = 0;
			}
			else
			{
				float5 = math.normalize(math.cross(x, float3));
				float num6 = WidthCurve.Evaluate(num5) * 0.5f * lineDescriptor.WidthScale * WidthScale;
				float num7 = num6 / math.dot(float5, float4);
				float5 *= num7;
				float5 = math.normalize(float5) * math.min(math.length(float5), num6 * 2f);
			}
			float3 float6 = point.Position - float5;
			float3 float7 = point.Position + float5;
			if (Space == Space.World)
			{
				float6 = math.mul(InvWorld, new float4(float6, 1f)).xyz;
				float7 = math.mul(InvWorld, new float4(float7, 1f)).xyz;
			}
			@float = float2;
			int num8 = geometryLineDescriptor.VertexOffset + j * 2;
			if (GenerateLightingData)
			{
				VertexColorUvNormalTangent vertexColorUvNormalTangent = default(VertexColorUvNormalTangent);
				VertexColorUvNormalTangent value2 = vertexColorUvNormalTangent;
				vertexColorUvNormalTangent.Pos = float6;
				value2.Pos = float7;
				vertexColorUvNormalTangent.Normal.xyz = (half3)float3;
				vertexColorUvNormalTangent.Normal.w = (half)0f;
				value2.Normal = vertexColorUvNormalTangent.Normal;
				vertexColorUvNormalTangent.Tangent.xyz = (half3)float4;
				vertexColorUvNormalTangent.Tangent.w = (half)0f;
				value2.Tangent = vertexColorUvNormalTangent.Tangent;
				Color color = ColorOverLength.Evaluate(num5);
				color.a *= point.Alpha;
				vertexColorUvNormalTangent.Color = (Color32)color;
				value2.Color = vertexColorUvNormalTangent.Color;
				switch (TextureMode)
				{
				case LineTextureMode.Stretch:
					vertexColorUvNormalTangent.Uv = new float2(num5, 0f);
					value2.Uv = new float2(num5, 1f);
					break;
				case LineTextureMode.Tile:
					vertexColorUvNormalTangent.Uv = new float2(num3, 0f);
					value2.Uv = new float2(num3, 1f);
					break;
				case LineTextureMode.RepeatPerSegment:
					vertexColorUvNormalTangent.Uv = new float2(j, 0f);
					value2.Uv = new float2(j, 1f);
					break;
				}
				vertexColorUvNormalTangent.Uv += lineDescriptor.UvOffset;
				value2.Uv += lineDescriptor.UvOffset;
				VerticesLit[num8] = vertexColorUvNormalTangent;
				VerticesLit[num8 + 1] = value2;
				value.Encapsulate(vertexColorUvNormalTangent.Pos);
				value.Encapsulate(value2.Pos);
			}
			else
			{
				VertexColorUv vertexColorUv = default(VertexColorUv);
				VertexColorUv value3 = vertexColorUv;
				vertexColorUv.Pos = float6;
				value3.Pos = float7;
				Color color2 = ColorOverLength.Evaluate(num5);
				color2.a *= point.Alpha;
				vertexColorUv.Color = (Color32)color2;
				value3.Color = vertexColorUv.Color;
				switch (TextureMode)
				{
				case LineTextureMode.Stretch:
					vertexColorUv.Uv = new float2(num5, 0f);
					value3.Uv = new float2(num5, 1f);
					break;
				case LineTextureMode.Tile:
					vertexColorUv.Uv = new float2(num3, 0f);
					value3.Uv = new float2(num3, 1f);
					break;
				case LineTextureMode.RepeatPerSegment:
					vertexColorUv.Uv = new float2(j, 0f);
					value3.Uv = new float2(j, 1f);
					break;
				}
				vertexColorUv.Uv += lineDescriptor.UvOffset;
				value3.Uv += lineDescriptor.UvOffset;
				VerticesUnlit[num8] = vertexColorUv;
				VerticesUnlit[num8 + 1] = value3;
				value.Encapsulate(vertexColorUv.Pos);
				value.Encapsulate(value3.Pos);
			}
			if (MeshIndicesIsDirty && j < positionCount - 1)
			{
				int num9 = geometryLineDescriptor.IndexOffset + j * 6;
				Indices[num9] = (uint)(geometryLineDescriptor.VertexOffset + j * 2);
				Indices[num9 + 1] = (uint)(geometryLineDescriptor.VertexOffset + j * 2 + 1);
				Indices[num9 + 2] = (uint)(geometryLineDescriptor.VertexOffset + j * 2 + 2);
				Indices[num9 + 3] = (uint)(geometryLineDescriptor.VertexOffset + j * 2 + 2);
				Indices[num9 + 4] = (uint)(geometryLineDescriptor.VertexOffset + j * 2 + 1);
				Indices[num9 + 5] = (uint)(geometryLineDescriptor.VertexOffset + j * 2 + 3);
			}
		}
		LineBounds[index] = value;
	}
}
