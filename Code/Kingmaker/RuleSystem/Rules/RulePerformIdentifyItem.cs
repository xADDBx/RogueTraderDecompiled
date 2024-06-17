using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformIdentifyItem : RulebookEvent<UnitEntity>
{
	public readonly ItemEntity Item;

	public readonly StatType StatType;

	public int Bonus { get; private set; }

	public int SpellBonus { get; private set; }

	public bool CanUseUntrainedStat { get; private set; }

	public RulePerformIdentifyItem([NotNull] UnitEntity initiator, ItemEntity item)
		: base(initiator)
	{
		Item = item;
		StatType = StatType.SkillTechUse;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (Item.IsIdentified || base.Initiator.IsDead)
		{
			return;
		}
		ModifiableValue statOptional = base.Initiator.GetStatOptional(StatType);
		if (statOptional == null)
		{
			PFLog.Default.Error($"Can't identify item with stat '{StatType}'");
			return;
		}
		int num = (CanUseUntrainedStat ? Math.Max(1, statOptional.BaseValue) : statOptional.BaseValue);
		if (num < 1)
		{
			return;
		}
		ItemEntity.IdentifyRollData identifyRollData = Item.GetIdentifyRollData(base.Initiator);
		if (num <= identifyRollData.SkillValue && (identifyRollData.UsedSpell || SpellBonus <= 0))
		{
			return;
		}
		identifyRollData.SkillValue = num;
		identifyRollData.UsedSpell = SpellBonus > 0;
		int identifyDC = Item.Blueprint.IdentifyDC;
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(base.Initiator, statOptional.Type, identifyDC)
		{
			Reason = new RuleReason(base.Initiator),
			Silent = true,
			Voice = RulePerformSkillCheck.VoicingType.None
		};
		rulePerformSkillCheck.DifficultyModifiers.Add(Bonus + SpellBonus, this, ModifierDescriptor.None);
		context.Trigger(rulePerformSkillCheck);
		if (rulePerformSkillCheck.ResultIsSuccess)
		{
			Item.Identify();
			EventBus.RaiseEvent((IItemEntity)Item, (Action<IIdentifyHandler>)delegate(IIdentifyHandler h)
			{
				h.OnItemIdentified(base.Initiator);
			}, isCheckRuntime: true);
		}
	}

	public void AddBonus(int bonus)
	{
		Bonus += bonus;
	}

	public void AddSpellBonus(int spellBonus)
	{
		SpellBonus += spellBonus;
	}

	public void AllowUseUntrainedStat()
	{
		CanUseUntrainedStat = true;
	}
}
