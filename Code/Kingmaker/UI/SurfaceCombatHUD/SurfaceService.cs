using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal sealed class SurfaceService : IDisposable
{
	private struct ScheduledJobData
	{
		public JobHandle jobHandle;

		public ProceduralMesh<FillVertex, uint> fillMesh;

		public ProceduralMesh<LineVertex, uint> outlineMesh;

		public SurfaceServiceRequest request;
	}

	private sealed class MaterialOverrideResolver
	{
		private readonly Dictionary<string, Material> m_MaterialNameToOverrideMaterialMap = new Dictionary<string, Material>(8);

		public void SetOverrideMaterials([CanBeNull] Material[] overrideMaterials)
		{
			m_MaterialNameToOverrideMaterialMap.Clear();
			if (overrideMaterials == null)
			{
				return;
			}
			foreach (Material material in overrideMaterials)
			{
				if (!(material == null))
				{
					string name = material.name;
					if (!string.IsNullOrEmpty(name))
					{
						m_MaterialNameToOverrideMaterialMap[name] = material;
					}
				}
			}
		}

		public void ResolveOverrides(List<MaterialData> materialDatas)
		{
			if (m_MaterialNameToOverrideMaterialMap.Count == 0)
			{
				return;
			}
			for (int i = 0; i < materialDatas.Count; i++)
			{
				MaterialData materialData = materialDatas[i];
				if (materialData.material == null)
				{
					continue;
				}
				string name = materialData.material.name;
				if (!string.IsNullOrEmpty(name) && m_MaterialNameToOverrideMaterialMap.TryGetValue(name, out var value))
				{
					if (value == null)
					{
						m_MaterialNameToOverrideMaterialMap.Remove(name);
					}
					else
					{
						materialDatas[i] = new MaterialData(value, materialData.overrides);
					}
				}
			}
		}
	}

	private SurfaceServiceRequest m_PendingRequest;

	private ScheduledJobData? m_ScheduledJobData;

	[NotNull]
	private readonly ProceduralMeshObject<FillVertex, uint> m_FillMeshObject;

	[NotNull]
	private readonly ProceduralMeshObject<LineVertex, uint> m_OutlineMeshObject;

	private readonly SurfaceServiceRequestPool m_RequestPool;

	private readonly MaterialOverrideResolver m_MaterialOverrideResolver = new MaterialOverrideResolver();

	public SurfaceService(MeshFilter fillMeshFilter, MeshRenderer fillMeshRenderer, MeshFilter outlineMeshFilter, MeshRenderer outlineMeshRenderer, SurfaceServiceRequestPool requestPool)
	{
		m_FillMeshObject = new ProceduralMeshObject<FillVertex, uint>(fillMeshRenderer, fillMeshFilter, FillVertex.kAttributes, IndexFormat.UInt32);
		m_OutlineMeshObject = new ProceduralMeshObject<LineVertex, uint>(outlineMeshRenderer, outlineMeshFilter, LineVertex.kAttributes, IndexFormat.UInt32);
		m_RequestPool = requestPool;
	}

	public void SetOverrideMaterials([CanBeNull] Material[] overrideMaterials)
	{
		m_MaterialOverrideResolver.SetOverrideMaterials(overrideMaterials);
	}

	public void SetPendingRequest(SurfaceServiceRequest request)
	{
		DiscardPendingRequest();
		m_PendingRequest = request;
	}

	public void DiscardPendingRequest()
	{
		if (m_PendingRequest != null)
		{
			m_RequestPool.Release(m_PendingRequest);
			m_PendingRequest = null;
		}
	}

	public void Update()
	{
		if (m_ScheduledJobData.HasValue)
		{
			CompleteJobs();
		}
		if (m_PendingRequest != null)
		{
			ScheduleJobs();
		}
	}

	public void Dispose()
	{
		if (m_ScheduledJobData.HasValue)
		{
			CompleteJobs();
		}
		DiscardPendingRequest();
		m_FillMeshObject.Dispose();
		m_OutlineMeshObject.Dispose();
	}

	private void ScheduleJobs()
	{
		SurfaceServiceRequest pendingRequest = m_PendingRequest;
		m_PendingRequest = null;
		CommandBuffer commandBuffer = pendingRequest.CommandBuffer;
		if (ContainsOutputCommands(in commandBuffer) && CombatHudGraphDataSource.IsGraphValid(pendingRequest.Graph))
		{
			m_MaterialOverrideResolver.ResolveOverrides(pendingRequest.Materials);
			ProceduralMesh<FillVertex, uint> proceduralMesh = new ProceduralMesh<FillVertex, uint>(Allocator.TempJob);
			ProceduralMesh<LineVertex, uint> proceduralMesh2 = new ProceduralMesh<LineVertex, uint>(Allocator.TempJob);
			try
			{
				CustomGridGraph graph = pendingRequest.Graph;
				JobHandle jobHandle = new BuildSurfaceJob(pendingRequest.OutlineSettings, pendingRequest.FillSettings, graph, pendingRequest.Areas, pendingRequest.CommandBuffer, proceduralMesh, proceduralMesh2).Schedule();
				m_ScheduledJobData = new ScheduledJobData
				{
					jobHandle = jobHandle,
					fillMesh = proceduralMesh,
					outlineMesh = proceduralMesh2,
					request = pendingRequest
				};
				return;
			}
			catch
			{
				proceduralMesh.Dispose();
				proceduralMesh2.Dispose();
				m_RequestPool.Release(pendingRequest);
				throw;
			}
		}
		m_FillMeshObject.Clear();
		m_OutlineMeshObject.Clear();
		m_RequestPool.Release(pendingRequest);
	}

	private void CompleteJobs()
	{
		ScheduledJobData value = m_ScheduledJobData.Value;
		m_ScheduledJobData = null;
		try
		{
			value.jobHandle.Complete();
			m_FillMeshObject.Update(value.fillMesh, value.request.Materials);
			m_OutlineMeshObject.Update(value.outlineMesh, value.request.Materials);
		}
		finally
		{
			value.fillMesh.Dispose();
			value.outlineMesh.Dispose();
			m_RequestPool.Release(value.request);
		}
	}

	private static bool ContainsOutputCommands(in CommandBuffer commandBuffer)
	{
		foreach (CommandRecord record in commandBuffer.recordList)
		{
			if (record.code == CommandCode.BuildFill || record.code == CommandCode.AppendOutlineMesh)
			{
				return true;
			}
		}
		return false;
	}
}
