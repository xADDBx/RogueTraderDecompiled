using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("820d299f01794e10bac98f5908e4a2cd")]
public class CompanionUnconsciousTrigger : UnitFactComponentDelegate, IUnitDeathHandler, ISubscriber, IUnitResurrectedHandler, ISubscriber<IAbstractUnitEntity>, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList Actions;

	public ActionList RessurectActions;

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity != base.Owner && unitEntity != null && unitEntity.IsInPlayerParty && Restrictions.IsPassed(base.Fact, unitEntity))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}

	public void HandleUnitResurrected()
	{
		UnitEntity unitEntity = EventInvokerExtensions.MechanicEntity as UnitEntity;
		if (unitEntity != base.Owner && unitEntity != null && unitEntity.IsInPlayerParty && Restrictions.IsPassed(base.Fact, unitEntity))
		{
			base.Fact.RunActionInContext(RessurectActions, base.OwnerTargetWrapper);
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
