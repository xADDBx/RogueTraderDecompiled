using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("a817822efea43ad4c9172f20919f3fd9")]
public class BlueprintPointOfInterestColonyTrait : BlueprintPointOfInterest
{
	[SerializeField]
	private LocalizedString m_MechanicString;

	[SerializeField]
	private BlueprintColonyTrait.Reference m_Trait;

	public string MechanicString => m_MechanicString;

	public BlueprintColonyTrait Trait => m_Trait.Get();
}
