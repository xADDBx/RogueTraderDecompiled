using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("617887ceade1439a909ca130150ed7e2")]
public class ContextCasterEvaluator : MechanicEntityEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate caster of Context";
	}

	protected override Entity GetValueInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.Context.MaybeCaster;
	}
}
