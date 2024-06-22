using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
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
		AbstractUnitEntityView view = value.View;
		if (Unhide)
		{
			view.UnFade();
			if (value.IsInGame)
			{
				view.UpdateViewActive();
			}
			else
			{
				value.IsInGame = true;
			}
		}
		else if (Fade)
		{
			value.Commands.InterruptAllInterruptible();
			view.FadeHide();
			view.MovementAgent.Blocker.Unblock();
			value.IsInGame = false;
		}
		else
		{
			value.IsInGame = false;
		}
	}
}
