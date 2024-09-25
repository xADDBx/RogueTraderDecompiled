using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("9b05ad74824a436ebe056cfc6306c9b9")]
public class ContextTargetEntityEvaluator : MechanicEntityEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate target entity of Context";
	}

	protected override Entity GetValueInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Entity;
	}
}
