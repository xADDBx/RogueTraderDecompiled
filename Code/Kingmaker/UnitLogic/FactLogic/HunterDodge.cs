using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("4c9ef06897b84501961d3f21f4f33afa")]
public class HunterDodge : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformDodge>, IRulebookHandler<RulePerformDodge>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		public int UsedInRound { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private bool m_TriggerOnlyOnMelee;

	[SerializeField]
	private bool m_ChooseSpaceRandomly;

	[SerializeField]
	private ActionList m_ActionAfterDodge;

	public void OnEventAboutToTrigger(RulePerformDodge evt)
	{
		if (base.Owner == Game.Instance.TurnController.CurrentUnit || (m_TriggerOnlyOnMelee && !evt.IsMelee) || evt.Ability.IsAOE)
		{
			return;
		}
		int gameRound = Game.Instance.TurnController.GameRound;
		Data data = RequestSavableData<Data>();
		if (data.UsedInRound != gameRound)
		{
			evt.ChancesRule.AutoDodgeFlagModifiers.Add(base.Fact);
			data.UsedInRound = gameRound;
			UnitMoveToProperParams unitMoveToProperParams = CreateMoveParams(evt);
			if (unitMoveToProperParams != null)
			{
				base.Owner.Commands.Run(unitMoveToProperParams);
			}
		}
	}

	public void OnEventDidTrigger(RulePerformDodge evt)
	{
		base.Fact.RunActionInContext(m_ActionAfterDodge);
	}

	private UnitMoveToProperParams CreateMoveParams(RulePerformDodge evt)
	{
		BaseUnitEntity unit;
		IEnumerable<KeyValuePair<GraphNode, WarhammerPathPlayerCell>> enumerable = from n in PathfindingService.Instance.FindAllReachableTiles_Blocking(base.Owner.MovementAgent, base.Owner.Position, 1, ignoreThreateningAreaCost: true)
			where !((CustomGridNodeBase)n.Key).TryGetUnit(out unit)
			select n;
		KeyValuePair<GraphNode, WarhammerPathPlayerCell> keyValuePair = (m_ChooseSpaceRandomly ? enumerable.Random(PFStatefulRandom.UnitLogic.Abilities) : enumerable.MaxBy((KeyValuePair<GraphNode, WarhammerPathPlayerCell> i) => (evt.Attacker.Position - i.Key.Vector3Position).sqrMagnitude));
		if (keyValuePair.Key == null)
		{
			return null;
		}
		UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(new List<Vector3>
		{
			base.Owner.Position,
			keyValuePair.Key.Vector3Position
		}), 0f);
		unitMoveToProperParams.DisableAttackOfOpportunity.Retain();
		return unitMoveToProperParams;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
