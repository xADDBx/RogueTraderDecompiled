using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0cf4362b4b88d344b985fb95c02d109d")]
public class OverrideAnimationRaceComponent : BlueprintComponent
{
	public BlueprintRaceReference BlueprintRace;

	public override string ToString()
	{
		return "Переопределить рассу для Variant анимаций";
	}
}
