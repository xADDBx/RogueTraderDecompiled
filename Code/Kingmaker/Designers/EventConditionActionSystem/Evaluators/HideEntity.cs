using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("4847fd6c11264ff4af657a8e278be3b8")]
public class HideEntity : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public EntityEvaluator Target;

	public bool Unhide;

	public override string GetDescription()
	{
		return string.Format("{0} энтити {1}", Unhide ? "Показывает " : "Прячет ", Target);
	}

	public override string GetCaption()
	{
		return (Unhide ? "Show " : "Hide ") + Target?.GetCaption();
	}

	protected override void RunAction()
	{
		Target.GetValue().IsInGame = Unhide;
	}
}
