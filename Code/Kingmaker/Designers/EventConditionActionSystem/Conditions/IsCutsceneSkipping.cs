using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("db9f89badbaa44e298e89a7425d26d59")]
public class IsCutsceneSkipping : Condition
{
	protected override string GetConditionCaption()
	{
		return "Cutscene is in skipping state";
	}

	protected override bool CheckCondition()
	{
		Cutscene cutscene = ContextData<NamedParametersContext.ContextData>.Current?.Context.Cutscene?.Cutscene;
		if (cutscene == null)
		{
			return false;
		}
		if (CutsceneController.Skipping && cutscene.LockControl)
		{
			return !cutscene.NonSkippable;
		}
		return false;
	}
}
