using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add proficiencies")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("5fa95b92f4e484742ad84c926183372c")]
public class AddProficiencies : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("RaceRestriction")]
	private BlueprintRaceReference m_RaceRestriction;

	public ArmorProficiencyGroup[] ArmorProficiencies;

	public WeaponProficiency[] WeaponProficiencies;

	public BlueprintRace RaceRestriction => m_RaceRestriction?.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (!RaceRestriction || base.Owner.Progression.Race == RaceRestriction)
		{
			ArmorProficiencies.EmptyIfNull().ForEach(delegate(ArmorProficiencyGroup i)
			{
				base.Owner.Proficiencies.Add(i);
			});
			WeaponProficiencies.EmptyIfNull().ForEach(delegate(WeaponProficiency i)
			{
				base.Owner.Proficiencies.Add(in i);
			});
		}
	}

	protected override void OnDeactivate()
	{
		if (!RaceRestriction || base.Owner.Progression.Race == RaceRestriction)
		{
			ArmorProficiencies.EmptyIfNull().ForEach(delegate(ArmorProficiencyGroup i)
			{
				base.Owner.Proficiencies.Remove(i);
			});
			WeaponProficiencies.EmptyIfNull().ForEach(delegate(WeaponProficiency i)
			{
				base.Owner.Proficiencies.Remove(in i);
			});
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
