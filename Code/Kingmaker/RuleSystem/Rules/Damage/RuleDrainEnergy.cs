using System;
using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleDrainEnergy : RulebookTargetEvent<UnitEntity>
{
	public EnergyDrainType Type;

	private TimeSpan? m_Duration;

	public DiceFormula DrainDice;

	public readonly int DrainValue;

	public bool StoryModeImmunity { get; private set; }

	public int Result { get; private set; }

	public bool Empower { get; set; }

	public bool Maximize { get; set; }

	public DamageCriticalModifierType? CriticalModifier { get; set; }

	public SavingThrowType SavingThrowType { get; set; }

	[CanBeNull]
	public MechanicsContext ParentContext { get; set; }

	public bool TargetIsImmune { get; set; }

	public RuleDrainEnergy([NotNull] MechanicEntity initiator, [NotNull] UnitEntity target, EnergyDrainType type, TimeSpan? duration, DiceFormula drainDice, int drainValue)
		: base(initiator, target)
	{
		if (type != EnergyDrainType.Permanent && !duration.HasValue)
		{
			throw new Exception("Energy drain type is not Permanent but duration is missing");
		}
		if (type == EnergyDrainType.Permanent && duration.HasValue)
		{
			throw new Exception("Energy drain type is Permanent but duration is specified");
		}
		Type = type;
		m_Duration = duration;
		DrainDice = drainDice;
		DrainValue = drainValue;
	}

	public RuleDrainEnergy([NotNull] IMechanicEntity initiator, [NotNull] UnitEntity target, EnergyDrainType type, TimeSpan? duration, DiceFormula drainDice, int drainValue)
		: this((MechanicEntity)initiator, target, type, duration, drainDice, drainValue)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (this.SkipBecauseOfShadow() || AbstractUnitCommand.CommandTargetUntargetable((MechanicEntity)base.Initiator, base.Target, this) || ((MechanicEntity)base.Initiator).IsAttackingGreenNPC(base.Target))
		{
			return;
		}
		StoryModeImmunity = false;
		TargetIsImmune |= StoryModeImmunity;
		if (!TargetIsImmune)
		{
			int num = DrainDice.Rolls;
			if (CriticalModifier.HasValue)
			{
				num *= CriticalModifier.Value.IntValue();
			}
			DiceFormula diceFormula = new DiceFormula(num, DrainDice.Dice);
			int num2 = (Maximize ? (diceFormula.Rolls * diceFormula.Dice.Sides()) : Rulebook.Trigger(new RuleRollDice(base.Initiator, diceFormula)).Result);
			Result = num2 + DrainValue;
			if (Empower)
			{
				Result = (int)((double)Result * 1.5);
			}
		}
	}
}
