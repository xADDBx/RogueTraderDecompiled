using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("635bfd81503374e4b8d8fc7eeefd9a7f")]
public class ContextCasterPositionEvaluator : PositionEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate caster position of Context";
	}

	protected override Vector3 GetValueInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.Context?.MaybeCaster?.Position ?? throw new FailToEvaluateException(this);
	}
}
