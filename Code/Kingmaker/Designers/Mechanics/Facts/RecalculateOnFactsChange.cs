using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("4cc225b5566d5c34583ecba62d752412")]
public class RecalculateOnFactsChange : UnitFactComponentDelegate, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CheckedFacts")]
	private BlueprintUnitFactReference[] m_CheckedFacts;

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (m_CheckedFacts == null || m_CheckedFacts.Length == 0 || m_CheckedFacts.HasReference(fact.Blueprint as BlueprintUnitFact))
		{
			base.Fact.Reapply();
		}
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		if (m_CheckedFacts == null || m_CheckedFacts.Length == 0 || m_CheckedFacts.HasReference(fact.Blueprint as BlueprintUnitFact))
		{
			base.Fact.Reapply();
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
