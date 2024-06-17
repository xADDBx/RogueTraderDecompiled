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
using Kingmaker.UnitLogic.Progression.Paths;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0235a93dc6eb6864a839fcc72bb44c36")]
public class AddFeatureIfHasFact : UnitFactComponentDelegate, IUnitGainPathRankHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintUnitFactReference m_CheckedFact;

	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public bool Not;

	public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

	public BlueprintUnitFact Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		Apply();
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void HandleUnitGainPathRank(BlueprintPath path)
	{
		Apply();
	}

	private void Apply()
	{
		if (base.Owner.Facts.FindBySource(Feature, base.Fact, this) == null && ((base.Owner.Facts.Contains(CheckedFact) && !Not) || (!base.Owner.Facts.Contains(CheckedFact) && Not)))
		{
			base.Owner.AddFact(Feature)?.AddSource(base.Fact, this);
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Blueprint == CheckedFact)
		{
			Apply();
		}
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		if (fact.Blueprint == CheckedFact)
		{
			RemoveAllFactsOriginatedFromThisComponent(base.Owner);
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
