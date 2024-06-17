using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("8cb1bfc127694b5e97d2b0862d73f6cf")]
public class TutorialTriggerPlayerMissedAGoodItem : TutorialTrigger, ICargoStateChangedHandler, ISubscriber, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference[] m_ItemsReferences;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAreaReference m_AreaReferences;

	private BlueprintItem[] Items => m_ItemsReferences.Select((BlueprintItemReference i) => i.Get()).ToArray();

	public void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
		if (Items.Contains(item.Blueprint) && Game.Instance.CurrentlyLoadedArea == m_AreaReferences.Get())
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
			});
		}
	}

	public void HandleCreateNewCargo(CargoEntity entity)
	{
	}

	public void HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
	}

	public void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
