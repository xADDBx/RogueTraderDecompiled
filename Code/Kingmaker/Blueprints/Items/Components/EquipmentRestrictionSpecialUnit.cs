using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("4aa2a6dd74a24558bceef9fe19bf0cb0")]
public class EquipmentRestrictionSpecialUnit : EquipmentRestriction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return unit.Blueprint == Blueprint;
	}
}
