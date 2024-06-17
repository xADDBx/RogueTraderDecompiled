using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowMultipleComponents]
[TypeId("18a82f0cb0644d5cb61b092feea5cfe2")]
public class PrerequisiteObsoleteTakenClass : Prerequisite_Obsolete
{
	[SerializeField]
	private BlueprintCharacterClassReference[] m_Classes;

	public ReferenceArrayProxy<BlueprintCharacterClass> Classes
	{
		get
		{
			BlueprintReference<BlueprintCharacterClass>[] classes = m_Classes;
			return classes;
		}
	}

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return CheckClassLevel(unit);
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		return "Fake requirement for Vertical Slice - the character can only level one class at a time";
	}

	private bool CheckClassLevel(BaseUnitEntity unit)
	{
		foreach (BlueprintCharacterClass @class in Classes)
		{
			int classLevel = unit.Progression.GetClassLevel(@class);
			if ((@class.PrestigeClass && classLevel < 15 && classLevel >= 1) || (!@class.PrestigeClass && classLevel < 12 && classLevel >= 1))
			{
				return false;
			}
		}
		return true;
	}
}
