using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[RequireComponent(typeof(AbstractUnitEntityView))]
public class FamiliarUnit : MonoBehaviour, IEntitySubscriber, IAreaLoadingStagesHandler, ISubscriber, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitCommandActHandler, EntitySubscriber>
{
	private LightweightUnitEntityView m_UnitView;

	private BaseUnitEntity m_Leader;

	private LightweightUnitEntity Unit
	{
		get
		{
			if (m_UnitView == null)
			{
				m_UnitView = GetComponent<LightweightUnitEntityView>();
			}
			return m_UnitView.Data;
		}
	}

	private BaseUnitEntity Leader
	{
		get
		{
			if (m_Leader == null)
			{
				m_Leader = Unit.GetOrCreate<UnitPartFamiliar>().Leader;
			}
			return m_Leader;
		}
	}

	private void Awake()
	{
		m_UnitView = GetComponent<LightweightUnitEntityView>();
		m_Leader = m_UnitView.Data.GetOrCreate<UnitPartFamiliar>().Leader;
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		TeleportToLeader();
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void TeleportToLeader()
	{
		UnitPartFollowedByUnits required = Leader.GetRequired<UnitPartFollowedByUnits>();
		Vector3 position = ObstacleAnalyzer.FindClosestPointToStandOn(Leader.Position, 0.3f);
		FollowersFormationController.CreateFollowerAction(Unit, required, position, FollowerActionType.Teleport);
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		TeleportToLeader();
	}

	public IEntity GetSubscribingEntity()
	{
		return Unit;
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (command is UnitMoveTo)
		{
			Unit.View.Asks?.OrderMoveExploration?.Schedule();
		}
	}
}
