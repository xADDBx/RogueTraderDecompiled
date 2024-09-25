using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a72c1eb397ac57c47b01bf4416ced9e9")]
public class WarhammerAbilityTooltipHelper : BlueprintComponent
{
	[SerializeField]
	private bool m_OverrideTargetType;

	[SerializeField]
	[ShowIf("m_OverrideTargetType")]
	private TargetType m_TargetType;

	public TargetType? TargetType
	{
		get
		{
			if (!m_OverrideTargetType)
			{
				return null;
			}
			return m_TargetType;
		}
	}
}
