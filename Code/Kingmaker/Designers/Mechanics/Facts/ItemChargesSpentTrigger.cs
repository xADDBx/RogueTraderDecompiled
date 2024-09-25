using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("51468477c7754999bcb031c6bccda748")]
public class ItemChargesSpentTrigger : EntityFactComponentDelegate, IItemChargesHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	[SerializeField]
	private ActionList m_Actions;

	[SerializeField]
	private bool m_Once;

	private bool CanTrigger => m_Item.Get().SpendCharges;

	public void HandleItemChargeSpent(ItemEntity item)
	{
		if (!CanTrigger)
		{
			throw new Exception("ItemChargesSpent: Cannot trigger if Item can't spend charges");
		}
		if ((!m_Once || base.ExecutesCount <= 0) && m_Item.Get() == item.Blueprint)
		{
			m_Actions.Run();
			base.ExecutesCount++;
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
