using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Visual.Sound;

public class SpaceCombatAsksController : IUnitAsksController, IDisposable, IPartyCombatHandler, ISubscriber, IUnitRunCommandHandler, IClickActionHandler, IDamageHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IShieldAbsorbsDamageHandler, ISubscriber<IStarshipEntity>
{
	public SpaceCombatAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!UnitAsksHelper.CanSpeakSpaceCombat)
		{
			return;
		}
		if (inCombat)
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.StartCombatReactionSpaceCombat);
		}
		else if (UnitAsksHelper.PlayerShip.LifeState.IsConscious)
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.WinCombatReactionSpaceCombat);
		}
	}

	void IUnitRunCommandHandler.HandleUnitRunCommand(AbstractUnitCommand command)
	{
		if (!UnitAsksHelper.CanSpeakSpaceCombat || !(command is UnitMoveToProper { Executor: var executor } unitMoveToProper) || !unitMoveToProper.Executor.IsPlayerShip())
		{
			return;
		}
		float num = Mathf.Min(unitMoveToProper.CalculatePathCost(executor.ToBaseUnitEntity()), executor.ToBaseUnitEntity().CombatState.ActionPointsBlue);
		int num2 = unitMoveToProper.ForcedPath.vectorPath.Count - 1;
		PartStarshipNavigation starshipNavigationOptional = Game.Instance.Player.PlayerShip.GetStarshipNavigationOptional();
		bool flag = starshipNavigationOptional == null || Game.Instance.Player.PlayerShip.CombatState.ActionPointsBlue - num < (float)starshipNavigationOptional.FinishingTilesCount || !starshipNavigationOptional.HasAnotherPlaceToStand;
		if (num2 >= UnitAsksHelper.TilesToBarkMoveOrderSpaceCombat || flag)
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.MoveOrderSpaceCombat);
		}
	}

	void IClickActionHandler.OnMoveRequested(Vector3 target)
	{
	}

	void IClickActionHandler.OnCastRequested(AbilityData ability, TargetWrapper target)
	{
	}

	void IClickActionHandler.OnItemUseRequested(AbilityData ability, TargetWrapper target)
	{
		if (!UnitAsksHelper.CanSpeakSpaceCombat || ability.StarshipWeapon == null)
		{
			return;
		}
		switch (ability.StarshipWeapon.Blueprint.WeaponType)
		{
		case StarshipWeaponType.Macrobatteries:
			if (ability.IsTargetInsideRestrictedFiringArc(target))
			{
				UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.FireMacroCannonsSpaceCombat);
			}
			break;
		case StarshipWeaponType.Lances:
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.LanceFireSpaceCombat);
			break;
		case StarshipWeaponType.TorpedoTubes:
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.LaunchTorpedosSpaceCombat);
			break;
		}
	}

	void IClickActionHandler.OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction)
	{
	}

	void IClickActionHandler.OnAttackRequested(BaseUnitEntity unit, UnitEntityView target)
	{
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage evt)
	{
		if (!UnitAsksHelper.CanSpeakSpaceCombat || !(evt.Target is StarshipEntity entity) || !entity.IsPlayerShip() || evt.TargetHealth == null)
		{
			return;
		}
		float num = (float)evt.TargetHealth.MaxHitPoints * UnitAsksHelper.LowHealthBarkHPPercent;
		bool flag = (float)evt.HPBeforeDamage < num;
		if ((float)evt.TargetHealth.HitPointsLeft < num && !flag)
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.LowHealthSpaceCombat);
		}
	}

	void IUnitLifeStateChanged.HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		if (!UnitAsksHelper.CanSpeakSpaceCombat)
		{
			return;
		}
		StarshipEntity ship = EventInvokerExtensions.StarshipEntity;
		if (ship != null && !ship.IsPlayerShip() && ship.LifeState.IsDead && UnitAsksHelper.EnemyShipSizesToBarkEnemyDeathSC.Any((Size x) => x == ship.Size))
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.EnemyDeathSpaceCombat);
		}
	}

	void IShieldAbsorbsDamageHandler.HandleShieldAbsorbsDamage(int shieldStrengthLoss, int shieldAfter, StarshipSectorShieldsType sector)
	{
		StarshipEntity unit = EventInvokerExtensions.StarshipEntity;
		bool flag = unit.IsPlayerShip();
		if (flag)
		{
			int shieldsSum = unit.Shields.ShieldsSum;
			int num = shieldsSum + shieldStrengthLoss;
			float num2 = (float)unit.Shields.ShieldsMaxSum * UnitAsksHelper.LowShieldBarkPercent;
			if ((float)shieldsSum <= num2 && (float)num > num2)
			{
				UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.LowShieldSpaceCombat);
			}
		}
		if (unit.Shields.GetCurrentShields(sector) > 0)
		{
			return;
		}
		if (flag)
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.ShieldSectionIsDownSpaceCombat);
		}
		else if (UnitAsksHelper.EnemyShipSizesToBarkShieldIsDownSC.Any((Size x) => x == unit.Size))
		{
			UnitAsksHelper.ScheduleRandomPersonalizedSpaceCombatBark((UnitBarksManager x) => x.EnemyShieldSectionIsDownSpaceCombat);
		}
	}
}
