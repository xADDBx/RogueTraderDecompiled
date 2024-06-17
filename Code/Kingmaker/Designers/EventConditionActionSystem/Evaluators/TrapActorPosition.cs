using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("918d5295d10469d48a4d5b8a15985911")]
public class TrapActorPosition : PositionEvaluator
{
	protected override Vector3 GetValueInternal()
	{
		return (ContextData<BlueprintTrap.ElementsData>.Current ?? throw new FailToEvaluateException(this)).TrapObject.Data.Position;
	}

	public override string GetCaption()
	{
		return "Trap actor position";
	}
}
