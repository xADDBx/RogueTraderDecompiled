using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.UnitLogic.UI;

public static class UIPropertyExtension
{
	public static IEnumerable<UIProperty> GetUIProperties(this BlueprintMechanicEntityFact fact, [CanBeNull] MechanicEntity owner, [CanBeNull] MechanicsContext context, [CanBeNull] ItemEntity item)
	{
		IEnumerable<UIPropertySettings> enumerable = fact.GetComponents<UIPropertiesComponent>()?.SelectMany((UIPropertiesComponent i) => i.Properties);
		if (enumerable == null)
		{
			yield break;
		}
		if (context == null)
		{
			context = owner?.MainFact.MaybeContext;
		}
		foreach (UIPropertySettings item2 in enumerable)
		{
			UIPropertyName nameType = item2.NameType;
			string name = item2.Name;
			string description = item2.Description;
			BlueprintMechanicEntityFact descriptionFact = item2.DescriptionFact;
			int? propertyValue = null;
			if (item2.PropertyName.HasValue && owner != null)
			{
				bool calculated;
				int propertyValue2 = (item2.PropertySource ?? fact).GetPropertyValue(item2.PropertyName.Value, owner, context, out calculated);
				if (calculated)
				{
					propertyValue = propertyValue2;
				}
			}
			yield return new UIProperty(nameType, name, description, descriptionFact, propertyValue);
		}
	}

	public static IEnumerable<UIProperty> GetUIProperties(this MechanicEntityFact fact, ItemEntity item)
	{
		return fact.Blueprint.GetUIProperties(fact.ConcreteOwner, fact.MaybeContext, item);
	}

	public static IEnumerable<UIProperty> GetUIProperties(this Ability ability)
	{
		return ability.GetUIProperties(ability.Data.Weapon);
	}

	public static IEnumerable<UIProperty> GetUIProperties(this AbilityData ability)
	{
		return ability.Fact.GetUIProperties(ability.Weapon);
	}
}
