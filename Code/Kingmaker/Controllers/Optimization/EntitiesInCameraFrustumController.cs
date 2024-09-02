using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class EntitiesInCameraFrustumController : IControllerDisable, IController, IControllerTick
{
	private static readonly Collider2D[] OverlapResults = new Collider2D[512];

	private static readonly Plane[] m_Planes = new Plane[6];

	public void OnDisable()
	{
		foreach (Entity item in Game.Instance.State.Entities.All)
		{
			item.IsInCameraFrustum = true;
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
			float radius = ((cameraType == 3) ? 70f : 80f);
			Vector3 position = cameraData.position;
			GeometryUtility.CalculateFrustumPlanes(cameraData.matrix, m_Planes);
			int num = EntityBoundsHelper.OverlapCircle(position.To2D(), radius, OverlapResults, 16777216);
			for (int i = 0; i < num; i++)
			{
				Collider2D collider2D = OverlapResults[i];
				Entity entity = EntityDataLink.GetEntity(collider2D);
				if (entity != null && !entity.IsInCameraFrustum)
				{
					Bounds bounds = GetBounds(entity.Position, collider2D.bounds);
					if (GeometryUtility.TestPlanesAABB(m_Planes, bounds))
					{
						entity.IsInCameraFrustum = true;
					}
				}
			}
			Array.Clear(OverlapResults, 0, OverlapResults.Length);
			break;
		}
		}
		static Bounds GetBounds(Vector3 entityPosition, Bounds collider2dBounds)
		{
			Vector3 size = new Vector3(collider2dBounds.size.x, 2f, collider2dBounds.size.y);
			size *= 1.25f;
			return new Bounds(entityPosition + Vector3.up * size.y / 2f, size);
		}
	}
}
