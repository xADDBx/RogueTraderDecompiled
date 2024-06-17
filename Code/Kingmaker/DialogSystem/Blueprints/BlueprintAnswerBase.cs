using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[NonOverridable]
[TypeId("e29428d274948784bb31643317d419ba")]
public abstract class BlueprintAnswerBase : BlueprintScriptableObject, IConditionDebugContext
{
	public SoulMarkShift SoulMarkRequirement = new SoulMarkShift();

	public bool IsSoulMarkRequirementSatisfied()
	{
		return SoulMarkShiftExtension.CheckShiftAtLeast(SoulMarkRequirement);
	}

	public void AddConditionDebugMessage(string message, Color color)
	{
		DialogDebug.Add(this, message, color);
	}
}
