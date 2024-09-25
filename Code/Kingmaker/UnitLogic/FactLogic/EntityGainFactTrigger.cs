using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("eb92fb96ff234caca266feb01a80878e")]
public class EntityGainFactTrigger : MechanicEntityFactComponentDelegate, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public RestrictionCalculator Restriction;

	public ActionList Action;

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact != null)
		{
			MechanicsContext maybeContext = fact.MaybeContext;
			if (maybeContext != null && Restriction.IsPassed(new PropertyContext(base.Owner, base.Fact).WithContext(maybeContext)))
			{
				base.Fact.RunActionInContext(Action);
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
