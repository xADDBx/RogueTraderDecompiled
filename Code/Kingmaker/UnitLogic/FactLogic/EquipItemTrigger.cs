using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("f833804310724fc8826ddc9b7340a38d")]
public class EquipItemTrigger : EntityFactComponentDelegate<ItemEntity>, IEquipItemHandler<EntitySubscriber>, IEquipItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<IEquipItemHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private ActionList m_OnDidEquipped;

	[SerializeField]
	private ActionList m_OnWillUnequip;

	public void OnDidEquipped()
	{
		if (!ContextData<UnitHelper.PreviewUnit>.Current && !base.Owner.IsPreview())
		{
			ActionList onDidEquipped = m_OnDidEquipped;
			if (onDidEquipped != null && onDidEquipped.HasActions)
			{
				m_OnDidEquipped.Run();
			}
		}
	}

	public void OnWillUnequip()
	{
		if (!ContextData<UnitHelper.PreviewUnit>.Current && !base.Owner.IsPreview())
		{
			ActionList onWillUnequip = m_OnWillUnequip;
			if (onWillUnequip != null && onWillUnequip.HasActions)
			{
				m_OnWillUnequip.Run();
			}
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
