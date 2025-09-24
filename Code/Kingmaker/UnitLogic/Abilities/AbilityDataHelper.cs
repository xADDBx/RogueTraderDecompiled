using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities;

public static class AbilityDataHelper
{
	public static bool HasIsAllyCondition(this ConditionsChecker conditionsChecker)
	{
		if (conditionsChecker.HasConditions)
		{
			Condition[] conditions = conditionsChecker.Conditions;
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] is ContextConditionIsAlly)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsValidTargetForAttack(this AbilityData ability, MechanicEntity target)
	{
		if (target.GetHealthOptional() == null)
		{
			return false;
		}
		PartLifeState lifeStateOptional = target.GetLifeStateOptional();
		bool flag = lifeStateOptional == null || lifeStateOptional.IsConscious;
		if ((flag && !ability.Blueprint.CanCastToAliveTarget()) || (!flag && !ability.Blueprint.CanCastToDeadTarget))
		{
			return false;
		}
		AbilityCanTargetOnlyPetUnits canTargetOnlyPetUnitsComponent = ability.Blueprint.CanTargetOnlyPetUnitsComponent;
		if (canTargetOnlyPetUnitsComponent != null)
		{
			if (canTargetOnlyPetUnitsComponent.Inverted)
			{
				if (target is BaseUnitEntity { IsPet: not false })
				{
					return false;
				}
			}
			else
			{
				if (!(target is BaseUnitEntity { IsPet: not false } baseUnitEntity2))
				{
					return false;
				}
				if (canTargetOnlyPetUnitsComponent.CanTargetOnlyOwnersPet && baseUnitEntity2.Master != ability.Caster)
				{
					return false;
				}
			}
		}
		MechanicEntity caster = ability.Caster;
		if (caster != null && caster.IsAttackingGreenNPC(target))
		{
			return false;
		}
		if (AbstractUnitCommand.CommandTargetUntargetable(ability.Caster, target))
		{
			return false;
		}
		return true;
	}

	public static bool IsPatternRestrictionPassed(this AbilityData abilityData, TargetWrapper target, out AbilityData.UnavailabilityReasonType unavailabilityReason)
	{
		unavailabilityReason = AbilityData.UnavailabilityReasonType.None;
		return abilityData.Blueprint?.GetComponent<IAbilityPatternRestriction>()?.IsPatternRestrictionPassed(abilityData, abilityData.Caster, target, out unavailabilityReason) ?? true;
	}

	public static ReadonlyList<CustomGridNodeBase> GetSingleShotAffectedNodes(this AbilityData ability, TargetWrapper target)
	{
		if (ability.Blueprint.AttackType != AttackAbilityType.SingleShot || target?.Entity == null)
		{
			return TempList.Get<CustomGridNodeBase>();
		}
		return AbilityProjectileAttack.GetSingleShotAffectedNodes(ability, target.Entity).Nodes;
	}

	public static bool IsChainLighting(this AbilityData ability)
	{
		return ability.Blueprint.GetComponent<AbilityDeliverChain>() != null;
	}

	public static HashSet<CustomGridNodeBase> GetChainLightingTargets(this AbilityData ability, TargetWrapper target)
	{
		if (ability?.Caster == null)
		{
			return TempHashSet.Get<CustomGridNodeBase>();
		}
		MechanicEntity mechanicEntity = target?.Entity;
		if (mechanicEntity == null)
		{
			return TempHashSet.Get<CustomGridNodeBase>();
		}
		AbilityDeliverChain component = ability.Blueprint.GetComponent<AbilityDeliverChain>();
		if (component == null)
		{
			return TempHashSet.Get<CustomGridNodeBase>();
		}
		HashSet<MechanicEntity> hashSet = new HashSet<MechanicEntity> { mechanicEntity };
		int value = component.TargetsCount.Value;
		int num = 0;
		while (num < value)
		{
			num++;
			if (num < value)
			{
				mechanicEntity = SelectNextTarget(ability, component, target, hashSet);
				if (mechanicEntity != null)
				{
					hashSet.Add(mechanicEntity);
				}
			}
		}
		return hashSet.SelectMany((MechanicEntity u) => u.GetOccupiedNodes()).ToTempHashSet();
	}

	public static PartAbilityPredictionForAreaEffect TryGetPatternDataFromAreaEffect(this AbilityData abilityData)
	{
		return abilityData.Caster.GetPartAbilityPredictionForAreaEffectOptional();
	}

	private static BaseUnitEntity SelectNextTarget(AbilityData abilityData, AbilityDeliverChain chainComponent, TargetWrapper originalTarget, HashSet<MechanicEntity> usedTargets)
	{
		Vector3 point = originalTarget.Point;
		float num = float.MaxValue;
		BaseUnitEntity result = null;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (abilityData.IsValidTargetForAttack(allBaseUnit))
			{
				float num2 = allBaseUnit.DistanceToInCells(point);
				if (CheckTarget(abilityData, chainComponent, allBaseUnit) && num2 <= (float)chainComponent.Radius && !usedTargets.Contains(allBaseUnit) && num2 < num)
				{
					num = num2;
					result = allBaseUnit;
				}
			}
		}
		return result;
	}

	private static bool CheckTarget(AbilityData abilityData, AbilityDeliverChain chainComponent, BaseUnitEntity unit)
	{
		if (abilityData.Caster == null)
		{
			return false;
		}
		if (unit.LifeState.IsDead && !chainComponent.TargetDead)
		{
			return false;
		}
		if ((chainComponent.TargetType == TargetType.Enemy && !abilityData.Caster.IsEnemy(unit)) || (chainComponent.TargetType == TargetType.Ally && abilityData.Caster.IsEnemy(unit)))
		{
			return false;
		}
		return true;
	}

	public static HealPredictionData GetHealPrediction(this AbilityData ability, MechanicEntity target)
	{
		AbilityEffectRunAction component = ability.Blueprint.GetComponent<AbilityEffectRunAction>();
		if (component == null)
		{
			return null;
		}
		try
		{
			return GetActionsHeal(ability, null, component.Actions, target);
		}
		catch (Exception ex)
		{
			LogChannel.Default.Error(ex);
		}
		return null;
	}

	private static HealPredictionData GetActionsHeal(AbilityData ability, [CanBeNull] AbilityExecutionContext context, ActionList actions, MechanicEntity target)
	{
		HealPredictionData healPredictionData = null;
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction action in actions2)
		{
			if (context == null)
			{
				context = ability.CreateExecutionContext(target, ability.Caster.Position);
			}
			HealPredictionData healPredictionData2 = null;
			healPredictionData2 = TryGetHealPredictionFromAction(ability, context, target, action);
			if (healPredictionData2 != null)
			{
				if (healPredictionData == null)
				{
					healPredictionData = new HealPredictionData();
				}
				healPredictionData += healPredictionData2;
			}
		}
		return healPredictionData;
	}

	private static HealPredictionData TryGetHealPredictionFromAction(AbilityData ability, AbilityExecutionContext context, MechanicEntity target, GameAction action)
	{
		if (action is Conditional conditional)
		{
			bool flag = false;
			using (context.GetDataScope(target.ToITargetWrapper()))
			{
				flag = conditional.ConditionsChecker.Check();
			}
			if (!flag)
			{
				return GetActionsHeal(ability, context, conditional.IfFalse, target);
			}
			return GetActionsHeal(ability, context, conditional.IfTrue, target);
		}
		if (action is ContextActionHealTarget contextActionHealTarget)
		{
			return contextActionHealTarget.GetHealPrediction(context, target);
		}
		return null;
	}

	public static DamagePredictionData GetDamagePrediction(this AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, AbilityExecutionContext context = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			DamagePredictionData damagePredictionData = new DamagePredictionData
			{
				MinDamage = 0,
				MaxDamage = 0,
				Penetration = 0
			};
			if (target is StarshipEntity && (ability.StarshipWeapon != null || (bool)ability.Blueprint.GetComponent<AbilityCustomStarshipRam>()))
			{
				return GetStarshipDamagePrediction(target as StarshipEntity, casterPosition, ability, ability.StarshipWeapon).resultDamage;
			}
			WarhammerAbilityAttackDelivery.WeaponAttackType? weaponAttackType = ability.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>()?.WeaponAttack;
			if (weaponAttackType.HasValue && weaponAttackType.GetValueOrDefault() != 0 && ability.Weapon != null)
			{
				bool flag = Rulebook.Trigger(new RuleCalculateHitChances(ability.Caster, target, ability, 0)).RighteousFuryChanceRule.ResultChance >= 100;
				int enemyTargetCountInPattern = GetEnemyTargetCountInPattern(ability);
				using (ContextData<EnemyTargetsInPatternData>.Request().Setup(enemyTargetCountInPattern))
				{
					MechanicEntity caster = ability.Caster;
					bool forceCrit = flag;
					DamageData resultDamage = new CalculateDamageParams(caster, target, ability, null, null, null, null, null, forceCrit).Trigger().ResultDamage;
					if (resultDamage == null)
					{
						Debug.LogError("Weapon calculate damage is broken: RuleCalculateDamage == NULL");
						return null;
					}
					DamagePredictionData damagePredictionData2 = new DamagePredictionData
					{
						MinDamage = resultDamage.MinValue,
						MaxDamage = resultDamage.MaxValue,
						Penetration = resultDamage.Penetration.Value
					};
					damagePredictionData += damagePredictionData2;
				}
			}
			else if (ability.Blueprint.GetComponent<AbilityMeleeBurst>() != null)
			{
				bool flag2 = Rulebook.Trigger(new RuleCalculateHitChances(ability.Caster, target, ability, 0)).RighteousFuryChanceRule.ResultChance >= 100;
				int enemyTargetCountInPattern2 = GetEnemyTargetCountInPattern(ability);
				using (ContextData<EnemyTargetsInPatternData>.Request().Setup(enemyTargetCountInPattern2))
				{
					MechanicEntity caster2 = ability.Caster;
					bool forceCrit = flag2;
					DamageData resultDamage2 = new CalculateDamageParams(caster2, target, ability, null, null, null, null, null, forceCrit).Trigger().ResultDamage;
					if (resultDamage2 == null)
					{
						Debug.LogError("Weapon calculate damage is broken: RuleCalculateDamage == NULL");
						return null;
					}
					int rateOfFire = ability.Blueprint.GetComponent<AbilityMeleeBurst>().GetRateOfFire(ability.CreateExecutionContext(target));
					DamagePredictionData damagePredictionData3 = new DamagePredictionData
					{
						MinDamage = resultDamage2.MinValue * rateOfFire,
						MaxDamage = resultDamage2.MaxValue * rateOfFire,
						Penetration = resultDamage2.Penetration.Value
					};
					damagePredictionData += damagePredictionData3;
				}
			}
			foreach (AbilityEffectRunAction component in ability.Blueprint.GetComponents<AbilityEffectRunAction>())
			{
				try
				{
					DamagePredictionData actionsDamage = GetActionsDamage(ability, component.Actions, context, casterPosition, target ?? Game.Instance.DefaultUnit);
					damagePredictionData += actionsDamage;
				}
				catch (Exception ex)
				{
					LogChannel.Default.Error(ex);
				}
			}
			return (damagePredictionData.MaxDamage == 0) ? null : damagePredictionData;
		}
	}

	private static int GetEnemyTargetCountInPattern(AbilityData abilityData)
	{
		if (abilityData == null)
		{
			return 0;
		}
		Game instance = Game.Instance;
		if (instance != null)
		{
			PointerController clickEventsController = instance.ClickEventsController;
			if (clickEventsController != null)
			{
				ClickWithSelectedAbilityHandler selectedAbilityHandler = instance.SelectedAbilityHandler;
				if (selectedAbilityHandler != null)
				{
					TargetWrapper target = selectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, abilityData, abilityData.Caster.Position);
					target = ((target != null) ? target : ((TargetWrapper)clickEventsController.WorldPosition));
					OrientedPatternData pattern = abilityData.GetPattern(target, abilityData.Caster.Position);
					int num = 0;
					{
						foreach (CustomGridNodeBase node in pattern.Nodes)
						{
							BaseUnitEntity unit = node.GetUnit();
							if (unit != null && unit != abilityData.Caster && !unit.IsAlly(abilityData.Caster))
							{
								num++;
							}
						}
						return num;
					}
				}
			}
		}
		return 0;
	}

	public static (DamagePredictionData resultDamage, ShieldDamageData resultShields) GetStarshipDamagePrediction(StarshipEntity target, Vector3 casterPosition, AbilityData ability, ItemEntityStarshipWeapon weapon)
	{
		DamagePredictionData damagePredictionData = new DamagePredictionData();
		DamagePredictionData damagePredictionData2 = new DamagePredictionData();
		ShieldDamageData shieldDamageData = new ShieldDamageData();
		ShieldDamageData shieldDamageData2 = new ShieldDamageData();
		StarshipEntity starship = ability.Caster as StarshipEntity;
		AbilityCustomStarshipRam component = ability.Blueprint.GetComponent<AbilityCustomStarshipRam>();
		RuleStarshipCalculateHitChances hitChanceRule;
		RuleStarshipCalculateHitLocation hitLocationRule;
		int defaultInstances;
		if (component != null)
		{
			AbilityCustomStarshipRam.RamDamagePrediction ramDamagePrediction = new AbilityCustomStarshipRam.RamDamagePrediction();
			component.EvaluateRamPrediction(starship, casterPosition, target, ramDamagePrediction);
			damagePredictionData = ramDamagePrediction.targetDamage;
			shieldDamageData = ramDamagePrediction.targetShields;
		}
		else
		{
			hitChanceRule = Rulebook.Trigger(new RuleStarshipCalculateHitChances(starship, target, ability.StarshipWeapon));
			hitLocationRule = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(starship, target));
			defaultInstances = weapon.Blueprint.DamageInstances;
			SingleRun(-20, damagePredictionData2, shieldDamageData2);
			SingleRun(20 + hitChanceRule.ResultCritChance / 2, damagePredictionData, shieldDamageData);
			damagePredictionData.MinDamage = damagePredictionData2.MinDamage;
			shieldDamageData.MinDamage = shieldDamageData2.MinDamage;
		}
		return (resultDamage: damagePredictionData, resultShields: shieldDamageData);
		void SingleRun(int delta, DamagePredictionData resultDamage, ShieldDamageData resultShields)
		{
			RuleStarshipPerformAttack[] array = new RuleStarshipPerformAttack[Math.Clamp((defaultInstances * hitChanceRule.ResultHitChance * (100 + delta) / 100 + 50) / 100, 0, defaultInstances * 3 / 2)];
			RuleStarshipPerformAttack ruleStarshipPerformAttack = null;
			RuleStarshipPerformAttack ruleStarshipPerformAttack2 = null;
			for (int i = 0; i < array.Length; i++)
			{
				RuleStarshipPerformAttack ruleStarshipPerformAttack3 = (array[i] = new RuleStarshipPerformAttack(starship, target, ability, ability.StarshipWeapon, hitLocationRule));
				if (ruleStarshipPerformAttack == null)
				{
					ruleStarshipPerformAttack = ruleStarshipPerformAttack3;
				}
				ruleStarshipPerformAttack3.FirstAttackInBurst = ruleStarshipPerformAttack;
				if (ruleStarshipPerformAttack2 != null)
				{
					ruleStarshipPerformAttack2.NextAttackInBurst = ruleStarshipPerformAttack3;
				}
				ruleStarshipPerformAttack2 = ruleStarshipPerformAttack3;
			}
			RuleStarshipPerformAttack[] array2 = array;
			foreach (RuleStarshipPerformAttack obj in array2)
			{
				obj.EvaluatePrediction();
				RuleStarshipCalculateDamageForTarget calculateDamageRule = obj.CalculateDamageRule;
				if (calculateDamageRule != null)
				{
					resultDamage.MinDamage += calculateDamageRule.ResultMinDamage;
					resultDamage.MaxDamage += calculateDamageRule.ResultMaxDamage;
				}
				RuleStarshipRollShieldAbsorption shieldAbsorptionRollRule = obj.ShieldAbsorptionRollRule;
				if (shieldAbsorptionRollRule != null)
				{
					resultShields.MinDamage += shieldAbsorptionRollRule.ResultAbsorbedOnFail;
					resultShields.MaxDamage += shieldAbsorptionRollRule.ResultAbsorbedDamage;
				}
				if (obj.NextAttackInBurst == null && shieldAbsorptionRollRule != null)
				{
					resultShields.CurrentShield = shieldAbsorptionRollRule.ResultShields;
					resultShields.MaxShield = shieldAbsorptionRollRule.ResultMaxShields;
				}
			}
		}
	}

	public static int GetShipHitChancePrediction(this AbilityData ability, [CanBeNull] StarshipEntity caster, [CanBeNull] StarshipEntity target)
	{
		if (caster == null || target == null || ability.StarshipWeapon == null)
		{
			return 0;
		}
		return new RuleStarshipCalculateHitChances(caster, target, ability.StarshipWeapon).ResultHitChance;
	}

	private static DamagePredictionData GetActionsDamage([NotNull] AbilityData ability, [NotNull] ActionList actions, [CanBeNull] AbilityExecutionContext context, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		DamagePredictionData damagePredictionData = null;
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			DamagePredictionData damagePredictionData2 = null;
			if (gameAction is Conditional conditional)
			{
				if (context == null)
				{
					context = ability.CreateExecutionContext(target, casterPosition);
				}
				using (ContextData<MechanicsContext.Data>.Request().Setup(context, target))
				{
					bool flag = false;
					using (context.GetDataScope(target))
					{
						flag = conditional.ConditionsChecker.Check();
					}
					damagePredictionData2 = (flag ? GetActionsDamage(ability, conditional.IfTrue, context, casterPosition, target) : GetActionsDamage(ability, conditional.IfFalse, context, casterPosition, target));
				}
			}
			else if (gameAction is ContextActionDealDamage contextActionDealDamage)
			{
				if (context == null)
				{
					context = ability.CreateExecutionContext(target, casterPosition);
				}
				using (ContextData<MechanicsContext.Data>.Request().Setup(context, target))
				{
					damagePredictionData2 = contextActionDealDamage.GetDamagePrediction(context, target.Entity);
				}
			}
			else if (gameAction is ContextActionSavingThrow contextActionSavingThrow)
			{
				if (context == null)
				{
					context = ability.CreateExecutionContext(target, casterPosition);
				}
				using (ContextData<MechanicsContext.Data>.Request().Setup(context, target))
				{
					DamagePredictionData actionsDamage;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 0, ability.Caster)))
					{
						actionsDamage = GetActionsDamage(ability, contextActionSavingThrow.Actions, context, casterPosition, target);
					}
					DamagePredictionData actionsDamage2;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 100, ability.Caster)))
					{
						actionsDamage2 = GetActionsDamage(ability, contextActionSavingThrow.Actions, context, casterPosition, target);
					}
					damagePredictionData2 = DamagePredictionData.Merge(actionsDamage, actionsDamage2);
				}
			}
			else if (gameAction is ContextActionConditionalSaved contextActionConditionalSaved)
			{
				if (context == null)
				{
					context = ability.CreateExecutionContext(target, casterPosition);
				}
				using (ContextData<MechanicsContext.Data>.Request().Setup(context, target))
				{
					DamagePredictionData actionsDamage3 = GetActionsDamage(ability, contextActionConditionalSaved.Failed, context, casterPosition, target);
					DamagePredictionData actionsDamage4 = GetActionsDamage(ability, contextActionConditionalSaved.Succeed, context, casterPosition, target);
					damagePredictionData2 = DamagePredictionData.Merge(actionsDamage3, actionsDamage4);
				}
			}
			else if (gameAction is ContextActionAttackWithFirstWeaponAbility contextActionAttackWithFirstWeaponAbility)
			{
				if (context == null)
				{
					context = ability.CreateExecutionContext(target, casterPosition);
				}
				using (ContextData<MechanicsContext.Data>.Request().Setup(context, target))
				{
					damagePredictionData2 = contextActionAttackWithFirstWeaponAbility.GetDamagePrediction(context, target.Entity, casterPosition);
				}
			}
			if (damagePredictionData2 != null)
			{
				if (damagePredictionData == null)
				{
					damagePredictionData = new DamagePredictionData();
				}
				damagePredictionData += damagePredictionData2;
			}
		}
		return damagePredictionData;
	}

	public static void GatherAffectedTargetsData(this AbilityData ability, OrientedPatternData pattern, Vector3 casterPosition, [CanBeNull] TargetWrapper clickedTarget, in List<AbilityTargetUIData> listToFill, [CanBeNull] MechanicEntity targetEntity = null)
	{
		if (clickedTarget == null)
		{
			return;
		}
		AbilityExecutionContext context = ability.CreateExecutionContext(clickedTarget);
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if ((targetEntity == null || targetEntity == allBaseAwakeUnit) && !allBaseAwakeUnit.IsExtra && CheckAffectedEntity(context, pattern, casterPosition, allBaseAwakeUnit, out var uiData))
			{
				listToFill.Add(uiData);
				ObjectExtensions.Or(AbilityTargetUIDataCache.Instance, null)?.AddOrReplace(uiData);
			}
		}
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			if ((targetEntity == null || targetEntity == destructibleEntity) && CheckAffectedEntity(context, pattern, casterPosition, destructibleEntity, out var uiData2))
			{
				listToFill.Add(uiData2);
				ObjectExtensions.Or(AbilityTargetUIDataCache.Instance, null)?.AddOrReplace(uiData2);
			}
		}
	}

	private static bool IsEntityAffected(AbilityExecutionContext context, MechanicEntity entity, OrientedPatternData pattern, Vector3? desiredPosition = null)
	{
		foreach (CustomGridNodeBase item in desiredPosition.HasValue ? entity.GetOccupiedNodes(desiredPosition.Value) : entity.GetOccupiedNodes())
		{
			if (pattern.Contains(item))
			{
				return true;
			}
		}
		return context.GetAdditionalTargets().Contains(entity);
	}

	private static IEnumerable<MechanicEntity> GetAdditionalTargets(this AbilityExecutionContext context)
	{
		using (context.GetDataScope())
		{
			return ((context.Ability.Blueprint.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.OfType<ContextActionOnOathOfVengeanceEnemies>().FirstOrDefault())?.GetTargets()).EmptyIfNull();
		}
	}

	private static bool CheckAffectedEntity(AbilityExecutionContext context, OrientedPatternData pattern, Vector3 casterPosition, MechanicEntity entity, out AbilityTargetUIData uiData)
	{
		AbilityData ability = context.Ability;
		if (!IsEntityAffected(context, entity, pattern, (ability.Caster == entity) ? new Vector3?(casterPosition) : null))
		{
			uiData = default(AbilityTargetUIData);
			return false;
		}
		if (ability.IsAOE || ability.IsCharge || ability.IsSingleShot || ability.IsStarshipAttack)
		{
			uiData = new AbilityTargetUIData(ability, entity, casterPosition);
			return true;
		}
		float num = 0f;
		float num2 = 0f;
		int num3 = 1;
		bool flag = false;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		List<float> list = TempList.Get<float>();
		NodeList occupiedNodes = entity.GetOccupiedNodes();
		foreach (CustomGridNodeBase item in occupiedNodes)
		{
			if (!pattern.TryGet(item, out var data))
			{
				continue;
			}
			if (ability.IsScatter)
			{
				num += data.ProbabilitiesSum;
				num2 += data.InitialAverageProbability;
				if (data.Lines > num3)
				{
					num3 = data.Lines;
				}
			}
			flag = flag || data.AlwaysHit;
			num4 += data.DodgeProbability;
			num5 += data.CoverProbability;
			num6 += data.EvasionProbability;
			for (int i = 0; i < data.InitialProbabilities.Length; i++)
			{
				float num7 = data.InitialProbabilities[i];
				if (list.Count <= i)
				{
					list.Add(num7 * 100f);
				}
				else
				{
					list[i] += num7 * 100f;
				}
			}
		}
		DamagePredictionData damagePrediction = ability.GetDamagePrediction(entity, casterPosition, context);
		int num8 = Math.Max(1, occupiedNodes.Count());
		num = Mathf.Clamp01(num) * 100f;
		num2 = Mathf.Clamp01(num2) * 100f;
		num4 = Mathf.Clamp01(num4 / (float)num8) * 100f;
		num5 = Mathf.Clamp01(num5 / (float)num8) * 100f;
		num6 = Mathf.Clamp01(num6 / (float)num8) * 100f;
		int minDamage = damagePrediction?.MinDamage ?? 0;
		int maxDamage = damagePrediction?.MaxDamage ?? 0;
		uiData = new AbilityTargetUIData(ability, entity, casterPosition, flag, num2, num, minDamage, maxDamage, num3, ability.BurstAttacksCount, list, num4, num5, num6);
		return true;
	}
}
