using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct FillMeshBuilder
{
	private struct AdjacentLayout
	{
		public bool hasE;

		public bool hasNE;

		public bool hasN;

		public bool hasNW;

		public bool hasW;

		public bool hasSW;

		public bool hasS;

		public bool hasSE;
	}

	private struct BaseLayout
	{
		public float3 sw;

		public float3 se;

		public float3 nw;

		public float3 ne;
	}

	private struct VertexState
	{
		public float3 posOffset;

		public float3 posScale;

		public float2 uvOffset;

		public float2 areaSize;
	}

	private struct TriangleFanBuilder
	{
		private readonly int m_CenterIndex;

		private readonly int m_FirstIndex;

		private int m_LastIndex;

		public TriangleFanBuilder(int centerIndex, int index)
		{
			m_CenterIndex = centerIndex;
			m_FirstIndex = index;
			m_LastIndex = index;
		}

		public void Push(int index, ref FillMeshBuilder state)
		{
			state.PushIndex(m_LastIndex);
			state.PushIndex(m_CenterIndex);
			state.PushIndex(index);
			m_LastIndex = index;
		}

		public void Close(ref FillMeshBuilder state)
		{
			state.PushIndex(m_LastIndex);
			state.PushIndex(m_CenterIndex);
			state.PushIndex(m_FirstIndex);
		}
	}

	private const byte kInvalidMaterialId = 0;

	private readonly float3 m_OffsetSW;

	private readonly float3 m_OffsetSE;

	private readonly float3 m_OffsetNW;

	private readonly float3 m_OffsetNE;

	private readonly float m_CellSize;

	private readonly float m_FadeSizeNormalized;

	private readonly float m_CutSizeNormalized;

	[ReadOnly]
	private CellBuffer m_CellBuffer;

	private FillBuffer m_FillBuffer;

	private ProceduralMesh<FillVertex, uint> m_ProceduralMesh;

	private MaterialAreaDescriptorBuffer m_MaterialAreaDescriptorBuffer;

	private int m_VertexCount;

	private int m_IndexCount;

	public FillMeshBuilder(float cellSize, float fadeSize, float cutSize, CellBuffer cellBuffer, FillBuffer fillBuffer, MaterialAreaDescriptorBuffer materialAreaDescriptorBuffer, ProceduralMesh<FillVertex, uint> proceduralMesh)
	{
		m_CellBuffer = cellBuffer;
		m_FillBuffer = fillBuffer;
		m_MaterialAreaDescriptorBuffer = materialAreaDescriptorBuffer;
		m_ProceduralMesh = proceduralMesh;
		float num = cellSize / 2f;
		m_OffsetSW = new float3(0f - num, 0f, 0f - num);
		m_OffsetSE = new float3(num, 0f, 0f - num);
		m_OffsetNW = new float3(0f - num, 0f, num);
		m_OffsetNE = new float3(num, 0f, num);
		m_CellSize = cellSize;
		m_FadeSizeNormalized = math.clamp(fadeSize, 0f, cellSize / 2f) / cellSize;
		m_CutSizeNormalized = math.clamp(cutSize, 0f, cellSize / 2f) / cellSize;
		m_VertexCount = 0;
		m_IndexCount = 0;
	}

	public void Build(in float3 layerOffset)
	{
		MaterialAreaDescriptorBuffer.Enumerator enumerator = m_MaterialAreaDescriptorBuffer.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MaterialAreaDescriptor materialAreaDescriptor = enumerator.Current;
			BuildSubMesh(in materialAreaDescriptor, in layerOffset);
		}
	}

	private void BuildSubMesh(in MaterialAreaDescriptor materialAreaDescriptor, in float3 layerOffset)
	{
		int indexCount = m_IndexCount;
		int i = 0;
		for (int chunksCount = m_FillBuffer.ChunksCount; i < chunksCount; i++)
		{
			if (m_FillBuffer.ChunkHasNoValue(i))
			{
				continue;
			}
			int j = i * 64;
			for (int num = j + 64; j < num; j++)
			{
				if (m_FillBuffer[j] == materialAreaDescriptor.materialId)
				{
					Cell cell = m_CellBuffer.GetCell(j);
					BuildCell(in materialAreaDescriptor, in cell, in layerOffset);
				}
			}
		}
		if (m_IndexCount > indexCount)
		{
			ProcesuralSubMesh procesuralSubMesh = default(ProcesuralSubMesh);
			procesuralSubMesh.indexStart = indexCount;
			procesuralSubMesh.indexCount = m_IndexCount - indexCount;
			procesuralSubMesh.materialId = materialAreaDescriptor.materialId;
			ProcesuralSubMesh value = procesuralSubMesh;
			m_ProceduralMesh.subMeshes.Add(in value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void BuildCell(in MaterialAreaDescriptor materialAreaDescriptor, in Cell cell, in float3 layerOffset)
	{
		if (m_FadeSizeNormalized > 0f || m_CutSizeNormalized > 0f)
		{
			ushort cellIndex;
			bool flag = cell.TryGetAdjacent(in GridDirection.E, out cellIndex) && m_FillBuffer[cellIndex] != 0;
			ushort cellIndex2;
			bool flag2 = cell.TryGetAdjacent(in GridDirection.NE, out cellIndex2) && m_FillBuffer[cellIndex2] != 0;
			ushort cellIndex3;
			bool flag3 = cell.TryGetAdjacent(in GridDirection.N, out cellIndex3) && m_FillBuffer[cellIndex3] != 0;
			ushort cellIndex4;
			bool flag4 = cell.TryGetAdjacent(in GridDirection.NW, out cellIndex4) && m_FillBuffer[cellIndex4] != 0;
			ushort cellIndex5;
			bool flag5 = cell.TryGetAdjacent(in GridDirection.W, out cellIndex5) && m_FillBuffer[cellIndex5] != 0;
			ushort cellIndex6;
			bool flag6 = cell.TryGetAdjacent(in GridDirection.SW, out cellIndex6) && m_FillBuffer[cellIndex6] != 0;
			ushort cellIndex7;
			bool flag7 = cell.TryGetAdjacent(in GridDirection.S, out cellIndex7) && m_FillBuffer[cellIndex7] != 0;
			ushort cellIndex8;
			bool flag8 = cell.TryGetAdjacent(in GridDirection.SE, out cellIndex8) && m_FillBuffer[cellIndex8] != 0;
			if (!(flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8))
			{
				if ((cell.flags & CellFlags.QuadSplitSWNE) != 0)
				{
					AdjacentLayout adjacentLayout = default(AdjacentLayout);
					adjacentLayout.hasE = flag;
					adjacentLayout.hasNE = flag2;
					adjacentLayout.hasN = flag3;
					adjacentLayout.hasNW = flag4;
					adjacentLayout.hasW = flag5;
					adjacentLayout.hasSW = flag6;
					adjacentLayout.hasS = flag7;
					adjacentLayout.hasSE = flag8;
					AdjacentLayout adjacentLayout2 = adjacentLayout;
					BaseLayout baseLayout = GetBaseLayoutSWNE(in cell);
					BuildComplexCell(in materialAreaDescriptor, in cell, in layerOffset, in adjacentLayout2, in baseLayout);
				}
				else
				{
					AdjacentLayout adjacentLayout = default(AdjacentLayout);
					adjacentLayout.hasE = flag7;
					adjacentLayout.hasNE = flag8;
					adjacentLayout.hasN = flag;
					adjacentLayout.hasNW = flag2;
					adjacentLayout.hasW = flag3;
					adjacentLayout.hasSW = flag4;
					adjacentLayout.hasS = flag5;
					adjacentLayout.hasSE = flag6;
					AdjacentLayout adjacentLayout3 = adjacentLayout;
					BaseLayout baseLayout = GetBaseLayoutSENW(in cell);
					BuildComplexCell(in materialAreaDescriptor, in cell, in layerOffset, in adjacentLayout3, in baseLayout);
				}
				return;
			}
		}
		BuildBasicCell(in materialAreaDescriptor, in cell, in layerOffset);
	}

	private void BuildComplexCell(in MaterialAreaDescriptor materialAreaDescriptor, in Cell cell, in float3 layerOffset, in AdjacentLayout adjacentLayout, in BaseLayout baseLayout)
	{
		VertexState state = default(VertexState);
		state.uvOffset = cell.coords - materialAreaDescriptor.coordsMin;
		state.areaSize = materialAreaDescriptor.coordsMax - materialAreaDescriptor.coordsMin + new int2(1);
		state.posScale = new float3(m_CellSize, 1f, m_CellSize);
		state.posOffset = new float3(cell.center.x, 0f, cell.center.z) + layerOffset - new float3(m_CellSize, 0f, m_CellSize) / 2f;
		BuildQuad(in adjacentLayout, in m_FadeSizeNormalized, in m_CutSizeNormalized, in state, baseLayout);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BaseLayout GetBaseLayoutSWNE(in Cell cell)
	{
		BaseLayout result = default(BaseLayout);
		float3 sw = new float3(0f, cell.cornerHeights.sw, 0f);
		float3 se = new float3(1f, cell.cornerHeights.se, 0f);
		float3 nw = new float3(0f, cell.cornerHeights.nw, 1f);
		float3 ne = new float3(1f, cell.cornerHeights.ne, 1f);
		result.sw = sw;
		result.se = se;
		result.nw = nw;
		result.ne = ne;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BaseLayout GetBaseLayoutSENW(in Cell cell)
	{
		BaseLayout result = default(BaseLayout);
		result.sw = new float3(0f, cell.cornerHeights.nw, 1f);
		result.se = new float3(0f, cell.cornerHeights.sw, 0f);
		result.nw = new float3(1f, cell.cornerHeights.ne, 1f);
		result.ne = new float3(1f, cell.cornerHeights.se, 0f);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int PushVertex(float3 pos, float opacity, in VertexState state)
	{
		ref NativeList<FillVertex> vertexBuffer = ref m_ProceduralMesh.vertexBuffer;
		FillVertex value = new FillVertex(state.posOffset + pos * state.posScale, new float4(1f, 1f, 1f, opacity), state.uvOffset + pos.xz, state.areaSize);
		vertexBuffer.Add(in value);
		return m_VertexCount++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PushIndex(int index)
	{
		ref NativeList<uint> indexBuffer = ref m_ProceduralMesh.indexBuffer;
		uint value = (ushort)index;
		indexBuffer.Add(in value);
		m_IndexCount++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void BuildBasicCell(in MaterialAreaDescriptor materialAreaDescriptor, in Cell cell, in float3 layerOffset)
	{
		float3 position = new float3(cell.center.x, cell.cornerHeights.sw, cell.center.z) + m_OffsetSW + layerOffset;
		float3 position2 = new float3(cell.center.x, cell.cornerHeights.se, cell.center.z) + m_OffsetSE + layerOffset;
		float3 position3 = new float3(cell.center.x, cell.cornerHeights.nw, cell.center.z) + m_OffsetNW + layerOffset;
		float3 position4 = new float3(cell.center.x, cell.cornerHeights.ne, cell.center.z) + m_OffsetNE + layerOffset;
		float2 @float = materialAreaDescriptor.coordsMin;
		float2 areaSize = materialAreaDescriptor.coordsMax - materialAreaDescriptor.coordsMin + new int2(1);
		float2 float2 = cell.coords - @float;
		ref NativeList<FillVertex> vertexBuffer = ref m_ProceduralMesh.vertexBuffer;
		FillVertex value = new FillVertex(position, new float4(1), float2 + new float2(0f, 0f), areaSize);
		vertexBuffer.Add(in value);
		ref NativeList<FillVertex> vertexBuffer2 = ref m_ProceduralMesh.vertexBuffer;
		value = new FillVertex(position2, new float4(1), float2 + new float2(1f, 0f), areaSize);
		vertexBuffer2.Add(in value);
		ref NativeList<FillVertex> vertexBuffer3 = ref m_ProceduralMesh.vertexBuffer;
		value = new FillVertex(position3, new float4(1), float2 + new float2(0f, 1f), areaSize);
		vertexBuffer3.Add(in value);
		ref NativeList<FillVertex> vertexBuffer4 = ref m_ProceduralMesh.vertexBuffer;
		value = new FillVertex(position4, new float4(1), float2 + new float2(1f, 1f), areaSize);
		vertexBuffer4.Add(in value);
		int vertexCount = m_VertexCount;
		if ((cell.flags & CellFlags.QuadSplitSWNE) != 0)
		{
			ref NativeList<uint> indexBuffer = ref m_ProceduralMesh.indexBuffer;
			uint value2 = (ushort)vertexCount;
			indexBuffer.Add(in value2);
			ref NativeList<uint> indexBuffer2 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 3);
			indexBuffer2.Add(in value2);
			ref NativeList<uint> indexBuffer3 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 1);
			indexBuffer3.Add(in value2);
			ref NativeList<uint> indexBuffer4 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)vertexCount;
			indexBuffer4.Add(in value2);
			ref NativeList<uint> indexBuffer5 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 2);
			indexBuffer5.Add(in value2);
			ref NativeList<uint> indexBuffer6 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 3);
			indexBuffer6.Add(in value2);
		}
		else
		{
			ref NativeList<uint> indexBuffer7 = ref m_ProceduralMesh.indexBuffer;
			uint value2 = (ushort)vertexCount;
			indexBuffer7.Add(in value2);
			ref NativeList<uint> indexBuffer8 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 2);
			indexBuffer8.Add(in value2);
			ref NativeList<uint> indexBuffer9 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 1);
			indexBuffer9.Add(in value2);
			ref NativeList<uint> indexBuffer10 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 1);
			indexBuffer10.Add(in value2);
			ref NativeList<uint> indexBuffer11 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 2);
			indexBuffer11.Add(in value2);
			ref NativeList<uint> indexBuffer12 = ref m_ProceduralMesh.indexBuffer;
			value2 = (ushort)(vertexCount + 3);
			indexBuffer12.Add(in value2);
		}
		m_VertexCount += 4;
		m_IndexCount += 6;
	}

	private void PushTriangle(int v0, int v1, int v2)
	{
		PushIndex(v0);
		PushIndex(v1);
		PushIndex(v2);
	}

	private void PushQuad(int v0, int v1, int v2, int v3)
	{
		PushIndex(v0);
		PushIndex(v3);
		PushIndex(v1);
		PushIndex(v0);
		PushIndex(v2);
		PushIndex(v3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void BuildQuad(in AdjacentLayout adjacent, in float border, in float cut, in VertexState state, BaseLayout baseLayout)
	{
		bool flag = adjacent.hasS && adjacent.hasW && !adjacent.hasSW;
		bool flag2 = adjacent.hasS && adjacent.hasE && !adjacent.hasSE;
		bool flag3 = adjacent.hasN && adjacent.hasW && !adjacent.hasNW;
		bool flag4 = adjacent.hasN && adjacent.hasE && !adjacent.hasNE;
		bool flag5 = !adjacent.hasW;
		bool flag6 = !adjacent.hasE;
		bool flag7 = !adjacent.hasS;
		bool flag8 = !adjacent.hasN;
		float3 @float = math.lerp(baseLayout.sw, baseLayout.ne, 0.5f);
		int centerIndex = PushVertex(@float, 1f, in state);
		BaseLayout baseLayout2 = default(BaseLayout);
		baseLayout2.sw = math.lerp(baseLayout.sw, @float, cut * 2f);
		baseLayout2.se = math.lerp(baseLayout.se, @float, cut * 2f);
		baseLayout2.nw = math.lerp(baseLayout.nw, @float, cut * 2f);
		baseLayout2.ne = math.lerp(baseLayout.ne, @float, cut * 2f);
		float3 float2 = default(float3);
		float3 float3 = default(float3);
		float3 float4 = default(float3);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		float3 float5 = default(float3);
		int num4 = 0;
		float3 float6 = default(float3);
		float3 float7 = default(float3);
		float3 float8 = default(float3);
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		float3 float9 = default(float3);
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = 0;
		int num17 = 0;
		int num18 = 0;
		TriangleFanBuilder triangleFanBuilder;
		if (flag)
		{
			float3 pos = math.lerp(baseLayout.sw, @float, cut + border);
			float3 pos2 = math.lerp(baseLayout.sw, baseLayout.nw, cut + border);
			float3 pos3 = math.lerp(baseLayout.sw, baseLayout.se, cut + border);
			num9 = PushVertex(pos, 1f, in state);
			num10 = PushVertex(pos2, 1f, in state);
			num11 = PushVertex(pos3, 1f, in state);
			triangleFanBuilder = new TriangleFanBuilder(centerIndex, num10);
			triangleFanBuilder.Push(num9, ref this);
			triangleFanBuilder.Push(num11, ref this);
		}
		else if (flag5 && flag7)
		{
			float2 = math.lerp(baseLayout.sw, @float, (cut + border) * 2f);
			num = PushVertex(float2, 1f, in state);
			triangleFanBuilder = new TriangleFanBuilder(centerIndex, num);
		}
		else if (flag5)
		{
			float2 = math.lerp(baseLayout.sw, @float, (cut + border) * 2f);
			num = PushVertex(float2, 1f, in state);
			float4 = math.lerp(baseLayout.sw, baseLayout.se, cut + border);
			num3 = PushVertex(float4, 1f, in state);
			triangleFanBuilder = new TriangleFanBuilder(centerIndex, num);
			triangleFanBuilder.Push(num3, ref this);
		}
		else if (flag7)
		{
			float2 = math.lerp(baseLayout.sw, @float, (cut + border) * 2f);
			num = PushVertex(float2, 1f, in state);
			float3 = math.lerp(baseLayout.sw, baseLayout.nw, cut + border);
			num2 = PushVertex(float3, 1f, in state);
			triangleFanBuilder = new TriangleFanBuilder(centerIndex, num2);
			triangleFanBuilder.Push(num, ref this);
		}
		else
		{
			int index = PushVertex(baseLayout.sw, 1f, in state);
			triangleFanBuilder = new TriangleFanBuilder(centerIndex, index);
		}
		if (flag2)
		{
			float3 pos4 = math.lerp(baseLayout.se, baseLayout.sw, cut + border);
			float3 pos5 = math.lerp(baseLayout.se, baseLayout.ne, cut + border);
			num12 = PushVertex(pos4, 1f, in state);
			num13 = PushVertex(pos5, 1f, in state);
			triangleFanBuilder.Push(num12, ref this);
			triangleFanBuilder.Push(num13, ref this);
		}
		else if (flag7 && flag6)
		{
			float5 = math.lerp(baseLayout.se, @float, (cut + border) * 2f);
			num4 = PushVertex(float5, 1f, in state);
			triangleFanBuilder.Push(num4, ref this);
		}
		else if (flag7)
		{
			float5 = math.lerp(baseLayout.se, baseLayout.ne, cut + border);
			num4 = PushVertex(float5, 1f, in state);
			triangleFanBuilder.Push(num4, ref this);
		}
		else if (flag6)
		{
			float5 = math.lerp(baseLayout.se, baseLayout.sw, cut + border);
			num4 = PushVertex(float5, 1f, in state);
			triangleFanBuilder.Push(num4, ref this);
		}
		else
		{
			int index2 = PushVertex(baseLayout.se, 1f, in state);
			triangleFanBuilder.Push(index2, ref this);
		}
		if (flag4)
		{
			float3 pos6 = math.lerp(baseLayout.ne, @float, cut + border);
			float3 pos7 = math.lerp(baseLayout.ne, baseLayout.se, cut + border);
			float3 pos8 = math.lerp(baseLayout.ne, baseLayout.nw, cut + border);
			num16 = PushVertex(pos6, 1f, in state);
			num17 = PushVertex(pos7, 1f, in state);
			num18 = PushVertex(pos8, 1f, in state);
			triangleFanBuilder.Push(num17, ref this);
			triangleFanBuilder.Push(num16, ref this);
			triangleFanBuilder.Push(num18, ref this);
		}
		else if (flag6 && flag8)
		{
			float6 = math.lerp(baseLayout.ne, @float, (cut + border) * 2f);
			num5 = PushVertex(float6, 1f, in state);
			triangleFanBuilder.Push(num5, ref this);
		}
		else if (flag6)
		{
			float6 = math.lerp(baseLayout.ne, @float, (cut + border) * 2f);
			float8 = math.lerp(baseLayout.ne, baseLayout.nw, cut + border);
			num5 = PushVertex(float6, 1f, in state);
			num7 = PushVertex(float8, 1f, in state);
			triangleFanBuilder.Push(num5, ref this);
			triangleFanBuilder.Push(num7, ref this);
		}
		else if (flag8)
		{
			float6 = math.lerp(baseLayout.ne, @float, (cut + border) * 2f);
			num5 = PushVertex(float6, 1f, in state);
			float7 = math.lerp(baseLayout.ne, baseLayout.se, cut + border);
			num6 = PushVertex(float7, 1f, in state);
			triangleFanBuilder.Push(num6, ref this);
			triangleFanBuilder.Push(num5, ref this);
		}
		else
		{
			int index3 = PushVertex(baseLayout.ne, 1f, in state);
			triangleFanBuilder.Push(index3, ref this);
		}
		if (flag3)
		{
			float3 pos9 = math.lerp(baseLayout.nw, baseLayout.ne, cut + border);
			float3 pos10 = math.lerp(baseLayout.nw, baseLayout.sw, cut + border);
			num14 = PushVertex(pos9, 1f, in state);
			num15 = PushVertex(pos10, 1f, in state);
			triangleFanBuilder.Push(num14, ref this);
			triangleFanBuilder.Push(num15, ref this);
		}
		else if (flag8 && flag5)
		{
			float9 = math.lerp(baseLayout.nw, @float, (cut + border) * 2f);
			num8 = PushVertex(float9, 1f, in state);
			triangleFanBuilder.Push(num8, ref this);
		}
		else if (flag8)
		{
			float9 = math.lerp(baseLayout.nw, baseLayout.sw, cut + border);
			num8 = PushVertex(float9, 1f, in state);
			triangleFanBuilder.Push(num8, ref this);
		}
		else if (flag5)
		{
			float9 = math.lerp(baseLayout.nw, baseLayout.ne, cut + border);
			num8 = PushVertex(float9, 1f, in state);
			triangleFanBuilder.Push(num8, ref this);
		}
		else
		{
			int index4 = PushVertex(baseLayout.nw, 1f, in state);
			triangleFanBuilder.Push(index4, ref this);
		}
		triangleFanBuilder.Close(ref this);
		int num19 = 0;
		int v = 0;
		int num20 = 0;
		int num21 = 0;
		int num22 = 0;
		int num23 = 0;
		int num24 = 0;
		int num25 = 0;
		if (flag7)
		{
			float3 pos11 = (flag6 ? baseLayout2.se : math.lerp(baseLayout.se, baseLayout.ne, cut));
			float3 sw = baseLayout2.sw;
			num19 = PushVertex(pos11, 0f, in state);
			v = PushVertex(sw, 0f, in state);
			PushQuad(v, num19, num, num4);
		}
		if (flag8)
		{
			float3 pos12 = (flag5 ? baseLayout2.nw : math.lerp(baseLayout.nw, baseLayout.sw, cut));
			float3 ne = baseLayout2.ne;
			num20 = PushVertex(pos12, 0f, in state);
			num21 = PushVertex(ne, 0f, in state);
			PushQuad(num8, num5, num20, num21);
		}
		if (flag5)
		{
			float3 pos13 = (flag8 ? baseLayout2.nw : math.lerp(baseLayout.nw, baseLayout.ne, cut));
			float3 sw2 = baseLayout2.sw;
			num22 = PushVertex(pos13, 0f, in state);
			num23 = PushVertex(sw2, 0f, in state);
			PushQuad(num, num8, num23, num22);
		}
		if (flag6)
		{
			float3 pos14 = (flag7 ? baseLayout2.se : math.lerp(baseLayout.se, baseLayout.sw, cut));
			float3 ne2 = baseLayout2.ne;
			num24 = PushVertex(pos14, 0f, in state);
			num25 = PushVertex(ne2, 0f, in state);
			PushQuad(num5, num4, num25, num24);
		}
		if (flag7 && !flag5)
		{
			float3 pos15 = math.lerp(baseLayout.sw, baseLayout.nw, cut);
			int v2 = PushVertex(pos15, 0f, in state);
			PushQuad(v, num, v2, num2);
		}
		if (flag8 && !flag6)
		{
			float3 pos16 = math.lerp(baseLayout.ne, baseLayout.se, cut);
			int v3 = PushVertex(pos16, 0f, in state);
			PushQuad(num21, num5, v3, num6);
		}
		if (flag5 && !flag7)
		{
			float3 pos17 = math.lerp(baseLayout.sw, baseLayout.se, cut);
			int v4 = PushVertex(pos17, 0f, in state);
			PushQuad(num23, v4, num, num3);
		}
		if (flag6 && !flag8)
		{
			float3 pos18 = math.lerp(baseLayout.ne, baseLayout.nw, cut);
			int v5 = PushVertex(pos18, 0f, in state);
			PushQuad(num25, v5, num5, num7);
		}
		if (flag)
		{
			float3 pos19 = math.lerp(baseLayout.sw, baseLayout.se, cut);
			float3 pos20 = math.lerp(baseLayout.sw, baseLayout.nw, cut);
			float3 pos21 = math.lerp(baseLayout.sw, @float, cut);
			int v6 = PushVertex(pos19, 0f, in state);
			int v7 = PushVertex(pos20, 0f, in state);
			int num26 = PushVertex(pos21, 0f, in state);
			PushQuad(v7, num26, num10, num9);
			PushQuad(num26, v6, num9, num11);
		}
		if (flag4)
		{
			float3 pos22 = math.lerp(baseLayout.ne, baseLayout.nw, cut);
			float3 pos23 = math.lerp(baseLayout.ne, baseLayout.se, cut);
			float3 pos24 = math.lerp(baseLayout.ne, @float, cut);
			int v8 = PushVertex(pos22, 0f, in state);
			int v9 = PushVertex(pos23, 0f, in state);
			int num27 = PushVertex(pos24, 0f, in state);
			PushQuad(v9, num27, num17, num16);
			PushQuad(num27, v8, num16, num18);
		}
		if (flag2)
		{
			float3 pos25 = math.lerp(baseLayout.se, baseLayout.sw, cut);
			float3 pos26 = math.lerp(baseLayout.se, baseLayout.ne, cut);
			int v10 = PushVertex(pos25, 0f, in state);
			int v11 = PushVertex(pos26, 0f, in state);
			PushQuad(v10, v11, num12, num13);
		}
		if (flag3)
		{
			float3 pos27 = math.lerp(baseLayout.nw, baseLayout.ne, cut);
			float3 pos28 = math.lerp(baseLayout.nw, baseLayout.sw, cut);
			int v12 = PushVertex(pos27, 0f, in state);
			int v13 = PushVertex(pos28, 0f, in state);
			PushQuad(v12, v13, num14, num15);
		}
	}
}
