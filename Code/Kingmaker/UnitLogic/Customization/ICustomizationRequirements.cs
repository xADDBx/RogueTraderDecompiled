namespace Kingmaker.UnitLogic.Customization;

public interface ICustomizationRequirements
{
	bool HasRequirements();

	bool FitsRequirements(UnitCustomizationVariation variation);
}
