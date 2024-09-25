using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("307ce9314664f8a4cadd54971536ae55")]
public class IsRespecAllowed : Condition
{
	protected override string GetConditionCaption()
	{
		return "Is allowed Respec";
	}

	protected override bool CheckCondition()
	{
		return SettingsRoot.Difficulty.RespecAllowed;
	}
}
