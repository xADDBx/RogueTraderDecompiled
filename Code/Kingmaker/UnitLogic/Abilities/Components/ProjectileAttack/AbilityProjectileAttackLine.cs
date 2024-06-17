using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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

		public bool Empty => Node == null;

		public HitData([NotNull] CustomGridNodeBase node, RulePerformAttackRoll performAttackRoll)
		{
			this = default(HitData);
			Node = node;
			RollPerformAttackRule = performAttackRoll;
		}
	}

	private readonly List<CustomGridNodeBase> m_Nodes;

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

	public AbilityProjectileAttackLine(AbilityProjectileAttack projectileAttack, int index, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, List<CustomGridNodeBase> nodes, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false)
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
		bool flag = false;
		DamageData damageData = null;
		foreach (var item4 in EnumerateTargets())
		{
			if (Game.Instance.TurnController.TbActive && !(item4.Entity is BaseUnitEntity) && item4.Entity != PriorityTarget && !item4.Entity.CanBeAttackedDirectly)
			{
				continue;
			}
			flag |= ProjectileAttack.IsControlledScatter && item4.Entity.IsAlly(Context.Caster);
			RulePerformAttackRoll rulePerformAttackRoll = new RulePerformAttackRoll(Context.Caster, item4.Entity, Context.Ability, Index, DodgeForAllyDisabled, FromNode.Vector3Position, item4.Node.Vector3Position, damageData?.EffectiveOverpenetrationFactor ?? 1f)
			{
				IsControlledScatterAutoMiss = flag,
				IsOverpenetration = (damageData != null)
			};
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(ProjectileAttack.AttackHitPolicy))
			{
				Rulebook.Trigger(rulePerformAttackRoll);
			}
			if (rulePerformAttackRoll != null && rulePerformAttackRoll.ResultIsCoverHit)
			{
				LosDescription item = item4.Los;
				MechanicEntity obstacleEntity = item.ObstacleEntity;
				if (obstacleEntity == null)
				{
					item = item4.Los;
					if (item.ObstacleNode != null)
					{
						item = item4.Los;
						list.Add(new HitData(item.ObstacleNode, rulePerformAttackRoll)
						{
							IsOverpenetration = (damageData != null)
						});
					}
					break;
				}
				MechanicEntity caster = Context.Caster;
				AbilityData ability = Context.Ability;
				RulePerformAttackRoll performAttackRoll = ((damageData == null) ? rulePerformAttackRoll : null);
				DamageData baseDamageOverride = damageData;
				bool calculatedOverpenetration = damageData != null;
				RuleCalculateDamage ruleCalculateDamage = new RuleCalculateDamage(caster, obstacleEntity, ability, performAttackRoll, baseDamageOverride, null, null, forceCrit: false, calculatedOverpenetration);
				Rulebook.Trigger(ruleCalculateDamage);
				RuleRollDamage ruleRollDamage = new RuleRollDamage(Context.Caster, obstacleEntity, ruleCalculateDamage.ResultDamage);
				Rulebook.Trigger(ruleRollDamage);
				list.Add(new HitData(item4.Node, rulePerformAttackRoll)
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
			if (rulePerformAttackRoll.ResultIsHit)
			{
				MechanicEntity caster2 = Context.Caster;
				MechanicEntity item2 = item4.Entity;
				AbilityData ability2 = Context.Ability;
				RulePerformAttackRoll performAttackRoll2 = ((damageData == null) ? rulePerformAttackRoll : null);
				DamageData baseDamageOverride2 = damageData;
				bool calculatedOverpenetration = damageData != null;
				RuleCalculateDamage ruleCalculateDamage2 = new RuleCalculateDamage(caster2, item2, ability2, performAttackRoll2, baseDamageOverride2, null, null, forceCrit: false, calculatedOverpenetration);
				Rulebook.Trigger(ruleCalculateDamage2);
				RuleRollDamage ruleRollDamage2 = new RuleRollDamage(Context.Caster, item4.Entity, ruleCalculateDamage2.ResultDamage);
				Rulebook.Trigger(ruleRollDamage2);
				list.Add(new HitData(item4.Node, rulePerformAttackRoll)
				{
					Entity = item4.Entity,
					RollDamageRule = ruleRollDamage2,
					IsOverpenetration = (damageData != null)
				});
				if (ruleRollDamage2.ResultOverpenetration == null)
				{
					break;
				}
				damageData = ruleRollDamage2.ResultOverpenetration;
			}
			else
			{
				HitData item3 = new HitData(item4.Node, rulePerformAttackRoll);
				(item3.Entity, _, _) = item4;
				list.Add(item3);
			}
		}
		return list;
	}

	private IEnumerable<(MechanicEntity Entity, LosDescription Los, CustomGridNodeBase Node)> EnumerateTargets()
	{
		List<MechanicEntity> targets = TempList.Get<MechanicEntity>();
		foreach (CustomGridNodeBase node in m_Nodes)
		{
			if (AbilityProjectileAttackLineHelper.IsNodeAffected(null, FromNode, node, StepHeight))
			{
				MechanicEntity targetByNode = GetTargetByNode(node);
				if (targetByNode != null && targetByNode != Context.Caster && Context.Ability.IsValidTargetForAttack(targetByNode) && !targets.Contains(targetByNode))
				{
					MechanicEntity caster = ProjectileAttack.Context.Caster;
					LosDescription warhammerLos = LosCalculations.GetWarhammerLos(FromNode, caster.SizeRect, node, targetByNode.SizeRect);
					targets.Add(targetByNode);
					yield return (Entity: targetByNode, Los: warhammerLos, Node: node);
				}
			}
		}
	}

	[CanBeNull]
	private static MechanicEntity GetTargetByNode(CustomGridNodeBase node)
	{
		BaseUnitEntity unit = node.GetUnit();
		if (unit != null)
		{
			return unit;
		}
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			if (destructibleEntity.GetOccupiedNodes().Contains(node))
			{
				return destructibleEntity;
			}
		}
		return null;
	}
}
