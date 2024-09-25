using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("eaa1747159d24b8e883ca5c0b6429137")]
public class DebuffSkillCheckRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintBuffReference m_Fatigued;

	[SerializeField]
	private BlueprintBuffReference m_Disturbed;

	[SerializeField]
	private BlueprintBuffReference m_Perplexed;

	public BlueprintBuff Fatigued => m_Fatigued;

	public BlueprintBuff Disturbed => m_Disturbed;

	public BlueprintBuff Perplexed => m_Perplexed;

	public void SetDebuff([CanBeNull] BaseUnitEntity user, StatType skill, bool isCriticalFail)
	{
		BlueprintBuff blueprintBuff = null;
		switch (skill)
		{
		case StatType.SkillAthletics:
		case StatType.SkillCoercion:
		case StatType.SkillCarouse:
		case StatType.SkillDemolition:
		case StatType.WarhammerStrength:
		case StatType.WarhammerToughness:
		case StatType.WarhammerAgility:
			blueprintBuff = Fatigued;
			break;
		case StatType.SkillPersuasion:
		case StatType.SkillCommerce:
		case StatType.WarhammerWillpower:
		case StatType.WarhammerFellowship:
			blueprintBuff = Disturbed;
			break;
		case StatType.SkillAwareness:
		case StatType.SkillTechUse:
		case StatType.SkillMedicae:
		case StatType.SkillLoreXenos:
		case StatType.SkillLoreWarp:
		case StatType.SkillLoreImperium:
		case StatType.SkillLogic:
		case StatType.WarhammerBallisticSkill:
		case StatType.WarhammerWeaponSkill:
		case StatType.WarhammerIntelligence:
		case StatType.WarhammerPerception:
			blueprintBuff = Perplexed;
			break;
		}
		if (blueprintBuff != null && user != null)
		{
			user.Buffs.Add(blueprintBuff, user);
			if (isCriticalFail)
			{
				user.Buffs.Add(blueprintBuff, user);
			}
		}
	}
}
