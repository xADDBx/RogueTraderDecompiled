using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class TechUseMultikeyItemRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillTechUse;
	}

	public override BlueprintItem GetItem()
	{
		return Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MultikeyItem;
	}
}
