using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.AI;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("433d4bafc1ae7c140bfc3dedc8c578c3")]
public class StarshipStrikecraftLogic : UnitFactComponentDelegate, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInitiatorRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private int m_ReturningBrainIndex;

	[SerializeField]
	private bool m_ExpendAllFuelOnAttack;

	[SerializeField]
	private BlueprintBuffReference m_FuelBuff;

	[SerializeField]
	private BlueprintBuffReference m_LandingBuff;

	public ActionList ExpirationActions;

	public BlueprintBuff FuelBuff => m_FuelBuff?.Get();

	public BlueprintBuff LandingBuff => m_LandingBuff?.Get();

	public StarshipEntity GetShipToLand(BaseUnitEntity unit)
	{
		if (unit.GetOptional<UnitPartSummonedMonster>()?.Summoner is StarshipEntity result)
		{
			return result;
		}
		return (from StarshipEntity ally in Game.Instance.TurnController.AllUnits.Where((MechanicEntity u) => u.IsInCombat && u != unit && unit.IsAlly(u) && u is StarshipEntity)
			where !ally.Blueprint.IsSoftUnit
			select ally).MinBy((StarshipEntity ally) => ally.DistanceToInCells(unit));
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (!isTurnBased || EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		if (GetShipToLand(base.Owner) == null && base.Owner.Brain.Blueprint is BlueprintStarshipBrain { IsStrikecraftReturningBrain: not false })
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				base.Fact.RunActionInContext(ExpirationActions, base.Owner.ToITargetWrapper());
				return;
			}
		}
		if (!FuelBuff)
		{
			return;
		}
		Buff buff = base.Owner.Buffs.GetBuff(FuelBuff);
		if (buff != null)
		{
			buff.RemoveRank();
			if (base.Owner.Buffs.GetBuff(FuelBuff) == null)
			{
				StartLanding();
			}
		}
	}

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
	{
		if (m_ExpendAllFuelOnAttack)
		{
			Buff buff = base.Owner.Buffs.GetBuff(FuelBuff);
			if (buff != null)
			{
				buff.Remove();
				StartLanding();
			}
		}
	}

	private void StartLanding()
	{
		if (m_ReturningBrainIndex >= 0)
		{
			base.Owner.Brain.SetBrain(base.Owner.Blueprint.AlternativeBrains[m_ReturningBrainIndex]);
		}
		base.Owner.Buffs.Add(LandingBuff, new BuffDuration(null, BuffEndCondition.CombatEnd));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
