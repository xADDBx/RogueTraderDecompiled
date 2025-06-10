using System;
using System.Linq;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class EntityPropertyExtension
{
	private static readonly int MaxValue;

	private static readonly Func<Entity, int?>[] Getters;

	static EntityPropertyExtension()
	{
		MaxValue = EnumUtils.GetMaxValue<EntityProperty>();
		Getters = new Func<Entity, int?>[MaxValue];
		Getter(EntityProperty.None, (Entity _) => 0);
		Getter(EntityProperty.BallisticSkill, delegate(Entity e)
		{
			return Stat(e, StatType.WarhammerBallisticSkill);
			static int? Stat(Entity entity, StatType type)
			{
				UnitPartStatsOverride optional = entity.GetOptional<UnitPartStatsOverride>();
				if (optional != null && optional.TryGetOverride(type, out var value))
				{
					return value;
				}
				return entity.GetOptional<PartStatsContainer>()?.GetStatOptional(type);
			}
		});
		Getter(EntityProperty.WeaponSkill, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerWeaponSkill));
		Getter(EntityProperty.Strength, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerStrength));
		Getter(EntityProperty.Toughness, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerToughness));
		Getter(EntityProperty.Agility, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerAgility));
		Getter(EntityProperty.Intelligence, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerIntelligence));
		Getter(EntityProperty.Willpower, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerWillpower));
		Getter(EntityProperty.Perception, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerPerception));
		Getter(EntityProperty.Fellowship, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerFellowship));
		Getter(EntityProperty.BallisticSkillBonus, delegate(Entity e)
		{
			return StatBonus(e, StatType.WarhammerBallisticSkill);
			static int? StatBonus(Entity entity, StatType type)
			{
				return entity.GetOptional<PartStatsContainer>()?.GetAttributeOptional(type)?.Bonus;
			}
		});
		Getter(EntityProperty.WeaponSkillBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerWeaponSkill));
		Getter(EntityProperty.StrengthBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerStrength));
		Getter(EntityProperty.ToughnessBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerToughness));
		Getter(EntityProperty.AgilityBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerAgility));
		Getter(EntityProperty.IntelligenceBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerIntelligence));
		Getter(EntityProperty.WillpowerBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerWillpower));
		Getter(EntityProperty.PerceptionBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerPerception));
		Getter(EntityProperty.FellowshipBonus, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__StatBonus_007C2_2(e, StatType.WarhammerFellowship));
		Getter(EntityProperty.Resolve, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.Resolve));
		Getter(EntityProperty.Wounds, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.HitPoints));
		Getter(EntityProperty.InitialAPBlue, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerInitialAPBlue));
		Getter(EntityProperty.InitialAPYellow, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.WarhammerInitialAPYellow));
		Getter(EntityProperty.CurrentWeaponRateOfFire, GetCurrentWeaponRateOfFire);
		Getter(EntityProperty.EnemiesAdjacent, GetEnemiesAdjacent);
		Getter(EntityProperty.CurrentAPBlue, (Entity e) => (int)(e.GetOptional<PartUnitCombatState>()?.ActionPointsBlue ?? 0f));
		Getter(EntityProperty.CurrentAPYellow, (Entity e) => e.GetOptional<PartUnitCombatState>()?.ActionPointsYellow ?? 0);
		Getter(EntityProperty.SkillAthletics, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillAthletics));
		Getter(EntityProperty.SkillAwareness, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillAwareness));
		Getter(EntityProperty.SkillCarouse, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillCarouse));
		Getter(EntityProperty.SkillPersuasion, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillPersuasion));
		Getter(EntityProperty.SkillDemolition, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillDemolition));
		Getter(EntityProperty.SkillCoercion, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillCoercion));
		Getter(EntityProperty.SkillMedicae, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillMedicae));
		Getter(EntityProperty.SkillLoreXenos, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillLoreXenos));
		Getter(EntityProperty.SkillLoreWarp, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillLoreWarp));
		Getter(EntityProperty.SkillLoreImperium, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillLoreImperium));
		Getter(EntityProperty.SkillTechUse, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillTechUse));
		Getter(EntityProperty.SkillCommerce, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillCommerce));
		Getter(EntityProperty.SkillLogic, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.SkillLogic));
		Getter(EntityProperty.PsyRating, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.PsyRating));
		Getter(EntityProperty.Absorption, GetAbsorption);
		Getter(EntityProperty.Deflection, GetDeflection);
		Getter(EntityProperty.ArmourFore, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.ArmourFore));
		Getter(EntityProperty.ArmourPort, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.ArmourPort));
		Getter(EntityProperty.ArmourStarboard, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.ArmourStarboard));
		Getter(EntityProperty.ArmourAft, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.ArmourAft));
		Getter(EntityProperty.Inertia, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.Inertia));
		Getter(EntityProperty.Evasion, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.Evasion));
		Getter(EntityProperty.Morale, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.Morale));
		Getter(EntityProperty.Crew, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.Crew));
		Getter(EntityProperty.TurretRating, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.TurretRating));
		Getter(EntityProperty.TurretRadius, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.TurretRadius));
		Getter(EntityProperty.MilitaryRating, (Entity e) => EntityPropertyExtension._003C_002Ecctor_003Eg__Stat_007C2_1(e, StatType.MilitaryRating));
		static void Getter(EntityProperty propertyName, Func<Entity, int?> getter)
		{
			Getters[(int)propertyName] = getter;
		}
	}

	public static int GetValue(this EntityProperty property, Entity entity)
	{
		try
		{
			if (entity == null)
			{
				PFLog.Default.ErrorWithReport($"Can't get property {property} from null");
				return 0;
			}
			int? num = Getters[(int)property]?.Invoke(entity);
			if (!num.HasValue)
			{
				PFLog.Default.ErrorWithReport($"Can't get property {property} from {entity}");
			}
			return num.GetValueOrDefault();
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, $"Exception in getter of property {property} ({entity})");
			return 0;
		}
	}

	private static int? GetCurrentWeaponRateOfFire(Entity e)
	{
		return (ContextData<MechanicsContext.Data>.Current?.Context.SourceAbilityContext?.Ability ?? ContextData<PropertyContextData>.Current?.Context.Ability ?? (ContextData<PropertyContextData>.Current?.Context.Rule as RuleCalculateDamage)?.Ability)?.GetWeaponStats().ResultRateOfFire;
	}

	private static int? GetEnemiesAdjacent(Entity e)
	{
		BaseUnitEntity unit = e as BaseUnitEntity;
		if (unit == null)
		{
			return null;
		}
		return Game.Instance.State.AllUnits.Count((AbstractUnitEntity p) => p.DistanceToInCells(unit) <= 1);
	}

	private static int? GetAbsorption(Entity e)
	{
		if (!(e is MechanicEntity initiator))
		{
			return null;
		}
		return Rulebook.Trigger(new RuleCalculateStatsArmor(initiator)).ResultAbsorption;
	}

	private static int? GetDeflection(Entity e)
	{
		if (!(e is MechanicEntity initiator))
		{
			return null;
		}
		return Rulebook.Trigger(new RuleCalculateStatsArmor(initiator)).ResultDeflection;
	}
}
