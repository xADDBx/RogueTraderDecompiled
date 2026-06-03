using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[TypeId("54e729d02e4c00544b52ba202683c833")]
public class AugmentEquippedTrigger : EntityFactComponentDelegate, IAugmentEquipHandler, ISubscriber, IHashable
{
	[SerializeField]
	private ActionList m_Actions;

	public void HandleAugmentEquip(BlueprintItemAugment augmentItem)
	{
		base.Fact.RunActionInContext(m_Actions);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
