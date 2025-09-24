using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("266118e23d104105aac575b11ee3ef60")]
public class OverrideAbilityFxSettings : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[SerializeField]
	private BlueprintAbilityReference m_EffectedAbility;

	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public BlueprintAbility Ability => m_EffectedAbility;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
