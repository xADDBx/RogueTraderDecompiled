using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Controllers.Optimization;

public class EntityBoundsController : IControllerTick, IController, IAsyncDisposable
{
	public interface IHasTick
	{
		void Tick();
	}

	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("EntityBoundsController");

	private readonly LinkedList<IHasTick> m_UpdateList = new LinkedList<IHasTick>();

	private Scene m_Scene;

	private PhysicsScene2D? m_PhysicsScene;

	public Scene Scene
	{
		get
		{
			if (m_Scene.IsValid())
			{
				return m_Scene;
			}
			m_Scene = SceneManager.CreateScene("EntityBounds", new CreateSceneParameters(LocalPhysicsMode.Physics2D | LocalPhysicsMode.Physics3D));
			return m_Scene;
		}
	}

	public PhysicsScene2D PhysicsScene
	{
		get
		{
			if (m_PhysicsScene.HasValue && m_PhysicsScene.Value.IsValid())
			{
				return m_PhysicsScene.Value;
			}
			m_PhysicsScene = Scene.GetPhysicsScene2D();
			return m_PhysicsScene.Value;
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (IHasTick update in m_UpdateList)
		{
			TickEntitySafe(update);
		}
		m_UpdateList.Clear();
		EntityBoundsHelper.ClearOverlapResults();
		PhysicsScene.Simulate((float)Game.Instance.TimeController.DeltaTimeSpan.TotalSeconds);
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	private static void LogToReplay()
	{
		foreach (AbstractUnitEntity allAwakeUnit in Game.Instance.State.AllAwakeUnits)
		{
			EntityBoundsPart optional = allAwakeUnit.GetOptional<EntityBoundsPart>();
			if (optional == null)
			{
				continue;
			}
			CircleCollider2D sphereVisionCollider = optional.SphereVisionCollider;
			if (!sphereVisionCollider)
			{
				continue;
			}
			EntityViewTrigger component = sphereVisionCollider.GetComponent<EntityViewTrigger>();
			if ((bool)component)
			{
				component.Exited.Sort((BaseUnitEntity a, BaseUnitEntity b) => string.Compare(a.UniqueId, b.UniqueId, StringComparison.Ordinal));
				component.Entered.Sort((BaseUnitEntity a, BaseUnitEntity b) => string.Compare(a.UniqueId, b.UniqueId, StringComparison.Ordinal));
				component.Exited.Clear();
				component.Entered.Clear();
			}
		}
	}

	private static void TickEntitySafe(IHasTick part)
	{
		try
		{
			part.Tick();
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Exception in EntityBoundsController on {0}", part);
		}
	}

	public void CancelUpdate(LinkedListNode<IHasTick> part)
	{
		if (part.List != null)
		{
			m_UpdateList.Remove(part);
		}
	}

	public void ScheduleUpdate(LinkedListNode<IHasTick> part)
	{
		if (part.List == null)
		{
			m_UpdateList.AddLast(part);
		}
	}

	public void HandleAreaBeginUnloading()
	{
		EntityDataLink.ClearCache();
	}

	public async ValueTask DisposeAsync()
	{
		if (m_Scene.IsValid())
		{
			Scene scene = m_Scene;
			m_Scene = default(Scene);
			await SceneManager.UnloadSceneAsync(scene);
		}
	}
}
