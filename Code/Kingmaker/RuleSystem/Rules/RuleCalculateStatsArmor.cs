using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateStatsArmor : RulebookEvent
{
	public readonly struct LimitEntry
	{
		public readonly EntityFactRef Fact;

		public readonly int Value;

		public LimitEntry(EntityFactRef fact, int value)
		{
			Fact = fact;
			Value = value;
		}
	}

	public class ArmorStatLimits
	{
		public LimitEntry? MinAbsorptionFlat;

		public LimitEntry? MinAbsorptionPct;

		public LimitEntry? MaxAbsorptionFlat;

		public LimitEntry? MaxAbsorptionPct;

		public LimitEntry? MinDeflectionFlat;

		public LimitEntry? MinDeflectionPct;

		public LimitEntry? MaxDeflectionFlat;

		public LimitEntry? MaxDeflectionPct;

		public void CopyFrom(ArmorStatLimits other)
		{
			MinAbsorptionFlat = other.MinAbsorptionFlat;
			MinAbsorptionPct = other.MinAbsorptionPct;
			MaxAbsorptionFlat = other.MaxAbsorptionFlat;
			MaxAbsorptionPct = other.MaxAbsorptionPct;
			MinDeflectionFlat = other.MinDeflectionFlat;
			MinDeflectionPct = other.MinDeflectionPct;
			MaxDeflectionFlat = other.MaxDeflectionFlat;
			MaxDeflectionPct = other.MaxDeflectionPct;
		}
	}

	public readonly CompositeModifiersManager AbsorptionCompositeModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager DeflectionCompositeModifiers = new CompositeModifiersManager();

	public ArmorStatLimits StatLimits { get; } = new ArmorStatLimits();


	[CanBeNull]
	private ItemEntityArmor Armor { get; }

	public int ResultBaseAbsorption { get; private set; }

	public int ResultBaseDeflection { get; private set; }

	public int ResultAbsorption { get; private set; }

	public int ResultDeflection { get; private set; }

	public RuleCalculateStatsArmor([NotNull] MechanicEntity initiator, [CanBeNull] ItemEntityArmor armor)
		: base(initiator)
	{
		Armor = armor;
	}

	public RuleCalculateStatsArmor([NotNull] MechanicEntity initiator)
		: this(initiator, initiator.GetBodyOptional()?.Armor.MaybeArmor)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		TryApplyEnchantmentsManually();
		ResultBaseAbsorption = base.ConcreteInitiator.GetStatOptional(StatType.DamageAbsorption);
		ResultBaseDeflection = base.ConcreteInitiator.GetStatOptional(StatType.DamageDeflection);
		if (Armor != null)
		{
			ResultBaseAbsorption += Armor.Blueprint.DamageAbsorption;
			ResultBaseDeflection += Armor.Blueprint.DamageDeflection;
		}
		int val = 0;
		if (StatLimits.MinAbsorptionFlat.HasValue)
		{
			val = Math.Max(val, StatLimits.MinAbsorptionFlat.Value.Value);
		}
		if (StatLimits.MinAbsorptionPct.HasValue)
		{
			val = Math.Max(val, Mathf.RoundToInt((float)(ResultBaseAbsorption * StatLimits.MinAbsorptionPct.Value.Value) / 100f));
		}
		int val2 = 0;
		if (StatLimits.MinDeflectionFlat.HasValue)
		{
			val2 = Math.Max(val2, StatLimits.MinDeflectionFlat.Value.Value);
		}
		if (StatLimits.MinDeflectionPct.HasValue)
		{
			val2 = Math.Max(val2, Mathf.RoundToInt((float)(ResultBaseDeflection * StatLimits.MinDeflectionPct.Value.Value) / 100f));
		}
		int val3 = int.MaxValue;
		if (StatLimits.MaxAbsorptionFlat.HasValue)
		{
			val3 = Math.Min(val3, StatLimits.MaxAbsorptionFlat.Value.Value);
		}
		if (StatLimits.MaxAbsorptionPct.HasValue)
		{
			val3 = Math.Min(val3, Mathf.RoundToInt((float)(ResultBaseAbsorption * StatLimits.MaxAbsorptionPct.Value.Value) / 100f));
		}
		int val4 = int.MaxValue;
		if (StatLimits.MaxDeflectionFlat.HasValue)
		{
			val4 = Math.Min(val4, StatLimits.MaxDeflectionFlat.Value.Value);
		}
		if (StatLimits.MaxDeflectionPct.HasValue)
		{
			val4 = Math.Min(val4, Mathf.RoundToInt((float)(ResultBaseDeflection * StatLimits.MaxDeflectionPct.Value.Value) / 100f));
		}
		ResultAbsorption = Math.Max(val, Math.Min(val3, ResultBaseAbsorption + AbsorptionCompositeModifiers.Value));
		ResultDeflection = Math.Max(val2, Math.Min(val4, ResultBaseDeflection + DeflectionCompositeModifiers.Value));
	}

	private void TryApplyEnchantmentsManually()
	{
		if (Armor != null && base.Initiator != Armor.Wielder)
		{
			foreach (ItemEnchantment enchantment in Armor.Enchantments)
			{
				enchantment.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsArmor> c)
				{
					try
					{
						c.OnEventAboutToTrigger(this);
					}
					catch (Exception ex2)
					{
						PFLog.Default.Exception(ex2);
					}
				});
			}
		}
		if (base.ConcreteInitiator.Buffs.IsSubscribedOnEventBus)
		{
			return;
		}
		foreach (Buff rawFact in base.ConcreteInitiator.Buffs.RawFacts)
		{
			rawFact.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsArmor> c)
			{
				try
				{
					c.OnEventAboutToTrigger(this);
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			});
		}
	}

	public static int CalculateChapterSpecificAbsorptionValue(UnitEntity unit, ItemEntityArmor armor)
	{
		int num = Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0;
		int resultAbsorption = Rulebook.Trigger(new RuleCalculateStatsArmor(unit, armor)).ResultAbsorption;
		int num2 = ((num > 43) ? 25 : ((num > 28) ? 20 : ((num > 15) ? 15 : ((num > 2) ? 10 : 5))));
		return Math.Max(resultAbsorption - num2, 0);
	}
}
