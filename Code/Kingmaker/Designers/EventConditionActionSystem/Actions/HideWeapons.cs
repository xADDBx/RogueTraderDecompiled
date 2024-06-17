using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/HideWeapons")]
[TypeId("2284267c23c69d442a9c3b9e9954283b")]
public class HideWeapons : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool Hide = true;

	public override string GetDescription()
	{
		return string.Format("{0} оружие у юнита {1}", Hide ? "Прячет" : "Достает", Target);
	}

	public override string GetCaption()
	{
		return (Hide ? "Hide" : "Show") + " weapons for " + (Target ? Target.GetCaption() : "???");
	}

	public override void RunAction()
	{
		Target.GetValue().View.ForcePeacefulLook(Hide);
	}
}
