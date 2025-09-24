using JetBrains.Annotations;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.UnitLogic.UI;

public readonly struct UIProperty
{
	public readonly UIPropertyName NameType;

	public readonly string Name;

	public readonly string Description;

	public readonly bool Main;

	[CanBeNull]
	public readonly BlueprintMechanicEntityFact DescriptionFact;

	public readonly int? PropertyValue;

	public UIProperty(UIPropertyName nameType, string name, string description, bool main, [CanBeNull] BlueprintMechanicEntityFact descriptionFact, int? propertyValue)
	{
		NameType = nameType;
		Name = name;
		Description = description;
		Main = main;
		DescriptionFact = descriptionFact;
		PropertyValue = propertyValue;
	}
}
