using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("c754d0a7a3d34baeac493e7d637a0360")]
public class TutorialTriggerShieldItemInserted : TutorialTrigger, IInsertItemHandler, ISubscriber, IHashable
{
	[SerializeField]
	private bool m_OnlyInPlayerPartyInserted = true;

	public void HandleInsertItem(ItemSlot slot)
	{
		bool flag = !m_OnlyInPlayerPartyInserted || slot.Owner.IsInPlayerParty;
		if (slot is WeaponSlot weaponSlot && weaponSlot.HasShield && flag)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = slot.Item;
				context.SourceUnit = slot.Item.Owner as BaseUnitEntity;
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
