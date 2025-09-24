using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public class ParticleSoA : StructureOfArrayBase
{
	public NativeArray<ParticlePositionPair> PositionPairs;

	public NativeArray<ParticleMotionPair> MotionPairs;

	public NativeArray<quaternion> Orientation;

	public NativeArray<quaternion> PredictedOrientation;

	public NativeArray<float4> AngularVelocity;

	public NativeArray<ParticleExtendedData> ExtendedData;

	public Particle this[int index]
	{
		get
		{
			Particle result = default(Particle);
			result.BasePosition = PositionPairs[index].BasePosition;
			result.Position = PositionPairs[index].Position;
			result.Predicted = MotionPairs[index].Predicted;
			result.Velocity = MotionPairs[index].Velocity;
			result.Orientation = Orientation[PBD.UseExperimentalFeatures ? index : 0];
			result.PredictedOrientation = PredictedOrientation[PBD.UseExperimentalFeatures ? index : 0];
			result.AngularVelocity = AngularVelocity[PBD.UseExperimentalFeatures ? index : 0];
			result.Flags = ExtendedData[index].Flags;
			result.Radius = ExtendedData[index].Radius;
			result.Mass = ExtendedData[index].Mass;
			return result;
		}
		set
		{
			ParticlePositionPair value2 = default(ParticlePositionPair);
			value2.BasePosition = value.BasePosition;
			value2.Position = value.Position;
			PositionPairs[index] = value2;
			ParticleMotionPair value3 = default(ParticleMotionPair);
			value3.Predicted = value.Predicted;
			value3.Velocity = value.Velocity;
			MotionPairs[index] = value3;
			if (PBD.UseExperimentalFeatures)
			{
				Orientation[index] = value.Orientation;
				PredictedOrientation[index] = value.PredictedOrientation;
				AngularVelocity[index] = value.AngularVelocity;
			}
			ParticleExtendedData value4 = default(ParticleExtendedData);
			value4.Flags = value.Flags;
			value4.Radius = value.Radius;
			value4.Mass = value.Mass;
			ExtendedData[index] = value4;
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
		PositionPairs = new NativeArray<ParticlePositionPair>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MotionPairs = new NativeArray<ParticleMotionPair>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Orientation = new NativeArray<quaternion>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PredictedOrientation = new NativeArray<quaternion>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AngularVelocity = new NativeArray<float4>((!PBD.UseExperimentalFeatures) ? 1 : newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ExtendedData = new NativeArray<ParticleExtendedData>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (PositionPairs.IsCreated)
		{
			PositionPairs.Dispose();
			MotionPairs.Dispose();
			Orientation.Dispose();
			PredictedOrientation.Dispose();
			AngularVelocity.Dispose();
			ExtendedData.Dispose();
		}
	}

	public ParticleSoASlice GetSlice(int offset, int count)
	{
		ParticleSoASlice result = default(ParticleSoASlice);
		result.PositionPairs = new NativeSlice<ParticlePositionPair>(PositionPairs, offset, count);
		result.MotionPairs = new NativeSlice<ParticleMotionPair>(MotionPairs, offset, count);
		result.Orientation = new NativeSlice<quaternion>(Orientation, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.PredictedOrientation = new NativeSlice<quaternion>(PredictedOrientation, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.AngularVelocity = new NativeSlice<float4>(AngularVelocity, PBD.UseExperimentalFeatures ? offset : 0, PBD.UseExperimentalFeatures ? count : 0);
		result.ExtendedData = new NativeSlice<ParticleExtendedData>(ExtendedData, offset, count);
		return result;
	}
}
