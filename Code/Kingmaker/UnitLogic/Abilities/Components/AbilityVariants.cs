using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("1bf4aff684fadda4c85160390570fdf2")]
public class AbilityVariants : BlueprintComponent
{
	[SerializeField]
	[FormerlySerializedAs("Variants")]
	private BlueprintAbilityReference[] m_Variants;

	public ReferenceArrayProxy<BlueprintAbility> Variants
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] variants = m_Variants;
			return variants;
		}
	}
}
