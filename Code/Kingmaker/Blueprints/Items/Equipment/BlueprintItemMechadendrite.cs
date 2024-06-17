using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.View.Equipment;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[TypeId("44a488a140c34c899dbc66359835c5fc")]
public class BlueprintItemMechadendrite : BlueprintItemEquipment
{
	[Serializable]
	public class BlueprintItemMechadendriteReference : BlueprintReference<BlueprintItemMechadendrite>
	{
	}

	[SerializeField]
	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	private GameObject m_MechadendriteModel;

	public override ItemsItemType ItemType => ItemsItemType.Mechadendrite;

	public override string InventoryEquipSound => "";

	public GameObject Model => m_MechadendriteModel;
}
