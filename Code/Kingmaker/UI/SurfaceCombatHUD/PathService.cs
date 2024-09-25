using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class PathService : IDisposable
{
	private struct ScheduledJobData
	{
		public PathServiceRequest request;

		public ProceduralMesh<LineVertex, uint> proceduralMesh;
	}

	private static readonly int _SpatialDistanceTraveled = Shader.PropertyToID("_SpatialDistanceTraveled");

	private readonly ProceduralMeshObject<LineVertex, uint> m_ProceduralMeshObject;

	private readonly PathServiceRequestPool m_RequestPool;

	private PathServiceRequest m_PendingRequest;

	private ScheduledJobData? m_ScheduledBuildJobData;

	private CombatHudPathProgressTracker m_PathProgressTracker;

	private Renderer m_Renderer;

	private MaterialPropertyBlock m_PropertyBlock;

	private JobHandle m_ScheduledJobHandle;

	public PathService(MeshFilter meshFilter, MeshRenderer meshRenderer, PathServiceRequestPool requestPool)
	{
		m_ProceduralMeshObject = new ProceduralMeshObject<LineVertex, uint>(meshRenderer, meshFilter, LineVertex.kAttributes, IndexFormat.UInt32);
		m_RequestPool = requestPool;
		m_PathProgressTracker = new CombatHudPathProgressTracker(Allocator.Persistent);
		m_Renderer = meshRenderer;
		m_PropertyBlock = new MaterialPropertyBlock();
	}

	public void Dispose()
	{
		m_ScheduledJobHandle.Complete();
		ConsumeBuildJobsResults();
		ConsumePathTrackingJobsResults();
		DiscardPendingRequest();
		m_ProceduralMeshObject.Dispose();
		m_PathProgressTracker.Dispose();
	}

	public void DiscardPendingRequest()
	{
		if (m_PendingRequest != null)
		{
			m_RequestPool.Release(m_PendingRequest);
			m_PendingRequest = null;
		}
	}

	public void SetPendingRequest(PathServiceRequest request)
	{
		DiscardPendingRequest();
		m_PendingRequest = request;
	}

	public void Update()
	{
		m_ScheduledJobHandle.Complete();
		ConsumeBuildJobsResults();
		ConsumePathTrackingJobsResults();
		JobHandle dependsOn = ScheduleBuildJobs(default(JobHandle));
		dependsOn = ScheduleProgressTrackingJobs(dependsOn);
		m_ScheduledJobHandle = dependsOn;
	}

	private JobHandle ScheduleBuildJobs(JobHandle dependsOn)
	{
		if (m_PendingRequest == null)
		{
			return dependsOn;
		}
		PathServiceRequest pendingRequest = m_PendingRequest;
		m_PendingRequest = null;
		ProceduralMesh<LineVertex, uint> proceduralMesh = new ProceduralMesh<LineVertex, uint>(Allocator.TempJob);
		try
		{
			JobHandle result = new BuildPathJob(pendingRequest.GridSettings, 0, pendingRequest.PathLineSettings, positionOffset: pendingRequest.positionOffset, graph: pendingRequest.Graph, pathSource: pendingRequest.source, proceduralMesh: proceduralMesh, approximatePath: m_PathProgressTracker.GetPath()).Schedule(dependsOn);
			m_ScheduledBuildJobData = new ScheduledJobData
			{
				request = pendingRequest,
				proceduralMesh = proceduralMesh
			};
			m_PathProgressTracker.SetTargetTransform(pendingRequest.progressTrackingTransform);
			m_PathProgressTracker.Invalidate();
			return result;
		}
		catch
		{
			proceduralMesh.Dispose();
			m_RequestPool.Release(pendingRequest);
			throw;
		}
	}

	private void ConsumeBuildJobsResults()
	{
		if (!m_ScheduledBuildJobData.HasValue)
		{
			return;
		}
		ScheduledJobData value = m_ScheduledBuildJobData.Value;
		m_ScheduledBuildJobData = null;
		try
		{
			List<MaterialData> value2;
			using (CollectionPool<List<MaterialData>, MaterialData>.Get(out value2))
			{
				value2.Add(new MaterialData(value.request.material, default(MaterialOverrides)));
				m_ProceduralMeshObject.Update(value.proceduralMesh, value2, applyMaterialPropertyOverrides: false);
			}
		}
		finally
		{
			value.proceduralMesh.Dispose();
			m_RequestPool.Release(value.request);
		}
	}

	private JobHandle ScheduleProgressTrackingJobs(JobHandle dependsOn)
	{
		return m_PathProgressTracker.ScheduleJobs(dependsOn);
	}

	private void ConsumePathTrackingJobsResults()
	{
		if (!(m_Renderer == null))
		{
			m_PropertyBlock.SetFloat(_SpatialDistanceTraveled, m_PathProgressTracker.GetTraveledDistance());
			m_Renderer.SetPropertyBlock(m_PropertyBlock);
		}
	}
}
