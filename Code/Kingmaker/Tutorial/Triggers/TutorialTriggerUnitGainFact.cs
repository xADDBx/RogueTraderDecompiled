using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[AllowMultipleComponents]
[TypeId("1711006cc1e74dbca3a18881ed67850e")]
public class TutorialTriggerUnitGainFact : TutorialTrigger, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public void HandleEntityGainFact(EntityFact fact)
	{
		IEntity owner = fact.Owner;
		BaseUnitEntity unit = owner as BaseUnitEntity;
		if (unit != null && m_Unit.Get() == unit.Blueprint && fact is UnitFact unitFact && m_Fact.Get() == unitFact.Blueprint)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
				context.SourceFact = fact;
			});
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
