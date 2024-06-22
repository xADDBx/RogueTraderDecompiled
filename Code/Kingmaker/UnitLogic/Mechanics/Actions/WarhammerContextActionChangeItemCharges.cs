using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("6a102fe0ab870d34b8b5153dd90e8bea")]
public class WarhammerContextActionChangeItemCharges : ContextAction
{
	public enum ChangingType
	{
		Set,
		Add,
		Substract
	}

	[SerializeField]
	private ChangingType m_Type;

	[SerializeField]
	private int m_Value;

	[SerializeField]
	private BlueprintItemEquipmentReference m_Item;

	[SerializeField]
	[HideIf("ItemIsProvided")]
	private StarshipWeaponType starshipWeaponType;

	private bool ItemIsProvided => Item != null;

	public BlueprintItemEquipment Item => m_Item?.Get();

	public override string GetCaption()
	{
		string result = "";
		string text = (ItemIsProvided ? Item.name : starshipWeaponType.ToString());
		switch (m_Type)
		{
		case ChangingType.Set:
			result = $"Set {text} charges equal to {m_Value}";
			break;
		case ChangingType.Add:
			result = $"Add {m_Value} charges to {text}";
			break;
		case ChangingType.Substract:
			result = $"Substract {m_Value} charges from {text}";
			break;
		}
		return result;
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = base.Context.MainTarget?.Entity;
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Target is missing");
			return;
		}
		ItemEntity item = GetItem(mechanicEntity, (BlueprintItemEquipment)m_Item, starshipWeaponType);
		if (item == null)
		{
			Element.LogError(this, "Target has no specified item");
			return;
		}
		switch (m_Type)
		{
		case ChangingType.Set:
			item.Charges = Math.Max(0, m_Value);
			break;
		case ChangingType.Add:
			item.Charges = Math.Max(0, item.Charges + m_Value);
			break;
		case ChangingType.Substract:
			item.Charges = Math.Max(0, item.Charges - m_Value);
			break;
		}
	}

	private ItemEntity GetItem(MechanicEntity entity, BlueprintItem bpItem, StarshipWeaponType weaponType)
	{
		if (entity is StarshipEntity starshipEntity)
		{
			foreach (ItemEntityStarshipWeapon weapon in starshipEntity.Hull.Weapons)
			{
				if ((ItemIsProvided && weapon.Blueprint == bpItem) || (!ItemIsProvided && weapon.Blueprint.WeaponType == weaponType))
				{
					return weapon;
				}
			}
		}
		else if (entity is UnitEntity unitEntity)
		{
			foreach (ItemEntity item in unitEntity.Body.Items)
			{
				if (item.Blueprint == bpItem)
				{
					return item;
				}
			}
		}
		return null;
	}
}
