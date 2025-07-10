using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add vendor table")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("abb987a0bcaf4f668297b79e447f4763")]
public class AddSharedVendor : EntityFactComponentDelegate<MechanicEntity>, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSharedVendorTableReference m_Table;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintVendorFaction.Reference m_Faction;

	[SerializeField]
	private bool m_NeedHidePfAndReputation;

	[SerializeField]
	private bool m_HideReputationCompletely;

	public BlueprintSharedVendorTable Table => m_Table?.Get();

	public BlueprintVendorFaction Faction => m_Faction?.Get();

	public bool NeedHidePfAndReputation => m_NeedHidePfAndReputation;

	public bool HideReputationCompletely => m_HideReputationCompletely;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<PartVendor>().SetSharedInventory(Table);
		base.Owner.GetOrCreate<PartVendor>().SetVendorFaction(Faction);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
