using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker;

[TypeId("2dbb3d0403a77fd498a3d2716dc3f5ea")]
public class EquipmentRestrictionAugmentTier : EquipmentRestriction
{
	[SerializeField]
	private AugmentTier m_AugmentTier;

	public AugmentTier AugmentTier => m_AugmentTier;

	public override bool CanBeEquippedBy(MechanicEntity _)
	{
		if (base.OwnerBlueprint is BlueprintItemAugment)
		{
			return Game.Instance.Player.PartyAugmentManager.CanEquipAugment(m_AugmentTier);
		}
		return false;
	}
}
