using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers;

public class SuppressEntitiesController : IControllerEnable, IController, IControllerDisable, IControllerTick, IAreaPartHandler, ISubscriber, IEntityPositionChangedHandler, ISubscriber<IEntity>, IInGameHandler
{
	private bool m_UpdateAll;

	private readonly HashSet<Entity> m_UpdateList = new HashSet<Entity>();

	private SceneEntitiesState m_CrossSceneState;

	public void OnEnable()
	{
		SceneEntitiesState.OnAdded += OnEntityAddedToState;
		m_UpdateAll = true;
		m_CrossSceneState = Game.Instance.Player.CrossSceneState;
	}

	public void OnDisable()
	{
		SceneEntitiesState.OnAdded -= OnEntityAddedToState;
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		BlueprintArea blueprintArea = SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedArea, null);
		if (blueprintArea == null || !blueprintArea.HasParts)
		{
			m_UpdateList.Clear();
			return;
		}
		Bounds? bounds = ObjectExtensions.Or(Game.Instance.CurrentlyLoadedAreaPart.Bounds, null)?.MechanicBounds;
		if (m_UpdateAll)
		{
			foreach (Entity item in Game.Instance.State.SuppressibleEntities.All)
			{
				UpdateEntity(bounds, item);
			}
		}
		else
		{
			foreach (Entity update in m_UpdateList)
			{
				UpdateEntity(bounds, update);
			}
		}
		m_UpdateAll = false;
		m_UpdateList.Clear();
	}

	private void UpdateEntity(Bounds? bounds, Entity entity)
	{
		using (ProfileScope.New("UpdateEntity"))
		{
			if (entity.HoldingState != null)
			{
				bool flag = entity.HoldingState == m_CrossSceneState;
				entity.Suppressed = !flag && bounds.HasValue && !bounds.Value.ContainsXZ(entity.Position);
			}
		}
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		m_UpdateAll = true;
	}

	private void OnEntityAddedToState(SceneEntitiesState state, Entity entity)
	{
		if (entity.IsSuppressible)
		{
			m_UpdateList.Add(entity);
		}
	}

	public void HandleEntityPositionChanged()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (entity.IsSuppressible)
		{
			m_UpdateList.Add(entity);
		}
	}

	public void HandleObjectInGameChanged()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (entity.IsInGame && entity.IsSuppressible)
		{
			m_UpdateList.Add(entity);
		}
	}
}
