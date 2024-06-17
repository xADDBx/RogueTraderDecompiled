using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("c064b7c6f2194f3aa8dcaf6b45a261cd")]
public class ContextTargetPositionEvaluator : PositionEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate target position of Context";
	}

	protected override Vector3 GetValueInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.CurrentTarget?.Point ?? throw new FailToEvaluateException(this);
	}
}
