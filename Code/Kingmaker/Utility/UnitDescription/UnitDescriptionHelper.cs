using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UnitLogic;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Utility.UnitDescription;

public static class UnitDescriptionHelper
{
	public static UnitDescription GetDescription(BlueprintUnit blueprint, BaseUnitEntity whippingBoy = null)
	{
		try
		{
			return GetDescriptionSafe(blueprint, whippingBoy);
		}
		catch (Exception ex)
		{
			PFLog.Default.Error(blueprint, $"Exception occured while generating description for {blueprint}");
			PFLog.Default.Exception(ex);
			return new UnitDescription
			{
				Blueprint = blueprint
			};
		}
	}

	private static UnitDescription GetDescriptionSafe(BlueprintUnit blueprint, BaseUnitEntity whippingBoy = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.PreviewUnit>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					BaseUnitEntity baseUnitEntity = null;
					BaseUnitEntity baseUnitEntity2 = null;
					string text = Uuid.Instance.CreateString();
					try
					{
						baseUnitEntity = blueprint.CreateEntity("description-helper-unit-" + text);
						baseUnitEntity2 = whippingBoy ?? BlueprintRoot.Instance.SystemMechanics.DefaultUnit.CreateEntity("description-helper-attack-target-" + text);
						return GetDescription(baseUnitEntity, baseUnitEntity2);
					}
					finally
					{
						if (whippingBoy == null)
						{
							baseUnitEntity2?.Dispose();
						}
						baseUnitEntity?.Dispose();
					}
				}
			}
		}
	}

	public static UnitDescription GetDescription(BaseUnitEntity unit, BaseUnitEntity whippingBoy)
	{
		BlueprintUnit blueprint = unit.Blueprint;
		Experience component = blueprint.GetComponent<Experience>();
		int experience = (component ? ExperienceHelper.GetXp(component.Encounter, component.CR) : 0);
		int cR = (component ? component.CR : 0);
		using (ContextData<GameLogDisabled>.Request())
		{
			return new UnitDescription
			{
				Blueprint = blueprint,
				Race = unit.Progression.Race,
				Type = null,
				Classes = unit.Progression.Classes.Select((ClassData d) => new UnitDescription.ClassData
				{
					Class = d.CharacterClass,
					Level = d.Level
				}).ToArray(),
				Size = unit.OriginalSize,
				Alignment = unit.Alignment.Value,
				CR = cR,
				Experience = experience,
				HD = unit.Progression.CharacterLevel,
				Initiative = unit.CombatState.InitiativeBonus,
				AC = ExtractAC(unit),
				HP = unit.Health.MaxHitPoints,
				FastHealing = ExtractFastHealing(unit),
				Saves = ExtractSaves(unit),
				SR = ExtractSpellResistance(unit),
				Speed = unit.Movable.Speed.ModifiedValue.Feet(),
				Reach = 0.Feet(),
				Attacks = ExtractAttacks(unit, whippingBoy),
				Stats = ExtractStats(unit),
				Skills = ExtractSkills(unit),
				BAB = 0,
				Abilities = unit.Abilities.Visible.Select((Ability a) => a.Blueprint).ToArray(),
				ActivatableAbilities = unit.ActivatableAbilities.Enumerable.Select((ActivatableAbility a) => a.Blueprint).ToArray(),
				Features = ExtractFeatures(unit),
				UIFeatures = ExtractFeaturesForUI(unit),
				Buffs = unit.Buffs.Enumerable.Select((Buff b) => b.Blueprint).ToArray(),
				Facts = unit.Facts.List.Select((EntityFact f) => f.Blueprint).OfType<BlueprintUnitFact>().ToArray(),
				Equipment = (from s in unit.Body.EquipmentSlots
					where s.MaybeItem != null && s.MaybeItem.IsLootable
					select s.Item.Blueprint).ToArray(),
				Spells = ExtractSpells(unit)
			};
		}
	}

	private static int ExtractFastHealing(BaseUnitEntity unit)
	{
		return 0;
	}

	private static UnitDescription.SpellResistanceData[] ExtractSpellResistance(BaseUnitEntity unit)
	{
		return unit.GetOptional<UnitPartSpellResistance>()?.SRs.Select((UnitPartSpellResistance.SpellResistanceValue i) => new UnitDescription.SpellResistanceData
		{
			Value = i.Value,
			OnlyAgainstSpellDescriptor = i.SpellDescriptor
		}).ToArray();
	}

	private static UnitDescription.FeatureData[] ExtractFeatures(BaseUnitEntity unit)
	{
		return unit.Progression.Features.Enumerable.Select((Feature f) => new UnitDescription.FeatureData
		{
			Feature = f.Blueprint,
			Param = f.Param
		}).ToArray();
	}

	private static FeatureUIData[] ExtractFeaturesForUI(BaseUnitEntity unit)
	{
		return unit.Progression.Features.Enumerable.Select((Feature f) => new FeatureUIData(f)).ToArray();
	}

	private static UnitDescription.SkillsData ExtractSkills(BaseUnitEntity unit)
	{
		return new UnitDescription.SkillsData
		{
			Acrobatics = unit.Skills.SkillAthletics.ExtractSkillValue(),
			Physique = unit.Skills.SkillAwareness.ExtractSkillValue(),
			Diplomacy = unit.Skills.SkillCarouse.ExtractSkillValue(),
			Thievery = unit.Skills.SkillPersuasion.ExtractSkillValue(),
			LoreNature = unit.Skills.SkillDemolition.ExtractSkillValue(),
			Perception = unit.Skills.SkillCoercion.ExtractSkillValue(),
			Stealth = unit.Skills.SkillMedicae.ExtractSkillValue(),
			UseMagicDevice = unit.Skills.SkillLoreXenos.ExtractSkillValue(),
			LoreReligion = unit.Skills.SkillLoreWarp.ExtractSkillValue(),
			KnowledgeWorld = unit.Skills.SkillLoreImperium.ExtractSkillValue(),
			KnowledgeArcana = unit.Skills.SkillTechUse.ExtractSkillValue()
		};
	}

	private static int ExtractSkillValue(this ModifiableValueSkill skill)
	{
		if (skill.BaseValue <= 0)
		{
			return 0;
		}
		return skill.ModifiedValue;
	}

	private static UnitDescription.StatsData ExtractStats(BaseUnitEntity unit)
	{
		return new UnitDescription.StatsData
		{
			WarhammerBallisticSkill = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerBallisticSkill)),
			WarhammerWeaponSkill = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerWeaponSkill)),
			WarhammerStrength = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerStrength)),
			WarhammerToughness = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerToughness)),
			WarhammerAgility = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerAgility)),
			WarhammerIntelligence = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerIntelligence)),
			WarhammerWillpower = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerWillpower)),
			WarhammerPerception = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerPerception)),
			WarhammerFellowship = PrepareStats(unit.Stats.GetAttribute(StatType.WarhammerFellowship))
		};
	}

	private static string PrepareStats(ModifiableValueAttributeStat stat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{stat.Type}: {stat.ModifiedValue.ToString()}");
		foreach (ModifiableValue.Modifier modifier in stat.Modifiers)
		{
			stringBuilder.AppendLine($"\t\t\t\tModifier Descriptor[{modifier.ModDescriptor}]:{modifier.ModValue.ToString()}");
		}
		return stringBuilder.ToString();
	}

	private static UnitDescription.SavesData ExtractSaves(BaseUnitEntity unit)
	{
		return new UnitDescription.SavesData
		{
			Fort = unit.Saves.SaveFortitude.ModifiedValue,
			Ref = unit.Saves.SaveReflex.ModifiedValue,
			Will = unit.Saves.SaveWill.ModifiedValue
		};
	}

	private static UnitDescription.ArmorClass ExtractAC(BaseUnitEntity unit)
	{
		return new UnitDescription.ArmorClass
		{
			Base = 0,
			Touch = 0,
			FlatFooted = 0
		};
	}

	private static void VisitFirstComponent<T>(BaseUnitEntity unit, Action<T, EntityFact> action) where T : BlueprintComponent
	{
		Func<T, EntityFact, bool> action2 = delegate(T t, EntityFact f)
		{
			action(t, f);
			return false;
		};
		VisitComponents(unit, action2);
	}

	private static void VisitComponents<T>(BaseUnitEntity unit, Action<T, EntityFact> action) where T : BlueprintComponent
	{
		Func<T, EntityFact, bool> action2 = delegate(T t, EntityFact f)
		{
			action(t, f);
			return true;
		};
		VisitComponents(unit, action2);
	}

	private static void VisitComponents<T>(BaseUnitEntity unit, Func<T, EntityFact, bool> action) where T : BlueprintComponent
	{
		foreach (T component in unit.Blueprint.GetComponents<T>())
		{
			if (!action(component, null))
			{
				return;
			}
		}
		foreach (Feature item in unit.Progression.Features.Enumerable)
		{
			foreach (T item2 in item.SelectComponents<T>())
			{
				if (!action(item2, item))
				{
					return;
				}
			}
		}
		foreach (Buff buff in unit.Buffs)
		{
			foreach (T item3 in buff.SelectComponents<T>())
			{
				if (!action(item3, buff))
				{
					return;
				}
			}
		}
	}

	private static UnitDescription.AttackData[] ExtractAttacks(BaseUnitEntity unit, BaseUnitEntity whippingBoy)
	{
		return Array.Empty<UnitDescription.AttackData>();
	}

	public static DamageBundle CreateDamage(BaseUnitEntity unit, ItemEntityWeapon weapon)
	{
		return CreateDamage(weapon.GetWeaponStats(unit));
	}

	private static DamageBundle CreateDamage(RuleCalculateStatsWeapon weaponStats)
	{
		DamageData damages = weaponStats.ResultDamage.Copy();
		return new DamageBundle(weaponStats.Weapon, damages);
	}

	private static BlueprintAbility[] ExtractSpells(BaseUnitEntity unit)
	{
		List<BlueprintAbility> list = new List<BlueprintAbility>();
		foreach (Spellbook spellbook in unit.Spellbooks)
		{
			list.AddRange(from slot in spellbook.GetAllMemorizedSpells()
				select slot.Spell?.Blueprint into s
				where s != null
				select s);
			list.AddRange(from ad in spellbook.GetAllKnownSpells()
				select ad.Blueprint);
		}
		return list.ToArray();
	}

	public static UnitDescription GetDescriptionForEditorCheck(BlueprintUnit blueprint)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			BaseUnitEntity baseUnitEntity = null;
			string text = Uuid.Instance.CreateString();
			try
			{
				baseUnitEntity = blueprint.CreateEntity("description-helper-unit-" + text, isInGame: false);
				return GetDescriptionForEditorCheck(baseUnitEntity);
			}
			finally
			{
				baseUnitEntity?.Dispose();
			}
		}
	}

	private static UnitDescription GetDescriptionForEditorCheck(BaseUnitEntity unit)
	{
		using (ContextData<GameLogDisabled>.Request())
		{
			return new UnitDescription
			{
				Blueprint = unit.Blueprint,
				HP = unit.Health.MaxHitPoints,
				Stats = ExtractStats(unit),
				InitialMovementPoints = unit.CombatState.WarhammerInitialAPBlue.ModifiedValue,
				InitialActionPoints = unit.CombatState.WarhammerInitialAPYellow.ModifiedValue
			};
		}
	}
}
