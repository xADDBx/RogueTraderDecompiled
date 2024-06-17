using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct PathSplineMeshBuilder
{
	public struct State
	{
		public int pointsCount;

		public float3 vertexPositionOffset;

		public float spatialLength;

		public float segmentedLength;
	}

	private unsafe readonly State* m_State;

	private readonly float m_HalfThickness;

	private MeshComposer<LineVertex, uint> m_MeshComposer;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe PathSplineMeshBuilder(State* state, float halfThickness, MeshComposer<LineVertex, uint> meshComposer)
	{
		m_State = state;
		m_HalfThickness = halfThickness;
		m_MeshComposer = meshComposer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void SetVertexPositionOffset(in float3 value)
	{
		m_State->vertexPositionOffset = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void StartLine()
	{
		m_State->pointsCount = 0;
		m_State->spatialLength = 0f;
		m_State->segmentedLength = 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void PushPoint(in SplinePoint point)
	{
		PushVertices(in point);
		m_State->pointsCount++;
		m_State->spatialLength = point.spatialDistance;
		m_State->segmentedLength = point.segmentedDistance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void FinishLine()
	{
		int pointsCount = m_State->pointsCount;
		if (pointsCount > 1)
		{
			PushIndices(pointsCount);
			SetSplineLength(pointsCount);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void PushVertices(in SplinePoint point)
	{
		float3 @float = math.rotate(point.rotation, new float3(m_HalfThickness, 0f, 0f));
		float3 float2 = point.position + m_State->vertexPositionOffset;
		float3 position = float2 - @float;
		float3 position2 = float2 + @float;
		LineVertex vertex = default(LineVertex);
		vertex.position = position;
		vertex.spatialUv = (half2)new float2(0f, point.spatialDistance);
		vertex.segmentedUv = (half2)new float2(0f, point.segmentedDistance);
		m_MeshComposer.PushVertex(in vertex);
		vertex.position = position2;
		vertex.spatialUv = (half2)new float2(1f, point.spatialDistance);
		vertex.segmentedUv = (half2)new float2(1f, point.segmentedDistance);
		m_MeshComposer.PushVertex(in vertex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PushIndices(int pointsCount)
	{
		int num = pointsCount - 1;
		int num2 = m_MeshComposer.GetVertexCount() - pointsCount * 2;
		uint num3 = (uint)((uint)num2 + num * 2);
		for (uint num4 = (uint)num2; num4 < num3; num4 += 2)
		{
			ref MeshComposer<LineVertex, uint> meshComposer = ref m_MeshComposer;
			uint i = num4;
			uint i2 = num4 + 2;
			uint i3 = num4 + 1;
			uint i4 = num4 + 3;
			meshComposer.PushQuad(in i, in i2, in i3, in i4);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void SetSplineLength(int pointsCount)
	{
		half2 segmentedLengthSpatialLength = new half2((half)m_State->segmentedLength, (half)m_State->spatialLength);
		NativeList<LineVertex> vertexBuffer = m_MeshComposer.GetProceduralMesh().vertexBuffer;
		int length = vertexBuffer.Length;
		for (int i = length - pointsCount * 2; i < length; i++)
		{
			vertexBuffer.ElementAt(i).segmentedLengthSpatialLength = segmentedLengthSpatialLength;
		}
	}
}
