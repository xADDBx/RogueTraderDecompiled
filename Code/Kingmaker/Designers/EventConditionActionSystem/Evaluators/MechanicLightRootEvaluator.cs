using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("a46af28f03764ce581de0af75e60ae67")]
public class MechanicLightRootEvaluator : MechanicEntityEvaluator
{
	[AllowedEntityType(typeof(MechanicLightRoot))]
	[ValidateNotEmpty]
	public EntityReference MechanicLightRoot;

	protected override Entity GetValueInternal()
	{
		MechanicLightRootView mechanicLightRootView = MechanicLightRoot.FindView() as MechanicLightRootView;
		if (!(mechanicLightRootView != null))
		{
			return null;
		}
		return mechanicLightRootView.Data;
	}

	public override string GetCaption()
	{
		return "MechanicLightRoot object";
	}
}
