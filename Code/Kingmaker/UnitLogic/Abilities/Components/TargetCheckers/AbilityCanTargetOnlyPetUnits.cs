using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("58590ce4af844cbd84c06187c8cc0ead")]
public class AbilityCanTargetOnlyPetUnits : BlueprintComponent
{
	[SerializeField]
	[Tooltip("Ограничивает цели применения только петом мастера, который кастует абилку")]
	[HideIf("m_Inverted")]
	private bool m_CanTargetOnlyOwnersPet;

	[SerializeField]
	[Tooltip("Inverted запрещает возможность каста абилки на петов")]
	private bool m_Inverted;

	public bool CanTargetOnlyOwnersPet => m_CanTargetOnlyOwnersPet;

	public bool Inverted => m_Inverted;
}
