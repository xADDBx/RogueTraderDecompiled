using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("28bf52e8da71401ab5e8ed286bac5385")]
public class WarhammerFreshInjuryTrigger : UnitFactComponentDelegate, IUnitWoundHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_ActionOnFreshInjuryReceived;

	[SerializeField]
	private ActionList m_ActionOnFreshInjuryAvoided;

	public void HandleWoundReceived()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Fact))
		{
			return;
		}
		using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnFreshInjuryReceived.Run();
		}
	}

	public void HandleWoundAvoided()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Fact))
		{
			return;
		}
		using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnFreshInjuryAvoided.Run();
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
