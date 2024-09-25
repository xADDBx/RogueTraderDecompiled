using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;

namespace Kingmaker.UnitLogic.Customization;

public class PresetObject
{
	public UnitCustomizationVariation Variation { get; set; }

	public string Path { get; set; }

	public Character Character { get; set; }

	public UnitEntityView UnitEntityView { get; set; }

	public PresetObject(UnitCustomizationVariation variation, string path, Character character, UnitEntityView unitEntityView)
	{
		Variation = variation;
		Path = path;
		Character = character;
		UnitEntityView = unitEntityView;
	}
}
