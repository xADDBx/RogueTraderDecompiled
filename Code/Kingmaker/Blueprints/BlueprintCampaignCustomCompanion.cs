using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintCampaign))]
[TypeId("e62c86d84e36410181ee03424f2779f3")]
public class BlueprintCampaignCustomCompanion : BlueprintComponent
{
	[SerializeField]
	private BlueprintUnitReference m_CustomCompanion;

	public BlueprintUnit CustomCompanion => m_CustomCompanion;
}
