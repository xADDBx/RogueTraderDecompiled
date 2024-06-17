using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("68a754d320793344bbf8376833fa27e1")]
public class StarshipSpendUnusedAP : UnitFactComponentDelegate, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_EvasionBuff;

	public BlueprintBuff EvasionBuff => m_EvasionBuff?.Get();

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (!isTurnBased || EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		base.Owner.Buffs.Remove(EvasionBuff);
		int actionPointsYellow = base.Owner.CombatState.ActionPointsYellow;
		if (actionPointsYellow > 0)
		{
			Buff buff = base.Owner.Buffs.Add(EvasionBuff, new BuffDuration(null, BuffEndCondition.CombatEnd));
			if (actionPointsYellow > 1)
			{
				buff.AddRank(actionPointsYellow - 1);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
