using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class AbilityProjectileAttack : IEnumerator<AbilityDeliveryTarget>, IEnumerator, IDisposable
{
	[NotNull]
	private readonly IEnumerator<AbilityDeliveryTarget> m_Process;

	[NotNull]
	[ItemCanBeNull]
	private readonly AbilityProjectileAttackLine[] m_Attacks;

	[NotNull]
	public readonly AbilityExecutionContext Context;

	[CanBeNull]
	public readonly MechanicEntity PriorityTarget;

	public bool IsControlledScatter { get; }

	[NotNull]
	public List<(UnitEntity Unit, RulePerformDodge DodgeRule)> DodgedUnits { get; } = new List<(UnitEntity, RulePerformDodge)>();


	public bool AttacksDisabled { get; private set; }

	public bool OverpenetrationDisabled { get; private set; }

	public AttackHitPolicyType AttackHitPolicy { get; private set; }

	[CanBeNull]
	public AbilityDeliveryTarget CurrentTarget { get; private set; }

	public bool IsFinished { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	public int Count => m_Attacks.Length;

	public AbilityProjectileAttackLine this[int index] => m_Attacks[index];

	AbilityDeliveryTarget IEnumerator<AbilityDeliveryTarget>.Current => CurrentTarget;

	object IEnumerator.Current => CurrentTarget;

	public AbilityProjectileAttack(AbilityExecutionContext context, int shotsCount, bool controlledScatter)
	{
		Context = context;
		IsControlledScatter = controlledScatter;
		m_Attacks = new AbilityProjectileAttackLine[shotsCount];
		m_Process = CreateProcess();
	}

	public AbilityProjectileAttack(AbilityExecutionContext context, MechanicEntity priorityTarget, int shotsCount)
		: this(context, shotsCount, controlledScatter: false)
	{
		PriorityTarget = priorityTarget;
	}

	public static AbilityProjectileAttack CreateScatter(AbilityExecutionContext context, TargetWrapper target, int shotsCount, bool controlledScatter)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, shotsCount, controlledScatter);
		(CustomGridNodeBase, CustomGridNodeBase, List<CustomGridNodeBase>[]) tuple = CalculateLines(maybeCaster, target, context.Ability.RangeCells.Cells(), context.Ability);
		for (int i = 0; i < abilityProjectileAttack.Count; i++)
		{
			RuleRollScatterShotHitDirection ruleRollScatterShotHitDirection = new RuleRollScatterShotHitDirection(maybeCaster, context.Ability, i)
			{
				Reason = context
			};
			Rulebook.Trigger(ruleRollScatterShotHitDirection);
			List<CustomGridNodeBase> list = tuple.Item3.Get((int)ruleRollScatterShotHitDirection.Result);
			abilityProjectileAttack.SetupLine(i, tuple.Item1, tuple.Item2, list);
		}
		return abilityProjectileAttack;
	}

	public static AbilityProjectileAttack CreateSingleTarget(AbilityExecutionContext context, MechanicEntity priorityTarget, int shotsCount)
	{
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		if (priorityTarget == null)
		{
			PFLog.Default.ErrorWithReport("PriorityTarget is missing");
			return null;
		}
		(ReadonlyList<CustomGridNodeBase>, CustomGridNodeBase, CustomGridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, priorityTarget);
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, priorityTarget, shotsCount);
		for (int i = 0; i < abilityProjectileAttack.Count; i++)
		{
			abilityProjectileAttack.SetupLine(i, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3, singleShotAffectedNodes.Item1);
		}
		return abilityProjectileAttack;
	}

	public static (ReadonlyList<CustomGridNodeBase> Nodes, CustomGridNodeBase From, CustomGridNodeBase To) GetSingleShotAffectedNodes(AbilityData ability, MechanicEntity target)
	{
		int num = ability.RangeCells.Cells().Value;
		CustomGridNodeBase casterNode = ability.GetBestShootingPositionForDesiredPosition(target);
		CustomGridNodeBase customGridNodeBase = target.GetOccupiedNodes().FirstOrDefault((CustomGridNodeBase node) => LosCalculations.GetDirectLos(casterNode.Vector3Position, node.Vector3Position)) ?? target.Position.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase customGridNodeBase2 = (ability.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(casterNode, default(IntRect), customGridNodeBase, default(IntRect)) : casterNode);
		if (customGridNodeBase == customGridNodeBase2)
		{
			return (Nodes: TempList.Get<CustomGridNodeBase>(), From: customGridNodeBase2, To: customGridNodeBase);
		}
		if (customGridNodeBase != null)
		{
			int warhammerLength = CustomGraphHelper.GetWarhammerLength(customGridNodeBase.CoordinatesInGrid - customGridNodeBase2.CoordinatesInGrid);
			if (warhammerLength > num && WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, ability.Caster.SizeRect, ability.Caster.Forward, target.Position, target.SizeRect, target.Forward) <= num)
			{
				num = warhammerLength;
			}
		}
		return CollectNodes(customGridNodeBase2, customGridNodeBase, target, num);
	}

	private static (ReadonlyList<CustomGridNodeBase> Nodes, CustomGridNodeBase From, CustomGridNodeBase To) CollectNodes(CustomGridNodeBase fromNode, CustomGridNodeBase toNode, MechanicEntity target, int range)
	{
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(fromNode.CoordinatesInGrid, (toNode.Vector3Position - fromNode.Vector3Position).To2D());
		Linecast.Ray2Nodes ray2Nodes = new Linecast.Ray2Nodes((CustomGridGraph)fromNode.Graph, in offsets);
		NodeList occupiedNodes = target.GetOccupiedNodes();
		List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
		foreach (CustomGridNodeBase item in ray2Nodes)
		{
			if (item == null || CustomGraphHelper.GetWarhammerLength(item.CoordinatesInGrid - fromNode.CoordinatesInGrid) > range)
			{
				if (list.Count == 0)
				{
					return (Nodes: ReadonlyList<CustomGridNodeBase>.Empty, From: fromNode, To: toNode);
				}
				break;
			}
			if (occupiedNodes.Contains(item) || list.Count > 0)
			{
				list.Add(item);
			}
		}
		return (Nodes: list, From: fromNode, To: toNode);
	}

	public static (ReadonlyList<CustomGridNodeBase> Nodes, CustomGridNodeBase From, CustomGridNodeBase To) CollectNodes(CustomGridNodeBase fromNode, MechanicEntity target, int range)
	{
		CustomGridNodeBase customGridNodeBase = target.GetOccupiedNodes().FirstOrDefault((CustomGridNodeBase node) => LosCalculations.GetDirectLos(fromNode.Vector3Position, node.Vector3Position)) ?? target.Position.GetNearestNodeXZUnwalkable();
		if (customGridNodeBase == fromNode)
		{
			return (Nodes: TempList.Get<CustomGridNodeBase>(), From: fromNode, To: customGridNodeBase);
		}
		return CollectNodes(fromNode, customGridNodeBase, target, range);
	}

	private static (CustomGridNodeBase From, CustomGridNodeBase To, List<CustomGridNodeBase>[] Lines) CalculateLines(MechanicEntity caster, TargetWrapper target, Cells range, AbilityData abilityData)
	{
		CustomGridNodeBase casterNode = abilityData.GetBestShootingPositionForDesiredPosition(target);
		CustomGridNodeBase customGridNodeBase = ((!target.HasEntity) ? target.Point.GetNearestNodeXZUnwalkable() : (target.Entity.GetOccupiedNodes().FirstOrDefault((CustomGridNodeBase node) => LosCalculations.GetDirectLos(casterNode.Vector3Position, node.Vector3Position)) ?? target.Point.GetNearestNodeXZUnwalkable()));
		CustomGridNodeBase obj = (abilityData.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(casterNode, default(IntRect), customGridNodeBase, default(IntRect)) : casterNode);
		List<CustomGridNodeBase>[] item = GridPatterns.CalcScatterShot(obj, customGridNodeBase, range.Value);
		return (From: obj, To: customGridNodeBase, Lines: item);
	}

	public void DisableAttacks()
	{
		AttacksDisabled = true;
	}

	public void DisableOverpenetration()
	{
		OverpenetrationDisabled = true;
	}

	public void DisableWeaponAttackDamage()
	{
		WeaponAttackDamageDisabled = true;
	}

	public void DisableDodgeForAlly()
	{
		DodgeForAllyDisabled = true;
	}

	public void AutoHit()
	{
		AttackHitPolicy = AttackHitPolicyType.AutoHit;
	}

	public void SetupLine(int index, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, ReadonlyList<CustomGridNodeBase> nodes)
	{
		if (nodes.Empty())
		{
			PFLog.Default.ErrorWithReport("Projectile path does not contains any of PriorityTarget's occupied nodes");
		}
		else
		{
			m_Attacks[index] = new AbilityProjectileAttackLine(this, index, fromNode, toNode, nodes, WeaponAttackDamageDisabled, DodgeForAllyDisabled);
		}
	}

	private IEnumerator<AbilityDeliveryTarget> CreateProcess()
	{
		while (true)
		{
			int i = 0;
			while (i < Math.Min(Context.ActionIndex, Count))
			{
				AbilityProjectileAttackLine line = this[i];
				if (line != null && !line.IsFinished)
				{
					while (line.Tick() && line.CurrentTarget != null)
					{
						yield return line.CurrentTarget;
					}
				}
				int num = i + 1;
				i = num;
			}
			IsFinished = m_Attacks.All((AbilityProjectileAttackLine i) => i == null || i.IsFinished);
			if (IsFinished)
			{
				ApplyDodge();
				break;
			}
			if (Context.MaybeCaster?.IsDeadOrUnconscious ?? false)
			{
				break;
			}
			yield return null;
		}
	}

	private void ApplyDodge()
	{
		foreach (var (unitEntity, dodgeRule) in DodgedUnits)
		{
			if (UnitPartJumpAsideDodge.CanDodge(unitEntity, dodgeRule, out var safePath, out var pathCost))
			{
				unitEntity.GetOrCreate<UnitPartJumpAsideDodge>().Dodge(safePath, pathCost);
			}
		}
	}

	public bool Tick()
	{
		if (IsFinished)
		{
			return false;
		}
		bool result = m_Process.MoveNext();
		CurrentTarget = m_Process.Current;
		return result;
	}

	bool IEnumerator.MoveNext()
	{
		return Tick();
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	void IDisposable.Dispose()
	{
	}
}
