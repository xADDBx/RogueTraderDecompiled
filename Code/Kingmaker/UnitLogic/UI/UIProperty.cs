using JetBrains.Annotations;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.UnitLogic.UI;

public readonly struct UIProperty
{
	public readonly UIPropertyName NameType;

	public readonly string Name;

	public readonly string Description;

	[CanBeNull]
	public readonly BlueprintMechanicEntityFact DescriptionFact;

	public readonly int? PropertyValue;

	public UIProperty(UIPropertyName nameType, string name, string description, [CanBeNull] BlueprintMechanicEntityFact descriptionFact, int? propertyValue)
	{
		NameType = nameType;
		Name = name;
		Description = description;
		DescriptionFact = descriptionFact;
		PropertyValue = propertyValue;
	}
}
