using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class DialogueNodeConditionTemplate : TextTemplate
{
	public const string TagName = "nc";

	public override int MinParameters => 2;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		DialogueNodeTagConditionsChecker component = ((ContextData<AnswerDataContext>.Current != null) ? ((BlueprintScriptableObject)ContextData<AnswerDataContext>.Current.Blueprint) : ((BlueprintScriptableObject)Game.Instance.DialogController.CurrentCue)).GetComponent<DialogueNodeTagConditionsChecker>();
		if (component == null)
		{
			PFLog.Default.Error("Dialogue node contains tag {nc} but node itself doesn't contains DialogueNodeTagConditionsChecker component!");
			return "";
		}
		int index = ((!component.CheckConditions()) ? 1 : 0);
		return parameters[index];
	}
}
