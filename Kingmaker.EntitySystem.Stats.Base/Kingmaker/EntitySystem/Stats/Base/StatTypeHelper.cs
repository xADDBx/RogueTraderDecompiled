using System;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Stats.Base;

public static class StatTypeHelper
{
	public static StatType[] Skills = new StatType[13]
	{
		StatType.SkillAthletics,
		StatType.SkillAwareness,
		StatType.SkillCarouse,
		StatType.SkillPersuasion,
		StatType.SkillDemolition,
		StatType.SkillCoercion,
		StatType.SkillMedicae,
		StatType.SkillLoreXenos,
		StatType.SkillLoreWarp,
		StatType.SkillLoreImperium,
		StatType.SkillTechUse,
		StatType.SkillCommerce,
		StatType.SkillLogic
	};

	public static StatType[] Attributes = new StatType[9]
	{
		StatType.WarhammerBallisticSkill,
		StatType.WarhammerWeaponSkill,
		StatType.WarhammerStrength,
		StatType.WarhammerToughness,
		StatType.WarhammerAgility,
		StatType.WarhammerIntelligence,
		StatType.WarhammerPerception,
		StatType.WarhammerWillpower,
		StatType.WarhammerFellowship
	};

	public static StatType[] Saves = new StatType[3]
	{
		StatType.SaveFortitude,
		StatType.SaveReflex,
		StatType.SaveWill
	};

	public static readonly StatType[] Knowledges = new StatType[4]
	{
		StatType.SkillDemolition,
		StatType.SkillLoreWarp,
		StatType.SkillLoreImperium,
		StatType.SkillTechUse
	};

	private static readonly StatType[] s_DisplayOrder = new StatType[34]
	{
		StatType.WarhammerBallisticSkill,
		StatType.WarhammerWeaponSkill,
		StatType.WarhammerStrength,
		StatType.WarhammerToughness,
		StatType.WarhammerAgility,
		StatType.WarhammerIntelligence,
		StatType.WarhammerPerception,
		StatType.WarhammerWillpower,
		StatType.WarhammerFellowship,
		StatType.SkillAthletics,
		StatType.SkillAwareness,
		StatType.SkillCarouse,
		StatType.SkillPersuasion,
		StatType.SkillDemolition,
		StatType.SkillCoercion,
		StatType.SkillMedicae,
		StatType.SkillLoreXenos,
		StatType.SkillLoreWarp,
		StatType.SkillLoreImperium,
		StatType.SkillTechUse,
		StatType.SkillCommerce,
		StatType.SkillLogic,
		StatType.CheckBluff,
		StatType.CheckDiplomacy,
		StatType.CheckIntimidate,
		StatType.SaveFortitude,
		StatType.SaveReflex,
		StatType.SaveFortitude,
		StatType.Initiative,
		StatType.HitPoints,
		StatType.TemporaryHitPoints,
		StatType.AttackOfOpportunityCount,
		StatType.Speed,
		StatType.PsyRating
	};

	private static Enum[] s_DisplayOrderActual = null;

	public static Enum[] DisplayOrder
	{
		get
		{
			if (s_DisplayOrderActual == null)
			{
				s_DisplayOrderActual = s_DisplayOrder.Concat(EnumUtils.GetValues<StatType>()).Distinct().Cast<Enum>()
					.ToArray();
			}
			return s_DisplayOrderActual;
		}
	}

	public static bool IsAttribute(this StatType stat)
	{
		return Attributes.Contains(stat);
	}

	public static bool IsSkill(this StatType stat)
	{
		return Skills.Contains(stat);
	}

	public static bool IsKnowledge(this StatType stat)
	{
		return Knowledges.HasItem(stat);
	}
}
