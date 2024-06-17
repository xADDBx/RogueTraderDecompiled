using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add vendor items")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAnomaly))]
[AllowMultipleComponents]
[TypeId("467fddff82e2032428ce9ceb134b552e")]
public class AddVendorItems : UnitFactComponentDelegate, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	public BlueprintUnitLoot Loot => m_Loot?.Get();

	protected override void OnActivate()
	{
		if ((bool)Loot)
		{
			BlueprintSharedVendorTable blueprintSharedVendorTable = Loot as BlueprintSharedVendorTable;
			PartVendor orCreate = base.Owner.GetOrCreate<PartVendor>();
			if ((bool)blueprintSharedVendorTable)
			{
				orCreate.SetSharedInventory(blueprintSharedVendorTable);
			}
			else
			{
				orCreate.AddLoot(Loot);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
