using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("1cdb53f94f674412b307c2142759b437")]
public class AbilityAmmoLogic : BlueprintComponent
{
	public bool NoAmmoRequired;

	[HideIf("NoAmmoRequired")]
	public int AdditionalAmmoCost;
}
