using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("43f625560fee8c841b17e6c48691119e")]
[AllowMultipleComponents]
public class EquipmentRestrictionClass : EquipmentRestriction
{
	[SerializeField]
	[FormerlySerializedAs("Class")]
	private BlueprintCharacterClassReference m_Class;

	public bool Not;

	public BlueprintCharacterClass Class => m_Class?.Get();

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		PartUnitProgression progressionOptional = unit.GetProgressionOptional();
		if (progressionOptional != null)
		{
			if (Not || progressionOptional.GetClassLevel(Class) <= 0)
			{
				if (Not)
				{
					return progressionOptional.GetClassLevel(Class) == 0;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
