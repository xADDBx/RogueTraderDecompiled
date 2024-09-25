using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintMultiEntranceEntry))]
[TypeId("54fcd119c98d4d4698864a79dd1dc72f")]
public class ChangeTransitionPointName : BlueprintComponent
{
	public ConditionsChecker Conditions = new ConditionsChecker();

	public LocalizedString AnotherName;
}
