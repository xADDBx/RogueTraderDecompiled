using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("c27089a6cbd2469e8b28ddb3f2fdd3ba")]
public class InteractedEntity : MechanicEntityEvaluator
{
	public override string GetCaption()
	{
		return "Interacted Entity";
	}

	protected override Entity GetValueInternal()
	{
		return MechanicEntityData.CurrentEntity;
	}
}
