using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/RecalculateItemByVail")]
[AllowedOn(typeof(BlueprintItemEquipment))]
[TypeId("dfd7f9a886d643b0a4e9a9d88a21aa55")]
public class RecalculateItemByVail : EntityFactComponentDelegate, IPsychicPhenomenaUIHandler, ISubscriber, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		EventBus.Subscribe(base.Owner);
	}

	protected override void OnDeactivate()
	{
		EventBus.Unsubscribe(base.Owner);
	}

	public void HandleVeilThicknessValueChanged(int delta, int value)
	{
		if (base.Owner is ItemEntity { Wielder: { } wielder } itemEntity)
		{
			itemEntity.OnWillUnequip();
			itemEntity.OnDidEquipped(wielder);
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
