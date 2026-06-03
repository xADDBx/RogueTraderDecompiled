using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class EntitiesInCameraFrustumController : IControllerDisable, IController, IControllerTick
{
	[BurstCompile]
	private struct TestInFrustrumJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<Plane> Planes;

		[ReadOnly]
		public NativeArray<Bounds> BoundsArray;

		[WriteOnly]
		public NativeArray<bool> Results;

		[WriteOnly]
		public NativeArray<bool> Results_Closer;

		public TestInFrustrumJob(Plane[] planes, NativeArray<Bounds> bounds)
		{
			Planes = new NativeArray<Plane>(planes, Allocator.TempJob);
			BoundsArray = bounds;
			Results = new NativeArray<bool>(bounds.Length, Allocator.TempJob);
			Results_Closer = new NativeArray<bool>(bounds.Length, Allocator.TempJob);
		}

		public void Cleanup()
		{
			if (Planes.IsCreated)
			{
				Planes.Dispose();
			}
			if (Results.IsCreated)
			{
				Results.Dispose();
			}
			if (Results_Closer.IsCreated)
			{
				Results_Closer.Dispose();
			}
		}

		public void Execute(int index)
		{
			Results[index] = TestPlanesAABB(index);
			Results_Closer[index] = TestPlanesAABB(index, 0.5f);
		}

		private bool TestPlanesAABB(int index, float mul = 1f)
		{
			Bounds bounds = BoundsArray[index];
			bounds = new Bounds(bounds.center, bounds.size * mul);
			Vector3 vector = default(Vector3);
			for (int i = 0; i < Planes.Length; i++)
			{
				Vector3 normal = Planes[i].normal;
				float distance = Planes[i].distance;
				if (normal.x < 0f)
				{
					vector.x = bounds.min.x;
				}
				else
				{
					vector.x = bounds.max.x;
				}
				if (normal.y < 0f)
				{
					vector.y = bounds.min.y;
				}
				else
				{
					vector.y = bounds.max.y;
				}
				if (normal.z < 0f)
				{
					vector.z = bounds.min.z;
				}
				else
				{
					vector.z = bounds.max.z;
				}
				if (normal.x * vector.x + normal.y * vector.y + normal.z * vector.z + distance < 0f)
				{
					return false;
				}
			}
			return true;
		}
	}

	private static readonly Plane[] m_Planes = new Plane[6];

	private static NativeArray<Bounds> m_BoundsArray;

	private static List<MechanicEntity> m_Entities = new List<MechanicEntity>(1024);

	public void OnDisable()
	{
		foreach (Entity item in Game.Instance.State.Entities.All)
		{
			item.IsInCameraFrustum = true;
		}
		if (m_BoundsArray.IsCreated)
		{
			m_BoundsArray.Dispose();
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (Entity item in Game.Instance.State.Entities.All)
		{
			item.IsInCameraFrustum = false;
		}
		foreach (PlayerCommands<SynchronizedData> player in Game.Instance.SynchronizedDataController.SynchronizedData.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				ProcessCamera(command.cameraType, command.camera);
			}
		}
	}

	private static void ProcessCamera(byte cameraType, CameraData cameraData)
	{
		if (cameraType == 0 && cameraData == null)
		{
			return;
		}
		switch (cameraType)
		{
		case 1:
			break;
		default:
			PFLog.Net.Error($"Unexpected CameraType value '{cameraType}'");
			break;
		case 3:
		case 4:
		{
			_ = 3;
			_ = cameraData.position;
			GeometryUtility.CalculateFrustumPlanes(cameraData.matrix, m_Planes);
			int num = Game.Instance.State.MechanicEntities.Count();
			m_Entities.Clear();
			if (!m_BoundsArray.IsCreated || m_BoundsArray.Length < num)
			{
				if (m_BoundsArray.IsCreated)
				{
					m_BoundsArray.Dispose();
				}
				m_BoundsArray = new NativeArray<Bounds>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}
			foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
			{
				if (mechanicEntity.IsInCameraFrustum)
				{
					continue;
				}
				Vector3 position = mechanicEntity.Position;
				Bounds value;
				if (mechanicEntity is AbstractUnitEntity abstractUnitEntity)
				{
					value = abstractUnitEntity.View.RenderersBounds;
				}
				else if (mechanicEntity is DestructibleEntity destructibleEntity)
				{
					value = destructibleEntity.View.RenderersBounds;
				}
				else
				{
					Bounds? bounds = mechanicEntity.GetOptional<EntityBoundsPart>()?.SphereBoundsCollider?.bounds;
					if (!bounds.HasValue)
					{
						continue;
					}
					value = bounds.Value;
					value.size += value.size + Vector3.one * value.size.x;
				}
				value.center = position;
				m_Entities.Add(mechanicEntity);
				m_BoundsArray[m_Entities.Count - 1] = value;
			}
			TestInFrustrumJob jobData = new TestInFrustrumJob(m_Planes, m_BoundsArray);
			IJobParallelForExtensions.Schedule(jobData, m_Entities.Count, 64).Complete();
			for (int i = 0; i < m_Entities.Count; i++)
			{
				m_Entities[i].IsInCameraFrustum = jobData.Results[i];
			}
			jobData.Cleanup();
			break;
		}
		}
	}
}
