using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/SetAICommand")]
[TypeId("aa87ed3d26e34d24688426a909aaa6bb")]
public class SetAICommand : ClockworkCommand
{
	public bool Dialogs;

	public bool Intercations;

	[ShowIf("Intercations")]
	public BlueprintUnitReference[] ExcludedUnits;

	[SerializeReference]
	public Condition EndCondition;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		Complete();
		runner.AIConfig.ResetConfig(this);
		return null;
	}

	public override string GetCaption()
	{
		string text = (Dialogs ? "Dialogs " : "");
		text += (Intercations ? "Intercations " : "");
		return GetStatusString() + "Set AI Config: " + text;
	}
}
