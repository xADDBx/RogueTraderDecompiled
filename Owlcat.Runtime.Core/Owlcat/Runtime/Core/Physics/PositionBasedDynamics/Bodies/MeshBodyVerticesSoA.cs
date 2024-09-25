using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class MeshBodyVerticesSoA : StructureOfArrayBase
{
	public NativeArray<Vector3> BaseVertices;

	public NativeArray<Vector3> Vertices;

	public NativeArray<Vector3> Normals;

	public NativeArray<Vector4> Tangents;

	public NativeArray<Vector2> Uvs;

	public MeshBodyVerticesSoA()
		: this(64)
	{
	}

	public MeshBodyVerticesSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = Marshal.SizeOf<Vector3>() * 5;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		BaseVertices = new NativeArray<Vector3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Vertices = new NativeArray<Vector3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Normals = new NativeArray<Vector3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Tangents = new NativeArray<Vector4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Uvs = new NativeArray<Vector2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (BaseVertices.IsCreated)
		{
			BaseVertices.Dispose();
			Vertices.Dispose();
			Normals.Dispose();
			Tangents.Dispose();
			Uvs.Dispose();
		}
	}
}
