using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.SpaceCombat.Blueprints;

[TypeId("1668e0f9273841eba5a323dd0b04e474")]
public class BlueprintShipPostExpertise : BlueprintFeature
{
	[SerializeField]
	public PostType PostType;

	[SerializeField]
	private BlueprintAbilityReference m_DefaultPostAbility;

	[SerializeField]
	private BlueprintAbilityReference m_ChangedPostAbility;

	public BlueprintAbility DefaultPostAbility => m_DefaultPostAbility?.Get();

	public BlueprintAbility ChangedPostAbility => m_ChangedPostAbility?.Get();
}
