using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintDlc))]
[TypeId("d6d9a61bb3f046b0b28866df71adf518")]
public class CantSwitchOnDlcReason : BlueprintComponent
{
	public ConditionsChecker Conditions = new ConditionsChecker();

	public LocalizedString Reason;
}
