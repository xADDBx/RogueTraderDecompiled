using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class BodyDescriptorSoA : StructureOfArrayBase
{
	public NativeArray<int2> ParticlesOffsetCount;

	public NativeArray<int2> ConstraintsOffsetCount;

	public NativeArray<int2> ConstraintsGroupsOffsetCount;

	public NativeArray<int> SkinnedDataOffset;

	public NativeArray<int> SkinnedDataCount;

	public NativeArray<int> SkinnedBoneIndicesMapOffset;

	public NativeArray<int> SkinnedBoneIndicesMapCount;

	public NativeArray<int> IndicesOffset;

	public NativeArray<int> IndicesCount;

	public NativeArray<int> VerticesOffset;

	public NativeArray<int> VerticesCount;

	public NativeArray<int> VertexTriangleMapOffset;

	public NativeArray<int> VertexTriangleMapCount;

	public NativeArray<int> VertexTriangleMapOffsetCountOffset;

	public NativeArray<int> VertexTriangleMapOffsetCountCount;

	public NativeArray<int2> LocalCollidersOffsetCount;

	public NativeArray<float2> MaterialParameters;

	public NativeArray<float> TeleportDistanceTreshold;

	public BodyDescriptor this[int index]
	{
		get
		{
			BodyDescriptor result = default(BodyDescriptor);
			result.ParticlesOffsetCount = ParticlesOffsetCount[index];
			result.ConstraintsOffsetCount = ConstraintsOffsetCount[index];
			result.ConstraintsGroupsOffsetCount = ConstraintsGroupsOffsetCount[index];
			result.SkinnedDataOffset = SkinnedDataOffset[index];
			result.SkinnedDataCount = SkinnedDataCount[index];
			result.SkinnedBoneIndicesMapOffset = SkinnedBoneIndicesMapOffset[index];
			result.SkinnedBoneIndicesMapCount = SkinnedBoneIndicesMapCount[index];
			result.IndicesOffset = IndicesOffset[index];
			result.IndicesCount = IndicesCount[index];
			result.VerticesOffset = VerticesOffset[index];
			result.VerticesCount = VerticesCount[index];
			result.VertexTriangleMapOffset = VertexTriangleMapOffset[index];
			result.VertexTriangleMapCount = VertexTriangleMapCount[index];
			result.VertexTriangleMapOffsetCountOffset = VertexTriangleMapOffsetCountOffset[index];
			result.VertexTriangleMapOffsetCountCount = VertexTriangleMapOffsetCountCount[index];
			result.LocalCollidersOffsetCount = LocalCollidersOffsetCount[index];
			result.MaterialParameters = MaterialParameters[index];
			result.TeleportDistanceTreshold = TeleportDistanceTreshold[index];
			return result;
		}
		set
		{
			ParticlesOffsetCount[index] = value.ParticlesOffsetCount;
			ConstraintsOffsetCount[index] = value.ConstraintsOffsetCount;
			ConstraintsGroupsOffsetCount[index] = value.ConstraintsGroupsOffsetCount;
			SkinnedDataOffset[index] = value.SkinnedDataOffset;
			SkinnedDataCount[index] = value.SkinnedDataCount;
			SkinnedBoneIndicesMapOffset[index] = value.SkinnedBoneIndicesMapOffset;
			SkinnedBoneIndicesMapCount[index] = value.SkinnedBoneIndicesMapCount;
			IndicesOffset[index] = value.IndicesOffset;
			IndicesCount[index] = value.IndicesCount;
			VerticesOffset[index] = value.VerticesOffset;
			VerticesCount[index] = value.VerticesCount;
			VertexTriangleMapOffset[index] = value.VertexTriangleMapOffset;
			VertexTriangleMapCount[index] = value.VertexTriangleMapCount;
			VertexTriangleMapOffsetCountOffset[index] = value.VertexTriangleMapOffsetCountOffset;
			VertexTriangleMapOffsetCountCount[index] = value.VertexTriangleMapOffsetCountCount;
			LocalCollidersOffsetCount[index] = value.LocalCollidersOffsetCount;
			MaterialParameters[index] = value.MaterialParameters;
			TeleportDistanceTreshold[index] = value.TeleportDistanceTreshold;
		}
	}

	public BodyDescriptorSoA()
		: this(64)
	{
	}

	public BodyDescriptorSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 92;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		ParticlesOffsetCount = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintsOffsetCount = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ConstraintsGroupsOffsetCount = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedDataOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedDataCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedBoneIndicesMapOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SkinnedBoneIndicesMapCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		IndicesOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		IndicesCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VerticesOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VerticesCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTriangleMapOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTriangleMapCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTriangleMapOffsetCountOffset = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexTriangleMapOffsetCountCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalCollidersOffsetCount = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MaterialParameters = new NativeArray<float2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		TeleportDistanceTreshold = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (ParticlesOffsetCount.IsCreated)
		{
			ParticlesOffsetCount.Dispose();
			ConstraintsOffsetCount.Dispose();
			ConstraintsGroupsOffsetCount.Dispose();
			SkinnedDataOffset.Dispose();
			SkinnedDataCount.Dispose();
			SkinnedBoneIndicesMapOffset.Dispose();
			SkinnedBoneIndicesMapCount.Dispose();
			IndicesOffset.Dispose();
			IndicesCount.Dispose();
			VerticesOffset.Dispose();
			VerticesCount.Dispose();
			VertexTriangleMapOffset.Dispose();
			VertexTriangleMapCount.Dispose();
			VertexTriangleMapOffsetCountOffset.Dispose();
			VertexTriangleMapOffsetCountCount.Dispose();
			LocalCollidersOffsetCount.Dispose();
			MaterialParameters.Dispose();
			TeleportDistanceTreshold.Dispose();
		}
	}
}
