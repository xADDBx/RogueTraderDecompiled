using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("3cdd881f75cfff44fa47508d5fc0a439")]
public class ContextConditionHasItem : ContextCondition
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
		return string.Format("{1} x{0}", Quantity, ItemToCheck.Name);
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
