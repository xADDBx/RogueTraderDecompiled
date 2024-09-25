using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("bd5e215abb372114fa3092696d5fee05")]
public class AddVendorItemsAction : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private VendorEvaluator m_VendorEvaluator;

	[SerializeField]
	private BlueprintUnitLootReference m_VendorTable;

	public override string GetCaption()
	{
		return $"Добавить {m_VendorTable.Get().NameSafe()} торговцу {m_VendorEvaluator}";
	}

	protected override void RunAction()
	{
		BlueprintUnitLoot loot = m_VendorTable.Get();
		m_VendorEvaluator.GetValue().AddLoot(loot);
	}
}
