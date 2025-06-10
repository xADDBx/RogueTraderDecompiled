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

[TypeId("ff24775eb13443cabb881d75d3650742")]
public class OldInjuryTrigger : UnitFactComponentDelegate, IUnitTraumaHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_ActionOnOldInjuryReceived;

	[SerializeField]
	private ActionList m_ActionOnOldInjuryAvoided;

	public void HandleTraumaReceived()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Fact))
		{
			return;
		}
		using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnOldInjuryReceived.Run();
		}
	}

	public void HandleTraumaAvoided()
	{
		if (EventInvokerExtensions.BaseUnitEntity != base.Fact.Owner || !m_Restrictions.IsPassed(base.Fact))
		{
			return;
		}
		using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			m_ActionOnOldInjuryAvoided.Run();
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
