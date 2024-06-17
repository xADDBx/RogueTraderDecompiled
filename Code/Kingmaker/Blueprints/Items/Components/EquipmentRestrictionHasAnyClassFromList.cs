using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("a530b41bdc333d248a17b9b193fb4383")]
public class EquipmentRestrictionHasAnyClassFromList : EquipmentRestriction
{
	public bool Not;

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

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		PartUnitProgression progressionOptional = unit.GetProgressionOptional();
		if (progressionOptional == null)
		{
			return false;
		}
		foreach (BlueprintCharacterClass @class in Classes)
		{
			if (progressionOptional.GetClassLevel(@class) > 0)
			{
				return !Not;
			}
		}
		return Not;
	}
}
