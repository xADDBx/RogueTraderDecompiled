using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("e77eb1db9e344376b35e4c4f0f5a0b69")]
public class OwnerJoinTBCombatTrigger : MechanicEntityFactComponentDelegate, IEntityJoinTBCombat<EntitySubscriber>, IEntityJoinTBCombat, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityJoinTBCombat, EntitySubscriber>, IHashable
{
	public RestrictionCalculator Restriction;

	public ActionList Action;

	public void HandleEntityJoinTBCombat()
	{
		if (Restriction.IsPassed(base.Fact))
		{
			base.Fact.RunActionInContext(Action);
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
