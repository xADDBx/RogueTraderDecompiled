using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items.Slots;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("a39b89cb076a6ba4eaaf7dbd22494793")]
[PlayerUpgraderAllowed(false)]
public class PlayerStarshipHasComponent : Condition
{
	[SerializeField]
	private BlueprintStarshipItemReference[] m_Items;

	public ReferenceArrayProxy<BlueprintStarshipItem> Items
	{
		get
		{
			BlueprintReference<BlueprintStarshipItem>[] items = m_Items;
			return items;
		}
	}

	protected override string GetConditionCaption()
	{
		return "Player ship has any component from list";
	}

	protected override bool CheckCondition()
	{
		IEnumerable<BlueprintItem> components = from x in Game.Instance.Player.PlayerShip.Hull.HullSlots.EquipmentSlots
			where x.HasItem
			select x.Item.Blueprint;
		return Items.Any((BlueprintStarshipItem x) => components.Contains(x));
	}
}
