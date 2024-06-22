using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyCalculatorComponentHelper
{
	public static int GetPropertyValue(this BlueprintScriptableObject blueprint, ContextPropertyName propertyName, MechanicEntity currentEntity, MechanicsContext context)
	{
		bool calculated;
		return blueprint.GetPropertyValue(propertyName, currentEntity, context, out calculated);
	}

	public static int GetPropertyValue(this BlueprintScriptableObject blueprint, ContextPropertyName propertyName, MechanicEntity currentEntity, MechanicsContext context, out bool calculated)
	{
		BlueprintComponent[] componentsArray = blueprint.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is PropertyCalculatorComponent propertyCalculatorComponent && propertyCalculatorComponent.Name == propertyName)
			{
				PropertyContext context2 = ((ContextData<PropertyContextData>.Current == null) ? new PropertyContext(currentEntity, null, null, context) : ContextData<PropertyContextData>.Current.Context.WithCurrentEntity(currentEntity).WithContext(context));
				int value = propertyCalculatorComponent.GetValue(context2);
				calculated = true;
				return value;
			}
		}
		calculated = false;
		PFLog.Default.ErrorWithReport($"Can't find local property '{propertyName}' in blueprint {blueprint.name}");
		return 0;
	}
}
