using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c0f063e48aaf4f658eecd454b2ed3fae")]
[PlayerUpgraderAllowed(true)]
public class ForcePlaceUnitAboveGround : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetCaption()
	{
		return $"Force place above ground {Unit}";
	}

	public override void RunAction()
	{
		if (Unit.TryGetValue(out var value))
		{
			value?.View.ForcePlaceAboveGround();
		}
	}
}
