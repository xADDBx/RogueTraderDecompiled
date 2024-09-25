using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem;

public abstract class RulebookEvent : IRulebookEvent
{
	public static class Dice
	{
		public static int D2 => D(1, DiceType.D2);

		public static int D3 => D(1, DiceType.D3);

		public static int D4 => D(1, DiceType.D4);

		public static int D6 => D(1, DiceType.D6);

		public static int D8 => D(1, DiceType.D8);

		public static int D12 => D(1, DiceType.D12);

		public static RuleRollD10 D10
		{
			get
			{
				RuleRollD10 ruleRollD = new RuleRollD10(Rulebook.CurrentContext.Current?.Initiator);
				Rulebook.Trigger(ruleRollD);
				return ruleRollD;
			}
		}

		public static RuleRollD20 D20
		{
			get
			{
				RuleRollD20 ruleRollD = new RuleRollD20(Rulebook.CurrentContext.Current?.Initiator);
				Rulebook.Trigger(ruleRollD);
				return ruleRollD;
			}
		}

		public static RuleRollD100 D100
		{
			get
			{
				RuleRollD100 ruleRollD = new RuleRollD100(Rulebook.CurrentContext.Current?.Initiator);
				Rulebook.Trigger(ruleRollD);
				return ruleRollD;
			}
		}

		public static bool MeasureNextRoll { get; set; }

		private static int D(int rolls, DiceType dice)
		{
			int num = 0;
			while (rolls-- > 0)
			{
				int num2 = PFStatefulRandom.RuleSystem.Range(1, dice.Sides() + 1);
				num += num2;
			}
			return num;
		}

		public static int D(DiceFormula formula)
		{
			return D(formula.Rolls, formula.Dice);
		}
	}

	public class CustomDataKey
	{
		private static int s_NextKey;

		private readonly int m_Key;

		private readonly string m_Name;

		public CustomDataKey(string name)
		{
			m_Key = s_NextKey++;
			m_Name = name;
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}

		public override int GetHashCode()
		{
			return m_Key;
		}

		public override string ToString()
		{
			return m_Name;
		}
	}

	[NotNull]
	private readonly IMechanicEntity m_Initiator;

	private bool m_Triggered;

	[CanBeNull]
	private Dictionary<CustomDataKey, object> m_CustomData;

	private bool m_GameLogDisabled;

	public MechanicEntity ConcreteInitiator => (MechanicEntity)m_Initiator;

	public IMechanicEntity Initiator => m_Initiator;

	public RuleReason Reason { get; set; }

	public bool DisableGameLog
	{
		set
		{
			m_GameLogDisabled = value;
		}
	}

	public bool FromRuleWarhammerAttackRoll { get; set; }

	public virtual bool IsGameLogDisabled => m_GameLogDisabled;

	[CanBeNull]
	public BaseUnitEntity InitiatorUnit => Initiator as BaseUnitEntity;

	public bool IsTriggered => m_Triggered;

	public Type RootType => typeof(RulebookEvent);

	protected RulebookEvent([NotNull] IMechanicEntity initiator)
	{
		m_Initiator = initiator;
	}

	[CanBeNull]
	public virtual IMechanicEntity GetRuleTarget()
	{
		return null;
	}

	public abstract void OnTrigger(RulebookEventContext context);

	public void OnDidTrigger()
	{
		m_Triggered = true;
	}

	public void SetCustomData<T>(CustomDataKey key, T value)
	{
		(m_CustomData ?? (m_CustomData = new Dictionary<CustomDataKey, object>()))[key] = value;
	}

	public bool TryGetCustomData<T>(CustomDataKey key, out T value)
	{
		if (m_CustomData == null || !m_CustomData.TryGetValue(key, out var value2) || !(value2 is T))
		{
			value = default(T);
			return false;
		}
		value = (T)value2;
		return true;
	}
}
public abstract class RulebookEvent<TInitiator> : RulebookEvent where TInitiator : MechanicEntity
{
	public new TInitiator Initiator => (TInitiator)base.Initiator;

	protected RulebookEvent([NotNull] TInitiator initiator)
		: base(initiator)
	{
	}
}
