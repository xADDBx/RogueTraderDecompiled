using System;
using Unity.Burst;
using Unity.Collections;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct ProceduralMesh<TVertex, TIndex> : IDisposable where TVertex : unmanaged where TIndex : unmanaged
{
	public NativeList<TVertex> vertexBuffer;

	public NativeList<TIndex> indexBuffer;

	public NativeList<ProcesuralSubMesh> subMeshes;

	public ProceduralMesh(Allocator allocator)
	{
		vertexBuffer = new NativeList<TVertex>(allocator);
		indexBuffer = new NativeList<TIndex>(allocator);
		subMeshes = new NativeList<ProcesuralSubMesh>(allocator);
	}

	public bool IsEmpty()
	{
		if (indexBuffer.Length != 0)
		{
			return subMeshes.Length == 0;
		}
		return true;
	}

	public void Dispose()
	{
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		subMeshes.Dispose();
	}

	[BurstCompile]
	public void MergeSubMeshes()
	{
		if (subMeshes.Length >= 2 && indexBuffer.Length != 0)
		{
			NativeArray<ProcesuralSubMesh> sortedSubMeshes = new NativeArray<ProcesuralSubMesh>(subMeshes.AsArray(), Allocator.Temp);
			sortedSubMeshes.Sort(default(ProceduralSubMeshMaterialComparer));
			if (CanMergeSubMeshes(in sortedSubMeshes))
			{
				RearrangeIndices(in sortedSubMeshes, ref indexBuffer);
				MergeSubMeshes(in sortedSubMeshes, ref subMeshes);
			}
			sortedSubMeshes.Dispose();
		}
	}

	private static bool CanMergeSubMeshes(in NativeArray<ProcesuralSubMesh> sortedSubMeshes)
	{
		for (int i = 1; i < sortedSubMeshes.Length; i++)
		{
			if (sortedSubMeshes[i].materialId == sortedSubMeshes[i - 1].materialId)
			{
				return true;
			}
		}
		return false;
	}

	private static void RearrangeIndices(in NativeArray<ProcesuralSubMesh> sortedSubMeshes, ref NativeList<TIndex> indexBuffer)
	{
		NativeArray<TIndex> src = new NativeArray<TIndex>(indexBuffer.AsArray(), Allocator.Temp);
		int num = 0;
		foreach (ProcesuralSubMesh sortedSubMesh in sortedSubMeshes)
		{
			if (sortedSubMesh.indexCount > 0)
			{
				NativeArray<TIndex>.Copy(src, sortedSubMesh.indexStart, indexBuffer, num, sortedSubMesh.indexCount);
				num += sortedSubMesh.indexCount;
			}
		}
		src.Dispose();
	}

	private static void MergeSubMeshes(in NativeArray<ProcesuralSubMesh> sortedSubMeshes, ref NativeList<ProcesuralSubMesh> mergedSubMeshes)
	{
		mergedSubMeshes.Clear();
		ProcesuralSubMesh procesuralSubMesh = default(ProcesuralSubMesh);
		procesuralSubMesh.materialId = sortedSubMeshes[0].materialId;
		procesuralSubMesh.indexStart = 0;
		procesuralSubMesh.indexCount = sortedSubMeshes[0].indexCount;
		ProcesuralSubMesh value = procesuralSubMesh;
		int num = value.indexCount;
		for (int i = 1; i < sortedSubMeshes.Length; i++)
		{
			ProcesuralSubMesh procesuralSubMesh2 = sortedSubMeshes[i];
			if (procesuralSubMesh2.materialId != value.materialId)
			{
				mergedSubMeshes.Add(in value);
				procesuralSubMesh = default(ProcesuralSubMesh);
				procesuralSubMesh.materialId = procesuralSubMesh2.materialId;
				procesuralSubMesh.indexStart = num;
				procesuralSubMesh.indexCount = procesuralSubMesh2.indexCount;
				value = procesuralSubMesh;
			}
			else
			{
				value.indexCount += procesuralSubMesh2.indexCount;
			}
			num += procesuralSubMesh2.indexCount;
		}
		mergedSubMeshes.Add(in value);
	}
}
