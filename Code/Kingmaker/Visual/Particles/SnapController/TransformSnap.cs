using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;

namespace Kingmaker.Visual.Particles.SnapController;

internal sealed class TransformSnap : ISnapBehaviour
{
	[BurstCompile]
	private struct BoneTransform
	{
		public float3 position;

		public float3 rotationEuler;
	}

	[BurstCompile]
	private struct BoneData
	{
		public float cameraOffset;

		public float particleSize;
	}

	[BurstCompile]
	private struct SnapSettings
	{
		public float3 cameraPosition;

		public float3 offset;

		public float cameraOffsetScale;

		public bool correctLifetime;
	}

	[BurstCompile]
	private struct BuildBoneTransformsJob : IJobParallelForTransform
	{
		[WriteOnly]
		private NativeArray<BoneTransform> m_BoneTransforms;

		public BuildBoneTransformsJob(NativeArray<BoneTransform> boneTransforms)
		{
			m_BoneTransforms = boneTransforms;
		}

		public void Execute(int index, TransformAccess transform)
		{
			float3 position;
			float3 rotationEuler;
			if (transform.isValid)
			{
				position = transform.position;
				quaternion q = transform.rotation;
				rotationEuler = QuaternionToEuler(in q);
			}
			else
			{
				position = default(float3);
				rotationEuler = default(float3);
			}
			m_BoneTransforms[index] = new BoneTransform
			{
				position = position,
				rotationEuler = rotationEuler
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float3 QuaternionToEuler(in quaternion q)
		{
			float num = q.value.x * q.value.x;
			float num2 = q.value.x * q.value.y;
			float num3 = q.value.x * q.value.z;
			float num4 = q.value.x * q.value.w;
			float num5 = q.value.y * q.value.y;
			float num6 = q.value.y * q.value.z;
			float num7 = q.value.y * q.value.w;
			float num8 = q.value.z * q.value.z;
			float num9 = q.value.z * q.value.w;
			float num10 = q.value.w * q.value.w;
			float num11 = num6 - num4;
			float a = -1f;
			float b = 2f * num11;
			if (math.abs(num11) < 0.499999f)
			{
				float y = 2f * (num3 + num7);
				float x = num8 - num - num5 + num10;
				float y2 = 2f * (num2 + num9);
				float x2 = num5 - num8 - num + num10;
				return new float3(Asin(a, b), math.atan2(y, x), math.atan2(y2, x2));
			}
			float num12 = num2 + num9;
			float num13 = 0f - num6 + num4;
			float num14 = num2 - num9;
			float num15 = num6 + num4;
			float y3 = num12 * num15 + num13 * num14;
			float x3 = num13 * num15 - num12 * num14;
			return new float3(Asin(a, b), math.atan2(y3, x3), 0f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Asin(float a, float b)
		{
			return a * math.asin(math.clamp(b, -1f, 1f));
		}
	}

	[BurstCompile]
	private struct UpdateParticlesJob : IJobParticleSystem
	{
		private interface IParticleUpdater
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[Pure]
			void Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData);
		}

		private struct CompositeUpdater<T1, T2> : IParticleUpdater where T1 : unmanaged, IParticleUpdater where T2 : unmanaged, IParticleUpdater
		{
			private readonly T1 m_Updater1;

			private readonly T2 m_Updater2;

			public CompositeUpdater(T1 updater1, T2 updater2)
			{
				m_Updater1 = updater1;
				m_Updater2 = updater2;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				m_Updater1.Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
				m_Updater2.Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
			}

			void IParticleUpdater.Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private readonly struct PositionUpdater : IParticleUpdater
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				float3 @float = math.normalize(snapSettings.cameraPosition - boneTransform.position);
				float3 float2 = boneTransform.position + snapSettings.offset + @float * (boneData.cameraOffset * snapSettings.cameraOffsetScale);
				ParticleSystemNativeArray3 positions = particles.positions;
				positions.x[index] = float2.x;
				positions.y[index] = float2.y;
				positions.z[index] = float2.z;
			}

			void IParticleUpdater.Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private readonly struct Rotation3DUpdater : IParticleUpdater
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				ParticleSystemNativeArray3 rotations = particles.rotations;
				rotations.x[index] = boneTransform.rotationEuler.x;
				rotations.y[index] = boneTransform.rotationEuler.y;
				rotations.z[index] = boneTransform.rotationEuler.z;
			}

			void IParticleUpdater.Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private readonly struct LifetimeUpdater : IParticleUpdater
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				NativeArray<float> aliveTimePercent = particles.aliveTimePercent;
				if (aliveTimePercent[index] >= 0.97f)
				{
					aliveTimePercent[index] = 0f;
				}
			}

			void IParticleUpdater.Update(in ParticleSystemJobData particles, in int index, in SnapSettings snapSettings, in BoneTransform boneTransform, in BoneData boneData)
			{
				Update(in particles, in index, in snapSettings, in boneTransform, in boneData);
			}
		}

		private readonly SnapSettings m_SnapSettings;

		private readonly bool m_OrientParticlesWithBone;

		private readonly bool m_CorrectLifetime;

		[ReadOnly]
		private NativeArray<BoneTransform> m_BoneTransforms;

		[ReadOnly]
		private NativeArray<BoneData> m_BoneDatas;

		public UpdateParticlesJob(SnapSettings snapSettings, bool orientParticlesWithBone, bool correctLifetime, NativeArray<BoneTransform> boneTransforms, NativeArray<BoneData> boneDatas)
		{
			m_SnapSettings = snapSettings;
			m_OrientParticlesWithBone = orientParticlesWithBone;
			m_CorrectLifetime = correctLifetime;
			m_BoneTransforms = boneTransforms;
			m_BoneDatas = boneDatas;
		}

		public void Execute(ParticleSystemJobData particles)
		{
			if (particles.count > 0)
			{
				AppendRotationUpdate(particles, default(PositionUpdater));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AppendRotationUpdate<T>(ParticleSystemJobData particles, T updater) where T : unmanaged, IParticleUpdater
		{
			if (m_OrientParticlesWithBone)
			{
				AppendLifetimeUpdate(particles, new CompositeUpdater<T, Rotation3DUpdater>(updater, default(Rotation3DUpdater)));
			}
			else
			{
				AppendLifetimeUpdate(particles, updater);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AppendLifetimeUpdate<T>(ParticleSystemJobData particles, T updater) where T : unmanaged, IParticleUpdater
		{
			if (m_CorrectLifetime)
			{
				CompositeUpdater<T, LifetimeUpdater> updater2 = new CompositeUpdater<T, LifetimeUpdater>(updater, default(LifetimeUpdater));
				Update(particles, in updater2);
			}
			else
			{
				Update(particles, in updater);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Update<T>(ParticleSystemJobData particles, in T updater) where T : unmanaged, IParticleUpdater
		{
			int i = 0;
			for (int count = particles.count; i < count; i++)
			{
				int index = i % m_BoneTransforms.Length;
				BoneTransform boneTransform = m_BoneTransforms[index];
				BoneData boneData = m_BoneDatas[index];
				updater.Update(in particles, in i, in m_SnapSettings, in boneTransform, in boneData);
			}
		}
	}

	private static readonly Stack<TransformSnap> s_Pool = new Stack<TransformSnap>();

	private bool m_OrientParticlesWithBone;

	private ParticleSystem m_ParticleSystem;

	private ParticleSystemRenderer m_ParticleSystemRenderer;

	private SnapMapBase m_SnapMap;

	private ParticleSystemSnapshot m_ParticleSystemSnapshot;

	private bool m_JobScheduled;

	private JobHandle m_ScheduledJobHandle;

	private TransformAccessArray m_BoneTransformAccessArray;

	private NativeArray<BoneTransform> m_BoneTransforms;

	private NativeArray<BoneData> m_BoneDataArray;

	private SnapSettings m_SnapSettings;

	public static TransformSnap GetPooled(ParticleSystem particleSystem, ParticleSystemRenderer particleSystemRenderer, SnapMapBase snapMap, ParticleSystemSnapshot particleSystemSnapshot, ICollection<FxBone> fxBones, bool orientParticlesWithBone)
	{
		if (!s_Pool.TryPop(out var result))
		{
			result = new TransformSnap();
		}
		result.m_ParticleSystem = particleSystem;
		result.m_ParticleSystemRenderer = particleSystemRenderer;
		result.m_SnapMap = snapMap;
		result.m_ParticleSystemSnapshot = particleSystemSnapshot;
		result.m_OrientParticlesWithBone = orientParticlesWithBone;
		result.m_BoneTransformAccessArray = new TransformAccessArray(fxBones.Count);
		result.m_BoneTransforms = new NativeArray<BoneTransform>(fxBones.Count, Allocator.Persistent);
		result.m_BoneDataArray = new NativeArray<BoneData>(fxBones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		foreach (FxBone fxBone in fxBones)
		{
			result.m_BoneTransformAccessArray.Add(fxBone.Transform);
			result.m_BoneDataArray[num++] = new BoneData
			{
				cameraOffset = fxBone.CameraOffset,
				particleSize = fxBone.ParticleSize
			};
		}
		return result;
	}

	private TransformSnap()
	{
	}

	public void Recycle()
	{
		CompleteJob();
		m_BoneTransformAccessArray.Dispose();
		m_BoneTransforms.Dispose();
		m_BoneDataArray.Dispose();
		m_OrientParticlesWithBone = false;
		m_ParticleSystem = null;
		m_ParticleSystemRenderer = null;
		m_SnapMap = null;
		m_ParticleSystemSnapshot = null;
		m_JobScheduled = false;
		m_ScheduledJobHandle = default(JobHandle);
		m_BoneTransformAccessArray = default(TransformAccessArray);
		m_BoneTransforms = default(NativeArray<BoneTransform>);
		m_BoneDataArray = default(NativeArray<BoneData>);
		m_SnapSettings = default(SnapSettings);
		s_Pool.Push(this);
	}

	public void Setup()
	{
		ParticleSystem.MainModule main = m_ParticleSystem.main;
		main.maxParticles = m_BoneTransformAccessArray.length;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		ParticleSystem.EmissionModule emission = m_ParticleSystem.emission;
		emission.enabled = false;
		m_ParticleSystem.Clear();
		m_ParticleSystem.Emit(m_BoneTransformAccessArray.length);
		InitializeParticlesStartSize();
		if (m_OrientParticlesWithBone)
		{
			main.startRotation3D = true;
			m_ParticleSystemRenderer.alignment = ParticleSystemRenderSpace.World;
			if (ParticleSystemRenderMode.Mesh != m_ParticleSystemRenderer.renderMode)
			{
				m_ParticleSystemRenderer.renderMode = ParticleSystemRenderMode.Mesh;
				m_ParticleSystemRenderer.mesh = RenderingUtils.QuadFlippedMesh;
			}
		}
	}

	private unsafe void InitializeParticlesStartSize()
	{
		int particleCount = m_ParticleSystem.particleCount;
		if (particleCount == 0)
		{
			return;
		}
		int length = m_BoneDataArray.Length;
		if (length == 0)
		{
			return;
		}
		float particleSizeScale = m_SnapMap.ParticleSizeScale;
		ParticleSystem.MainModule main = m_ParticleSystem.main;
		float3 @float;
		if (main.startSize3D)
		{
			ParticleSystem.MinMaxCurve minMaxCurveSource = main.startSizeX;
			ParticleSystem.MinMaxCurve minMaxCurveSource2 = main.startSizeY;
			ParticleSystem.MinMaxCurve minMaxCurveSource3 = main.startSizeZ;
			@float = new float3(m_ParticleSystemSnapshot.startSizeX.Evaluate(in minMaxCurveSource, UnityEngine.Random.value) * particleSizeScale, m_ParticleSystemSnapshot.startSizeY.Evaluate(in minMaxCurveSource2, UnityEngine.Random.value) * particleSizeScale, m_ParticleSystemSnapshot.startSizeZ.Evaluate(in minMaxCurveSource3, UnityEngine.Random.value) * particleSizeScale);
		}
		else
		{
			ref ParticleSystemSnapshot.MinMaxCurveSnapshot startSize = ref m_ParticleSystemSnapshot.startSize;
			ParticleSystem.MinMaxCurve minMaxCurveSource4 = main.startSize;
			@float = new float3(startSize.Evaluate(in minMaxCurveSource4, UnityEngine.Random.value) * particleSizeScale);
		}
		NativeArray<ParticleSystem.Particle> nativeArray = new NativeArray<ParticleSystem.Particle>(particleCount, Allocator.Temp);
		try
		{
			m_ParticleSystem.GetParticles(nativeArray);
			ParticleSystem.Particle* ptr = (ParticleSystem.Particle*)nativeArray.GetUnsafePtr();
			BoneData* unsafePtr = (BoneData*)m_BoneDataArray.GetUnsafePtr();
			if (m_ParticleSystem.main.startSize3D)
			{
				int num = 0;
				while (num < particleCount)
				{
					int num2 = num % length;
					float particleSize = unsafePtr[num2].particleSize;
					ptr->startSize3D = @float * particleSize;
					num++;
					ptr++;
				}
			}
			else
			{
				int num3 = 0;
				while (num3 < particleCount)
				{
					int num4 = num3 % length;
					float particleSize2 = unsafePtr[num4].particleSize;
					ptr->startSize = @float.x * particleSize2;
					num3++;
					ptr++;
				}
			}
			m_ParticleSystem.SetParticles(nativeArray);
		}
		finally
		{
			nativeArray.Dispose();
		}
	}

	public void Update(in CameraData cameraData, in AnimationSample animationSample)
	{
		m_SnapSettings.offset = animationSample.offset;
		m_SnapSettings.cameraPosition = cameraData.position;
		m_SnapSettings.cameraOffsetScale = animationSample.cameraOffsetScale;
	}

	public void OnParticleUpdateJobScheduled()
	{
		CompleteJob();
		ScheduleJob();
	}

	private void ScheduleJob()
	{
		ParticleSystem.MainModule main = m_ParticleSystem.main;
		JobHandle dependsOn = new BuildBoneTransformsJob(m_BoneTransforms).ScheduleReadOnly(m_BoneTransformAccessArray, m_BoneTransformAccessArray.length);
		JobHandle scheduledJobHandle = new UpdateParticlesJob(m_SnapSettings, m_OrientParticlesWithBone, main.loop, m_BoneTransforms, m_BoneDataArray).Schedule(m_ParticleSystem, dependsOn);
		m_JobScheduled = true;
		m_ScheduledJobHandle = scheduledJobHandle;
	}

	private void CompleteJob()
	{
		if (m_JobScheduled)
		{
			m_JobScheduled = false;
			m_ScheduledJobHandle.Complete();
		}
	}

	void ISnapBehaviour.Update(in CameraData cameraData, in AnimationSample animationSample)
	{
		Update(in cameraData, in animationSample);
	}
}
