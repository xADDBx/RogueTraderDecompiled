using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
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
	public readonly CompositeModifiersManager AbsorptionCompositeModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager DeflectionCompositeModifiers = new CompositeModifiersManager();

	public int? PctMinAbsorption;

	public int? PctMinDeflection;

	public int? MinAbsorptionValue;

	public int? MinDeflectionValue;

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
		if (MinAbsorptionValue.HasValue)
		{
			val = Math.Max(val, MinAbsorptionValue.Value);
		}
		if (PctMinAbsorption.HasValue)
		{
			val = Math.Max(val, Mathf.RoundToInt((float)(ResultBaseAbsorption * PctMinAbsorption.Value) / 100f));
		}
		int val2 = 0;
		if (MinDeflectionValue.HasValue)
		{
			val2 = Math.Max(val2, MinDeflectionValue.Value);
		}
		if (PctMinDeflection.HasValue)
		{
			val2 = Math.Max(val2, Mathf.RoundToInt((float)(ResultBaseAbsorption * PctMinDeflection.Value) / 100f));
		}
		ResultAbsorption = Math.Max(val, ResultBaseAbsorption + AbsorptionCompositeModifiers.Value);
		ResultDeflection = Math.Max(val2, ResultBaseDeflection + DeflectionCompositeModifiers.Value);
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
