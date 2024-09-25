using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class DemolitionMeltaChargeRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillDemolition;
	}

	public override BlueprintItem GetItem()
	{
		return Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem;
	}
}
