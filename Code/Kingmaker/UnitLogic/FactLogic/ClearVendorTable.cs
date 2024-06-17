using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("21cf2aa69dbf443bbee1c7bd3e07508b")]
public class ClearVendorTable : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintSharedVendorTableReference m_Table;

	public BlueprintSharedVendorTable Table => m_Table;

	public override string GetCaption()
	{
		return $"Clear vendor table: {Table}";
	}

	public override void RunAction()
	{
		Game.Instance.Player.SharedVendorTables.GetCollection(Table).RemoveAll();
	}
}
