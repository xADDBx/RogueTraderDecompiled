using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[TypeId("0a39cda6c3c9425799b6c89009d48da7")]
public class BlueprintItemEquipmentPetProtocol : BlueprintItemEquipmentSimple
{
	[SerializeField]
	private PetType m_PetType;

	public PetType PetType => m_PetType;

	public override ItemsItemType ItemType => ItemsItemType.PetProtocol;

	public override bool CanBeEquippedBy(MechanicEntity entity)
	{
		if ((entity as BaseUnitEntity)?.Master?.GetOptional<UnitPartPetOwner>()?.PetType == PetType)
		{
			return base.CanBeEquippedBy(entity);
		}
		return false;
	}
}
