using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
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
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent is AreaEffectClusterComponent areaEffectClusterComponent)
			{
				BlueprintComponent[] componentsArray2 = areaEffectClusterComponent.ClusterLogicBlueprint.ComponentsArray;
				foreach (BlueprintComponent blueprintComponent2 in componentsArray2)
				{
					if (blueprintComponent2 is PropertyCalculatorComponent propertyCalculatorComponent && propertyCalculatorComponent.Name == propertyName)
					{
						return CalculatePropertyValue(propertyName, currentEntity, context, out calculated, propertyCalculatorComponent, blueprintComponent2);
					}
				}
			}
			if (blueprintComponent is PropertyCalculatorComponent propertyCalculatorComponent2 && propertyCalculatorComponent2.Name == propertyName)
			{
				return CalculatePropertyValue(propertyName, currentEntity, context, out calculated, propertyCalculatorComponent2, blueprintComponent);
			}
		}
		calculated = false;
		PFLog.Default.ErrorWithReport($"Can't find local property '{propertyName}' in blueprint {blueprint.name}");
		return 0;
	}

	private static int CalculatePropertyValue(ContextPropertyName propertyName, MechanicEntity currentEntity, MechanicsContext context, out bool calculated, PropertyCalculatorComponent property, BlueprintComponent component)
	{
		PropertyContext context2 = ((ContextData<PropertyContextData>.Current == null) ? new PropertyContext(currentEntity, null, null, context) : ContextData<PropertyContextData>.Current.Context.WithCurrentEntity(currentEntity).WithContext(context));
		int value = property.GetValue(context2);
		calculated = true;
		return value;
	}
}
