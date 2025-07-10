using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("3bfa85df33501ca4c9aa7ce6f096d7f5")]
public class ItemInventoryFiltersOverride : BlueprintComponent
{
	[SerializeField]
	private ItemsFilterType m_ItemFilterOverride;

	public ItemsFilterType ItemFilterOverride => m_ItemFilterOverride;
}
