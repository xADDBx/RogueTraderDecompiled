using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItemWeapon))]
[AllowedOn(typeof(BlueprintWeaponType))]
[TypeId("d9cd604667ba411497ff3e02b94985c1")]
public class OverrideOverpenetrationFactor : BlueprintComponent
{
	public int FactorPercents = 100;
}
