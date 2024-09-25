using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("76f3713a528441968267c8d93ba89f39")]
public class MakeItemNonRemovable : GameAction
{
	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public bool NonRemovable = true;

	public BlueprintItem Item => m_Item?.Get();

	public override string GetDescription()
	{
		return string.Format("Делает предмет {0} {1}снимаемым)", Item, NonRemovable ? "не" : "");
	}

	public override string GetCaption()
	{
		return string.Format("Make {0} {1}removable)", Item, NonRemovable ? "non-" : "");
	}

	protected override void RunAction()
	{
		ItemEntity itemEntity = GameHelper.GetPlayerCharacter().Inventory.Items.FirstOrDefault((ItemEntity i) => i.Blueprint == Item);
		if (itemEntity != null)
		{
			itemEntity.IsNonRemovable = NonRemovable;
		}
	}
}
