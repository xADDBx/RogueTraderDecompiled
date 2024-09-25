using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

[Serializable]
public struct Particle
{
	public float3 BasePosition;

	public float3 Position;

	public float3 Predicted;

	public float3 Velocity;

	public quaternion Orientation;

	public quaternion PredictedOrientation;

	public float4 AngularVelocity;

	public float Mass;

	public uint Flags;

	public float Radius;

	public bool HasFlag(ParticleFlags flag)
	{
		return (Flags & (uint)flag) != 0;
	}
}
