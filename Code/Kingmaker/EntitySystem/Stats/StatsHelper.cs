using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.EntitySystem.Stats;

public static class StatsHelper
{
	public static StatBaseValue GetUnitStatBaseValue(StatType type, BlueprintUnit blueprint)
	{
		BlueprintStarship blueprintStarship = blueprint as BlueprintStarship;
		return type switch
		{
			StatType.Unknown => 0, 
			StatType.HitPoints => blueprintStarship?.HullIntegrity ?? blueprint.MaxHP, 
			StatType.TemporaryHitPoints => 0, 
			StatType.AttackOfOpportunityCount => 1, 
			StatType.SaveFortitude => 0, 
			StatType.SaveWill => 0, 
			StatType.SaveReflex => 0, 
			StatType.SkillAthletics => blueprint.Skills.Athletics, 
			StatType.SkillAwareness => blueprint.Skills.Awareness, 
			StatType.SkillCarouse => blueprint.Skills.Carouse, 
			StatType.SkillPersuasion => blueprint.Skills.Persuasion, 
			StatType.SkillDemolition => blueprint.Skills.Demolition, 
			StatType.SkillCoercion => blueprint.Skills.Coercion, 
			StatType.SkillMedicae => blueprint.Skills.Medicae, 
			StatType.SkillLoreXenos => blueprint.Skills.LoreXenos, 
			StatType.SkillLoreWarp => blueprint.Skills.LoreWarp, 
			StatType.SkillLoreImperium => blueprint.Skills.LoreImperium, 
			StatType.SkillTechUse => blueprint.Skills.TechUse, 
			StatType.SkillCommerce => blueprint.Skills.Commerce, 
			StatType.SkillLogic => blueprint.Skills.Logic, 
			StatType.CheckBluff => 0, 
			StatType.CheckDiplomacy => 0, 
			StatType.CheckIntimidate => 0, 
			StatType.Speed => blueprint.Speed.Value, 
			StatType.WarhammerBallisticSkill => (Value: blueprint.WarhammerBallisticSkill, Enabled: blueprint.WarhammerBallisticSkill > 0), 
			StatType.WarhammerWeaponSkill => (Value: blueprint.WarhammerWeaponSkill, Enabled: blueprint.WarhammerWeaponSkill > 0), 
			StatType.WarhammerStrength => (Value: blueprint.WarhammerStrength, Enabled: blueprint.WarhammerStrength > 0), 
			StatType.WarhammerToughness => (Value: blueprint.WarhammerToughness, Enabled: blueprint.WarhammerToughness > 0), 
			StatType.WarhammerAgility => (Value: blueprint.WarhammerAgility, Enabled: blueprint.WarhammerAgility > 0), 
			StatType.WarhammerIntelligence => (Value: blueprint.WarhammerIntelligence, Enabled: blueprint.WarhammerIntelligence > 0), 
			StatType.WarhammerWillpower => (Value: blueprint.WarhammerWillpower, Enabled: blueprint.WarhammerWillpower > 0), 
			StatType.WarhammerPerception => (Value: blueprint.WarhammerPerception, Enabled: blueprint.WarhammerPerception > 0), 
			StatType.WarhammerFellowship => (Value: blueprint.WarhammerFellowship, Enabled: blueprint.WarhammerFellowship > 0), 
			StatType.WarhammerInitialAPBlue => blueprintStarship?.StarshipSpeed ?? blueprint.WarhammerInitialAPBlue, 
			StatType.WarhammerInitialAPYellow => blueprintStarship?.InspirationAmount ?? blueprint.WarhammerInitialAPYellow, 
			StatType.Resolve => 0, 
			StatType.DamageNonLethal => 0, 
			StatType.ArmourFore => blueprintStarship?.ArmourFore ?? 0, 
			StatType.ArmourPort => blueprintStarship?.ArmourPort ?? 0, 
			StatType.ArmourStarboard => blueprintStarship?.ArmourStarboard ?? 0, 
			StatType.ArmourAft => blueprintStarship?.ArmourAft ?? 0, 
			StatType.Inertia => blueprintStarship?.Inertia ?? 0, 
			StatType.Evasion => blueprintStarship?.Evasion ?? 0, 
			StatType.Morale => blueprintStarship?.Morale ?? 0, 
			StatType.Initiative => blueprintStarship?.Initiative ?? 0, 
			StatType.Crew => blueprintStarship?.CrewCount ?? 0, 
			StatType.TurretRating => blueprintStarship?.TurretRating ?? 0, 
			StatType.TurretRadius => blueprintStarship?.TurretRadius ?? 0, 
			StatType.MilitaryRating => blueprintStarship?.MilitaryRating ?? 0, 
			StatType.PsyRating => 0, 
			StatType.MachineTrait => 0, 
			StatType.InspirationInitialAmount => 0, 
			StatType.InspirationRegeneration => 0, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
