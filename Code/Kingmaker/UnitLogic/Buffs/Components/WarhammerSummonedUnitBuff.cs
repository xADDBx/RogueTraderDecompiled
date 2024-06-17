using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[ComponentName("Buffs/Special/Summoned unit")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("01dcaacf0399dc2449b639320aec66cb")]
public class WarhammerSummonedUnitBuff : UnitBuffComponentDelegate, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private bool m_ExpireWhenAloneAsStarship;

	[SerializeField]
	private ActionList m_ActionsOnExpiration;

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		bool flag = false;
		if (m_ExpireWhenAloneAsStarship)
		{
			StarshipEntity starship = base.Owner as StarshipEntity;
			if (starship != null)
			{
				flag = !Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity u) => u is StarshipEntity && u.IsAlly(starship) && u != starship && !(u as StarshipEntity).Blueprint.IsSoftUnit).Any();
			}
		}
		if (base.Buff.Rank > 1 && !flag)
		{
			base.Buff.RemoveRank();
		}
		else
		{
			base.Fact.RunActionInContext(m_ActionsOnExpiration, base.OwnerTargetWrapper);
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
