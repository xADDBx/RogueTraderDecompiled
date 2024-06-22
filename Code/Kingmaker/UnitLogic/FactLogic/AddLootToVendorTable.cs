using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("8abe2f7c2ba6427e838d50009aa57c09")]
public class AddLootToVendorTable : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSharedVendorTableReference m_Table;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	private BlueprintSharedVendorTable Table => m_Table?.Get();

	private BlueprintUnitLoot Loot => m_Loot?.Get();

	public override string GetCaption()
	{
		return $"Add loot {Loot} to vendor table {Table}";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.SharedVendorTables.AddLoot(Table, Loot);
	}
}
