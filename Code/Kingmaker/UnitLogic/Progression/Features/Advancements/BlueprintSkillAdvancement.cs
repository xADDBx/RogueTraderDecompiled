using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[TypeId("bd939c4ccfa84c4188d8ea01b8bc2592")]
public class BlueprintSkillAdvancement : BlueprintStatAdvancement
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintSkillAdvancement>
	{
	}

	public enum SkillType
	{
		Athletics,
		Awareness,
		Carouse,
		Persuasion,
		Demolition,
		Coercion,
		Medicae,
		LoreXenos,
		LoreWarp,
		LoreImperium,
		TechUse,
		Commerce,
		Logic
	}

	[SerializeField]
	private SkillType m_Skill;

	public override int ValuePerRank => 10;

	public override StatType Stat => GetStatTypeBySkill(m_Skill);

	public static StatType GetStatTypeBySkill(SkillType skill)
	{
		return skill switch
		{
			SkillType.Athletics => StatType.SkillAthletics, 
			SkillType.Awareness => StatType.SkillAwareness, 
			SkillType.Carouse => StatType.SkillCarouse, 
			SkillType.Persuasion => StatType.SkillPersuasion, 
			SkillType.Demolition => StatType.SkillDemolition, 
			SkillType.Coercion => StatType.SkillCoercion, 
			SkillType.Medicae => StatType.SkillMedicae, 
			SkillType.LoreXenos => StatType.SkillLoreXenos, 
			SkillType.LoreWarp => StatType.SkillLoreWarp, 
			SkillType.LoreImperium => StatType.SkillLoreImperium, 
			SkillType.TechUse => StatType.SkillTechUse, 
			SkillType.Commerce => StatType.SkillCommerce, 
			SkillType.Logic => StatType.SkillLogic, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static SkillType GetSkillTypeFromStatType(StatType stat)
	{
		return stat switch
		{
			StatType.SkillAthletics => SkillType.Athletics, 
			StatType.SkillAwareness => SkillType.Awareness, 
			StatType.SkillCarouse => SkillType.Carouse, 
			StatType.SkillPersuasion => SkillType.Persuasion, 
			StatType.SkillDemolition => SkillType.Demolition, 
			StatType.SkillCoercion => SkillType.Coercion, 
			StatType.SkillMedicae => SkillType.Medicae, 
			StatType.SkillLoreXenos => SkillType.LoreXenos, 
			StatType.SkillLoreWarp => SkillType.LoreWarp, 
			StatType.SkillLoreImperium => SkillType.LoreImperium, 
			StatType.SkillTechUse => SkillType.TechUse, 
			StatType.SkillCommerce => SkillType.Commerce, 
			StatType.SkillLogic => SkillType.Logic, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
