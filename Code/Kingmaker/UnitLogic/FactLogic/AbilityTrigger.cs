using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a0905c3e64e84a978a09a1c77eb114dc")]
public abstract class AbilityTrigger : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	protected ActionList Action;

	[SerializeField]
	protected bool ForOneAbility;

	[ShowIf("ForOneAbility")]
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	protected bool ForMultipleAbilities;

	[ShowIf("ForMultipleAbilities")]
	[SerializeField]
	protected List<BlueprintAbilityReference> Abilities;

	[SerializeField]
	protected bool ForAbilityGroup;

	[ShowIf("ForAbilityGroup")]
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	[ShowIf("ForAbilityGroup")]
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_OtherAbilityGroupList;

	[SerializeField]
	protected bool ForUltimateAbilities;

	public BlueprintAbility Ability => m_Ability?.Get();

	private IEnumerable<BlueprintAbilityGroup> GetAbilityGroups()
	{
		if (!ForAbilityGroup)
		{
			yield break;
		}
		if (m_AbilityGroup != null)
		{
			yield return m_AbilityGroup.Get();
		}
		if (m_OtherAbilityGroupList == null)
		{
			yield break;
		}
		BlueprintAbilityGroupReference[] otherAbilityGroupList = m_OtherAbilityGroupList;
		foreach (BlueprintAbilityGroupReference blueprintAbilityGroupReference in otherAbilityGroupList)
		{
			if (blueprintAbilityGroupReference != null)
			{
				yield return blueprintAbilityGroupReference.Get();
			}
		}
	}

	protected void RunAction(BlueprintAbility ability, [CanBeNull] MechanicEntity initiator, [CanBeNull] TargetWrapper target, bool assignAsTarget, AbilityTrigger componentType)
	{
		MechanicEntity mechanicEntity = initiator ?? target?.Entity;
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
		}
		else if (((mechanicEntity == base.Context.MaybeOwner && componentType is AbilityRuleTriggerInitiator) || (target == base.Context.MaybeOwner && componentType is AbilityRuleTriggerTarget)) && CanRunActions(ability))
		{
			base.Fact.RunActionInContext(Action, assignAsTarget ? ((TargetWrapper)initiator) : target);
		}
	}

	protected void RunAction(BlueprintAbility ability, AbilityExecutionContext context, TargetWrapper target, bool assignAsTarget, bool assignContextFromAbility)
	{
		if (!CanRunActions(ability))
		{
			return;
		}
		if (assignContextFromAbility)
		{
			using (ContextData<MechanicsContext.Data>.Request().Setup(context, assignAsTarget ? ((TargetWrapper)context.Caster) : target))
			{
				Action.Run();
				return;
			}
		}
		base.Fact.RunActionInContext(Action, assignAsTarget ? ((TargetWrapper)context.Caster) : target);
	}

	protected bool CheckAbilityGroup(BlueprintAbility ability)
	{
		if (!ForAbilityGroup)
		{
			return true;
		}
		foreach (BlueprintAbilityGroup abilityGroup in GetAbilityGroups())
		{
			if (abilityGroup != null && ability.AbilityGroups.Contains(abilityGroup))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanRunActions(BlueprintAbility ability)
	{
		if ((!ForOneAbility || ability == Ability) && (!ForMultipleAbilities || Abilities.HasItem((BlueprintAbilityReference r) => r.Is(ability))) && CheckAbilityGroup(ability))
		{
			if (ForUltimateAbilities && !ability.IsHeroicAct)
			{
				return ability.IsDesperateMeasure;
			}
			return true;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
