using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("ff4c44d7f5f642be8b63e686bfd528dc")]
public class DealTraumasForTarget : GameAction
{
	[Tooltip("Traumas count")]
	public int Count;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Вешает на юнита {Unit} {Count} травм";
	}

	public override string GetCaption()
	{
		return $"Deal {Unit} {Count} traumas";
	}

	protected override void RunAction()
	{
		Unit.GetValue().GetHealthOptional()?.DealTraumas(Count);
	}
}
