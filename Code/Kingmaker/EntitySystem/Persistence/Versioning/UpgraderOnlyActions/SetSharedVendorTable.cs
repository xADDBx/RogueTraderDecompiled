using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("76052fdfe674cfd41988bb8e4fd1f815")]
internal class SetSharedVendorTable : PlayerUpgraderOnlyAction
{
	[SerializeField]
	private BlueprintSharedVendorTableReference m_Table;

	[SerializeReference]
	[SerializeField]
	private BaseUnitEvaluator m_Unit;

	public SetSharedVendorTable(BaseUnitEvaluator unit)
	{
		m_Unit = unit;
	}

	protected override void RunActionOverride()
	{
		BlueprintSharedVendorTable blueprintSharedVendorTable = m_Table.Get();
		if ((bool)blueprintSharedVendorTable)
		{
			BaseUnitEntity value = null;
			m_Unit?.TryGetValue(out value);
			if (value != null)
			{
				value.Parts.GetOrCreate<PartVendor>().SetSharedInventory(blueprintSharedVendorTable);
			}
			else
			{
				PFLog.Default.ErrorWithReport($"Failed to run uprgader {base.Owner}: unit not found");
			}
		}
	}

	public override string GetCaption()
	{
		return "Add vendor table to " + m_Unit;
	}

	public override string GetDescription()
	{
		return $"Adds vendor table {m_Table.NameSafe()} to {m_Unit}. Will overwrite any other table if there was one";
	}
}
