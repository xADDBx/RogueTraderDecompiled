using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public class ParticleSoA : StructureOfArrayBase
{
	public NativeArray<float3> BasePosition;

	public NativeArray<float3> Position;

	public NativeArray<float3> Predicted;

	public NativeArray<float3> Velocity;

	public NativeArray<quaternion> Orientation;

	public NativeArray<quaternion> PredictedOrientation;

	public NativeArray<float4> AngularVelocity;

	public NativeArray<float> Mass;

	public NativeArray<float> Radius;

	public NativeArray<uint> Flags;

	public Particle this[int index]
	{
		get
		{
			Particle result = default(Particle);
			result.BasePosition = BasePosition[index];
			result.Position = Position[index];
			result.Predicted = Predicted[index];
			result.Velocity = Velocity[index];
			result.Orientation = Orientation[PBD.UseExperimentalFeatures ? index : 0];
			result.PredictedOrientation = PredictedOrientation[PBD.UseExperimentalFeatures ? index : 0];
			result.AngularVelocity = AngularVelocity[PBD.UseExperimentalFeatures ? index : 0];
			result.Flags = Flags[index];
			result.Radius = Radius[index];
			result.Mass = Mass[index];
			return result;
		}
		set
		{
			BasePosition[index] = value.BasePosition;
			Position[index] = value.Position;
			Predicted[index] = value.Predicted;
			Velocity[index] = value.Velocity;
			if (PBD.UseExperimentalFeatures)
			{
				Orientation[index] = value.Orientation;
				PredictedOrientation[index] = value.PredictedOrientation;
				AngularVelocity[index] = value.AngularVelocity;
			}
			Flags[index] = value.Flags;
			Radius[index] = value.Radius;
			Mass[index] = value.Mass;
		}
	}

	public ParticleSoA()
		: this(64)
	{
	}

	public ParticleSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 124;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		BasePosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Predicted = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Velocity = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Orientation = new NativeArray<quaternion>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PredictedOrientation = new NativeArray<quaternion>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AngularVelocity = new NativeArray<float4>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Mass = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Radius = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Flags = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (BasePosition.IsCreated)
		{
			BasePosition.Dispose();
			Position.Dispose();
			Predicted.Dispose();
			Velocity.Dispose();
			Orientation.Dispose();
			PredictedOrientation.Dispose();
			AngularVelocity.Dispose();
			Mass.Dispose();
			Radius.Dispose();
			Flags.Dispose();
		}
	}

	public ParticleSoASlice GetSlice(int offset, int count)
	{
		ParticleSoASlice result = default(ParticleSoASlice);
		result.BasePosition = new NativeSlice<float3>(BasePosition, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		result.Predicted = new NativeSlice<float3>(Predicted, offset, count);
		result.Velocity = new NativeSlice<float3>(Velocity, offset, count);
		result.Orientation = new NativeSlice<quaternion>(Orientation, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.PredictedOrientation = new NativeSlice<quaternion>(PredictedOrientation, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.AngularVelocity = new NativeSlice<float4>(AngularVelocity, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.Mass = new NativeSlice<float>(Mass, offset, count);
		result.Flags = new NativeSlice<uint>(Flags, offset, count);
		result.Radius = new NativeSlice<float>(Radius, offset, count);
		return result;
	}
}
