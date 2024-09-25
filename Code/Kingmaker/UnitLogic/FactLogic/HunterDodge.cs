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
	public enum TriggerOnlyOn
	{
		All,
		Melee,
		Ranged
	}

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
	private TriggerOnlyOn m_TriggerOnlyOn;

	[SerializeField]
	private bool m_CanTriggerMoreThanOncePerRound;

	[SerializeField]
	private bool m_AllowDodgeOnAoe;

	[SerializeField]
	private bool m_ChooseSpaceRandomly;

	[SerializeField]
	private ActionList m_ActionAfterDodge;

	public void OnEventAboutToTrigger(RulePerformDodge evt)
	{
		if (base.Owner == Game.Instance.TurnController.CurrentUnit || (m_TriggerOnlyOn == TriggerOnlyOn.Melee && !evt.IsMelee) || (m_TriggerOnlyOn == TriggerOnlyOn.Ranged && !evt.IsRanged) || (evt.Ability.IsAOE && !m_AllowDodgeOnAoe))
		{
			return;
		}
		int gameRound = Game.Instance.TurnController.GameRound;
		Data data = RequestSavableData<Data>();
		if (m_CanTriggerMoreThanOncePerRound || data.UsedInRound != gameRound)
		{
			evt.ChancesRule.AutoDodgeFlagModifiers.Add(base.Fact);
			data.UsedInRound = gameRound;
			UnitJumpAsideDodgeParams unitJumpAsideDodgeParams = CreateMoveParams(evt);
			if (unitJumpAsideDodgeParams != null)
			{
				base.Owner.Commands.Run(unitJumpAsideDodgeParams);
			}
		}
	}

	public void OnEventDidTrigger(RulePerformDodge evt)
	{
		ActionList actionAfterDodge = m_ActionAfterDodge;
		if (actionAfterDodge != null && actionAfterDodge.HasActions)
		{
			base.Fact.RunActionInContext(m_ActionAfterDodge);
		}
	}

	private UnitJumpAsideDodgeParams CreateMoveParams(RulePerformDodge evt)
	{
		BaseUnitEntity unit;
		IEnumerable<KeyValuePair<GraphNode, WarhammerPathPlayerCell>> enumerable = from n in PathfindingService.Instance.FindAllReachableTiles_Blocking(base.Owner.MovementAgent, base.Owner.Position, 1f, ignoreThreateningAreaCost: true)
			where !((CustomGridNodeBase)n.Key).TryGetUnit(out unit)
			select n;
		KeyValuePair<GraphNode, WarhammerPathPlayerCell> keyValuePair = (m_ChooseSpaceRandomly ? enumerable.Random(PFStatefulRandom.UnitLogic.Abilities) : enumerable.MaxBy((KeyValuePair<GraphNode, WarhammerPathPlayerCell> i) => (evt.Attacker.Position - i.Key.Vector3Position).sqrMagnitude));
		if (keyValuePair.Key == null)
		{
			return null;
		}
		return new UnitJumpAsideDodgeParams(ForcedPath.Construct(new List<Vector3>
		{
			base.Owner.Position,
			keyValuePair.Key.Vector3Position
		}));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
