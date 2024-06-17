using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("eea597eea6e44e7b9f8b3638ea1d0f45")]
public class TutorialTriggerItemInserted : TutorialTrigger, IInsertItemHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	public void HandleInsertItem(ItemSlot slot)
	{
		if (m_Item.GetBlueprint() == slot.Item.Blueprint)
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
