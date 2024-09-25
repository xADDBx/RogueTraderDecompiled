using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/ItemsEnough")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("4976252585b024c499c47cd56966ab2b")]
public class ItemsEnough : Condition
{
	[ValidateNotNull]
	public bool Money;

	[HideIf("Money")]
	[SerializeField]
	[FormerlySerializedAs("ItemToCheck")]
	private BlueprintItemReference m_ItemToCheck;

	public int Quantity;

	public BlueprintItem ItemToCheck => m_ItemToCheck?.Get();

	protected override string GetConditionCaption()
	{
		return $"Items ({Quantity} of {ItemToCheck})";
	}

	protected override bool CheckCondition()
	{
		if (Money || (bool)ItemToCheck.GetComponent<MoneyReplacement>())
		{
			return Game.Instance.Player.Money >= Quantity;
		}
		return GameHelper.GetPlayerCharacter().Inventory.Contains(ItemToCheck, Quantity);
	}
}
