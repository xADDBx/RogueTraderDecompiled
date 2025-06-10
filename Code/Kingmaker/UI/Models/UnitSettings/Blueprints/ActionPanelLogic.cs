using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.UI.Models.UnitSettings.Blueprints;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("0e059d6e61c843769f77ed36b3bf86b2")]
public class ActionPanelLogic : BlueprintComponent
{
	public int Priority;

	public ConditionsChecker AutoFillConditions;

	public ConditionsChecker AutoCastConditions;

	[InfoBox("The KeyName used in the ActionBar to save the position of deactivated abilities, and also for Coop pings. If you assign another ability here, its KeyName will be used as the source for generating the KeyName.")]
	public BlueprintUnitFactReference UseKeyNameFromFact;
}
