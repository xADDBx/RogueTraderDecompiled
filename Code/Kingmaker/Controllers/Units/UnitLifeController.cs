using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitLifeController : BaseUnitController
{
	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		PartLifeState lifeStateOptional = unit.GetLifeStateOptional();
		if (lifeStateOptional != null)
		{
			return !lifeStateOptional.IsFinallyDead;
		}
		return false;
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		ForceTickOnUnit(unit);
	}

	public static void ForceTickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.LifeState.ScriptedKill || unit.LifeState.MarkedForDeath)
		{
			unit.Health.SetHitPointsLeft(0);
		}
		unit.LifeState.MarkedForDeath = false;
		UnitLifeState newLifeState = CalculateLifeState(unit);
		SetLifeState(unit, newLifeState);
		if ((bool)unit.Features.Immortality && !unit.LifeState.ScriptedKill && !Game.Instance.TurnController.TbActive && !LoadingProcess.Instance.IsLoadingInProcess)
		{
			UnitAnimationManager maybeAnimationManager = unit.MaybeAnimationManager;
			if ((object)maybeAnimationManager != null && maybeAnimationManager.IsGoingProne)
			{
				unit.LifeState.Resurrect();
			}
		}
	}

	private static UnitLifeState CalculateLifeState(AbstractUnitEntity unit)
	{
		if (unit.LifeState.ScriptedKill)
		{
			return UnitLifeState.Dead;
		}
		if (unit.Health.HitPointsLeft > 0)
		{
			return UnitLifeState.Conscious;
		}
		if (!unit.Features.UnconsciousOnZeroHealth && !IsActiveCompanionUnit(unit))
		{
			return UnitLifeState.Dead;
		}
		return UnitLifeState.Unconscious;
	}

	private static bool IsActiveCompanionUnit(AbstractUnitEntity unit)
	{
		if (!(unit is StarshipEntity))
		{
			UnitPartCompanion companionOptional = unit.GetCompanionOptional();
			if (companionOptional != null)
			{
				return companionOptional.State != CompanionState.ExCompanion;
			}
			return false;
		}
		return false;
	}

	public static void ForceUnitConscious(AbstractUnitEntity unit)
	{
		unit.LifeState.ScriptedKill = false;
		unit.LifeState.MarkedForDeath = false;
		if (unit.Health.HitPointsLeft < 1)
		{
			unit.Health.SetHitPointsLeft(1);
		}
		ForceTickOnUnit(unit);
	}

	private static void SetLifeStateAfterCheck(AbstractUnitEntity unit, UnitLifeState newLifeState, UnitLifeState prevLifeState)
	{
		using (ContextData<GameLogDisabled>.RequestIf(unit.GetOptional<Kill.SilentDeathUnitPart>() != null))
		{
			unit.Remove<Kill.SilentDeathUnitPart>();
			unit.LifeState.Set(newLifeState);
			switch (newLifeState)
			{
			case UnitLifeState.Dead:
				OnUnitDeath(unit);
				break;
			case UnitLifeState.Conscious:
				unit.Initiative.WasPreparedForRound = Mathf.Max(unit.Initiative.WasPreparedForRound, Game.Instance.TurnController.GameRound - 1);
				unit.GetCombatStateOptional()?.ReturnToStartingPositionIfNeeded();
				break;
			}
			if (!unit.LifeState.IsConscious)
			{
				if (unit.LifeState.IsDead)
				{
					unit.Commands.InterruptAll((AbstractUnitCommand _) => true);
				}
				else
				{
					unit.Commands.InterruptAllInterruptible();
				}
			}
			EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitLifeStateChanged>)delegate(IUnitLifeStateChanged h)
			{
				h.HandleUnitLifeStateChanged(prevLifeState);
			}, isCheckRuntime: true);
			if (newLifeState == UnitLifeState.Unconscious || newLifeState == UnitLifeState.Dead)
			{
				EventBus.RaiseEvent(delegate(IUnitDeathHandler h)
				{
					h.HandleUnitDeath(unit);
				});
			}
			if (unit.IsInCombat && newLifeState != 0)
			{
				unit.GetCombatStateOptional()?.LeaveCombat();
			}
		}
	}

	private static void SetLifeState(AbstractUnitEntity unit, UnitLifeState newLifeState)
	{
		UnitLifeState state = unit.LifeState.State;
		if (state != newLifeState)
		{
			SetLifeStateAfterCheck(unit, newLifeState, state);
		}
	}

	private static void TryGiveExperience(BaseUnitEntity unit)
	{
		if (!unit.GiveExperienceOnDeath || !unit.Faction.IsPlayerEnemy)
		{
			return;
		}
		if (unit.CR > 0)
		{
			unit.Blueprint.CallComponents(delegate(Experience c)
			{
				Experience.Apply(c, unit);
			});
		}
		else
		{
			PartStarshipNavigation starshipNavigationOptional = unit.GetStarshipNavigationOptional();
			if (starshipNavigationOptional != null && starshipNavigationOptional.IsSoftUnit)
			{
				return;
			}
			float num = ((unit.GetSummonedMonsterOption() != null) ? Root.Common.Progression.SummonedUnitExperienceFactor : 1f);
			GameHelper.GainExperience(Mathf.RoundToInt((float)ExperienceHelper.GetMobExp(unit.Blueprint.DifficultyType, Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0) * num), isExperienceForDeath: true);
		}
		unit.GiveExperienceOnDeath = false;
	}

	private static void OnUnitDeath(AbstractUnitEntity unit)
	{
		if (unit is BaseUnitEntity baseUnitEntity)
		{
			TryGiveExperience(baseUnitEntity);
			foreach (ItemEntity item in baseUnitEntity.Inventory)
			{
				item.UpdateSlotIndex(force: true);
			}
		}
		unit.View.HandleDeath();
		EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitDieHandler>)delegate(IUnitDieHandler x)
		{
			x.OnUnitDie();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent((IAbstractUnitEntity)unit, (Action<IUnitHandler>)delegate(IUnitHandler h)
		{
			h.HandleUnitDeath();
		}, isCheckRuntime: true);
	}
}
