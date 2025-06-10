using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("43a5d7af6ac4446d99df2b0e76d51ae9")]
public class CheckBuffForMPSpendTooltip : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_BuffToCaster;

	public bool CheckContainsBuff(BaseUnitEntity unit)
	{
		return unit.Buffs.Contains((BlueprintBuff)m_BuffToCaster);
	}
}
