using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Units;

public class UnitActivatableAbilitiesController : BaseUnitController
{
	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		return true;
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		if (!(unit is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		List<ActivatableAbility> rawFacts = baseUnitEntity.ActivatableAbilities.RawFacts;
		if (rawFacts.Count <= 0)
		{
			return;
		}
		foreach (ActivatableAbility ability in rawFacts)
		{
			if (ability.IsWaitingForTarget)
			{
				bool num = ability.Owner.Commands.Contains((AbstractUnitCommand c) => (c as UnitUseAbility)?.Ability.Fact == ability.SelectTargetAbility && !c.IsFinished);
				bool flag = Game.Instance.SelectedAbilityHandler.RootAbility?.Fact == ability.SelectTargetAbility;
				if (!num && !flag)
				{
					ability.IsOn = false;
				}
				continue;
			}
			bool flag2 = baseUnitEntity.GetOptional<UnitPartPreventInterruption>()?.CanInterrupt(ability.Blueprint) ?? true;
			PartUnitState state = ability.Owner.State;
			PartLifeState lifeState = ability.Owner.LifeState;
			bool flag3 = (ability.Blueprint.DeactivateIfOwnerDisabled && !state.CanAct && (state.IsAble || flag2)) || (ability.Blueprint.DeactivateIfOwnerUnconscious && !lifeState.IsConscious) || !ability.Blueprint.DoNotTurnOffOnRest;
			if (ability.IsStarted)
			{
				if (flag3)
				{
					ability.Stop();
					continue;
				}
				ability.TimeToNextRound = GetTimeToNextRound(ability.TimeToNextRound, baseUnitEntity);
				if (ability.TimeToNextRound < 5f && baseUnitEntity.IsCurrentUnit())
				{
					ability.TimeToNextRound = 0f;
				}
				if (ability.TimeToNextRound <= 0f && CanTickNewRound(baseUnitEntity))
				{
					ability.OnNewRound();
					ability.TimeToNextRound = 5f;
				}
			}
			else if (ability.IsOn && (ability.Blueprint.ActivateImmediately || ability.ReadyToStart) && !((ability.Blueprint.ActivateOnCombatStarts && !baseUnitEntity.IsInCombat) || flag3))
			{
				ability.TryStart();
			}
		}
		baseUnitEntity.GetOptional<UnitPartActivatableAbility>()?.Update();
	}

	public void ForceTickOnUnit(BaseUnitEntity unit)
	{
		TickOnUnit(unit);
	}

	private float GetTimeToNextRound(float timeToNextRound, BaseUnitEntity unit)
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			return timeToNextRound;
		}
		return timeToNextRound - Game.Instance.TimeController.GameDeltaTime;
	}

	private bool CanTickNewRound(BaseUnitEntity unit)
	{
		return true;
	}
}
