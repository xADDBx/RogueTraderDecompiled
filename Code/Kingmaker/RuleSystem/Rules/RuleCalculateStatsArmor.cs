using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateStatsArmor : RulebookEvent
{
	public readonly CompositeModifiersManager AbsorptionCompositeModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager DeflectionCompositeModifiers = new CompositeModifiersManager();

	[CanBeNull]
	public ItemEntityArmor Armor { get; }

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
		if (Armor != null && base.Initiator != Armor.Wielder)
		{
			ApplyEnchantmentsManually();
		}
		ResultBaseAbsorption = base.ConcreteInitiator.GetStatOptional(StatType.DamageAbsorption);
		ResultBaseDeflection = base.ConcreteInitiator.GetStatOptional(StatType.DamageDeflection);
		if (Armor != null)
		{
			ResultBaseAbsorption += Armor.Blueprint.DamageAbsorption;
			ResultBaseDeflection += Armor.Blueprint.DamageDeflection;
		}
		ResultAbsorption = Math.Max(0, ResultBaseAbsorption + AbsorptionCompositeModifiers.Value);
		ResultDeflection = Math.Max(0, ResultBaseDeflection + DeflectionCompositeModifiers.Value);
	}

	private void ApplyEnchantmentsManually()
	{
		if (Armor == null)
		{
			return;
		}
		foreach (ItemEnchantment enchantment in Armor.Enchantments)
		{
			enchantment.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsArmor> c)
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
