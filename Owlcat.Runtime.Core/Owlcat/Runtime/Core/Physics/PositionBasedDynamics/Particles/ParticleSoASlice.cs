using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public struct ParticleSoASlice
{
	public NativeSlice<float3> BasePosition;

	public NativeSlice<float3> Position;

	public NativeSlice<float3> Predicted;

	public NativeSlice<float3> Velocity;

	public NativeSlice<quaternion> Orientation;

	public NativeSlice<quaternion> PredictedOrientation;

	public NativeSlice<float4> AngularVelocity;

	public NativeSlice<float> Mass;

	public NativeSlice<float> Radius;

	public NativeSlice<uint> Flags;
}
