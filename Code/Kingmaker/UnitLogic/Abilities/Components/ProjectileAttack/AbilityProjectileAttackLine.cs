using System.Collections.Generic;
using System.Linq;
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
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class AbilityProjectileAttackLine
{
	public readonly struct HitData
	{
		[NotNull]
		public readonly CustomGridNodeBase Node;

		[NotNull]
		public readonly RulePerformAttackRoll RollPerformAttackRule;

		[CanBeNull]
		public readonly RuleRollDamage RollDamageRule;

		[CanBeNull]
		public readonly MechanicEntity Entity;

		public readonly LosDescription Los;

		public bool IsOverpenetration => (RollDamageRule?.ResultOverpenetration?.Overpenetrating).GetValueOrDefault();

		public bool IsRicochet => (RollDamageRule?.ResultOverpenetration?.IsRicochet).GetValueOrDefault();

		public bool IsRedirecting => (RollPerformAttackRule?.ResultParryRule?.DeflectResult).GetValueOrDefault();

		public bool IsBlocking => (RollPerformAttackRule?.ResultBlockRule?.Result).GetValueOrDefault();

		public bool Empty => Node == null;

		public HitData([NotNull] CustomGridNodeBase node, [NotNull] RulePerformAttackRoll performAttackRoll, [CanBeNull] RuleRollDamage rollDamageRule, [CanBeNull] MechanicEntity entity)
		{
			this = default(HitData);
			Node = node;
			RollPerformAttackRule = performAttackRoll;
			RollDamageRule = rollDamageRule;
			Entity = entity;
		}

		public HitData([NotNull] CustomGridNodeBase node, [NotNull] RulePerformAttackRoll performAttackRoll, [CanBeNull] RuleRollDamage rollDamageRule = null)
			: this(node, performAttackRoll, rollDamageRule, null)
		{
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

	public DamageData OverpenetrationDamage { get; private set; }

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

	private HashSet<MechanicEntity> HitEntities { get; set; }

	public AbilityProjectileAttackLine(AbilityProjectileAttack projectileAttack, int index, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, ReadonlyList<CustomGridNodeBase> nodes, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false, DamageData overPenetrationData = null, HashSet<MechanicEntity> hitEntities = null)
	{
		Index = index;
		FromNode = fromNode;
		ToNode = toNode;
		m_Nodes = nodes;
		WeaponAttackDamageDisabled = disableWeaponAttackDamage;
		DodgeForAllyDisabled = disableDodgeForAlly;
		ProjectileAttack = projectileAttack;
		OverpenetrationDamage = overPenetrationData;
		HitEntities = hitEntities;
		StepHeight = AbilityProjectileAttackLineHelper.GetStepHeightBetweenCells(FromNode, ToNode);
		if (Context.PassedAreaEffects == null)
		{
			RuleCalculatePassedAreaEffects evt = new RuleCalculatePassedAreaEffects(Context.Caster, fromNode, toNode);
			Context.PassedAreaEffects = Rulebook.Trigger(evt).PassedAreas;
		}
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
		foreach (var item7 in EnumerateTargets())
		{
			if (Game.Instance.TurnController.TbActive && !(item7.Entity is BaseUnitEntity) && item7.Entity != PriorityTarget && !item7.Entity.CanBeAttackedDirectly)
			{
				continue;
			}
			flag |= ProjectileAttack.IsControlledScatter && item7.Entity.IsAlly(Context.Caster);
			RulePerformAttackRoll rulePerformAttackRoll = new RulePerformAttackRoll(Context.Caster, item7.Entity, Context.Ability, Index, DodgeForAllyDisabled, FromNode.Vector3Position, item7.Node.Vector3Position, OverpenetrationDamage?.EffectiveOverpenetrationFactor ?? 1f)
			{
				IsControlledScatterAutoMiss = flag,
				IsOverpenetration = (OverpenetrationDamage != null),
				IsRicochet = (OverpenetrationDamage?.IsRicochet ?? false),
				IsBlockPreviewScatterHit = Context.IsUnitBlockingAttack(item7.Entity as UnitEntity)
			};
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(ProjectileAttack.AttackHitPolicy))
			{
				Rulebook.Trigger(rulePerformAttackRoll);
			}
			OverpenetrationData? overpenetrationData = ((OverpenetrationDamage == null) ? null : new OverpenetrationData?(new OverpenetrationData(OverpenetrationDamage)));
			bool calculatedOverpenetration;
			OverpenetrationData? overpenetrationData2;
			if (rulePerformAttackRoll != null && rulePerformAttackRoll.ResultIsCoverHit)
			{
				LosDescription item = item7.Los;
				MechanicEntity obstacleEntity = item.ObstacleEntity;
				GetOrCreateHitEntities().Add(item7.Entity);
				if (obstacleEntity == null)
				{
					item = item7.Los;
					if (item.ObstacleNode != null)
					{
						MechanicEntity caster = Context.Caster;
						MechanicEntity item2 = item7.Entity;
						AbilityData ability = Context.Ability;
						calculatedOverpenetration = OverpenetrationDamage != null;
						overpenetrationData2 = overpenetrationData;
						RuleCalculateDamage ruleCalculateDamage = new CalculateDamageParams(caster, item2, ability, rulePerformAttackRoll, null, null, null, overpenetrationData2, forceCrit: false, calculatedOverpenetration).Trigger();
						RuleRollDamage ruleRollDamage = new RuleRollDamage(Context.Caster, item7.Entity, ruleCalculateDamage.ResultDamage, ProjectileAttack.OverpenetrationDisabled);
						Rulebook.Trigger(ruleRollDamage);
						if (ruleRollDamage.ResultOverpenetration != null)
						{
							OverpenetrationDamage = ruleRollDamage.ResultOverpenetration;
							rulePerformAttackRoll.IsOverpenetration = true;
							rulePerformAttackRoll.IsRicochet = ruleRollDamage.ResultOverpenetration.IsRicochet;
						}
						item = item7.Los;
						list.Add(new HitData(item.ObstacleNode, rulePerformAttackRoll, ruleRollDamage));
					}
					break;
				}
				MechanicEntity caster2 = Context.Caster;
				AbilityData ability2 = Context.Ability;
				calculatedOverpenetration = OverpenetrationDamage != null;
				overpenetrationData2 = overpenetrationData;
				RuleCalculateDamage ruleCalculateDamage2 = new CalculateDamageParams(caster2, obstacleEntity, ability2, rulePerformAttackRoll, null, null, null, overpenetrationData2, forceCrit: false, calculatedOverpenetration).Trigger();
				RuleRollDamage ruleRollDamage2 = new RuleRollDamage(Context.Caster, obstacleEntity, ruleCalculateDamage2.ResultDamage, ProjectileAttack.OverpenetrationDisabled);
				Rulebook.Trigger(ruleRollDamage2);
				item = item7.Los;
				HitData item3 = new HitData(item.ObstacleNode, rulePerformAttackRoll, ruleRollDamage2, obstacleEntity);
				list.Add(item3);
				OverpenetrationDamage = ruleRollDamage2.ResultOverpenetration;
				if (OverpenetrationDamage == null || item3.IsRicochet)
				{
					break;
				}
				if (OverpenetrationDamage != null)
				{
					rulePerformAttackRoll.IsOverpenetration = true;
					rulePerformAttackRoll.IsRicochet = OverpenetrationDamage?.IsRicochet ?? false;
					overpenetrationData = new OverpenetrationData(OverpenetrationDamage);
				}
			}
			RuleRollParry resultParryRule;
			if (!rulePerformAttackRoll.ResultIsHit)
			{
				resultParryRule = rulePerformAttackRoll.ResultParryRule;
				if (resultParryRule == null || !resultParryRule.Result)
				{
					MechanicEntity caster3 = Context.Caster;
					MechanicEntity item4 = item7.Entity;
					AbilityData ability3 = Context.Ability;
					calculatedOverpenetration = OverpenetrationDamage != null;
					overpenetrationData2 = overpenetrationData;
					RuleCalculateDamage ruleCalculateDamage3 = new CalculateDamageParams(caster3, item4, ability3, rulePerformAttackRoll, null, null, null, overpenetrationData2, forceCrit: false, calculatedOverpenetration).Trigger();
					RuleRollDamage ruleRollDamage3 = new RuleRollDamage(Context.Caster, item7.Entity, ruleCalculateDamage3.ResultDamage, ProjectileAttack.OverpenetrationDisabled);
					Rulebook.Trigger(ruleRollDamage3);
					AttackResult result = rulePerformAttackRoll.Result;
					bool flag2 = result == AttackResult.Miss || result == AttackResult.Dodge;
					bool flag3 = ruleRollDamage3.ResultRuleOverpenetration?.AllowRicochetWithoutHitting ?? false;
					HitData item5 = new HitData(item7.Node, rulePerformAttackRoll, (flag2 && !flag3) ? null : ruleRollDamage3, item7.Entity);
					list.Add(item5);
					if (!flag2 || flag3)
					{
						OverpenetrationDamage = ruleRollDamage3.ResultOverpenetration;
					}
					if (flag3 && flag2 && (OverpenetrationDamage == null || item5.IsRicochet))
					{
						break;
					}
					goto IL_06da;
				}
			}
			MechanicEntity actualParryUnit = rulePerformAttackRoll.ActualParryUnit;
			resultParryRule = rulePerformAttackRoll.ResultParryRule;
			if (resultParryRule != null && resultParryRule.Result && actualParryUnit.HasMechanicFeature(MechanicsFeatureType.RangedParry))
			{
				MechanicEntity caster4 = Context.Caster;
				AbilityData ability4 = Context.Ability;
				calculatedOverpenetration = OverpenetrationDamage != null;
				RuleCalculateDamage ruleCalculateDamage4 = new CalculateDamageParams(caster4, actualParryUnit, ability4, rulePerformAttackRoll, null, null, null, null, forceCrit: false, calculatedOverpenetration).Trigger();
				RuleRollDamage ruleRollDamage4 = new RuleRollDamage(Context.Caster, actualParryUnit, ruleCalculateDamage4.ResultDamage, ProjectileAttack.OverpenetrationDisabled);
				Rulebook.Trigger(ruleRollDamage4);
				list.Add(new HitData(item7.Node, rulePerformAttackRoll, ruleRollDamage4, actualParryUnit));
				if (list.Last().IsRicochet)
				{
					OverpenetrationDamage = ruleRollDamage4.ResultOverpenetration;
				}
				return list;
			}
			MechanicEntity caster5 = Context.Caster;
			AbilityData ability5 = Context.Ability;
			calculatedOverpenetration = OverpenetrationDamage != null;
			overpenetrationData2 = overpenetrationData;
			RuleCalculateDamage ruleCalculateDamage5 = new CalculateDamageParams(caster5, actualParryUnit, ability5, rulePerformAttackRoll, null, null, null, overpenetrationData2, forceCrit: false, calculatedOverpenetration).Trigger();
			RuleRollDamage ruleRollDamage5 = new RuleRollDamage(Context.Caster, actualParryUnit, ruleCalculateDamage5.ResultDamage, ProjectileAttack.OverpenetrationDisabled);
			Rulebook.Trigger(ruleRollDamage5);
			HitData item6 = new HitData((CustomGridNodeBase)actualParryUnit.CurrentNode.node, rulePerformAttackRoll, ruleRollDamage5, actualParryUnit);
			list.Add(item6);
			OverpenetrationDamage = ruleRollDamage5.ResultOverpenetration;
			if (OverpenetrationDamage == null || item6.IsRicochet)
			{
				break;
			}
			goto IL_06da;
			IL_06da:
			RuleRollBlock resultBlockRule = rulePerformAttackRoll.ResultBlockRule;
			if (resultBlockRule != null && resultBlockRule.Result)
			{
				Context.AddUnitBlockingAttack(item7.Entity as UnitEntity);
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
			if (targetByNode == Context.Caster)
			{
				bool flag = targetByNode.Facts.GetComponents((EnableRicochet c) => !c.DisableFriendlyFire).Any();
				bool num = targetByNode.Facts.HasComponent<WarhammerDeflectionTarget>();
				AbilityDeliveryTarget currentTarget = ProjectileAttack.CurrentTarget;
				bool flag2 = currentTarget != null && currentTarget.AttackRule?.Result == AttackResult.Parried;
				if (!num && !flag2 && !flag)
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

	public HashSet<MechanicEntity> GetOrCreateHitEntities()
	{
		if (HitEntities == null)
		{
			HashSet<MechanicEntity> hashSet2 = (HitEntities = CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get());
		}
		return HitEntities;
	}

	public void ReleaseHitEntities()
	{
		if (HitEntities != null)
		{
			CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Release(HitEntities);
			HitEntities = null;
		}
	}
}
