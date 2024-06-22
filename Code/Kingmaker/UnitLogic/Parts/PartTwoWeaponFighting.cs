using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartTwoWeaponFighting : UnitPart, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IInterruptTurnStartHandler<EntitySubscriber>, IInterruptTurnStartHandler, IEventTag<IInterruptTurnStartHandler, EntitySubscriber>, IUnitCommandEndHandler, IHashable
{
	private static readonly BlueprintAbilityGroup PrimaryHandAbilityGroup;

	private static readonly BlueprintAbilityGroup SecondaryHandAbilityGroup;

	[JsonProperty]
	private bool m_IsAttackedThisTurn;

	public bool EnableAttackWithPairedWeapon { get; set; }

	public bool IsAttackedThisTurn => m_IsAttackedThisTurn;

	static PartTwoWeaponFighting()
	{
		BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
		PrimaryHandAbilityGroup = combatRoot.PrimaryHandAbilityGroup.Get();
		SecondaryHandAbilityGroup = combatRoot.SecondaryHandAbilityGroup.Get();
	}

	public bool IsOtherAbilityGroupOnCooldown(AbilityData abilityData)
	{
		if (abilityData.Weapon == null)
		{
			return false;
		}
		if (GetCroupCooldown(SecondaryHandAbilityGroup) > 0 && abilityData.Caster.GetFirstWeapon() == abilityData.Weapon)
		{
			return true;
		}
		if (GetCroupCooldown(PrimaryHandAbilityGroup) > 0 && abilityData.Caster.GetSecondWeapon() == abilityData.Weapon)
		{
			return true;
		}
		return false;
	}

	private int GetCroupCooldown(BlueprintAbilityGroup group)
	{
		return base.Owner.GetAbilityCooldownsOptional()?.GroupCooldown(group) ?? 0;
	}

	public void HandleGroupCooldownRemoved(BlueprintAbilityGroup group)
	{
		BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
		if (group == combatRoot.PrimaryHandAbilityGroup.Get() || group == combatRoot.SecondaryHandAbilityGroup.Get())
		{
			UpdateIsAttackedThisTurn();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.Executor == base.Owner)
		{
			UpdateIsAttackedThisTurn();
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		ResetAttacks();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		ResetAttacks();
	}

	public void ResetAttacks()
	{
		m_IsAttackedThisTurn = false;
	}

	private void UpdateIsAttackedThisTurn()
	{
		m_IsAttackedThisTurn = base.Owner.AbilityCooldowns.GroupIsOnCooldown(PrimaryHandAbilityGroup) || base.Owner.AbilityCooldowns.GroupIsOnCooldown(SecondaryHandAbilityGroup);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_IsAttackedThisTurn);
		return result;
	}
}
