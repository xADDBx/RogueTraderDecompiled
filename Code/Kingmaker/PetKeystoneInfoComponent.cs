using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("122ec7c91e97dfc4ea7a7ced0e5c7b61")]
public class PetKeystoneInfoComponent : BlueprintComponent
{
	public PetType PetType;

	public LocalizedString PetInfoTitleName;

	public VideoLink PetVideo;

	public BlueprintEncyclopediaGlossaryEntryReference DescriptionReference;

	public List<PetKeyStat> KeyStats;

	public List<BlueprintAbilityReference> CoreAbilitiesReferences;

	public BlueprintUnitReference PetUnitReference;

	public List<PetRecommendedFeature> RecommendedFeatures;
}
