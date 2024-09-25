using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class SkinnedBodySoA : StructureOfArrayBase
{
	public NativeArray<Matrix4x4> Boneposes;

	public NativeArray<Matrix4x4> Bindposes;

	public NativeArray<int> ParentMap;

	public SkinnedBodySoA()
		: this(64)
	{
	}

	public SkinnedBodySoA(int size)
		: base(size)
	{
		m_Allocator.Stride = Marshal.SizeOf<Matrix4x4>() * 2 + 4;
	}

	public override void Dispose()
	{
		if (Boneposes.IsCreated)
		{
			Boneposes.Dispose();
			Bindposes.Dispose();
			ParentMap.Dispose();
		}
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Boneposes = new NativeArray<Matrix4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Bindposes = new NativeArray<Matrix4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParentMap = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}
}
