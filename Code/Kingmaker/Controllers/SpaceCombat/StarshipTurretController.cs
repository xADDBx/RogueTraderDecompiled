using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Controllers.SpaceCombat;

public class StarshipTurretController : IControllerTick, IController, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	private EntityRef<StarshipEntity> m_CurrentUnit;

	private readonly HashSet<StarshipEntity> m_AttackedBy = new HashSet<StarshipEntity>();

	[CanBeNull]
	private StarshipEntity CurrentUnit
	{
		get
		{
			return m_CurrentUnit;
		}
		set
		{
			m_CurrentUnit = value;
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			return;
		}
		StarshipEntity currentUnit = CurrentUnit;
		if (currentUnit == null || !IsTurretTarget(currentUnit))
		{
			return;
		}
		Vector3 position = currentUnit.View.ViewTransform.position;
		CustomGridNodeBase customGridNodeBase = AstarPath.active.GetNearest(position).node as CustomGridNodeBase;
		if ((double)(position - (Vector3)customGridNodeBase.position).sqrMagnitude > 0.1)
		{
			return;
		}
		foreach (UnitGroupMemory.UnitInfo enemy in currentUnit.CombatGroup.Memory.Enemies)
		{
			StarshipEntity starshipEntity = (StarshipEntity)enemy.Unit;
			if (m_AttackedBy.Contains(starshipEntity))
			{
				continue;
			}
			int num = starshipEntity.Starship.TurretRating;
			int num2 = starshipEntity.Starship.TurretRadius;
			if (num != 0 && num2 != 0)
			{
				CustomGridNodeBase b = AstarPath.active.GetNearest(starshipEntity.View.ViewTransform.position).node as CustomGridNodeBase;
				if (customGridNodeBase.CellDistanceTo(b) <= num2)
				{
					m_AttackedBy.Add(starshipEntity);
					DealTurretRatingDamage(starshipEntity, currentUnit, num);
				}
			}
		}
	}

	private bool IsTurretTarget(StarshipEntity unit)
	{
		if (unit.IsStarship())
		{
			return (int)unit.Starship.Crew == 0;
		}
		return false;
	}

	private void DealTurretRatingDamage(StarshipEntity initiator, StarshipEntity target, int turretRating)
	{
		RuleStarshipCalculateDamageForTarget ruleStarshipCalculateDamageForTarget = Rulebook.Trigger(new RuleStarshipCalculateDamageForTarget(initiator, target, turretRating));
		Rulebook.Trigger(new RuleDealDamage(initiator, target, ruleStarshipCalculateDamageForTarget.ResultDamage));
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (isTurnBased && mechanicEntity is StarshipEntity currentUnit)
		{
			CurrentUnit = currentUnit;
			m_AttackedBy.Clear();
		}
	}
}
