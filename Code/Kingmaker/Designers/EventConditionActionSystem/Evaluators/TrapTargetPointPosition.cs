using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d1ce8a1877389b046ad6cb9cdb9da99b")]
public class TrapTargetPointPosition : PositionEvaluator
{
	protected override Vector3 GetValueInternal()
	{
		return (ContextData<BlueprintTrap.ElementsData>.Current ?? throw new FailToEvaluateException(this)).TrapObject.Settings.TargetPoint.position;
	}

	public override string GetCaption()
	{
		return "Trap target point position";
	}
}
