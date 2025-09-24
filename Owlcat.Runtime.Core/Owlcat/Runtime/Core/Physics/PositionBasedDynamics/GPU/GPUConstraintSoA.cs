using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUConstraintSoA : GPUSoABase
{
	public static int _PbdConstraintsDataBuffer = Shader.PropertyToID("_PbdConstraintsDataBuffer");

	public ComputeBufferWrapper<ConstraintSoA.ConstraintData> DataBuffer;

	public override string Name => "GPUConstraintSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[1] { DataBuffer = new ComputeBufferWrapper<ConstraintSoA.ConstraintData>("_PbdConstraintsDataBuffer", size) };
	}
}
