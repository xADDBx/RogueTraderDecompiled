using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;

namespace Kingmaker.UnitLogic.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("2f9bd7c5553520a49a16bba33d80b7ab")]
public class PetCharscreenUnitComponent : BlueprintComponent
{
	public VideoLink PetGameplayVideoLinkPC;

	public VideoLink PetGameplayVideoLinkPS;

	public VideoLink PetGameplayVideoLinkXbox;

	public BlueprintEncyclopediaGlossaryEntryReference NarrativeDescription;

	public LocalizedString StrategyDescription;

	public LocalizedString TipsDescription;

	public VideoLink VideoToShow => PetGameplayVideoLinkPC;
}
