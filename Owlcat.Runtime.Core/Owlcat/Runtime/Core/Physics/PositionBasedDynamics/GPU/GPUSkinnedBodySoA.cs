using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUSkinnedBodySoA : GPUSoABase
{
	public static int _Bindposes = Shader.PropertyToID("_Bindposes");

	public static int _Boneposes = Shader.PropertyToID("_Boneposes");

	public static int _ParentMap = Shader.PropertyToID("_ParentMap");

	public static int _SimulatedBindposes = Shader.PropertyToID("_SimulatedBindposes");

	public ComputeBufferWrapper<Matrix4x4> BindposesBuffer;

	public ComputeBufferWrapper<Matrix4x4> BoneposesBuffer;

	public ComputeBufferWrapper<int> ParentMapBuffer;

	public ComputeBufferWrapper<Matrix4x4> SimulatedBindposesBuffer;

	public override string Name => "GPUSkinnedBodySoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[4]
		{
			BindposesBuffer = new ComputeBufferWrapper<Matrix4x4>("_Bindposes", size),
			BoneposesBuffer = new ComputeBufferWrapper<Matrix4x4>("_Boneposes", size),
			ParentMapBuffer = new ComputeBufferWrapper<int>("_ParentMap", size),
			SimulatedBindposesBuffer = new ComputeBufferWrapper<Matrix4x4>("_SimulatedBindposes", size)
		};
	}
}
