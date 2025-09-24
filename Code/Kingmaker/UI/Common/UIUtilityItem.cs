using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.UI.Common;

public static class UIUtilityItem
{
	public struct ArmorData
	{
		public int DamageAbsorption;

		public int DamageDeflection;

		public int RangedHitChanceBonus;

		public int MovePointAdjustment;

		public string ArmorDodgePenalty;

		public string ArmourDodgeChanceDescription;

		public string ArmorDamageReduceDescription;
	}

	public class UIPatternData
	{
		public PatternGridData PatternCells;

		public Vector2Int[] MainCellsIndexes;

		public Vector2Int? OwnerCell;
	}

	public class UIScatterHitChanceData
	{
		public int MainLineClose;

		public int ScatterClose;

		public int MainLine;

		public int ScatterNear;

		public int ScatterFar;
	}

	public class UIAbilityData
	{
		public BlueprintAbility BlueprintAbility;

		public string AttackType;

		public string DamageText;

		public string BaseDamageText;

		public int MinDamage;

		public bool IsSpaceCombatAbility;

		public bool IsReload;

		public ItemEntityWeapon Weapon;

		public int MaxDamage;

		public int Penetration;

		public bool IsRange;

		public bool IsScatter;

		public int BurstAttacksCount;

		public string CostAP;

		public MomentumAbilityType? MomentumAbilityType;

		public UIPatternData PatternData;

		public int HitChance;

		public UIScatterHitChanceData ScatterHitChanceData;

		public IEnumerable<UIProperty> UIProperties;

		public string Name => BlueprintAbility.Name;

		public Sprite Icon => BlueprintAbility.Icon;
	}

	public class RestrictionData
	{
		public readonly List<RestrictionItem> RestrictionItems;

		public readonly bool Inverted;

		public readonly bool CanEquip;

		public RestrictionData(RestrictionItem restrictionItem, bool inverted, bool canEquip)
		{
			Inverted = inverted;
			RestrictionItems = new List<RestrictionItem> { restrictionItem };
			CanEquip = canEquip;
		}

		public RestrictionData(List<RestrictionItem> restrictionItems, bool inverted, bool canEquip)
		{
			Inverted = inverted;
			RestrictionItems = new List<RestrictionItem>(restrictionItems);
			CanEquip = canEquip;
		}
	}

	public class RestrictionItem
	{
		public BlueprintUnitFact UnitFact;

		public string Key;

		public string Value;

		public bool MeetPrerequisite;
	}

	private static CalculatorUnitPair m_CalculatorUnitPair;

	private static readonly UIPatternData SingleShotPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int> { Vector2Int.zero }, disposable: false),
		MainCellsIndexes = new Vector2Int[1] { Vector2Int.zero }
	};

	private static readonly UIPatternData ScatterShotPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int>
		{
			new Vector2Int(1, 3),
			new Vector2Int(2, 3),
			new Vector2Int(3, 3),
			new Vector2Int(0, 2),
			new Vector2Int(1, 2),
			new Vector2Int(2, 2),
			new Vector2Int(3, 2),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(3, 1),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(2, 0)
		}, disposable: false),
		MainCellsIndexes = new Vector2Int[4]
		{
			new Vector2Int(3, 3),
			new Vector2Int(2, 2),
			new Vector2Int(1, 1),
			new Vector2Int(0, 0)
		}
	};

	private static readonly UIPatternData StratagemPatternData = new UIPatternData
	{
		PatternCells = PatternGridData.Create(new HashSet<Vector2Int>
		{
			new Vector2Int(0, 3),
			new Vector2Int(1, 3),
			new Vector2Int(2, 3),
			new Vector2Int(3, 3),
			new Vector2Int(0, 2),
			new Vector2Int(1, 2),
			new Vector2Int(2, 2),
			new Vector2Int(3, 2),
			new Vector2Int(0, 1),
			new Vector2Int(1, 1),
			new Vector2Int(2, 1),
			new Vector2Int(3, 1),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(2, 0),
			new Vector2Int(3, 0)
		}, disposable: false),
		MainCellsIndexes = Array.Empty<Vector2Int>()
	};

	public static bool CanReadItem(ItemEntity item)
	{
		if (Game.Instance.Player.Inventory.Contains(item))
		{
			return true;
		}
		if (item.Owner == null || item.Owner.IsDead)
		{
			return true;
		}
		return false;
	}

	public static bool CanReadItem(BlueprintItem blueprintItem)
	{
		if (Game.Instance.Player.Inventory.Contains(blueprintItem))
		{
			return true;
		}
		return false;
	}

	public static ArmorData GetArmorData(ItemEntityArmor armor, ItemEntityShield shield)
	{
		using (GameLogContext.Scope)
		{
			float num = (armor?.Blueprint.Category ?? shield.Blueprint.Category) switch
			{
				WarhammerArmorCategory.Power => 0.75f, 
				WarhammerArmorCategory.Heavy => 0.5f, 
				WarhammerArmorCategory.Medium => 0.75f, 
				_ => 0f, 
			};
			ArmorData armorData = default(ArmorData);
			armorData.DamageAbsorption = armor?.Blueprint.DamageAbsorption ?? 0;
			armorData.DamageDeflection = armor?.Blueprint.DamageDeflection ?? 0;
			armorData.MovePointAdjustment = 0;
			armorData.RangedHitChanceBonus = 0;
			armorData.ArmorDodgePenalty = ((num > 0f) ? $"{num}" : string.Empty);
			ArmorData result = armorData;
			UnitEntity unitEntity = ((armor?.Wielder ?? shield?.Wielder) as UnitEntity) ?? (Game.Instance?.SelectionCharacter?.SelectedUnitInUI?.Value as UnitEntity);
			if (unitEntity == null)
			{
				return result;
			}
			GameLogContext.ResultDamage = RuleCalculateStatsArmor.CalculateChapterSpecificAbsorptionValue(unitEntity, armor);
			result.ArmorDamageReduceDescription = UIStrings.Instance.Tooltips.ArmourDamageReduceDescription;
			GameLogContext.DodgeChance = RuleCalculateDodgeChance.CalculateChapterSpecificDodgeValue(unitEntity, armor);
			result.ArmourDodgeChanceDescription = UIStrings.Instance.Tooltips.ArmourDodgeChanceDescription;
			return result;
		}
	}

	public static UIAbilityData GetUIAbilityData(BlueprintAbility blueprintAbility, BlueprintItem blueprintItem, MechanicEntity caster = null)
	{
		ItemEntity itemEntity = null;
		if (blueprintItem != null)
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				itemEntity = blueprintItem.CreateEntity();
			}
		}
		return GetUIAbilityData(blueprintAbility, itemEntity, caster);
	}

	public static UIAbilityData GetUIAbilityData(BlueprintAbility blueprintAbility, ItemEntity itemEntity = null, MechanicEntity caster = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			string damageText = string.Empty;
			string baseDamageText = string.Empty;
			int minDamage = 0;
			int maxDamage = 0;
			int penetration = 0;
			int hitChance = 0;
			UIScatterHitChanceData scatterHitChanceData = null;
			if (caster == null)
			{
				caster = itemEntity?.Owner ?? UIUtility.GetCurrentSelectedUnit() ?? Game.Instance.DefaultUnit;
			}
			if (caster == null || caster.IsDisposed)
			{
				return null;
			}
			AbilityData abilityData = new AbilityData(blueprintAbility, caster)
			{
				OverrideWeapon = ((blueprintAbility.GetComponent<WarhammerOverrideAbilityWeapon>()?.Weapon?.CreateEntity() ?? itemEntity) as ItemEntityWeapon)
			};
			if (itemEntity is ItemEntityWeapon itemEntityWeapon)
			{
				int val = itemEntityWeapon.Blueprint.WeaponAbilities.Select((WeaponAbility i) => i.Ability).IndexOf(blueprintAbility);
				abilityData.ItemSlotIndex = Math.Max(0, val);
			}
			RuleCalculateStatsWeapon weaponStats = abilityData.GetWeaponStats();
			BlueprintItemWeapon blueprintItemWeapon = abilityData.Weapon?.Blueprint;
			if (blueprintItemWeapon != null && abilityData.Blueprint.AttackType.HasValue)
			{
				damageText = (abilityData.Blueprint.IsBurst ? $"({weaponStats.ResultDamage}-{blueprintItemWeapon.WarhammerMaxDamage})x{abilityData.RateOfFire}" : $"{weaponStats.ResultDamage}-{blueprintItemWeapon.WarhammerMaxDamage}");
				baseDamageText = $"{weaponStats.ResultDamage.MinValueBase}-{weaponStats.ResultDamage.MaxValueBase}";
				int minValue = weaponStats.ResultDamage.MinValue;
				int warhammerMaxDamage = blueprintItemWeapon.WarhammerMaxDamage;
				minDamage = minValue;
				maxDamage = warhammerMaxDamage;
				penetration = weaponStats.BaseDamage.Penetration.Value;
			}
			if ((bool)AstarPath.active)
			{
				DamagePredictionData damagePrediction = abilityData.GetDamagePrediction(Game.Instance.DefaultUnit, Game.Instance.DefaultUnit.Position);
				if (damagePrediction != null)
				{
					damageText = ((damagePrediction.MinDamage != damagePrediction.MaxDamage) ? $"{damagePrediction.MinDamage}-{damagePrediction.MaxDamage}" : $"{damagePrediction.MinDamage}");
					int minValue = damagePrediction.MinDamage;
					int maxDamage2 = damagePrediction.MaxDamage;
					minDamage = minValue;
					maxDamage = maxDamage2;
					penetration = damagePrediction.Penetration;
				}
			}
			MomentumAbilityType? momentumAbilityType = null;
			AbilitySpecialMomentumAction component = abilityData.Blueprint.GetComponent<AbilitySpecialMomentumAction>();
			if (component != null)
			{
				momentumAbilityType = component.MomentumType;
			}
			if (abilityData.Weapon != null)
			{
				if (abilityData.IsScatter)
				{
					RuleCalculateScatterShotHitDirectionProbability ruleCalculateScatterShotHitDirectionProbability = Rulebook.Trigger(new RuleCalculateScatterShotHitDirectionProbability(caster, abilityData, 0));
					scatterHitChanceData = new UIScatterHitChanceData
					{
						MainLine = ruleCalculateScatterShotHitDirectionProbability.ResultMainLine,
						ScatterNear = ruleCalculateScatterShotHitDirectionProbability.ResultScatterNear,
						ScatterFar = ruleCalculateScatterShotHitDirectionProbability.ResultScatterFar,
						MainLineClose = ruleCalculateScatterShotHitDirectionProbability.ResultMainLine + ruleCalculateScatterShotHitDirectionProbability.ResultScatterNear,
						ScatterClose = ruleCalculateScatterShotHitDirectionProbability.ResultScatterFar
					};
					if (abilityData.IsMelee)
					{
						hitChance = Rulebook.Trigger(new RuleCalculateHitChances(caster, Game.Instance.DefaultUnit, abilityData, 0, Vector3.zero, Vector3.zero)).ResultHitChance;
					}
				}
				else
				{
					hitChance = Rulebook.Trigger(new RuleCalculateHitChances(caster, Game.Instance.DefaultUnit, abilityData, 0, Vector3.zero, Vector3.zero)).ResultHitChance;
				}
			}
			return new UIAbilityData
			{
				BlueprintAbility = blueprintAbility,
				MinDamage = minDamage,
				MaxDamage = maxDamage,
				DamageText = damageText,
				BaseDamageText = baseDamageText,
				Penetration = penetration,
				AttackType = UIStrings.Instance.AbilityTexts.GetAttackType(abilityData.Blueprint.AttackType),
				CostAP = $"{abilityData.CalculateActionPointCost()} {UIStrings.Instance.Tooltips.AP.Text}",
				PatternData = GetAbilityPatternData(abilityData, itemEntity),
				MomentumAbilityType = momentumAbilityType,
				HitChance = hitChance,
				ScatterHitChanceData = scatterHitChanceData,
				IsRange = (blueprintItemWeapon?.IsRanged ?? false),
				IsScatter = abilityData.IsScatter,
				BurstAttacksCount = abilityData.BurstAttacksCount,
				UIProperties = abilityData.Blueprint.GetUIProperties(caster, null, itemEntity),
				IsSpaceCombatAbility = (Game.Instance.CurrentMode == GameModeType.SpaceCombat),
				IsReload = IsReload(abilityData),
				Weapon = abilityData.Weapon
			};
		}
	}

	private static UIPatternData GetAbilityPatternData(AbilityData ability, ItemEntity itemEntity)
	{
		AttackAbilityType? attackType = ability.Blueprint.AttackType;
		AoEPattern aoEPattern = ability.Blueprint.PatternSettings?.Pattern;
		if (aoEPattern != null)
		{
			PatternGridData gridData = aoEPattern.GetGridData(Vector2.up);
			using PatternGridData.Enumerator enumerator = gridData.GetEnumerator();
			enumerator.MoveNext();
			Vector2Int[] mainCellsIndexes = ((itemEntity != null) ? Array.Empty<Vector2Int>() : new Vector2Int[1]
			{
				new Vector2Int(0, 0)
			});
			return new UIPatternData
			{
				PatternCells = gridData,
				MainCellsIndexes = mainCellsIndexes,
				OwnerCell = ((ability.TargetAnchor == AbilityTargetAnchor.Owner) ? new Vector2Int?(new Vector2Int(0, 0)) : ((ability.RangeCells == 1) ? new Vector2Int?(new Vector2Int(0, -1)) : null))
			};
		}
		if (attackType == AttackAbilityType.Scatter)
		{
			return ScatterShotPatternData;
		}
		if (ability.Blueprint.IsStratagem)
		{
			return StratagemPatternData;
		}
		return SingleShotPatternData;
	}

	private static void FillItemRestrictions(ItemTooltipData data, BlueprintItem blueprintItem)
	{
		BaseUnitEntity unit = UIUtility.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		foreach (EquipmentRestriction component in blueprintItem.GetComponents<EquipmentRestriction>())
		{
			EquipmentRestrictionHasFacts equipmentRestrictionHasFacts = component as EquipmentRestrictionHasFacts;
			if (equipmentRestrictionHasFacts == null)
			{
				if (!(component is EquipmentRestrictionStat equipmentRestrictionStat))
				{
					if (component is EquipmentRestrictionMachineTrait equipmentRestrictionMachineTrait)
					{
						string statText = UIUtility.GetStatText(StatType.MachineTrait);
						RestrictionItem restrictionItem = new RestrictionItem
						{
							Key = statText,
							Value = equipmentRestrictionMachineTrait.MinRank.ToString(),
							MeetPrerequisite = component.CanBeEquippedBy(unit)
						};
						data.Restrictions.Add(new RestrictionData(restrictionItem, inverted: false, equipmentRestrictionMachineTrait.CanBeEquippedBy(unit)));
					}
				}
				else
				{
					string statText2 = UIUtility.GetStatText(equipmentRestrictionStat.Stat);
					RestrictionItem restrictionItem2 = new RestrictionItem
					{
						Key = statText2,
						Value = equipmentRestrictionStat.MinValue.ToString(),
						MeetPrerequisite = component.CanBeEquippedBy(unit)
					};
					data.Restrictions.Add(new RestrictionData(restrictionItem2, inverted: false, equipmentRestrictionStat.CanBeEquippedBy(unit)));
				}
				continue;
			}
			List<RestrictionItem> list = (from fact in equipmentRestrictionHasFacts.Facts
				where !(fact is BlueprintFeature { HideInUI: not false }) && !fact.IsDlcRestricted()
				select fact into unitFact
				select new RestrictionItem
				{
					UnitFact = unitFact,
					MeetPrerequisite = equipmentRestrictionHasFacts.CheckFactRestriction(unit, unitFact)
				}).ToList();
			if (equipmentRestrictionHasFacts.All)
			{
				data.Restrictions.AddRange(list.Select((RestrictionItem i) => new RestrictionData(i, equipmentRestrictionHasFacts.Inverted, equipmentRestrictionHasFacts.CanBeEquippedBy(unit))));
			}
			else
			{
				data.Restrictions.Add(new RestrictionData(list, equipmentRestrictionHasFacts.Inverted, equipmentRestrictionHasFacts.CanBeEquippedBy(unit)));
			}
		}
	}

	private static void FillWeaponDamage(ItemTooltipData data, RuleCalculateStatsWeapon defaultWeaponStats, RuleCalculateStatsWeapon equippedWeaponStats, ItemEntityWeapon weapon)
	{
		(int, int) damageMinMax = GetDamageMinMax(defaultWeaponStats.ResultDamage);
		data.Texts[TooltipElement.Damage] = $"{damageMinMax.Item1}-{damageMinMax.Item2}";
		data.CompareData[TooltipElement.Damage] = new CompareData
		{
			Value = damageMinMax.Item2 + damageMinMax.Item1
		};
		if (equippedWeaponStats != null)
		{
			(int, int) damageMinMax2 = GetDamageMinMax(equippedWeaponStats.ResultDamage);
			data.Texts[TooltipElement.EquipDamage] = $"{damageMinMax2.Item1}-{damageMinMax2.Item2}";
			(int, int) baseDamageMinMax = GetBaseDamageMinMax(equippedWeaponStats.ResultDamage);
			data.Texts[TooltipElement.BaseDamage] = $"{baseDamageMinMax.Item1}-{baseDamageMinMax.Item2}";
			data.CompareData[TooltipElement.EquipDamage] = new CompareData
			{
				Value = damageMinMax2.Item2 + damageMinMax2.Item1
			};
			equippedWeaponStats.ResultDamage.Modifiers.ValueModifiersList.Where((Modifier m) => m.Stat != StatType.Unknown).ForEach(delegate(Modifier mod)
			{
				data.BonusDamageFromStat[mod.Stat] = UIUtility.AddSign(mod.Value);
			});
		}
		else
		{
			(int, int) baseDamageMinMax2 = GetBaseDamageMinMax(defaultWeaponStats.ResultDamage);
			data.Texts[TooltipElement.BaseDamage] = $"{baseDamageMinMax2.Item1}-{baseDamageMinMax2.Item2}";
		}
	}

	private static void FillWeaponStats(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		if (weapon.Blueprint.IsRanged)
		{
			data.Texts[TooltipElement.MaxDistance] = weapon.AttackRange.ToString();
			data.Texts[TooltipElement.MaxAmmo] = weapon.Blueprint.WarhammerMaxAmmo.ToString();
			int resultAdditionalHitChance = weapon.GetWeaponStats().ResultAdditionalHitChance;
			data.Texts[TooltipElement.AdditionalHitChance] = ((resultAdditionalHitChance > 0) ? UIConfig.Instance.PercentHelper.AddPercentTo(resultAdditionalHitChance) : string.Empty);
		}
		int resultDodgePenetration = weapon.GetWeaponStats().ResultDodgePenetration;
		data.Texts[TooltipElement.DodgeReduction] = ((resultDodgePenetration > 0) ? resultDodgePenetration.ToString() : string.Empty);
	}

	private static void FillWeaponAbilities(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		foreach (WeaponAbility weaponAbility in weapon.Blueprint.WeaponAbilities)
		{
			if (weaponAbility.Ability != BlueprintRoot.Instance.UIConfig.ReloadAbility)
			{
				try
				{
					data.Abilities.Add(GetUIAbilityData(weaponAbility.Ability, weapon));
				}
				catch (Exception ex)
				{
					LogChannel.Default.Error(ex);
				}
			}
		}
	}

	private static void FillShieldAbilities(ItemTooltipData data, ItemEntityShield shield)
	{
		foreach (WeaponAbility weaponAbility in shield.Blueprint.WeaponAbilities)
		{
			if (weaponAbility.Ability != BlueprintRoot.Instance.UIConfig.ReloadAbility)
			{
				try
				{
					data.Abilities.Add(GetUIAbilityData(weaponAbility.Ability, shield));
				}
				catch (Exception ex)
				{
					LogChannel.Default.Error(ex);
				}
			}
		}
	}

	private static void FillEquipmentDamage(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		BlueprintAbility blueprintAbility = itemEquipment.Abilities.FirstOrDefault();
		if (blueprintAbility != null)
		{
			UIAbilityData uIAbilityData = GetUIAbilityData(blueprintAbility);
			data.Texts[TooltipElement.Damage] = uIAbilityData.DamageText;
			data.CompareData[TooltipElement.Damage] = new CompareData
			{
				Value = uIAbilityData.MaxDamage + uIAbilityData.MinDamage
			};
			data.Texts[TooltipElement.Penetration] = uIAbilityData.Penetration.ToString();
			data.CompareData[TooltipElement.Penetration] = new CompareData
			{
				Value = uIAbilityData.Penetration
			};
		}
	}

	private static void FillEquipmentAbilities(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		foreach (BlueprintAbility ability in itemEquipment.Abilities)
		{
			data.Abilities.Add(GetUIAbilityData(ability));
		}
	}

	private static void FillEquipmentStatsBonuses(ItemTooltipData data, BlueprintItemEquipment itemEquipment)
	{
		foreach (AddFactToEquipmentWielder component in itemEquipment.GetComponents<AddFactToEquipmentWielder>())
		{
			foreach (AddStatBonus component2 in component.Fact.GetComponents<AddStatBonus>())
			{
				data.StatBonus.Add(component2.Stat, component2.Value);
			}
		}
	}

	private static string GetMechanicDescription(ItemEntity item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		bool flag = item.Blueprint.GetComponents<AddFactToEquipmentWielder>().Any((AddFactToEquipmentWielder c) => c?.Fact.GetComponent<UIPropertiesComponent>() != null);
		if (item.Owner != null || !flag)
		{
			return UIUtilityTexts.UpdateDescriptionWithUIProperties(item.Description, item.Owner);
		}
		return UpdateDescriptionWithOwner(item, item.Description);
	}

	private static string UpdateDescriptionWithOwner(ItemEntity item, string description)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					if (m_CalculatorUnitPair == null)
					{
						m_CalculatorUnitPair = new CalculatorUnitPair(Game.Instance.SelectionCharacter.SelectedUnitInUI);
					}
					if (m_CalculatorUnitPair.CurrentSelectedUnit == null)
					{
						m_CalculatorUnitPair.Dispose();
						m_CalculatorUnitPair = new CalculatorUnitPair(Game.Instance.SelectionCharacter.SelectedUnitInUI);
					}
					if (m_CalculatorUnitPair.CalculatorUnit == null)
					{
						return description;
					}
					ItemEntity itemEntity = item.Blueprint.CreateEntity();
					if (itemEntity == null)
					{
						return description;
					}
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						using (ContextData<GameCommandHelper.PreviewItem>.Request())
						{
							GameCommandHelper.EquipItemAutomatically(itemEntity, m_CalculatorUnitPair.CalculatorUnit);
							description = UIUtilityTexts.UpdateDescriptionWithUIProperties(description, m_CalculatorUnitPair.CalculatorUnit);
						}
					}
					return description;
				}
			}
		}
	}

	private static string UpdateAbilityShortenedDescription(ItemEntityUsable usable, BlueprintAbility ability)
	{
		string description = usable.Abilities.FirstOrDefault()?.Data.ShortenedDescription ?? ability?.ShortenedDescription;
		return UpdateDescriptionWithOwner(usable, description);
	}

	private static string UpdateAbilityDescription(ItemEntityUsable usable, BlueprintAbility ability)
	{
		string description = usable.Abilities.FirstOrDefault()?.Data.ShortenedDescription ?? ability?.ShortenedDescription;
		return UpdateDescriptionWithOwner(usable, description);
	}

	public static string UpdateAbilityDescription(ItemEntity item, string description)
	{
		return UpdateDescriptionWithOwner(item, description);
	}

	private static string GetMechanicDescription(BlueprintItem blueprintItem)
	{
		if (blueprintItem == null)
		{
			return string.Empty;
		}
		return blueprintItem.Description;
	}

	private static string GetFlavorDescription(ItemEntity item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		if (!CanReadItem(item))
		{
			return string.Empty;
		}
		return item.FlavorText;
	}

	private static string GetFlavorDescription(BlueprintItem blueprintItem)
	{
		if (blueprintItem == null)
		{
			return string.Empty;
		}
		if (!CanReadItem(blueprintItem))
		{
			return string.Empty;
		}
		return blueprintItem.FlavorText;
	}

	public static string GetHandUse(ItemEntity item)
	{
		if (item is ItemEntityWeapon)
		{
			return UIStrings.Instance.Tooltips.OneHanded;
		}
		return string.Empty;
	}

	public static string GetHandUse(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.OneHanded;
	}

	private static void FillRange(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		if (!weapon.Blueprint.IsMelee)
		{
			data.Texts[TooltipElement.Range] = weapon.AttackOptimalRange.ToString();
			data.AddTexts[TooltipElement.Range] = string.Format(UIConfig.Instance.UITooltipMaxRangeFormat, UIStrings.Instance.Tooltips.MaximumRange.Text, weapon.AttackRange.ToString());
		}
	}

	private static void FillRateOfFire(ItemTooltipData data, ItemEntityWeapon weapon, RuleCalculateStatsWeapon weaponStats)
	{
		bool flag = !weapon.Blueprint.IsRanged;
		string value = Math.Max(1, weaponStats.ResultRateOfFire).ToString();
		data.Texts[flag ? TooltipElement.RateOfFireMelee : TooltipElement.RateOfFire] = value;
		if (!flag)
		{
			int resultRecoil = weaponStats.ResultRecoil;
			if (resultRecoil > 0)
			{
				data.AddTexts[TooltipElement.RateOfFire] = string.Format(UIStrings.Instance.Tooltips.Recoil, resultRecoil.ToString());
			}
		}
	}

	public static string GetItemOwnerName(ItemEntity item)
	{
		PartUnitDescription partUnitDescription = item.Owner?.GetDescriptionOptional();
		if (partUnitDescription != null)
		{
			return partUnitDescription.Name;
		}
		return string.Empty;
	}

	public static string GetWielderSlot(ItemEntity item)
	{
		return string.Empty ?? "";
	}

	public static string GetItemType(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			ItemsStrings items = LocalizedTexts.Instance.Items;
			if (!string.IsNullOrEmpty(item.Blueprint.SubtypeName))
			{
				return item.Blueprint.SubtypeName + " " + items.NotIdentifiedSuffix;
			}
			return items.NotIdentified;
		}
		return item.Blueprint.SubtypeName;
	}

	public static string GetCargoVolume(BlueprintItem item)
	{
		return UIConfig.Instance.PercentHelper.AddPercentTo(item.CargoVolumePercent);
	}

	public static string GetItemType(BlueprintItem blueprintItem)
	{
		return blueprintItem.SubtypeName;
	}

	private static string GetEnhancementBonus(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			return string.Empty;
		}
		if (!(item is ItemEntityWeapon) && !(item is ItemEntityArmor) && !(item is ItemEntityShield))
		{
			return "none";
		}
		int itemEnhancementBonus = GameHelper.GetItemEnhancementBonus(item);
		if (itemEnhancementBonus == 0)
		{
			return string.Empty;
		}
		return UIUtility.AddSign(itemEnhancementBonus);
	}

	private static string GetQualities(ItemEntity item)
	{
		if (!item.IsIdentified)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = item is ItemEntityShield;
		foreach (ItemEnchantment enchantment in item.Enchantments)
		{
			if (!string.IsNullOrEmpty(enchantment.Blueprint.Name) && (!flag || !(enchantment.Owner is ItemEntityWeapon)))
			{
				UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
				stringBuilder.Append(enchantment.Blueprint.Name);
			}
		}
		if (!(item is ItemEntityWeapon itemEntityWeapon))
		{
			return stringBuilder.ToString();
		}
		WeaponCategory category = itemEntityWeapon.Blueprint.Category;
		if (category.HasSubCategory(WeaponSubCategory.Finessable))
		{
			UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
			stringBuilder.Append(LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Finessable));
		}
		if (category.HasSubCategory(WeaponSubCategory.Monk))
		{
			UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
			stringBuilder.Append(LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Monk));
		}
		stringBuilder.Replace(" ,", ",");
		return stringBuilder.ToString();
	}

	public static string GetDamageDiceText(DiceFormula dice, int damageBonus)
	{
		if (damageBonus <= 0)
		{
			if (damageBonus != 0)
			{
				return $"{dice}{damageBonus}";
			}
			return $"{dice}";
		}
		return $"{dice}+{damageBonus}";
	}

	public static int MinDiceValue(UnitDescription.UIDamageInfo damageEntry)
	{
		return damageEntry.MinValue();
	}

	public static int MaxDiceValue(UnitDescription.UIDamageInfo damageEntry)
	{
		return damageEntry.MaxValue();
	}

	private static (int Min, int Max) GetDamageMinMax(DamageData damageInfo)
	{
		int val = 0;
		int item = Math.Max(0, damageInfo.MinValue);
		val = Math.Max(val, damageInfo.MaxValue);
		return (Min: item, Max: val);
	}

	private static (int Min, int Max) GetBaseDamageMinMax(DamageData damageInfo)
	{
		int val = 0;
		int item = Math.Max(0, damageInfo.MinValueBase);
		val = Math.Max(val, damageInfo.MaxValueBase);
		return (Min: item, Max: val);
	}

	private static void FillPenetration(ItemTooltipData data, ItemEntityWeapon weapon)
	{
		int warhammerPenetration = weapon.Blueprint.WarhammerPenetration;
		data.Texts[TooltipElement.Penetration] = warhammerPenetration.ToString();
		data.CompareData[TooltipElement.Penetration] = new CompareData
		{
			Value = warhammerPenetration
		};
	}

	private static string GetRangeType(ItemEntity item)
	{
		if (item is ItemEntityWeapon itemEntityWeapon)
		{
			return GetRangeType(itemEntityWeapon.Blueprint);
		}
		return string.Empty;
	}

	private static string GetRangeType(BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemWeapon blueprintItemWeapon))
		{
			return string.Empty;
		}
		AttackType val = ((!blueprintItemWeapon.IsMelee) ? AttackType.Ranged : AttackType.Melee);
		return LocalizedTexts.Instance.AttackTypes.GetText(val);
	}

	public static string GetProficiencyGroup(ItemEntity item)
	{
		if (!(item is ItemEntityWeapon itemEntityWeapon))
		{
			return string.Empty;
		}
		WeaponCategory category = itemEntityWeapon.Blueprint.Category;
		if (category.HasSubCategory(WeaponSubCategory.Exotic))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Exotic);
		}
		if (category.HasSubCategory(WeaponSubCategory.Martial))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Martial);
		}
		if (category.HasSubCategory(WeaponSubCategory.Simple))
		{
			return LocalizedTexts.Instance.WeaponSubCategories.GetText(WeaponSubCategory.Simple);
		}
		return string.Empty;
	}

	public static string GetItemGroup(ItemEntity item)
	{
		return ItemsFilter.GetItemType(item).ToString();
	}

	public static string GetItemGroup(BlueprintItem blueprintItem)
	{
		return ItemsFilter.GetItemType(blueprintItem).ToString();
	}

	public static bool CanEquipItem(ItemEntity item)
	{
		BaseUnitEntity unit = UIUtility.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		return InventoryHelper.CanEquipItem(item, unit);
	}

	public static bool IsEquipPossible(ItemEntity item)
	{
		BaseUnitEntity baseUnitEntity = UIUtility.GetCurrentSelectedUnit() ?? Game.Instance.Player.MainCharacterEntity;
		if ((baseUnitEntity.IsPet && !(item.Blueprint is BlueprintItemEquipmentPetProtocol)) || (baseUnitEntity.IsPet && item.Blueprint is BlueprintItemEquipmentPetProtocol && !item.CanBeEquippedBy(baseUnitEntity)))
		{
			return false;
		}
		if (item.Blueprint is BlueprintItemEquipment)
		{
			return item.CanBeEquippedBy(baseUnitEntity);
		}
		return false;
	}

	public static bool IsQuestItem(BlueprintItem blueprintItem)
	{
		if (!blueprintItem.IsNotable)
		{
			return blueprintItem.Rarity == BlueprintItem.ItemRarity.Quest;
		}
		return true;
	}

	public static bool[] GetEquipPosibility(ItemEntity item)
	{
		BaseUnitEntity baseUnitEntity = UIUtility.GetCurrentSelectedUnit() ?? Game.Instance?.Player?.MainCharacterEntity;
		bool flag = true;
		bool flag2 = false;
		if (item != null && baseUnitEntity != null)
		{
			if (item.Blueprint is BlueprintItemEquipment)
			{
				flag = item.CanBeEquippedBy(baseUnitEntity);
			}
			if (baseUnitEntity.IsPet && !(item.Blueprint is BlueprintItemEquipmentPetProtocol))
			{
				flag = false;
			}
			return new bool[2] { flag, flag2 };
		}
		return new bool[2] { flag, flag2 };
	}

	public static bool IsItemAbilityInSpellListOfUnit(BlueprintItemEquipmentUsable item, BaseUnitEntity unit)
	{
		return item.Abilities.Any((BlueprintAbility v) => v?.IsInSpellListOfUnit(unit) ?? false);
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemTooltipData itemTooltipData = new ItemTooltipData(item);
			ItemEntityUsable usable = itemTooltipData.Usable;
			itemTooltipData.Texts[TooltipElement.Name] = item.Name;
			itemTooltipData.Texts[TooltipElement.Count] = (item.IsStackable ? item.Count.ToString() : "");
			itemTooltipData.Texts[TooltipElement.ItemType] = GetItemGroup(item);
			itemTooltipData.Texts[TooltipElement.ItemCost] = UIUtility.GetProfitFactorFormatted(item.Blueprint.ProfitFactorCost);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(item);
			itemTooltipData.Texts[TooltipElement.Price] = Game.Instance.Vendor.GetItemBuyPrice(item).ToString();
			itemTooltipData.Texts[TooltipElement.Wielder] = GetItemOwnerName(item);
			itemTooltipData.Texts[TooltipElement.WielderSlot] = GetWielderSlot(item);
			itemTooltipData.Texts[TooltipElement.Twohanded] = GetHandUse(item);
			itemTooltipData.Texts[TooltipElement.CargoVolume] = GetCargoVolume(item.Blueprint);
			FillItemRestrictions(itemTooltipData, item.Blueprint);
			if (item.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				StatsStrings stats = LocalizedTexts.Instance.Stats;
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = stats.GetText(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = stats.GetText(blueprintItemWeapon.Family);
			}
			if (item.Blueprint is BlueprintItemNote || item.Blueprint is BlueprintItemKey)
			{
				if (CanReadItem(item))
				{
					if (string.IsNullOrEmpty(GetFlavorDescription(item)))
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetMechanicDescription(item);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item);
					}
					else
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(item);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetFlavorDescription(item);
					}
				}
			}
			else
			{
				itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(item);
				itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(item);
			}
			string value = FillEnchantmentDescription(item, itemTooltipData);
			if (item.IsIdentified && usable != null)
			{
				itemTooltipData.Texts[TooltipElement.Charges] = usable.Charges.ToString();
				itemTooltipData.Texts[TooltipElement.CasterLevel] = usable.GetCasterLevel().ToString();
				BlueprintAbility blueprintAbility = usable.Blueprint.Abilities.FirstOrDefault();
				if (blueprintAbility != null)
				{
					string text = Game.Instance.BlueprintRoot.LocalizedTexts.AbilityTargets.Personal;
					itemTooltipData.Texts[TooltipElement.Cooldown] = blueprintAbility.CooldownRounds.ToString();
					itemTooltipData.Texts[TooltipElement.Target] = ((usable.Blueprint.Type == UsableItemType.Potion) ? text : blueprintAbility.GetTarget(-1, item.Owner));
					itemTooltipData.BlueprintAbility = blueprintAbility;
					itemTooltipData.Texts[TooltipElement.ShortDescription] = UpdateAbilityShortenedDescription(usable, blueprintAbility);
					itemTooltipData.Texts[TooltipElement.LongDescription] = UpdateAbilityDescription(usable, blueprintAbility);
					itemTooltipData.Texts[TooltipElement.SpellDescriptor] = UIUtilityTexts.GetSpellDescriptorsText(blueprintAbility);
					itemTooltipData.Texts[TooltipElement.CastingTime] = UIUtilityTexts.GetAbilityActionText(blueprintAbility, usable.Blueprint);
					FillEquipmentAbilities(itemTooltipData, usable.Blueprint);
					FillEquipmentDamage(itemTooltipData, usable.Blueprint);
					FillEquipmentStatsBonuses(itemTooltipData, usable.Blueprint);
				}
			}
			if (!string.IsNullOrEmpty(value))
			{
				itemTooltipData.Texts[TooltipElement.EnchantmentsDescription] = value;
			}
			return itemTooltipData;
		}
	}

	public static ItemTooltipData GetItemTooltipData(ItemEntity item, bool replenishing)
	{
		ItemTooltipData itemTooltipData = GetItemTooltipData(item);
		if (replenishing)
		{
			itemTooltipData.Texts[TooltipElement.Replenishing] = UIStrings.Instance.Tooltips.ReplenishingItem;
		}
		return itemTooltipData;
	}

	public static ItemTooltipData GetItemTooltipData(BlueprintItem blueprintItem)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemTooltipData itemTooltipData = new ItemTooltipData(blueprintItem);
			BlueprintItemEquipmentUsable blueprintUsable = itemTooltipData.BlueprintUsable;
			itemTooltipData.Texts[TooltipElement.Name] = blueprintItem.Name;
			itemTooltipData.Texts[TooltipElement.Count] = string.Empty;
			itemTooltipData.Texts[TooltipElement.ItemType] = GetItemGroup(blueprintItem);
			itemTooltipData.Texts[TooltipElement.ItemCost] = UIUtility.GetProfitFactorFormatted(blueprintItem.ProfitFactorCost);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(blueprintItem);
			itemTooltipData.Texts[TooltipElement.Wielder] = string.Empty;
			itemTooltipData.Texts[TooltipElement.WielderSlot] = string.Empty;
			itemTooltipData.Texts[TooltipElement.Twohanded] = GetHandUse(blueprintItem);
			itemTooltipData.Texts[TooltipElement.CargoVolume] = GetCargoVolume(blueprintItem);
			FillItemRestrictions(itemTooltipData, blueprintItem);
			if (blueprintItem is BlueprintItemWeapon blueprintItemWeapon)
			{
				StatsStrings stats = LocalizedTexts.Instance.Stats;
				itemTooltipData.Texts[TooltipElement.WeaponCategory] = stats.GetText(blueprintItemWeapon.Category);
				itemTooltipData.Texts[TooltipElement.WeaponFamily] = stats.GetText(blueprintItemWeapon.Family);
			}
			if (blueprintItem is BlueprintItemNote || blueprintItem is BlueprintItemKey)
			{
				if (CanReadItem(blueprintItem))
				{
					if (string.IsNullOrEmpty(GetFlavorDescription(blueprintItem)))
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetMechanicDescription(blueprintItem);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(blueprintItem);
					}
					else
					{
						itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(blueprintItem);
						itemTooltipData.Texts[TooltipElement.ShortDescription] = GetFlavorDescription(blueprintItem);
					}
				}
			}
			else
			{
				itemTooltipData.Texts[TooltipElement.ArtisticDescription] = GetFlavorDescription(blueprintItem);
				itemTooltipData.Texts[TooltipElement.ShortDescription] = GetMechanicDescription(blueprintItem);
			}
			if (blueprintUsable != null)
			{
				itemTooltipData.Texts[TooltipElement.Charges] = blueprintUsable.Charges.ToString();
				itemTooltipData.Texts[TooltipElement.CasterLevel] = blueprintUsable.GetCasterLevel().ToString();
				BlueprintAbility blueprintAbility = blueprintUsable.Abilities.FirstOrDefault();
				if (blueprintAbility != null)
				{
					string text = Game.Instance.BlueprintRoot.LocalizedTexts.AbilityTargets.Personal;
					itemTooltipData.Texts[TooltipElement.Cooldown] = blueprintAbility.CooldownRounds.ToString();
					itemTooltipData.Texts[TooltipElement.Target] = ((blueprintUsable.Type == UsableItemType.Potion) ? text : blueprintAbility.GetTarget());
					itemTooltipData.BlueprintAbility = blueprintAbility;
					itemTooltipData.Texts[TooltipElement.ShortDescription] = SimpleBlueprintExtendAsObject.Or(blueprintAbility, null)?.ShortenedDescription;
					itemTooltipData.Texts[TooltipElement.LongDescription] = SimpleBlueprintExtendAsObject.Or(blueprintAbility, null)?.Description;
					itemTooltipData.Texts[TooltipElement.SpellDescriptor] = UIUtilityTexts.GetSpellDescriptorsText(blueprintAbility);
					itemTooltipData.Texts[TooltipElement.CastingTime] = UIUtilityTexts.GetAbilityActionText(blueprintAbility, blueprintUsable);
					FillEquipmentAbilities(itemTooltipData, blueprintUsable);
					FillEquipmentDamage(itemTooltipData, blueprintUsable);
					FillEquipmentStatsBonuses(itemTooltipData, blueprintUsable);
				}
			}
			return itemTooltipData;
		}
	}

	private static string FillEnchantmentDescription(ItemEntity item, ItemTooltipData itemTooltipData)
	{
		string text = string.Empty;
		if (item is ItemEntityWeapon itemEntityWeapon)
		{
			RuleCalculateStatsWeapon weaponStats = itemEntityWeapon.GetWeaponStats();
			MechanicEntity mechanicEntity = item.Owner ?? UIUtility.GetCurrentSelectedUnit();
			RuleCalculateStatsWeapon ruleCalculateStatsWeapon = ((mechanicEntity != null) ? itemEntityWeapon.GetWeaponStats(mechanicEntity) : null);
			RuleCalculateStatsWeapon weaponStats2 = ruleCalculateStatsWeapon ?? weaponStats;
			itemTooltipData.Texts[TooltipElement.AttackType] = GetRangeType(itemEntityWeapon);
			itemTooltipData.Texts[TooltipElement.ProficiencyGroup] = GetProficiencyGroup(itemEntityWeapon);
			text = FillWeaponQualities(itemTooltipData, itemEntityWeapon, text);
			FillPenetration(itemTooltipData, itemEntityWeapon);
			FillRange(itemTooltipData, itemEntityWeapon);
			FillRateOfFire(itemTooltipData, itemEntityWeapon, weaponStats2);
			FillWeaponDamage(itemTooltipData, weaponStats, ruleCalculateStatsWeapon, itemEntityWeapon);
			FillWeaponStats(itemTooltipData, itemEntityWeapon);
			FillWeaponAbilities(itemTooltipData, itemEntityWeapon);
		}
		else if (item is ItemEntityArmor itemEntityArmor)
		{
			ArmorData armorData = GetArmorData(itemEntityArmor, null);
			itemTooltipData.Texts[TooltipElement.ArmorDeflection] = armorData.DamageDeflection.ToString();
			itemTooltipData.CompareData[TooltipElement.ArmorDeflection] = new CompareData
			{
				Value = armorData.DamageDeflection
			};
			itemTooltipData.Texts[TooltipElement.ArmorAbsorption] = UIConfig.Instance.PercentHelper.AddPercentTo(armorData.DamageAbsorption);
			itemTooltipData.CompareData[TooltipElement.ArmorAbsorption] = new CompareData
			{
				Value = armorData.DamageAbsorption
			};
			itemTooltipData.Texts[TooltipElement.ArmorDodgePenalty] = armorData.ArmorDodgePenalty;
			itemTooltipData.Texts[TooltipElement.FullArmorClass] = LocalizedTexts.Instance.Stats.GetText(itemEntityArmor.Blueprint.Category);
			text = FillArmorEnchantments(itemTooltipData, itemEntityArmor, text);
			itemTooltipData.Texts[TooltipElement.ArmorCheckPenalty] = armorData.RangedHitChanceBonus.ToString();
			itemTooltipData.Texts[TooltipElement.ArcaneSpellFailure] = armorData.MovePointAdjustment.ToString();
			if (!armorData.ArmorDamageReduceDescription.IsNullOrEmpty())
			{
				itemTooltipData.Texts[TooltipElement.ArmorDamageReduceDescription] = armorData.ArmorDamageReduceDescription;
			}
			if (!armorData.ArmourDodgeChanceDescription.IsNullOrEmpty())
			{
				itemTooltipData.Texts[TooltipElement.ArmourDodgeChanceDescription] = armorData.ArmourDodgeChanceDescription;
			}
		}
		else if (item is ItemEntityShield shield)
		{
			ArmorData armorData2 = GetArmorData(null, shield);
			itemTooltipData.Texts[TooltipElement.Subname] = GetItemType(item);
			itemTooltipData.Texts[TooltipElement.FullArmorClass] = armorData2.DamageDeflection.ToString();
			itemTooltipData.Texts[TooltipElement.ArmorClass] = armorData2.DamageAbsorption.ToString();
			itemTooltipData.Texts[TooltipElement.ArmorDodgePenalty] = armorData2.ArmorDodgePenalty;
			itemTooltipData.Texts[TooltipElement.ArmorCheckPenalty] = armorData2.RangedHitChanceBonus.ToString();
			itemTooltipData.Texts[TooltipElement.ArcaneSpellFailure] = armorData2.MovePointAdjustment.ToString();
			text = FillShieldEnchantments(itemTooltipData, shield, text);
			FillShieldAbilities(itemTooltipData, shield);
		}
		return text;
	}

	private static string FillShieldEnchantments(ItemTooltipData itemTooltipData, ItemEntityShield shield, string enchantmentDescription)
	{
		if (shield.IsIdentified)
		{
			foreach (ItemEnchantment enchantment in shield.Enchantments)
			{
				if (!(enchantment.Owner is ItemEntityWeapon))
				{
					if (!itemTooltipData.Texts.ContainsKey(TooltipElement.Qualities))
					{
						itemTooltipData.Texts[TooltipElement.Qualities] = GetQualities(shield);
					}
					if (!string.IsNullOrEmpty(enchantment.Blueprint.Description))
					{
						enchantmentDescription += $"<b><align=\"center\">{enchantment.Blueprint.Name}</align></b>\n";
						enchantmentDescription = enchantmentDescription + enchantment.Blueprint.Description + "\n\n";
					}
				}
			}
		}
		return enchantmentDescription;
	}

	private static string FillArmorEnchantments(ItemTooltipData itemTooltipData, ItemEntityArmor armor, string enchantmentDescription)
	{
		if (armor.IsIdentified)
		{
			if (GameHelper.GetItemEnhancementBonus(armor) > 0)
			{
				itemTooltipData.Texts[TooltipElement.Enhancement] = GetEnhancementBonus(armor);
			}
			foreach (ItemEnchantment enchantment in armor.Enchantments)
			{
				if (!itemTooltipData.Texts.ContainsKey(TooltipElement.Qualities))
				{
					itemTooltipData.Texts[TooltipElement.Qualities] = GetQualities(armor);
				}
				if (!string.IsNullOrEmpty(enchantment.Blueprint.Description))
				{
					enchantmentDescription += $"<b><align=\"center\">{enchantment.Blueprint.Name}</align></b>\n";
					enchantmentDescription = enchantmentDescription + enchantment.Blueprint.Description + "\n\n";
				}
			}
		}
		return enchantmentDescription;
	}

	private static string FillWeaponQualities(ItemTooltipData itemTooltipData, ItemEntityWeapon weapon, string enchantmentDescription)
	{
		if (weapon.IsIdentified)
		{
			itemTooltipData.Texts[TooltipElement.Qualities] = GetQualities(weapon);
			foreach (ItemEnchantment enchantment in weapon.Enchantments)
			{
				if (!string.IsNullOrEmpty(enchantment.Blueprint.Description))
				{
					enchantmentDescription += $"<b><align=\"center\">{enchantment.Blueprint.Name}</align></b>\n";
					enchantmentDescription = enchantmentDescription + enchantment.Blueprint.Description + "\n\n";
				}
			}
			if (weapon.Enchantments.Any() && !itemTooltipData.Texts.ContainsKey(TooltipElement.Qualities))
			{
				itemTooltipData.Texts[TooltipElement.Enhancement] = GetEnhancementBonus(weapon);
			}
			if (GameHelper.GetItemEnhancementBonus(weapon) > 0)
			{
				itemTooltipData.Texts[TooltipElement.Enhancement] = GetEnhancementBonus(weapon);
			}
		}
		return enchantmentDescription;
	}

	public static bool IsReload(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility?.GetComponent<WeaponReloadLogic>() != null;
	}

	public static bool IsReload(AbilityData abilityData)
	{
		return IsReload(abilityData?.Blueprint);
	}

	public static int GetMaxAbilityAmmo(ItemEntityWeapon weapon)
	{
		return (weapon?.Abilities.Max((Ability a) => a?.Data?.AmmoRequired)).GetValueOrDefault();
	}

	public static bool ShouldShowTalentIcons(BlueprintFeature feature)
	{
		if (feature.TalentIconInfo.HasGroups)
		{
			return feature.Icon == null;
		}
		return false;
	}
}
