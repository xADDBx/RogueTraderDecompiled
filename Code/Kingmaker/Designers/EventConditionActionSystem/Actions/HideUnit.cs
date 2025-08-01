using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0edf8920d3df9b54c9db189bdad67cac")]
[PlayerUpgraderAllowed(true)]
public class HideUnit : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool Unhide;

	[HideIf("Unhide")]
	public bool Fade;

	public override string GetDescription()
	{
		return string.Format("{0} юнита {1}", Unhide ? "Показывает " : "Прячет", Target);
	}

	public override string GetCaption()
	{
		return (Unhide ? "Show " : "Hide") + Target?.GetCaption();
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		UnitPartPetOwner optional = value.GetOptional<UnitPartPetOwner>();
		if (optional != null)
		{
			optional.InHidingProcess = true;
		}
		doRun(value);
		if (optional != null && optional.PetUnit != null)
		{
			if (Unhide)
			{
				if (optional.ShouldUnhidePet)
				{
					optional.ShouldUnhidePet = false;
					doRun(optional.PetUnit);
				}
			}
			else if (optional.PetUnit.IsInGame)
			{
				optional.ShouldUnhidePet = true;
				doRun(optional.PetUnit);
			}
		}
		if (optional != null)
		{
			optional.InHidingProcess = false;
		}
		void doRun(AbstractUnitEntity abstractUnitEntity)
		{
			AbstractUnitEntityView view = abstractUnitEntity.View;
			if (Unhide)
			{
				view.UnFade();
				if (abstractUnitEntity.IsInGame)
				{
					view.UpdateViewActive();
				}
				else
				{
					abstractUnitEntity.IsInGame = true;
				}
			}
			else if (Fade)
			{
				abstractUnitEntity.Commands.InterruptAllInterruptible();
				view.FadeHide();
				view.MovementAgent.Blocker.Unblock();
				abstractUnitEntity.IsInGame = false;
			}
			else
			{
				abstractUnitEntity.IsInGame = false;
			}
		}
	}
}
