using System.Text;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[TypeId("8e6b3a77f08a57042bd423dd80723887")]
public class PrerequisiteObsoleteNotProficient : Prerequisite_Obsolete
{
	public ArmorProficiencyGroup[] ArmorProficiencies;

	public WeaponProficiency[] WeaponProficiencies;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		bool checkArmor = false;
		ArmorProficiencies.EmptyIfNull().ForEach(delegate(ArmorProficiencyGroup i)
		{
			if (!unit.Proficiencies.Contains(i))
			{
				checkArmor = true;
			}
		});
		bool checkWeapon = false;
		WeaponProficiencies.EmptyIfNull().ForEach(delegate(WeaponProficiency i)
		{
			if (!unit.Proficiencies.Contains(i))
			{
				checkWeapon = true;
			}
		});
		return checkWeapon || checkArmor;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{UIStrings.Instance.Tooltips.NoProficiencies}:\n");
		for (int i = 0; i < ArmorProficiencies.Length; i++)
		{
			stringBuilder.Append(LocalizedTexts.Instance.Stats.GetText(ArmorProficiencies[i]));
			if (i < ArmorProficiencies.Length - 1 || WeaponProficiencies.Length != 0)
			{
				stringBuilder.Append("\n");
			}
		}
		for (int j = 0; j < WeaponProficiencies.Length; j++)
		{
			stringBuilder.Append(LocalizedTexts.Instance.Stats.GetText(WeaponProficiencies[j].Category));
			if (j < WeaponProficiencies.Length - 1)
			{
				stringBuilder.Append("\n");
			}
		}
		return stringBuilder.ToString();
	}
}
