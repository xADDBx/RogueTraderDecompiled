using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SellCollectibleItems")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("ba07b06141cb08f44a197690bf49a923")]
public class TransferItemsToCargoGameAction : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("ItemToSell")]
	private BlueprintItemReference m_ItemToSell;

	public BlueprintItem ItemToSell => m_ItemToSell?.Get();

	public override string GetCaption()
	{
		return $"Transfer all ({ItemToSell}) to cargo";
	}

	protected override void RunAction()
	{
		if (GameHelper.GetPlayerCharacter().Inventory.Count((ItemEntity i) => i.Blueprint == ItemToSell) > 0)
		{
			Game.Instance.GameCommandQueue.TransferItemsToCargo(GameHelper.GetPlayerCharacter().Inventory.Items.Select((ItemEntity x) => new EntityRef<ItemEntity>(x)).ToList());
		}
	}
}
