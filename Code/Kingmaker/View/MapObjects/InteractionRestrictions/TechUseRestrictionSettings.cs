using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class TechUseRestrictionSettings : SkillUseRestrictionSettings
{
	public override StatType GetSkill()
	{
		return StatType.SkillTechUse;
	}

	public override BlueprintItem GetItem()
	{
		return null;
	}
}
