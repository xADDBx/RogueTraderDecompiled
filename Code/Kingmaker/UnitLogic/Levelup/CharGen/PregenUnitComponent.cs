using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;

namespace Kingmaker.UnitLogic.Levelup.CharGen;

[ComponentName("PregenInformation")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("850ff304434d83049b01dd85cd363e3d")]
public class PregenUnitComponent : BlueprintComponent
{
	public LocalizedString PregenName;

	public LocalizedString PregenDescription;

	public LocalizedString PregenClass;

	public LocalizedString PregenRole;
}
