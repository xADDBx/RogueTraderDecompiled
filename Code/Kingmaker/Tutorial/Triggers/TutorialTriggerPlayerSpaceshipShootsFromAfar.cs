using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("b71eea4eaad04cbf8e9c93dbd38d1b07")]
public class TutorialTriggerPlayerSpaceshipShootsFromAfar : TutorialTrigger, IStarshipAttackHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[ValidateNotEmpty]
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts;

	[SerializeField]
	private int m_CellDistanceToTarget = 2;

	[SerializeField]
	private int m_TimesCanShootFromAfar = 2;

	private int m_TimesShotFromAfar;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit.IsPlayerShip() && m_TimesShotFromAfar > m_TimesCanShootFromAfar)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}

	public void HandleAttack(RuleStarshipPerformAttack starshipAttack)
	{
		if (IsSuitableTarget(starshipAttack))
		{
			int value = Mathf.FloorToInt(Vector3.Dot(starshipAttack.Target.Position, starshipAttack.Initiator.Position)).Cells().Value;
			if (starshipAttack.Initiator.IsInPlayerParty && value >= m_CellDistanceToTarget)
			{
				m_TimesShotFromAfar++;
			}
		}
	}

	private bool IsSuitableTarget(RuleStarshipPerformAttack rule)
	{
		BlueprintUnitFactReference[] facts = m_Facts;
		foreach (BlueprintUnitFactReference blueprintUnitFactReference in facts)
		{
			if (rule.TargetUnit != null && rule.TargetUnit.Facts.Contains(blueprintUnitFactReference.Get()))
			{
				return true;
			}
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
