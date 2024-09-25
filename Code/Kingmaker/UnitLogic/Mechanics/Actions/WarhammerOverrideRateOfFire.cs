using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("2b7693c404d44cb8a226c9bb5f513e49")]
public class WarhammerOverrideRateOfFire : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private int m_RateOfFire;

	public int RateOfFire => Math.Max(1, m_RateOfFire);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
