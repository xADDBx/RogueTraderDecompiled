using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUMeshBodyVerticesSoA : GPUSoABase
{
	public static int _Vertices = Shader.PropertyToID("_Vertices");

	public static int _Uvs = Shader.PropertyToID("_Uvs");

	public static int _Normals = Shader.PropertyToID("_Normals");

	public static int _Tangents = Shader.PropertyToID("_Tangents");

	public ComputeBufferWrapper<Vector3> VerticesBuffer;

	public ComputeBufferWrapper<Vector2> UvsBuffer;

	public ComputeBufferWrapper<Vector3> NormalsBuffer;

	public ComputeBufferWrapper<Vector4> TangentsBuffer;

	public override string Name => "GPUMeshBodyVerticesSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[4]
		{
			VerticesBuffer = new ComputeBufferWrapper<Vector3>("_Vertices", size),
			UvsBuffer = new ComputeBufferWrapper<Vector2>("_Uvs", size),
			NormalsBuffer = new ComputeBufferWrapper<Vector3>("_Normals", size),
			TangentsBuffer = new ComputeBufferWrapper<Vector4>("_Tangents", size)
		};
	}
}
