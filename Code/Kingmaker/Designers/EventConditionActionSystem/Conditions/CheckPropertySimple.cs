using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("c260efd0e44d4869af22ab326a66f8ae")]
public class CheckPropertySimple : Condition
{
	public PropertyCalculator Value;

	protected override string GetConditionCaption()
	{
		return $"Check property simple: {Value}";
	}

	protected override bool CheckCondition()
	{
		PropertyContext? propertyContext = ContextData<PropertyContextData>.Current?.Context;
		if (!propertyContext.HasValue)
		{
			MechanicEntity mechanicEntity = ContextData<MechanicsContext.Data>.Current?.Context?.MaybeCaster;
			if (mechanicEntity == null)
			{
				PFLog.Default.ErrorWithReport("CurrentEntity is missing");
				return false;
			}
			propertyContext = new PropertyContext(mechanicEntity, null);
		}
		return Value.GetBoolValue(propertyContext.Value);
	}
}
