using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct MeshComposer<TVertex, TIndex> where TVertex : unmanaged where TIndex : unmanaged
{
	public struct State
	{
		public int subMeshIndexStart;
	}

	private ProceduralMesh<TVertex, TIndex> m_ProceduralMesh;

	private unsafe State* m_State;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe MeshComposer(State* state, in ProceduralMesh<TVertex, TIndex> proceduralMesh)
	{
		this = default(MeshComposer<TVertex, TIndex>);
		m_State = state;
		m_ProceduralMesh = proceduralMesh;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ProceduralMesh<TVertex, TIndex> GetProceduralMesh()
	{
		return m_ProceduralMesh;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetVertexCount()
	{
		return m_ProceduralMesh.vertexBuffer.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetIndexCount()
	{
		return m_ProceduralMesh.indexBuffer.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushVertex(in TVertex vertex)
	{
		m_ProceduralMesh.vertexBuffer.Add(in vertex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushTriangle(in TIndex i0, in TIndex i1, in TIndex i2)
	{
		m_ProceduralMesh.indexBuffer.Add(in i0);
		m_ProceduralMesh.indexBuffer.Add(in i1);
		m_ProceduralMesh.indexBuffer.Add(in i2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushQuad(in TIndex i0, in TIndex i1, in TIndex i2, in TIndex i3)
	{
		m_ProceduralMesh.indexBuffer.Add(in i0);
		m_ProceduralMesh.indexBuffer.Add(in i2);
		m_ProceduralMesh.indexBuffer.Add(in i3);
		m_ProceduralMesh.indexBuffer.Add(in i0);
		m_ProceduralMesh.indexBuffer.Add(in i3);
		m_ProceduralMesh.indexBuffer.Add(in i1);
	}

	public unsafe void StartSubMesh()
	{
		int length = m_ProceduralMesh.indexBuffer.Length;
		m_State->subMeshIndexStart = length;
	}

	public unsafe void PushSubMesh(byte materialId)
	{
		int length = m_ProceduralMesh.indexBuffer.Length;
		int subMeshIndexStart = m_State->subMeshIndexStart;
		if (subMeshIndexStart < length)
		{
			ProcesuralSubMesh procesuralSubMesh = default(ProcesuralSubMesh);
			procesuralSubMesh.indexStart = subMeshIndexStart;
			procesuralSubMesh.indexCount = length - subMeshIndexStart;
			procesuralSubMesh.materialId = materialId;
			ProcesuralSubMesh value = procesuralSubMesh;
			m_ProceduralMesh.subMeshes.Add(in value);
		}
	}

	public unsafe void PushSubMeshMerged(byte materialId)
	{
		int length = m_ProceduralMesh.indexBuffer.Length;
		int subMeshIndexStart = m_State->subMeshIndexStart;
		if (subMeshIndexStart < length)
		{
			int num = length - subMeshIndexStart;
			int num2 = m_ProceduralMesh.subMeshes.Length - 1;
			if (num2 >= 0 && m_ProceduralMesh.subMeshes[num2].materialId == materialId)
			{
				ProcesuralSubMesh value = m_ProceduralMesh.subMeshes[num2];
				value.indexCount += num;
				m_ProceduralMesh.subMeshes[num2] = value;
				return;
			}
			ProcesuralSubMesh procesuralSubMesh = default(ProcesuralSubMesh);
			procesuralSubMesh.indexStart = subMeshIndexStart;
			procesuralSubMesh.indexCount = num;
			procesuralSubMesh.materialId = materialId;
			ProcesuralSubMesh value2 = procesuralSubMesh;
			m_ProceduralMesh.subMeshes.Add(in value2);
		}
	}
}
