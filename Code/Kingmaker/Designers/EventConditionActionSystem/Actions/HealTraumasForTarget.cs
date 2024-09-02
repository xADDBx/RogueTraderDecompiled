using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("f37489e23a624a23bd522683adb70c63")]
public class HealTraumasForTarget : GameAction
{
	[Tooltip("Traumas count, 0 heal all traumas")]
	public int Count;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return string.Format("Снимает с юнита {0} {1}", Unit, (Count > 0) ? $"({Count} травм)" : "(все травмы)");
	}

	public override string GetCaption()
	{
		return "Heal Traumas " + ((Count > 0) ? $"({Count} stacks)" : "(all stacks)") + $" from {Unit}";
	}

	protected override void RunAction()
	{
		Unit.GetValue().GetHealthOptional()?.HealTrauma((Count > 0) ? Count : int.MaxValue);
	}
}
