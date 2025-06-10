using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[TypeId("e6b1b9773bd54abdb01e0e1347e02e22")]
public class AbilityRedirectTrigger : UnitFactComponentDelegate, IAbilityExecutionProcessRedirectHandler<EntitySubscriber>, IAbilityExecutionProcessRedirectHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessRedirectHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList Actions;

	public void HandleAbilityRedirected(AbilityExecutionContext context)
	{
		if (m_Restrictions.IsPassed(base.Fact, context, null, context.Ability))
		{
			base.Fact.RunActionInContext(Actions);
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
