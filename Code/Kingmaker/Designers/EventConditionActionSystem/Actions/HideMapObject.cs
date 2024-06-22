using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/HideMapObject")]
[AllowMultipleComponents]
[TypeId("3abef1c138b2b3344bebcf6fbbe5cf47")]
[PlayerUpgraderAllowed(true)]
public class HideMapObject : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MechanicEntityEvaluator MapObject;

	public bool Unhide;

	public override string GetDescription()
	{
		return string.Format("{0} мапобжект {1}", Unhide ? "Показывает " : "Прячет", MapObject);
	}

	protected override void RunAction()
	{
		MapObject.GetValue().IsInGame = Unhide;
	}

	public override string GetCaption()
	{
		return (Unhide ? "Show " : "Hide") + MapObject?.GetCaption();
	}
}
