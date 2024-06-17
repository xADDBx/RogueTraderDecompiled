using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("99d65f0e88e14fd0b81ad24d47629fc8")]
public class RecalculateOnEquipmentChange : MechanicEntityFactComponentDelegate, IEntitySubscriber, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>, IEquipItemHandler<EntitySubscriber>, IEquipItemHandler, ISubscriber<IItemEntity>, IEventTag<IEquipItemHandler, EntitySubscriber>, IHashable
{
	IEntity IEntitySubscriber.GetSubscribingEntity()
	{
		return base.Fact.Owner;
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		base.Fact.Recalculate();
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		base.Fact.Recalculate();
	}

	public void OnDidEquipped()
	{
		base.Fact.Recalculate();
	}

	public void OnWillUnequip()
	{
		base.Fact.Recalculate();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
