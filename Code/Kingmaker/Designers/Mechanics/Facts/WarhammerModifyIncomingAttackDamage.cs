using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c0a2f956ae0c481c8f8d1468d1ba6212")]
public class WarhammerModifyIncomingAttackDamage : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITurnStartHandler<EntitySubscriber>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ITurnStartHandler, EntitySubscriber>, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool WasAttackedThisTurn;

		public int Direction;
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue AdditionalAbsorption;

	public ContextValue AdditionalDeflection;

	public ContextValue AdditionalRighteousFuryChances;

	public ContextValue PercentDamageModifier;

	public bool OnlyFirstAttack;

	public bool OnlyAgainstCaster;

	public bool OnlyAgainstCasterPriorityTarget;

	public bool OnlyAgainstDirection;

	public bool ActionsOnlyOnMelee;

	public bool DoNotUseOnDOTs;

	public ActionList ActionsOnAttack;

	[SerializeField]
	[ShowIf("OnlyAgainstCasterPriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	public float Multiplier = 1f;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool MultiplyByBuffRank;

	[SerializeField]
	[ShowIf("MultiplyByBuffRank")]
	private BlueprintBuffReference m_StackingBuff;

	public bool OnlyAgainstFact;

	[SerializeField]
	[ShowIf("OnlyAgainstFact")]
	private BlueprintUnitFactReference m_CheckFact;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public BlueprintBuff StackingBuff => m_StackingBuff?.Get();

	public BlueprintUnitFact CheckFact => m_CheckFact?.Get();

	protected override void OnActivate()
	{
		base.OnActivate();
		TargetWrapper mainTarget = base.Context.MainTarget;
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (OnlyAgainstDirection)
		{
			if (mainTarget?.Entity == maybeCaster)
			{
				Vector3 point = base.Context.SourceAbilityContext.ClickedTarget.Point;
				componentData.Direction = CustomGraphHelper.GuessDirection((point - maybeCaster.Position).normalized);
			}
			else
			{
				componentData.Direction = CustomGraphHelper.GuessDirection((maybeCaster.Position - mainTarget.Point).normalized);
			}
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (CheckConditions(evt, evt.Ability))
		{
			float num = (MultiplyByBuffRank ? (Multiplier * (float)(base.Owner.Buffs.GetBuff(StackingBuff)?.GetRank() ?? 0)) : Multiplier);
			float num2 = ((num < 0f) ? (-0.5f) : 0.5f);
			evt.MinValueModifiers.Add((int)((float)AdditionalDamageMin.Calculate(base.Context) * num + num2), base.Fact);
			evt.MaxValueModifiers.Add((int)((float)AdditionalDamageMax.Calculate(base.Context) * num + num2), base.Fact);
			evt.Penetration.Add(ModifierType.ValAdd, (int)((float)AdditionalArmorPenetration.Calculate(base.Context) * num + num2), base.Fact);
			evt.Absorption.Add(ModifierType.ValAdd, (int)((float)AdditionalAbsorption.Calculate(base.Context) * num + num2), base.Fact);
			evt.Deflection.Add(ModifierType.ValAdd, (int)((float)AdditionalDeflection.Calculate(base.Context) * num + num2), base.Fact);
			MechanicEntity maybeCaster = base.Context.MaybeCaster;
			float num3 = ((maybeCaster == null) ? 1f : (maybeCaster.Facts.GetComponents<WarhammerMultiplyIncomingDamageBonus>().Sum((WarhammerMultiplyIncomingDamageBonus p) => p.PercentIncreaseMultiplier - 1f) + 1f));
			int value = (int)((float)PercentDamageModifier.Calculate(base.Context) * num3);
			evt.ValueModifiers.Add(ModifierType.PctAdd, value, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateRighteousFuryChance evt)
	{
		if (CheckConditions(evt, evt.Ability))
		{
			float num = (MultiplyByBuffRank ? (Multiplier * (float)(base.Owner.Buffs.GetBuff(StackingBuff)?.GetRank() ?? 0)) : Multiplier);
			float num2 = ((num < 0f) ? (-0.5f) : 0.5f);
			evt.ChanceModifiers.Add((int)((float)AdditionalRighteousFuryChances.Calculate(base.Context) * num + num2), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateRighteousFuryChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (CheckConditions(evt, evt.SourceAbility))
		{
			bool flag = evt.SourceAbility?.Weapon != null && evt.SourceAbility.Weapon.Blueprint.IsMelee;
			if (ActionsOnlyOnMelee && !flag)
			{
				return;
			}
			using (base.Context.GetDataScope(evt.ConcreteInitiator.ToITargetWrapper()))
			{
				ActionsOnAttack.Run();
			}
		}
		componentData.WasAttackedThisTurn = true;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		RequestTransientData<ComponentData>().WasAttackedThisTurn = false;
	}

	public bool CheckConditions(RulebookEvent evt, AbilityData ability)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!Restrictions.IsPassed(base.Fact, evt, ability))
		{
			return false;
		}
		if (DoNotUseOnDOTs && ability == null)
		{
			return false;
		}
		if (OnlyFirstAttack && componentData.WasAttackedThisTurn)
		{
			return false;
		}
		if (OnlyAgainstCaster && base.Context.MaybeCaster != evt.Initiator)
		{
			return false;
		}
		if (SpecificRangeType && !WeaponRangeType.IsSuitableWeapon(ability.Weapon))
		{
			return false;
		}
		if (OnlyAgainstCasterPriorityTarget)
		{
			BaseUnitEntity baseUnitEntity = base.Context.MaybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
			if (baseUnitEntity == null || baseUnitEntity != evt.Initiator)
			{
				return false;
			}
		}
		if (OnlyAgainstFact && !evt.ConcreteInitiator.Facts.Contains(CheckFact))
		{
			return false;
		}
		if (OnlyAgainstDirection && !CheckIncomingDamageDirection(componentData.Direction, evt))
		{
			return false;
		}
		return true;
	}

	private bool CheckIncomingDamageDirection(int direction, RulebookEvent evt)
	{
		int num = CustomGraphHelper.GuessDirection((evt.Initiator.Position - evt.GetRuleTarget().Position).normalized);
		if (num != direction && num != CustomGraphHelper.LeftNeighbourDirection[direction])
		{
			return num == CustomGraphHelper.RightNeighbourDirection[direction];
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
