using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateSuperiority : RulebookTargetEvent
{
	public readonly ValueModifiersManager SuperiorityValueModifiers = new ValueModifiersManager();

	private readonly AbilityData m_Ability;

	public int ResultSuperiorityNumber { get; private set; }

	public int ResultRawSuperiorityNumber { get; private set; }

	public int ResultTargetSuperiorityPenalty { get; private set; }

	private bool IsMelee
	{
		get
		{
			if (m_Ability.Weapon == null)
			{
				return m_Ability.Blueprint.GetRange() == 1;
			}
			return m_Ability.Weapon.Blueprint.IsMelee;
		}
	}

	public RuleCalculateSuperiority([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability)
	{
	}

	public RuleCalculateSuperiority([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability)
		: base(initiator, target)
	{
		m_Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!IsMelee)
		{
			return;
		}
		int num = Math.Max(0, (int)(base.Initiator.Size - 4));
		PartUnitBody optional = ((MechanicEntity)Target).GetOptional<PartUnitBody>();
		int num2 = ((optional != null && optional.PrimaryHand.MaybeWeapon?.Blueprint.IsMelee == true) ? Math.Max(0, (int)(Target.Size - 4)) : 0);
		ResultSuperiorityNumber = num - num2;
		if (base.InitiatorUnit.HasMechanicFeature(MechanicsFeatureType.HiveOutnumber) && ResultSuperiorityNumber > 0)
		{
			ResultSuperiorityNumber++;
		}
		if (base.TargetUnit != null && base.TargetUnit != Game.Instance.DefaultUnit && base.InitiatorUnit != null)
		{
			foreach (BaseUnitEntity engagedByUnit in base.TargetUnit.GetEngagedByUnits())
			{
				if (engagedByUnit != base.Initiator)
				{
					ResultSuperiorityNumber += Math.Max(1, (int)(engagedByUnit.Size - 3));
				}
			}
			foreach (BaseUnitEntity engagedByUnit2 in base.InitiatorUnit.GetEngagedByUnits())
			{
				if (engagedByUnit2 != Target)
				{
					ResultSuperiorityNumber -= Math.Max(1, (int)(engagedByUnit2.Size - 3));
				}
			}
		}
		BaseUnitEntity targetUnit = base.TargetUnit;
		ResultSuperiorityNumber = ((targetUnit == null || !targetUnit.HasMechanicFeature(MechanicsFeatureType.IgnoreMeleeOutnumbering)) ? ResultSuperiorityNumber : 0);
		ResultRawSuperiorityNumber = ResultSuperiorityNumber * (10 + SuperiorityValueModifiers.Value);
		ResultSuperiorityNumber = Math.Max(0, ResultSuperiorityNumber);
		ResultTargetSuperiorityPenalty = ResultSuperiorityNumber * (10 + SuperiorityValueModifiers.Value);
	}
}
