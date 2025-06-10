using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[TypeId("1bbb85c386578f142b49f4dc6e8827cc")]
public abstract class BlueprintItemEquipment : BlueprintItem
{
	[ShowIf("GainAbility")]
	public bool SpendCharges = true;

	[ShowIf("ShowCharges")]
	public int Charges = 1;

	[ShowIf("ShowCharges")]
	public bool RestoreChargesAfterCombat;

	public bool IsNonRemovable;

	public bool IsUnlootable;

	[SerializeField]
	private KingmakerEquipmentEntityReference m_EquipmentEntity;

	[SerializeField]
	private KingmakerEquipmentEntityReference[] m_EquipmentEntityAlternatives = new KingmakerEquipmentEntityReference[0];

	[SerializeField]
	private int m_ForcedRampColorPresetIndex = -1;

	public int ForcedRampColorPresetIndex
	{
		get
		{
			return m_ForcedRampColorPresetIndex;
		}
		set
		{
			m_ForcedRampColorPresetIndex = value;
		}
	}

	public virtual IEnumerable<BlueprintAbility> Abilities => (from i in this.GetComponents<AddFactToEquipmentWielder>()
		select i.Fact).OfType<BlueprintAbility>();

	public virtual bool GainAbility => base.ComponentsArray.HasItem((BlueprintComponent i) => (i is AddFactToEquipmentWielder { Fact: var fact } && (fact is BlueprintAbility || fact is BlueprintActivatableAbility)) ? true : false);

	private bool ShowCharges
	{
		get
		{
			if (GainAbility)
			{
				return SpendCharges;
			}
			return false;
		}
	}

	public virtual KingmakerEquipmentEntity EquipmentEntity => m_EquipmentEntity.Get();

	[NotNull]
	public virtual IEnumerable<KingmakerEquipmentEntity> EquipmentEntityAlternatives => m_EquipmentEntityAlternatives.Dereference();

	public abstract override ItemsItemType ItemType { get; }

	public abstract string InventoryEquipSound { get; }

	public virtual bool CanBeEquippedBy(MechanicEntity entity)
	{
		return this.GetComponents<EquipmentRestriction>().Aggregate(seed: true, (bool r, EquipmentRestriction restriction) => r && restriction.CanBeEquippedBy(entity));
	}
}
