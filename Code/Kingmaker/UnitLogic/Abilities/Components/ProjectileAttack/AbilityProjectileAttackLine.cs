using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Block;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class AbilityProjectileAttackLine
{
	public struct HitData
	{
		[NotNull]
		public readonly CustomGridNodeBase Node;

		[NotNull]
		public readonly RulePerformAttackRoll RollPerformAttackRule;

		public LosDescription Los;

		[CanBeNull]
		public MechanicEntity Entity;

		[CanBeNull]
		public RuleRollDamage RollDamageRule;

		public bool IsOverpenetration;

		public bool IsRedirecting;

		public bool IsBlocking => (RollPerformAttackRule?.ResultBlockRule?.Result).GetValueOrDefault();

		public bool Empty => Node == null;

		public HitData([NotNull] CustomGridNodeBase node, RulePerformAttackRoll performAttackRoll)
		{
			this = default(HitData);
			Node = node;
			RollPerformAttackRule = performAttackRoll;
		}
	}

	private readonly ReadonlyList<CustomGridNodeBase> m_Nodes;

	private readonly IEnumerator<AbilityDeliveryTarget> m_DeliveryProcess;

	public readonly AbilityProjectileAttack ProjectileAttack;

	public readonly int Index;

	public readonly CustomGridNodeBase FromNode;

	public readonly CustomGridNodeBase ToNode;

	public readonly float StepHeight;

	public Projectile Projectile { get; set; }

	public bool IsFinished { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	public AbilityExecutionContext Context => ProjectileAttack.Context;

	[CanBeNull]
	public AbilityDeliveryTarget CurrentTarget => m_DeliveryProcess?.Current;

	public ReadonlyList<CustomGridNodeBase> Nodes => m_Nodes;

	[CanBeNull]
	public MechanicEntity PriorityTarget => ProjectileAttack.PriorityTarget;

	public bool BlockHasTriggered { get; private set; }

	public AbilityProjectileAttackLine(AbilityProjectileAttack projectileAttack, int index, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, ReadonlyList<CustomGridNodeBase> nodes, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false)
	{
		Index = index;
		FromNode = fromNode;
		ToNode = toNode;
		m_Nodes = nodes;
		WeaponAttackDamageDisabled = disableWeaponAttackDamage;
		DodgeForAllyDisabled = disableDodgeForAlly;
		ProjectileAttack = projectileAttack;
		StepHeight = AbilityProjectileAttackLineHelper.GetStepHeightBetweenCells(FromNode, ToNode);
		BlueprintProjectile projectileBlueprint = Context.Ability?.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities);
		m_DeliveryProcess = AbilityProjectileAttackLineHelper.DeliverLine(Context, projectileBlueprint, this);
	}

	public bool Tick()
	{
		bool flag = !IsFinished && (m_DeliveryProcess?.MoveNext() ?? false);
		IsFinished = !flag;
		return flag;
	}

	public IEnumerable<HitData> CalculateHits()
	{
		List<HitData> list = TempList.Get<HitData>();
		BlockHasTriggered = false;
		bool flag = false;
		DamageData damageData = null;
		foreach (var item3 in EnumerateTargets())
		{
			if (Game.Instance.TurnController.TbActive && !(item3.Entity is BaseUnitEntity) && item3.Entity != PriorityTarget && !item3.Entity.CanBeAttackedDirectly)
			{
				continue;
			}
			flag |= ProjectileAttack.IsControlledScatter && item3.Entity.IsAlly(Context.Caster);
			RulePerformAttackRoll rulePerformAttackRoll = new RulePerformAttackRoll(Context.Caster, item3.Entity, Context.Ability, Index, DodgeForAllyDisabled, FromNode.Vector3Position, item3.Node.Vector3Position, damageData?.EffectiveOverpenetrationFactor ?? 1f)
			{
				IsControlledScatterAutoMiss = flag,
				IsOverpenetration = (damageData != null),
				IsBlockPreviewScatterHit = Context.IsUnitBlockingAttack(item3.Entity as UnitEntity)
			};
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(ProjectileAttack.AttackHitPolicy))
			{
				Rulebook.Trigger(rulePerformAttackRoll);
			}
			bool calculatedOverpenetration;
			OverpenetrationData? overpenetrationData;
			if (rulePerformAttackRoll != null && rulePerformAttackRoll.ResultIsCoverHit)
			{
				LosDescription item = item3.Los;
				MechanicEntity obstacleEntity = item.ObstacleEntity;
				if (obstacleEntity == null)
				{
					item = item3.Los;
					if (item.ObstacleNode != null)
					{
						item = item3.Los;
						list.Add(new HitData(item.ObstacleNode, rulePerformAttackRoll)
						{
							IsOverpenetration = (damageData != null)
						});
					}
					break;
				}
				MechanicEntity caster = Context.Caster;
				AbilityData ability = Context.Ability;
				calculatedOverpenetration = damageData != null;
				overpenetrationData = ((damageData == null) ? null : new OverpenetrationData?(new OverpenetrationData
				{
					OverpenetrationPercent = damageData.OverpenetrationFactorPercents,
					DamageRoll = damageData.Roll,
					MinBaseValue = damageData.MinValueBase,
					MaxBaseValue = damageData.MaxValueBase
				}));
				RuleCalculateDamage ruleCalculateDamage = new CalculateDamageParams(caster, obstacleEntity, ability, rulePerformAttackRoll, null, null, null, overpenetrationData, forceCrit: false, calculatedOverpenetration).Trigger();
				RuleRollDamage ruleRollDamage = new RuleRollDamage(Context.Caster, obstacleEntity, ruleCalculateDamage.ResultDamage);
				Rulebook.Trigger(ruleRollDamage);
				list.Add(new HitData(item3.Node, rulePerformAttackRoll)
				{
					Entity = obstacleEntity,
					RollDamageRule = ruleRollDamage,
					IsOverpenetration = (damageData != null)
				});
				if (ruleRollDamage.ResultOverpenetration == null)
				{
					break;
				}
				damageData = ruleRollDamage.ResultOverpenetration;
			}
			RuleRollParry resultParryRule;
			if (!rulePerformAttackRoll.ResultIsHit)
			{
				resultParryRule = rulePerformAttackRoll.ResultParryRule;
				if (resultParryRule == null || !resultParryRule.Result)
				{
					HitData item2 = new HitData(item3.Node, rulePerformAttackRoll);
					(item2.Entity, _, _) = item3;
					list.Add(item2);
					goto IL_052e;
				}
			}
			MechanicEntity actualParryUnit = rulePerformAttackRoll.ActualParryUnit;
			resultParryRule = rulePerformAttackRoll.ResultParryRule;
			if (resultParryRule != null && resultParryRule.Result && actualParryUnit.HasMechanicFeature(MechanicsFeatureType.RangedParry))
			{
				MechanicEntity caster2 = Context.Caster;
				AbilityData ability2 = Context.Ability;
				calculatedOverpenetration = damageData != null;
				RuleCalculateDamage ruleCalculateDamage2 = new CalculateDamageParams(caster2, actualParryUnit, ability2, rulePerformAttackRoll, null, null, null, null, forceCrit: false, calculatedOverpenetration).Trigger();
				Rulebook.Trigger(new RuleRollDamage(Context.Caster, actualParryUnit, ruleCalculateDamage2.ResultDamage));
				list.Add(new HitData(item3.Node, rulePerformAttackRoll)
				{
					Entity = actualParryUnit,
					RollDamageRule = null,
					IsOverpenetration = (damageData != null),
					IsRedirecting = (rulePerformAttackRoll.ResultParryRule?.DeflectResult ?? false)
				});
				return list;
			}
			MechanicEntity caster3 = Context.Caster;
			AbilityData ability3 = Context.Ability;
			calculatedOverpenetration = damageData != null;
			overpenetrationData = ((damageData == null) ? null : new OverpenetrationData?(new OverpenetrationData
			{
				OverpenetrationPercent = damageData.OverpenetrationFactorPercents,
				DamageRoll = damageData.Roll,
				MinBaseValue = damageData.MinValueBase,
				MaxBaseValue = damageData.MaxValueBase
			}));
			RuleCalculateDamage ruleCalculateDamage3 = new CalculateDamageParams(caster3, actualParryUnit, ability3, rulePerformAttackRoll, null, null, null, overpenetrationData, forceCrit: false, calculatedOverpenetration).Trigger();
			RuleRollDamage ruleRollDamage2 = new RuleRollDamage(Context.Caster, actualParryUnit, ruleCalculateDamage3.ResultDamage);
			Rulebook.Trigger(ruleRollDamage2);
			list.Add(new HitData((CustomGridNodeBase)actualParryUnit.CurrentNode.node, rulePerformAttackRoll)
			{
				Entity = actualParryUnit,
				RollDamageRule = ruleRollDamage2,
				IsOverpenetration = (damageData != null)
			});
			if (ruleRollDamage2.ResultOverpenetration == null)
			{
				break;
			}
			damageData = ruleRollDamage2.ResultOverpenetration;
			goto IL_052e;
			IL_052e:
			RuleRollBlock resultBlockRule = rulePerformAttackRoll.ResultBlockRule;
			if (resultBlockRule != null && resultBlockRule.Result)
			{
				Context.AddUnitBlockingAttack(item3.Entity as UnitEntity);
				BlockHasTriggered = true;
				break;
			}
		}
		return list;
	}

	private IEnumerable<(MechanicEntity Entity, LosDescription Los, CustomGridNodeBase Node)> EnumerateTargets()
	{
		List<MechanicEntity> targets = TempList.Get<MechanicEntity>();
		foreach (CustomGridNodeBase node in m_Nodes)
		{
			if (!AbilityProjectileAttackLineHelper.IsNodeAffected(Context.Ability, FromNode, node, StepHeight))
			{
				continue;
			}
			MechanicEntity targetByNode = GetTargetByNode(node, Context.Ability);
			if (targetByNode == null || targets.Contains(targetByNode))
			{
				continue;
			}
			if (targetByNode == Context.Caster && !targetByNode.Facts.HasComponent<WarhammerDeflectionTarget>())
			{
				AbilityDeliveryTarget currentTarget = ProjectileAttack.CurrentTarget;
				if (currentTarget == null || currentTarget.AttackRule?.Result != AttackResult.Parried)
				{
					continue;
				}
			}
			MechanicEntity caster = ProjectileAttack.Context.Caster;
			LosDescription warhammerLos = LosCalculations.GetWarhammerLos(FromNode, caster.SizeRect, node, targetByNode.SizeRect);
			targets.Add(targetByNode);
			yield return (Entity: targetByNode, Los: warhammerLos, Node: node);
		}
	}

	[CanBeNull]
	private static MechanicEntity GetTargetByNode(CustomGridNodeBase node, AbilityData abilityData)
	{
		BaseUnitEntity baseUnitEntity = node.GetAllUnits().FirstOrDefault(abilityData.IsValidTargetForAttack);
		if (baseUnitEntity != null)
		{
			return baseUnitEntity;
		}
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			if (destructibleEntity.GetOccupiedNodes().Contains(node) && abilityData.IsValidTargetForAttack(destructibleEntity))
			{
				return destructibleEntity;
			}
		}
		return null;
	}
}
