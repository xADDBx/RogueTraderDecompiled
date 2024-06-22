using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Conditional")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[Obsolete]
[TypeId("65ae140109ad4f05a0ae3e0f4f226214")]
public class DodgeActions : ContextAction
{
	public ActionList ActionsOnHit;

	[HideInInspector]
	public int DodgePenalty;

	[HideInInspector]
	public ActionList ActionsOnDodge;

	public override string GetCaption()
	{
		return "Run ActionsOnHit, do nothing else";
	}

	public override string GetDescription()
	{
		return "Run ActionsOnHit, do nothing else";
	}

	protected override void RunAction()
	{
		ActionsOnHit.Run();
	}
}
