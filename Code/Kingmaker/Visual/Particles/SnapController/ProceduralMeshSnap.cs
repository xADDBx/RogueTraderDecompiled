using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Particles.SnapController;

internal sealed class ProceduralMeshSnap : ISnapBehaviour
{
	[BurstCompile]
	private struct BoneData
	{
		public float cameraOffset;

		public float3 localOffset;
	}

	[BurstCompile]
	private struct GenerateVertexBufferJob : IJobParallelForTransform
	{
		public float3 m_CameraPosition;

		public float3 m_Offset;

		public float m_CameraOffsetScale;

		public Matrix4x4 m_ParticleSystemRendererWorldToLocalMatrix;

		[ReadOnly]
		public NativeArray<BoneData> m_BoneDataBuffer;

		[WriteOnly]
		public NativeArray<float3> m_VertexBuffer;

		public void Execute(int index, TransformAccess transform)
		{
			if (transform.isValid)
			{
				BoneData boneData = m_BoneDataBuffer[index];
				float3 @float = math.transform(transform.localToWorldMatrix, boneData.localOffset);
				float3 float2 = math.normalize(m_CameraPosition - @float);
				float3 float3 = m_Offset + float2 * boneData.cameraOffset * m_CameraOffsetScale;
				m_VertexBuffer[index] = math.transform(m_ParticleSystemRendererWorldToLocalMatrix, @float + float3);
			}
			else
			{
				m_VertexBuffer[index] = default(float3);
			}
		}
	}

	[BurstCompile]
	private struct GenerateIndexBufferJob : IJob
	{
		[WriteOnly]
		public NativeArray<int> m_IndexBuffer;

		public int m_VertexCount;

		public void Execute()
		{
			int i = 0;
			int num = 0;
			for (; i < m_VertexCount; i++)
			{
				m_IndexBuffer[num++] = i;
				m_IndexBuffer[num++] = i;
				m_IndexBuffer[num++] = i;
			}
		}
	}

	private static readonly Stack<ProceduralMeshSnap> s_Pool = new Stack<ProceduralMeshSnap>();

	private static readonly VertexAttributeDescriptor[] s_VertexAttributeDescriptors = new VertexAttributeDescriptor[1]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0)
	};

	private readonly Mesh m_Mesh;

	private ParticleSystem m_ParticleSystem;

	private SnapMapBase m_SnapMap;

	private ParticleSystemSnapshot m_ParticleSystemSnapshot;

	private bool m_ApplySizeScale;

	private bool m_ApplyLifetimeScale;

	private bool m_ApplyRateOverTimeScale;

	private bool m_ApplyBurstScale;

	private TransformAccessArray m_BoneTransformAccessArray;

	private NativeArray<BoneData> m_BoneDatas;

	private bool m_MeshInitialized;

	private CameraData m_CameraData;

	private AnimationSample m_AnimationSample;

	private Matrix4x4 m_ParticleSystemRendererWorldToLocalMatrix;

	private bool m_JobScheduled;

	private JobHandle m_ScheduledJobHandle;

	private NativeArray<float3> m_VertexBuffer;

	private NativeArray<int> m_ScheduledJobIndexBuffer;

	public static ProceduralMeshSnap GetPooled(ICollection<FxBone> fxBones, bool applySizeScale, bool applyLifetimeScale, bool applyRateOverTimeScale, bool applyBurstScale, ParticleSystem particleSystem, SnapMapBase snapMap, ParticleSystemSnapshot particleSystemSnapshot)
	{
		if (!s_Pool.TryPop(out var result))
		{
			result = new ProceduralMeshSnap();
		}
		result.m_ParticleSystem = particleSystem;
		result.m_SnapMap = snapMap;
		result.m_ParticleSystemSnapshot = particleSystemSnapshot;
		result.m_ApplySizeScale = applySizeScale;
		result.m_ApplyLifetimeScale = applyLifetimeScale;
		result.m_ApplyRateOverTimeScale = applyRateOverTimeScale;
		result.m_ApplyBurstScale = applyBurstScale;
		result.m_BoneTransformAccessArray = new TransformAccessArray(fxBones.Count);
		result.m_BoneDatas = new NativeArray<BoneData>(fxBones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.m_VertexBuffer = new NativeArray<float3>(fxBones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		int num = 0;
		foreach (FxBone fxBone in fxBones)
		{
			result.m_BoneTransformAccessArray.Add(fxBone.Transform);
			result.m_BoneDatas[num++] = new BoneData
			{
				cameraOffset = fxBone.CameraOffset,
				localOffset = fxBone.LocalOffset
			};
		}
		return result;
	}

	private ProceduralMeshSnap()
	{
		m_Mesh = new Mesh();
		m_Mesh.name = "Snap Controller Mesh";
		m_Mesh.MarkDynamic();
	}

	public void Recycle()
	{
		if (m_JobScheduled)
		{
			CompleteJobs();
		}
		m_VertexBuffer.Dispose();
		m_BoneTransformAccessArray.Dispose();
		m_BoneDatas.Dispose();
		m_ParticleSystem = null;
		m_SnapMap = null;
		m_ParticleSystemSnapshot = null;
		m_ApplySizeScale = false;
		m_ApplyLifetimeScale = false;
		m_ApplyRateOverTimeScale = false;
		m_ApplyBurstScale = false;
		m_BoneTransformAccessArray = default(TransformAccessArray);
		m_BoneDatas = default(NativeArray<BoneData>);
		m_MeshInitialized = false;
		m_CameraData = default(CameraData);
		m_AnimationSample = default(AnimationSample);
		m_ParticleSystemRendererWorldToLocalMatrix = default(Matrix4x4);
		m_JobScheduled = false;
		m_ScheduledJobHandle = default(JobHandle);
		m_VertexBuffer = default(NativeArray<float3>);
		m_ScheduledJobIndexBuffer = default(NativeArray<int>);
		s_Pool.Push(this);
	}

	public void Setup()
	{
		ParticleSystem.MainModule main = m_ParticleSystem.main;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		ParticleSystem.EmissionModule emission = m_ParticleSystem.emission;
		emission.enabled = true;
		ParticleSystem.ShapeModule shape = m_ParticleSystem.shape;
		shape.shapeType = ParticleSystemShapeType.Mesh;
		shape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
		shape.mesh = m_Mesh;
		shape.enabled = false;
		if (m_ApplySizeScale)
		{
			float sizeScale = m_SnapMap.SizeScale;
			if (main.startSize3D)
			{
				ParticleSystem.MinMaxCurve minMaxCurve = main.startSizeX;
				m_ParticleSystemSnapshot.startSizeX.ApplyTo(ref minMaxCurve, sizeScale);
				main.startSizeX = minMaxCurve;
				ParticleSystem.MinMaxCurve minMaxCurve2 = main.startSizeY;
				m_ParticleSystemSnapshot.startSizeY.ApplyTo(ref minMaxCurve2, sizeScale);
				main.startSizeY = minMaxCurve2;
				ParticleSystem.MinMaxCurve minMaxCurve3 = main.startSizeZ;
				m_ParticleSystemSnapshot.startSizeZ.ApplyTo(ref minMaxCurve3, sizeScale);
				main.startSizeZ = minMaxCurve3;
			}
			else
			{
				ParticleSystem.MinMaxCurve minMaxCurve4 = main.startSize;
				m_ParticleSystemSnapshot.startSize.ApplyTo(ref minMaxCurve4, sizeScale);
				main.startSize = minMaxCurve4;
			}
		}
		if (m_ApplyLifetimeScale)
		{
			float lifetimeScale = m_SnapMap.LifetimeScale;
			ParticleSystem.MinMaxCurve minMaxCurve5 = main.startLifetime;
			m_ParticleSystemSnapshot.startLifetime.ApplyTo(ref minMaxCurve5, lifetimeScale);
			main.startLifetime = minMaxCurve5;
		}
		if (m_ApplyRateOverTimeScale)
		{
			float rateOverTimeScale = m_SnapMap.RateOverTimeScale;
			ParticleSystem.MinMaxCurve minMaxCurve6 = emission.rateOverTime;
			m_ParticleSystemSnapshot.rateOverTime.ApplyTo(ref minMaxCurve6, rateOverTimeScale);
			emission.rateOverTime = minMaxCurve6;
		}
		if (m_ApplyBurstScale)
		{
			float burstScale = m_SnapMap.BurstScale;
			int num = Mathf.Min(emission.burstCount, m_ParticleSystemSnapshot.bursts.Count);
			for (int i = 0; i < num; i++)
			{
				ParticleSystem.Burst burst = emission.GetBurst(i);
				burst.minCount = (short)((float)m_ParticleSystemSnapshot.bursts[i].minCount * burstScale);
				burst.maxCount = (short)((float)m_ParticleSystemSnapshot.bursts[i].maxCount * burstScale);
				emission.SetBurst(i, burst);
			}
		}
	}

	public void Update(in CameraData cameraData, in AnimationSample animationSample)
	{
		m_CameraData = cameraData;
		m_AnimationSample = animationSample;
		m_ParticleSystemRendererWorldToLocalMatrix = m_ParticleSystem.transform.worldToLocalMatrix;
	}

	public void OnParticleUpdateJobScheduled()
	{
		if (m_JobScheduled)
		{
			CompleteJobs();
		}
		ScheduleJobs();
	}

	private void ScheduleJobs()
	{
		if (m_MeshInitialized)
		{
			int length = m_BoneTransformAccessArray.length;
			GenerateVertexBufferJob jobData = default(GenerateVertexBufferJob);
			jobData.m_CameraPosition = m_CameraData.position;
			jobData.m_Offset = m_AnimationSample.offset;
			jobData.m_CameraOffsetScale = m_AnimationSample.cameraOffsetScale;
			jobData.m_ParticleSystemRendererWorldToLocalMatrix = m_ParticleSystemRendererWorldToLocalMatrix;
			jobData.m_BoneDataBuffer = m_BoneDatas;
			jobData.m_VertexBuffer = m_VertexBuffer;
			JobHandle scheduledJobHandle = jobData.ScheduleReadOnly(m_BoneTransformAccessArray, length);
			m_JobScheduled = true;
			m_ScheduledJobHandle = scheduledJobHandle;
			m_ScheduledJobIndexBuffer = default(NativeArray<int>);
			return;
		}
		int length2 = m_BoneTransformAccessArray.length;
		NativeArray<int> nativeArray = new NativeArray<int>(length2 * 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		try
		{
			GenerateVertexBufferJob jobData2 = default(GenerateVertexBufferJob);
			jobData2.m_CameraPosition = m_CameraData.position;
			jobData2.m_Offset = m_AnimationSample.offset;
			jobData2.m_CameraOffsetScale = m_AnimationSample.cameraOffsetScale;
			jobData2.m_ParticleSystemRendererWorldToLocalMatrix = m_ParticleSystemRendererWorldToLocalMatrix;
			jobData2.m_BoneDataBuffer = m_BoneDatas;
			jobData2.m_VertexBuffer = m_VertexBuffer;
			GenerateIndexBufferJob jobData3 = default(GenerateIndexBufferJob);
			jobData3.m_IndexBuffer = nativeArray;
			jobData3.m_VertexCount = length2;
			JobHandle scheduledJobHandle2 = JobHandle.CombineDependencies(jobData2.ScheduleReadOnly(m_BoneTransformAccessArray, length2), jobData3.Schedule());
			m_JobScheduled = true;
			m_ScheduledJobHandle = scheduledJobHandle2;
			m_ScheduledJobIndexBuffer = nativeArray;
		}
		catch
		{
			nativeArray.Dispose();
			throw;
		}
	}

	private void CompleteJobs()
	{
		try
		{
			m_ScheduledJobHandle.Complete();
			if (m_MeshInitialized)
			{
				m_Mesh.SetVertexBufferData(m_VertexBuffer, 0, 0, m_VertexBuffer.Length);
				return;
			}
			m_Mesh.SetVertexBufferParams(m_VertexBuffer.Length, s_VertexAttributeDescriptors);
			m_Mesh.SetIndexBufferParams(m_ScheduledJobIndexBuffer.Length, IndexFormat.UInt32);
			m_Mesh.SetVertexBufferData(m_VertexBuffer, 0, 0, m_VertexBuffer.Length);
			m_Mesh.SetIndexBufferData(m_ScheduledJobIndexBuffer, 0, 0, m_ScheduledJobIndexBuffer.Length);
			m_Mesh.subMeshCount = 1;
			m_Mesh.SetSubMesh(0, new SubMeshDescriptor(0, m_ScheduledJobIndexBuffer.Length));
			ParticleSystem.ShapeModule shape = m_ParticleSystem.shape;
			shape.enabled = true;
		}
		finally
		{
			m_JobScheduled = false;
			if (!m_MeshInitialized)
			{
				m_ScheduledJobIndexBuffer.Dispose();
			}
			m_MeshInitialized = true;
		}
	}

	void ISnapBehaviour.Update(in CameraData cameraData, in AnimationSample animationSample)
	{
		Update(in cameraData, in animationSample);
	}
}
