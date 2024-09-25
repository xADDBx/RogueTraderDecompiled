using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Controllers;

public class InspectUnitsController : IControllerTick, IController, IEntityRevealedHandler, ISubscriber<IEntity>, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	private readonly HashSet<BaseUnitEntity> m_Units = new HashSet<BaseUnitEntity>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			foreach (BaseUnitEntity unit in m_Units)
			{
				Game.Instance.Player.InspectUnitsManager.TryMakeKnowledgeCheck(unit);
			}
			m_Units.Clear();
		}
	}

	public void HandleEntityRevealed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && Game.Instance.Player.InspectUnitsManager.GetInfo(baseUnitEntity) == null)
		{
			m_Units.Add(baseUnitEntity);
		}
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			m_Units.Add(baseUnitEntity);
		}
	}
}
