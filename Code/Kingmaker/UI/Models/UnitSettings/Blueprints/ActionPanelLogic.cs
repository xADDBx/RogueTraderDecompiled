using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace Kingmaker.UI.Models.UnitSettings.Blueprints;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("0e059d6e61c843769f77ed36b3bf86b2")]
public class ActionPanelLogic : BlueprintComponent
{
	public int Priority;

	public ConditionsChecker AutoFillConditions;

	public ConditionsChecker AutoCastConditions;
}
