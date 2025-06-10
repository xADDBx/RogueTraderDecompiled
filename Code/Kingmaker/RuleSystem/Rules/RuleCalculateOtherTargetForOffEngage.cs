using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateOtherTargetForOffEngage : RulebookOptionalTargetEvent
{
	private EntityRef<MechanicEntity> m_OtherTarget;

	private CustomGridNodeBase m_OtherTargetNode;

	private readonly CustomGridNodeBase m_InitiatorNode;

	private readonly CustomGridNodeBase m_TargetNode;

	private readonly int m_Range;

	private bool m_IsRandomizeAttackLine;

	private float m_AttackLineAngle;

	private int m_RangeBetweenAttackerAndTarget;

	public MechanicEntity OtherTarget => m_OtherTarget;

	public CustomGridNodeBase OtherTargetNode => m_OtherTargetNode;

	public RuleCalculateOtherTargetForOffEngage([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, CustomGridNodeBase initiatorNode, CustomGridNodeBase targetNode, int range)
		: base(initiator, target)
	{
		m_InitiatorNode = initiatorNode;
		m_TargetNode = targetNode;
		m_Range = range;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		m_OtherTarget = null;
		m_OtherTargetNode = null;
		BaseUnitEntity initiatorUnit = base.InitiatorUnit;
		if (!m_IsRandomizeAttackLine || !(MaybeTarget is BaseUnitEntity baseUnitEntity) || initiatorUnit == null || !baseUnitEntity.IsOffEngageForTarget(initiatorUnit) || CustomGraphHelper.GetWarhammerLength(m_TargetNode.CoordinatesInGrid - m_InitiatorNode.CoordinatesInGrid) > m_RangeBetweenAttackerAndTarget)
		{
			return;
		}
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit.IsInCombat && allBaseAwakeUnit != initiatorUnit && initiatorUnit.IsAlly(allBaseAwakeUnit) && Vector3.Angle(baseUnitEntity.Position - initiatorUnit.Position, allBaseAwakeUnit.Position - initiatorUnit.Position) <= m_AttackLineAngle)
			{
				CustomGridNodeBase customGridNodeBase = allBaseAwakeUnit.GetOccupiedNodes().FirstOrDefault((CustomGridNodeBase node) => LosCalculations.GetDirectLos(m_InitiatorNode.Vector3Position, node.Vector3Position)) ?? allBaseAwakeUnit.Position.GetNearestNodeXZUnwalkable();
				if (customGridNodeBase != null && CustomGraphHelper.GetWarhammerLength(customGridNodeBase.CoordinatesInGrid - m_InitiatorNode.CoordinatesInGrid) <= m_Range)
				{
					m_OtherTarget = allBaseAwakeUnit;
					m_OtherTargetNode = customGridNodeBase;
					break;
				}
			}
		}
	}

	public void SetupRandomizeAttackLineSettings(bool isRandomizeAttackLine, float attackLineAngle, int rangeBetweenAttackerAndTarget)
	{
		m_IsRandomizeAttackLine = isRandomizeAttackLine;
		m_AttackLineAngle = attackLineAngle;
		m_RangeBetweenAttackerAndTarget = rangeBetweenAttackerAndTarget;
	}
}
