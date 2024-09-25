using Kingmaker.Blueprints.Items.Weapons;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

public abstract class BlueprintItemEquipmentHand : BlueprintItemEquipment
{
	[SerializeField]
	protected WeaponVisualParameters m_VisualParameters;

	public WeaponVisualParameters VisualParameters => m_VisualParameters;

	public override string InventoryEquipSound => VisualParameters.InventoryEquipSound;

	public override string InventoryPutSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryPutSound))
			{
				return base.InventoryPutSound;
			}
			return VisualParameters.InventoryPutSound;
		}
	}

	public override string InventoryTakeSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryTakeSound))
			{
				return base.InventoryTakeSound;
			}
			return VisualParameters.InventoryTakeSound;
		}
	}
}
