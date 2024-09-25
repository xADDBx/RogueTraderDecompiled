using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("70f6a1b620e541a59972fae9a14075b4")]
public class TutorialTriggerItemChargesSpent : TutorialTrigger, IItemChargesHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	private bool CanTrigger => m_Item.Get().SpendCharges;

	public void HandleItemChargeSpent(ItemEntity item)
	{
		if (!CanTrigger)
		{
			throw new Exception("TutorialTriggerItemChargesSpent: Cannot trigger if Item can't spend charges");
		}
		if (m_Item.Get() == item.Blueprint)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
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
