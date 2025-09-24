using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public struct ParticleSoASlice
{
	public NativeSlice<ParticlePositionPair> PositionPairs;

	public NativeSlice<ParticleMotionPair> MotionPairs;

	public NativeSlice<quaternion> Orientation;

	public NativeSlice<quaternion> PredictedOrientation;

	public NativeSlice<float4> AngularVelocity;

	public NativeSlice<ParticleExtendedData> ExtendedData;
}
