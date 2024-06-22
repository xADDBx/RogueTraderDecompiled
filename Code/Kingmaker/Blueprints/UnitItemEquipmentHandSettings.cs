using System;
using Kingmaker.Blueprints.Items.Equipment;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
public class UnitItemEquipmentHandSettings
{
	[SerializeField]
	private BlueprintItemEquipmentHandReference m_PrimaryHand;

	[SerializeField]
	private BlueprintItemEquipmentHandReference m_SecondaryHand;

	[SerializeField]
	private BlueprintItemEquipmentHandReference m_PrimaryHandAlternative1;

	[SerializeField]
	private BlueprintItemEquipmentHandReference m_SecondaryHandAlternative1;

	public int ActiveHandSet;

	public BlueprintItemEquipmentHand PrimaryHand
	{
		get
		{
			return m_PrimaryHand?.Get();
		}
		set
		{
			m_PrimaryHand = value.ToReference<BlueprintItemEquipmentHandReference>();
		}
	}

	public BlueprintItemEquipmentHand SecondaryHand
	{
		get
		{
			return m_SecondaryHand?.Get();
		}
		set
		{
			m_SecondaryHand = value.ToReference<BlueprintItemEquipmentHandReference>();
		}
	}

	public BlueprintItemEquipmentHand PrimaryHandAlternative1
	{
		get
		{
			return m_PrimaryHandAlternative1?.Get();
		}
		set
		{
			m_PrimaryHandAlternative1 = value.ToReference<BlueprintItemEquipmentHandReference>();
		}
	}

	public BlueprintItemEquipmentHand SecondaryHandAlternative1
	{
		get
		{
			return m_SecondaryHandAlternative1?.Get();
		}
		set
		{
			m_SecondaryHandAlternative1 = value.ToReference<BlueprintItemEquipmentHandReference>();
		}
	}
}
