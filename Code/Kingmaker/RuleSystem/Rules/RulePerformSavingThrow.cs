using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformSavingThrow : RulebookEvent
{
	public readonly StatType StatType;

	public readonly SavingThrowType Type;

	private readonly ValueModifiersManager m_ValueModifiers = new ValueModifiersManager();

	private readonly int m_DifficultyClassBase;

	private IMechanicEntity SavingThrowInitiator;

	public bool AutoPass { get; private set; }

	public RuleRollD100 D100 { get; }

	public int StatValue { get; private set; }

	public ReadonlyList<Modifier> ValueModifiersList => m_ValueModifiers.List;

	public bool AlwaysSucceed { get; private set; }

	public bool AlwaysFail { get; private set; }

	public EntityFact AlwaysSource { get; private set; }

	public ModifierDescriptor AlwaysDescriptor { get; private set; }

	public int DifficultyClass => m_DifficultyClassBase;

	public bool IsPassed
	{
		get
		{
			if (!AutoPass)
			{
				return IsSuccessRoll(D100);
			}
			return true;
		}
	}

	public override IMechanicEntity GetRuleTarget()
	{
		return SavingThrowInitiator;
	}

	public RulePerformSavingThrow(MechanicEntity entity, SavingThrowType saveType, int difficultyClass, MechanicEntity initiator = null)
		: base(entity)
	{
		Type = saveType;
		StatType = GetSave(saveType);
		m_DifficultyClassBase = difficultyClass;
		D100 = new RuleRollD100(entity);
		SavingThrowInitiator = initiator;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ModifiableValueSavingThrow savingThrowOptional = base.ConcreteInitiator.GetSavingThrowOptional(StatType);
		if (savingThrowOptional == null)
		{
			AutoPass = true;
		}
		else if (!base.ConcreteInitiator.IsDead)
		{
			Rulebook.Trigger(D100);
			if ((bool)base.ConcreteInitiator.Features?.CanRerollSavingThrow)
			{
				D100.Reroll(base.ConcreteInitiator?.MainFact, takeBest: true);
			}
			StatValue = savingThrowOptional.ModifiedValue + m_ValueModifiers.Value;
		}
	}

	public void AddValueModifiers(int value, [NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		m_ValueModifiers.Add(value, source, descriptor);
	}

	public void SetAlwaysSucceed([NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		if (!AlwaysSucceed && !AlwaysFail)
		{
			AlwaysSucceed = true;
			AlwaysSource = source;
			AlwaysDescriptor = descriptor;
		}
	}

	public void SetAlwaysFail([NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		if (!AlwaysFail)
		{
			AlwaysFail = true;
			AlwaysSucceed = false;
			AlwaysSource = source;
			AlwaysDescriptor = descriptor;
		}
	}

	private bool IsSuccessRoll(int d100)
	{
		if (AlwaysFail)
		{
			return false;
		}
		if (!AlwaysSucceed && d100 != 1)
		{
			if (d100 > 1)
			{
				return d100 <= StatValue + DifficultyClass;
			}
			return false;
		}
		return true;
	}

	private static StatType GetSave(SavingThrowType type)
	{
		return type switch
		{
			SavingThrowType.Fortitude => StatType.SaveFortitude, 
			SavingThrowType.Reflex => StatType.SaveReflex, 
			SavingThrowType.Will => StatType.SaveWill, 
			_ => StatType.Unknown, 
		};
	}
}
