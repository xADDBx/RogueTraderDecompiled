using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("0cc6d74312fa44a69eaacfe59935c4c4")]
public class AddVendorFaction : UnitFactComponentDelegate, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintVendorFaction.Reference m_Faction;

	public BlueprintVendorFaction Faction => m_Faction?.Get();

	protected override void OnActivate()
	{
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
