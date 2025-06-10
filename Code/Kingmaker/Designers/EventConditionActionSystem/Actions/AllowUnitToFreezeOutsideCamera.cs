using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("8fb7289742c04cf59b588e37fbe2c14c")]
[PlayerUpgraderAllowed(true)]
public class AllowUnitToFreezeOutsideCamera : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool FreezeOutsideCamera;

	public override string GetCaption()
	{
		return (FreezeOutsideCamera ? "Allow" : "Don't allow") + " unit " + Target?.GetCaption() + " to freeze outside camera";
	}

	protected override void RunAction()
	{
		Target.GetValue().FreezeOutsideCamera = FreezeOutsideCamera;
	}
}
